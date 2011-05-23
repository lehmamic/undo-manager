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
using Diskordia.UndoRedo.Invocation;
using Diskordia.UndoRedo.Properties;
using Diskordia.UndoRedo.State;
using Diskordia.UndoRedo.Transaction;

namespace Diskordia.UndoRedo
{
	/// <summary>
	/// The undo manager records undo operations to provide undo and redo logic.
	/// </summary>
	public sealed partial class UndoManager : IUndoManager, IStateHost
	{
		private readonly Stack<IInvokable> undoHistory = new Stack<IInvokable>();
		private readonly Stack<IInvokable> redoHistory = new Stack<IInvokable>();
		private readonly Stack<UndoRedoTransaction> transactionStack = new Stack<UndoRedoTransaction>();

		private UndoRedoState state = UndoRedoState.Idle;
		private string actionName = string.Empty;

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
			get { return this.undoHistory.Any() || this.transactionStack.Any(); }
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
		/// Registers an operation into the undo history.
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <param name="target">The target instance.</param>
		/// <param name="selector">The invocation delegate of the undo operation.</param>
		/// <exception cref="ArgumentNullException">
		///		<para><paramref name="target"/> is a <see langword="null"/> reference</para>
		///		<para>- or -</para>
		///		<para><paramref name="selector"/> is a <see langword="null"/> reference.</para>
		/// </exception>
		public void RegisterInvocation<TSource>(TSource target, Expression<Action<TSource>> selector)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}

			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}

			if (this.state != UndoRedoState.RollingBack)
			{
				UndoRedoTransaction recordingTransaction = this.FetchRecordingTransaction();
				if (recordingTransaction != null)
				{
					recordingTransaction.RegisterInvokation(target, selector);
				}
				else
				{
					using (UndoRedoTransaction transaction = this.InnerCreateTransaction())
					{
						transaction.RegisterInvokation(target, selector);
					}
				}
			}
		}

		/// <summary>
		/// Invokes the last recorded undo operation or transaction.
		/// </summary>
		public void Undo()
		{
			this.CommitTransactions();

			if (!this.undoHistory.Any())
			{
				throw new InvalidOperationException("No undo operations recorded.");
			}

			using (new StateSwitcher(this, UndoRedoState.Undoing))
			{
				using (this.CreateTransaction())
				{
					IInvokable invocableToUndo = this.undoHistory.Pop();
					invocableToUndo.Invoke();
				}
			}
		}

		/// <summary>
		/// Invokes the last recorded redo operation or transaction.
		/// </summary>
		public void Redo()
		{
			if (!this.redoHistory.Any())
			{
				throw new InvalidOperationException("No redo operations recorded.");
			}

			using (new StateSwitcher(this, UndoRedoState.Redoing))
			{
				IInvokable invocableToRedo = this.redoHistory.Pop();
				invocableToRedo.Invoke();
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
			return this.InnerCreateTransaction();
		}

		/// <summary>
		/// Commits the open transactions and records the registered undo operations in the <see cref="IUndoManager"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">No open <see cref="ITransaction"/> to commit available.</exception>
		public void CommitTransactions()
		{
			if (!this.transactionStack.Any())
			{
				throw new InvalidOperationException("There is no open transaction available to commit.");
			}

			UndoRedoTransaction transaction = this.transactionStack.First();
			this.CommitTransaction(transaction);
		}

		/// <summary>
		/// Rollbacks the open transaction and invokes the regsitered undo operations.
		/// </summary>
		public void RollbackTransactions()
		{
			if (!this.transactionStack.Any())
			{
				throw new InvalidOperationException("There is no open transaction available to roll back.");
			}

			UndoRedoTransaction transaction = this.transactionStack.First();
			this.RollbackTransaction(transaction);
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

			UndoRedoTransaction currentTransaction = this.transactionStack.FirstOrDefault();
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

		#region Internal members

		/// <summary>
		/// Commits the provided transaction.
		/// </summary>
		/// <param name="transaction">The transaction to commit.</param>
		/// <exception cref="ArgumentNullException"><paramref name="transaction"/> is a <see langword="null"/> reference.</exception>
		/// <exception cref="ArgumentException">The <see cref="UndoManager"/> does not contain <paramref name="transaction"/>.</exception>
		internal void CommitTransaction(UndoRedoTransaction transaction)
		{
			if (transaction == null)
			{
				throw new ArgumentNullException("transaction");
			}

			if (!this.transactionStack.Contains(transaction))
			{
				throw new ArgumentException("Can not find the transaction to commit", "transaction");
			}

			using (new StateSwitcher(this, UndoRedoState.Committing))
			{
				while (this.transactionStack.Contains(transaction))
				{
					UndoRedoTransaction toCommit = this.transactionStack.Pop();

					if (this.transactionStack.Any())
					{
						UndoRedoTransaction topMost = this.transactionStack.Peek();
						topMost.RegisterInvocation(toCommit);
					}
					else
					{
						Stack<IInvokable> history = this.IsUndoing ? this.redoHistory : this.undoHistory;
						history.Push(transaction);
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
		internal void RollbackTransaction(UndoRedoTransaction transaction)
		{
			if (transaction == null)
			{
				throw new ArgumentNullException("transaction");
			}

			if (!this.transactionStack.Contains(transaction))
			{
				throw new ArgumentException("Can not find the transaction to roll back", "transaction");
			}

			while (this.transactionStack.Contains(transaction))
			{
				UndoRedoTransaction toRollback = this.transactionStack.Pop();
				toRollback.Invoke();
			}
		}

		#endregion

		#region Private members

		private UndoRedoTransaction InnerCreateTransaction()
		{
			UndoRedoTransaction transaction = new UndoRedoTransaction(this);
			transaction.ActionName = this.actionName;
			this.transactionStack.Push(transaction);

			return transaction;
		}

		private UndoRedoTransaction FetchRecordingTransaction()
		{
			return this.transactionStack.Count > 0 ? this.transactionStack.Peek() : null;
		}

		private static string GetMenuItemTitelForAction(string operation, string actionName)
		{
			return !string.IsNullOrEmpty(actionName) ? string.Format(CultureInfo.CurrentUICulture, Resources.UndoRedoMenuLabelPattern, operation, actionName) : operation;
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