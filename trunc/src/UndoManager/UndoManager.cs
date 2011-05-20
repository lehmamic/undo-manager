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

namespace UndoRedo
{
	/// <summary>
	/// The undo manager records undo operations to provide undo and redo logic.
	/// </summary>
	public sealed class UndoManager
	{
		#region Constructors & Declarations

		private readonly Stack<IInvokable> undoHistory = new Stack<IInvokable>();
		private readonly Stack<IInvokable> redoHistory = new Stack<IInvokable>();
		private readonly Stack<Transaction> transactionStack = new Stack<Transaction>();

		private UndoRedoState state = UndoRedoState.Recording;

		#endregion

		#region Exception Save State Switch

		/// <summary>
		/// This class switchs the state of the <see cref="UndoManager"/> exception save.
		/// </summary>
		private class StateSwitcher : IDisposable
		{
			private readonly UndoManager owner;
			private readonly UndoRedoState backup;

			private bool disposed = false;

			/// <summary>
			/// Initializes a new instance of the <see cref="StateSwitcher"/> class.
			/// </summary>
			/// <param name="target">The target <see cref="UndoManager"/> to switch the state.</param>
			/// <param name="state">The <see cref="UndoRedoState"/> to set on the <paramref name="target"/>.</param>
			/// <exception cref="ArgumentNullException"><paramref name="target"/> is a <see langword="null"/> reference.</exception>
			public StateSwitcher(UndoManager target, UndoRedoState state)
			{
				if (target == null)
				{
					throw new ArgumentNullException("target");
				}

				this.owner = target;

				this.backup = this.owner.state;
				this.owner.state = state;
			}

			#region Dispose Members

			#region IDisposable Members

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose()
			{
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}

			#endregion

			/// <summary>
			/// Releases unmanaged and - optionally - managed resources.
			/// </summary>
			/// <param name="disposing"><c>True</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
			private void Dispose(bool disposing)
			{
				// check to see if Dispose has already been called
				if (!this.disposed)
				{
					// if IDisposable.Dispose was called, dispose all managed resources
					if (disposing)
					{
						this.owner.state = this.backup;
					}

					// if Finalize or IDisposable.Dispose, free your own state (unmanaged objects)
					this.disposed = true;
				}
			}

			/// <summary>
			/// Finalizes an instance of the <see cref="StateSwitcher"/> class.
			/// </summary>
			/// <remark>
			/// Releases unmanaged resources and performs other cleanup operations before the
			/// <see cref="StateSwitcher"/> is reclaimed by garbage collection.
			/// </remark>
			~StateSwitcher()
			{
				this.Dispose(false);
			}

			#endregion
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

		#region Public Members

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
				Transaction recordingTransaction = this.RecordingTransaction();
				if (recordingTransaction != null)
				{
					recordingTransaction.Add(target, selector);
				}
				else
				{
					using (Transaction transaction = this.CreateTransaction())
					{
						transaction.Add(target, selector);
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
		/// Creates a <see cref="Transaction"/> for recording the undo operations. Undo operations
		/// which are recorded while a <see cref="Transaction"/> is opened get recorded by the <see cref="UndoManager"/> only if the <see cref="Transaction"/>
		/// is commited. A rollback will execute the undo operations which where registered while the <see cref="Transaction"/> has been open.
		/// </summary>
		/// <returns>A new instance of the <see cref="Transaction"/> class.</returns>
		public Transaction CreateTransaction()
		{
			Transaction transaction = new Transaction(this);
			this.transactionStack.Push(transaction);

			return transaction;
		}

		/// <summary>
		/// Commits the created <see cref="Transaction"/> and records the registered undo operation in the <see cref="UndoManager"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">No open <see cref="Transaction"/> to commit available.</exception>
		public void CommitTransaction()
		{
			if (this.transactionStack.Count == 0)
			{
				throw new InvalidOperationException("There is no open transaction available to commit.");
			}

			Transaction transaction = this.transactionStack.Pop();

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
					Transaction transactionToRollback = this.transactionStack.Pop();
					transactionToRollback.Invoke();
				}
			}
		}

		#endregion

		#region Private Members

		private void CommitOpenTransactions()
		{
			while (this.transactionStack.Count > 0)
			{
				Transaction transaction = this.transactionStack.Peek();
				transaction.Commit();
			}
		}

		private void ProcessTransactionToCommit(Transaction transaction)
		{
			if (this.transactionStack.Count == 0)
			{
				Stack<IInvokable> history = this.IsUndoing ? this.redoHistory : this.undoHistory;
				history.Push(transaction);
			}
			else
			{
				Transaction parent = this.transactionStack.Peek();
				parent.Add(transaction);
			}
		}

		private Transaction RecordingTransaction()
		{
			return this.transactionStack.Count > 0 ? this.transactionStack.Peek() : null;
		}

		#endregion
	}
}
