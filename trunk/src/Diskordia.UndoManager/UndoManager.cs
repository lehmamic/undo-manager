/*****************************************************************************
 * UndoManager. An easy to use undo API.
 * Copyright (C) 2009 Michael Lehmann 
 ******************************************************************************
 * This file is part of UndoManager.
 *
 * UndoManager is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * UndoManager is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with UndoManager.  If not, see <http://www.gnu.org/licenses/>.
 *****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Diskordia.UndoRedo.Invokations;
using Diskordia.UndoRedo.Properties;
using Diskordia.UndoRedo.State;
using Diskordia.UndoRedo.Transactions;

namespace Diskordia.UndoRedo
{
	/// <summary>
	/// The undo manager records undo operations to provide undo and redo logic.
	/// </summary>
	public sealed partial class UndoManager : IUndoManager, ITransactionManager, IStateHost
	{
		private readonly ITransactionFactory transactionFactory;
		private readonly Stack<IInvokableTransaction> undoHistory = new Stack<IInvokableTransaction>();
		private readonly Stack<IInvokableTransaction> redoHistory = new Stack<IInvokableTransaction>();
		private readonly Stack<IInvokableTransaction> openTransactions = new Stack<IInvokableTransaction>();

		private UndoRedoState state = UndoRedoState.Idle;
		private string actionName = string.Empty;

		/// <summary>
		/// Initializes a new instance of the <see cref="UndoManager"/> class.
		/// </summary>
		public UndoManager()
			: this(new TransactionFactory())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UndoManager"/> class.
		/// </summary>
		/// <param name="transactionFactory">The transaction factory.</param>
		/// <exception cref="ArgumentNullException"><paramref name="transactionFactory"/> is a <see langword="null"/> reference.</exception>
		internal UndoManager(ITransactionFactory transactionFactory)
		{
			if (transactionFactory == null)
			{
				throw new ArgumentNullException("transactionFactory");
			}

			this.transactionFactory = transactionFactory;
		}

		#region IStateHost members

		/// <summary>
		/// Gets or sets the <see cref="UndoRedoState"/> indicating the status of the <see cref="Diskordia.UndoRedo.UndoManager"/>.
		/// </summary>
		UndoRedoState IStateHost.State
		{
			get
			{
				return this.state;
			}

			set
			{
				this.state = value;
			}
		}

		#endregion

		#region IUndoManager members

		/// <summary>
		/// Gets a value indicating whether the <see cref="UndoManager"/> is undoing.
		/// </summary>
		/// <value>
		/// 	<c>True</c> if the <see cref="UndoManager"/> is undoing; otherwise, <c>false</c>.
		/// </value>
		public bool IsUndoing
		{
			get { return this.state == UndoRedoState.Undoing; }
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="UndoManager"/> is redoing.
		/// </summary>
		/// <value>
		/// 	<c>True</c> if the <see cref="UndoManager"/> is redoing; otherwise, <c>false</c>.
		/// </value>
		public bool IsRedoing
		{
			get { return this.state == UndoRedoState.Redoing; }
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="UndoManager"/> can perform an undo operation.
		/// </summary>
		/// <value><c>True</c> if the <see cref="UndoManager"/> can perform an undo operation; otherwise, <c>false</c>.</value>
		public bool CanUndo
		{
			get { return this.undoHistory.Any() || this.openTransactions.Any(); }
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="UndoManager"/> can perform an redo operation.
		/// </summary>
		/// <value><c>True</c> if the <see cref="UndoManager"/> can perform an redo operation, <c>false</c>.</value>
		public bool CanRedo
		{
			get { return this.redoHistory.Any(); }
		}

		/// <summary>
		/// Registers an operation as lambda expression, which will be invoked when an undo is performed.
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <param name="target">The target instance.</param>
		/// <param name="selector">The invocation delegate of the undo operation.</param>
		/// <exception cref="ArgumentNullException">
		///		<para><paramref name="target"/> is a <see langword="null"/> reference</para>
		///		<para>- or -</para>
		///		<para><paramref name="selector"/> is a <see langword="null"/> reference.</para>
		/// </exception>
		public void RegisterInvokation<TSource>(TSource target, Expression<Action<TSource>> selector)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}

			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}

			LambdaExpressionInvokation<TSource> invokation = new LambdaExpressionInvokation<TSource>(target, selector);
			this.RegisterInvokation(invokation);
		}

		/// <summary>
		/// Registers an operation with the provided argument, which will be invoked when an undo is performed.
		/// </summary>
		/// <typeparam name="TArgument">The type of the argument.</typeparam>
		/// <param name="selector">The invocation delegate of the undo operation.</param>
		/// <param name="argument">The argument to pass the teh method call while invoking the registered invokation.</param>
		/// <exception cref="ArgumentNullException">
		///		<para><paramref name="selector"/> is a <see langword="null"/> reference</para>
		///		<para>- or -</para>
		///		<para><paramref name="argument"/> is a <see langword="null"/> reference.</para>
		/// </exception>
		public void RegisterInvokation<TArgument>(Action<TArgument> selector, TArgument argument)
		{
			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}

			if (argument == null)
			{
				throw new ArgumentNullException("argument");
			}

			ActionInvokation<TArgument> invokation = new ActionInvokation<TArgument>(selector, argument);
			this.RegisterInvokation(invokation);
		}

		/// <summary>
		/// Registers an <see cref="IInvokable"/> implementation to the <see cref="ITransaction"/>.
		/// </summary>
		/// <param name="invokation">The invokation to register.</param>
		/// <exception cref="ArgumentNullException"><paramref name="invokation"/> is a <see langword="null"/> reference.</exception>
		public void RegisterInvokation(IInvokable invokation)
		{
			if (invokation == null)
			{
				throw new ArgumentNullException("invokation");
			}

			if (this.state != UndoRedoState.RollingBack)
			{
				IInvokableTransaction recordingTransaction = this.FetchRecordingTransaction();
				if (recordingTransaction != null)
				{
					recordingTransaction.RegisterInvokation(invokation);
				}
				else
				{
					using (IInvokableTransaction transaction = this.InnerCreateTransaction(this.actionName))
					{
						transaction.RegisterInvokation(invokation);
					}
				}
			}
		}

		/// <summary>
		/// Prepares the target object as the subject for the dynamically invoked undo/redo operations.
		/// </summary>
		/// <param name="target">The target of the dynamic invokation.</param>
		/// <returns>The dynamic object targeting the provided <paramref name="target"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="target"/> is a <see langword="null"/> reference.</exception>
		public dynamic PrepareWithInvocationTarget(object target)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}

			return new UndressedInvokationTarget(this, target);
		}

		/// <summary>
		/// Invokes the last recorded undo operation or transaction.
		/// </summary>
		/// <exception cref="ActionInvokationException">An error occured while invoking the registered undo operation.</exception>
		public void Undo()
		{
			if (this.openTransactions.Any())
			{
				this.CommitTransactions();
			}

			if (!this.undoHistory.Any())
			{
				throw new InvalidOperationException("No undo operations recorded.");
			}

			using (new StateSwitcher(this, UndoRedoState.Undoing))
			{
				IInvokableTransaction invocableToUndo = this.undoHistory.Pop();

				using (this.InnerCreateTransaction(invocableToUndo.ActionName))
				{
					InvokeInvocation(invocableToUndo);
				}
			}
		}

		/// <summary>
		/// Invokes the last recorded redo operation or transaction.
		/// </summary>
		/// <exception cref="ActionInvokationException">An error occured while invoking the registered redo operation.</exception>
		public void Redo()
		{
			if (!this.redoHistory.Any())
			{
				throw new InvalidOperationException("No redo operations recorded.");
			}

			using (new StateSwitcher(this, UndoRedoState.Redoing))
			{
				IInvokableTransaction invocableToRedo = this.redoHistory.Pop();

				using (this.InnerCreateTransaction(invocableToRedo.ActionName))
				{
					InvokeInvocation(invocableToRedo);
				}
			}
		}

		/// <summary>
		/// Creates a <see cref="ITransaction"/> for recording the undo operations. Undo operations
		/// which are recorded while a <see cref="ITransaction"/> is opened get recorded by the <see cref="IUndoManager"/> only if the <see cref="ITransaction"/>
		/// is commited. A rollback will execute the undo operations which where registered while the <see cref="ITransaction"/> has been open.
		/// </summary>
		/// <returns>A new instance of the <see cref="ITransaction"/> class.</returns>
		public ITransaction CreateTransaction()
		{
			return this.InnerCreateTransaction(this.actionName);
		}

		/// <summary>
		/// Commits the open transactions and records the registered undo operations in the <see cref="IUndoManager"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">No open <see cref="ITransaction"/> to commit available.</exception>
		public void CommitTransactions()
		{
			if (!this.openTransactions.Any())
			{
				throw new InvalidOperationException("There is no open transaction available to commit.");
			}

			IInvokableTransaction transaction = this.openTransactions.First();
			transaction.Commit();
		}

		/// <summary>
		/// Rollbacks the open transaction and invokes the regsitered undo operations.
		/// </summary>
		/// <exception cref="ActionInvokationException">An error occured while invoking the undo operations within the open transaction.</exception>
		public void RollbackTransactions()
		{
			if (!this.openTransactions.Any())
			{
				throw new InvalidOperationException("There is no open transaction available to roll back.");
			}

			IInvokableTransaction transaction = this.openTransactions.First();
			transaction.Rollback();
		}

		/// <summary>
		/// Sets the action name of the undo/redo operation, which will be appended to a localized undo/redo menu item label.
		/// </summary>
		/// <remarks>
		/// The action name should always be set atthe same time like the registration of the operation.
		/// </remarks>
		/// <param name="actionName">The name of the undo redo operation.</param>
		/// <exception cref="ArgumentNullException"><paramref name="actionName"/> is a <see langword="null"/> reference.</exception>
		public void SetActionName(string actionName)
		{
			if (actionName == null)
			{
				throw new ArgumentNullException("actionName");
			}

			this.actionName = actionName;

			IInvokableTransaction currentTransaction = this.openTransactions.FirstOrDefault();
			if (currentTransaction != null)
			{
				currentTransaction.ActionName = actionName;
			}
		}

		/// <summary>
		/// Gets the action name of the undo operation.
		/// </summary>
		public string UndoActionName
		{
			get
			{
				return this.undoHistory.Count > 0 ? this.undoHistory.Peek().ActionName : string.Empty;
			}
		}

		/// <summary>
		/// Gets the localized titel of the menu item according to the current undo action name.
		/// </summary>
		public string UndoMenuItemTitel
		{
			get { return UndoManager.GetUndoMenuTitleForUndoActionName(this.UndoActionName); }
		}

		/// <summary>
		/// Gets the action name of the redo operation.
		/// </summary>
		public string RedoActionName
		{
			get
			{
				return this.redoHistory.Count > 0 ? this.redoHistory.Peek().ActionName : string.Empty;
			}
		}

		/// <summary>
		/// Gets the localized titel of the menu item according to the current redo action name.
		/// </summary>
		public string RedoMenuItemTitel
		{
			get { return UndoManager.GetRedoMenuTitleForRedoActionName(this.UndoActionName); }
		}

		#endregion

		#region ITransactionManager members

		/// <summary>
		/// Commits the provided transaction.
		/// </summary>
		/// <param name="transaction">The transaction to commit.</param>
		/// <exception cref="ArgumentNullException"><paramref name="transaction"/> is a <see langword="null"/> reference.</exception>
		/// <exception cref="ArgumentException">The <see cref="UndoManager"/> does not contain <paramref name="transaction"/>.</exception>
		void ITransactionManager.CommitTransaction(IInvokableTransaction transaction)
		{
			if (transaction == null)
			{
				throw new ArgumentNullException("transaction");
			}

			if (!this.openTransactions.Contains(transaction))
			{
				throw new ArgumentException("Can not find the transaction to commit", "transaction");
			}

			// only switch the state to committing if the undo manager is not doing another task (e.g. undoing).
			UndoRedoState state = this.state == UndoRedoState.Idle ? UndoRedoState.Committing : this.state;

			using (new StateSwitcher(this, state))
			{
				while (this.openTransactions.Contains(transaction))
				{
					IInvokableTransaction toCommit = this.openTransactions.Pop();

					if (toCommit.Any())
					{
						if (toCommit.Equals(transaction))
						{
							Stack<IInvokableTransaction> history = this.IsUndoing ? this.redoHistory : this.undoHistory;
							history.Push(transaction);
						}
						else
						{
							IInvokableTransaction topMost = this.openTransactions.Peek();
							topMost.RegisterInvokation(toCommit);
						}
					}
				}
			}
		}

		/// <summary>
		/// Rollbacks the provided transaction.
		/// </summary>
		/// <param name="transaction">The transaction to roll back.</param>
		/// <exception cref="ArgumentNullException"><paramref name="transaction"/> is a <see langword="null"/> reference.</exception>
		/// <exception cref="ArgumentException">The <see cref="UndoManager"/> does not contain <paramref name="transaction"/>.</exception>
		/// <exception cref="ActionInvokationException">An error occured while invoking the undo operations within the transaction.</exception>
		void ITransactionManager.RollbackTransaction(IInvokableTransaction transaction)
		{
			if (transaction == null)
			{
				throw new ArgumentNullException("transaction");
			}

			if (!this.openTransactions.Contains(transaction))
			{
				throw new ArgumentException("Can not find the transaction to roll back", "transaction");
			}

			while (this.openTransactions.Contains(transaction))
			{
				IInvokableTransaction toRollback = this.openTransactions.Pop();
			}
		}

		#endregion

		#region Private members

		private IInvokableTransaction InnerCreateTransaction(string undoActionName)
		{
			IInvokableTransaction transaction = this.transactionFactory.CreateTransaction(this);
			transaction.ActionName = undoActionName;
			this.openTransactions.Push(transaction);

			return transaction;
		}

		private IInvokableTransaction FetchRecordingTransaction()
		{
			return this.openTransactions.Count > 0 ? this.openTransactions.Peek() : null;
		}

		private static string GetMenuItemTitelForAction(string operation, string actionName)
		{
			return !string.IsNullOrEmpty(actionName) ? string.Format(CultureInfo.CurrentUICulture, Resources.UndoRedoMenuLabelPattern, operation, actionName) : operation;
		}

		private static void InvokeInvocation(IInvokable invokation)
		{
			try
			{
				invokation.Invoke();
			}
			catch (Exception e)
			{
				throw new ActionInvokationException(Resources.ActionInvokeExceptionMessage, e);
			}
		}

		#endregion

		#region Public static members

		/// <summary>
		/// Gets the localized title of the undo menu item for the action identified by the given name.
		/// </summary>
		/// <param name="actionName">Name of the action.</param>
		/// <returns>The localized titel of the undo menu item for the provided action name.</returns>
		public static string GetUndoMenuTitleForUndoActionName(string actionName)
		{
			return GetMenuItemTitelForAction(Resources.UndoMenuItemName, actionName);
		}

		/// <summary>
		/// Gets the localized title of the redo menu item for the action identified by the given name.
		/// </summary>
		/// <param name="actionName">Name of the action.</param>
		/// <returns>The localized titel of the redo menu item for the provided action name.</returns>
		public static string GetRedoMenuTitleForRedoActionName(string actionName)
		{
			return GetMenuItemTitelForAction(Resources.RedoMenuItemName, actionName);
		}

		#endregion
	}
}