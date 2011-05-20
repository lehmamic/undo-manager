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
using System.Linq.Expressions;
using UndoRedo.Invocation;
using UndoRedo.State;
using UndoRedo.Transaction;

namespace UndoRedo
{
	/// <summary>
	/// The undo manager records undo operations to provide undo and redo logic.
	/// </summary>
	public sealed class UndoManager : IUndoManager, IStateHost
	{
		private readonly Stack<IInvokable> undoHistory = new Stack<IInvokable>();
		private readonly Stack<IInvokable> redoHistory = new Stack<IInvokable>();
		private readonly Stack<UndoRedoTransaction> transactionStack = new Stack<UndoRedoTransaction>();

		private UndoRedoState state = UndoRedoState.Recording;

		#region IStateHost members

		/// <summary>
		/// The <see cref="UndoRedoState"/> indicating the status of the <see cref="UndoRedo.UndoManager"/>.
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
			get { return this.undoHistory.Count > 0 || this.transactionStack.Count > 0; }
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="UndoManager"/> can perform an redo operation.
		/// </summary>
		/// <value><c>True</c> if the <see cref="UndoManager"/> can perform an redo operation, <c>false</c>.</value>
		public bool CanRedo
		{
			get { return this.redoHistory.Count > 0; }
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

			if (this.state != UndoRedoState.RollingBackTransaction)
			{
				UndoRedoTransaction recordingTransaction = this.RecordingTransaction();
				if (recordingTransaction != null)
				{
					recordingTransaction.RegisterInvocation(target, selector);
				}
				else
				{
					using (UndoRedoTransaction transaction = this.InnerCreateTransaction())
					{
						transaction.RegisterInvocation(target, selector);
					}
				}
			}
		}

		/// <summary>
		/// Invokes the last recorded undo operation or transaction.
		/// </summary>
		public void Undo()
		{
			this.CommitOpenTransactions();

			if (this.undoHistory.Count == 0)
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
			if (this.redoHistory.Count == 0)
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
		/// which are recorded while a <see cref="ITransaction"/> is opened get recorded by the <see cref="UndoManager"/> only if the <see cref="ITransaction"/>
		/// is commited. A rollback will execute the undo operations which where registered while the <see cref="ITransaction"/> has been open.
		/// </summary>
		/// <returns>A new instance of the <see cref="ITransaction"/> class.</returns>
		public ITransaction CreateTransaction()
		{
			return this.InnerCreateTransaction();
		}

		/// <summary>
		/// Commits the created <see cref="ITransaction"/> and records the registered undo operation in the <see cref="UndoManager"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">No open <see cref="ITransaction"/> to commit available.</exception>
		public void CommitTransaction()
		{
			if (this.transactionStack.Count == 0)
			{
				throw new InvalidOperationException("There is no open transaction available to commit.");
			}

			UndoRedoTransaction transaction = this.transactionStack.Pop();

			if (transaction.Count > 0)
			{
				this.ProcessTransactionToCommit(transaction);
			}
		}

		/// <summary>
		/// Rollbacks the last created <see cref="Transaction"/> and invokes the regsitered undo operations.
		/// </summary>
		public void RollbackTransaction()
		{

			if (this.transactionStack.Count > 0)
			{
				using (new StateSwitcher(this, UndoRedoState.RollingBackTransaction))
				{
					UndoRedoTransaction transactionToRollback = this.transactionStack.Pop();
					transactionToRollback.Invoke();
				}
			}
		}

		#endregion

		#region Private Members

		private UndoRedoTransaction InnerCreateTransaction()
		{
			UndoRedoTransaction transaction = new UndoRedoTransaction(this);
			this.transactionStack.Push(transaction);

			return transaction;
		}

		private void CommitOpenTransactions()
		{
			while (this.transactionStack.Count > 0)
			{
				UndoRedoTransaction transaction = this.transactionStack.Peek();
				transaction.Commit();
			}
		}

		private void ProcessTransactionToCommit(UndoRedoTransaction transaction)
		{
			if (this.transactionStack.Count == 0)
			{
				Stack<IInvokable> history = this.IsUndoing ? this.redoHistory : this.undoHistory;
				history.Push(transaction);
			}
			else
			{
				UndoRedoTransaction parent = this.transactionStack.Peek();
				parent.Add(transaction);
			}
		}

		private UndoRedoTransaction RecordingTransaction()
		{
			return this.transactionStack.Count > 0 ? this.transactionStack.Peek() : null;
		}

		#endregion

		#region Signleton Creator

		/// <summary>
		/// Internal class to support lazy initialization.
		/// </summary>
		private class SingletonCreator
		{
			/// <summary>
			/// Initializes static members of the <see cref="SingletonCreator"/> class.
			/// </summary>
			static SingletonCreator()
			{
			}

			/// <summary>
			/// Prevents a default instance of the <see cref="SingletonCreator"/> class from being created.
			/// </summary>
			private SingletonCreator()
			{
			}

			/// <summary>
			/// Threadsafe implementation (compiler guaranteed whith static initializatrion).
			/// This implementation uses an inner class to make the .NET instantiation fully lazy.
			/// </summary>
			internal static readonly UndoManager Instance = new UndoManager();
		}

		/// <summary>
		/// Gets the default undo manager.
		/// </summary>
		/// <value>The default undo manager.</value>
		public static UndoManager DefaultUndoManager
		{
			get
			{
				return SingletonCreator.Instance;
			}
		}

		#endregion
	}
}
