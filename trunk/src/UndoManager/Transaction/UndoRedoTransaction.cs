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
using System.Collections.Generic;
using System.Linq.Expressions;
using UndoRedo.Invocation;

namespace UndoRedo.Transaction
{
	/// <summary>
	/// A <see cref="UndoRedoTransaction"/> records the undo operations, which are registered in the <see cref="UndoManager"/> while commiting the <see cref="UndoRedoTransaction"/>.
	/// </summary>
	public sealed class UndoRedoTransaction : ITransaction, IInvokable
	{
		private readonly Stack<IInvokable> invokables = new Stack<IInvokable>();
		private readonly UndoManager owner;

		private string actionName = string.Empty;
		private bool disposed = false;

		/// <summary>
		/// Initializes a new instance of the <see cref="UndoRedoTransaction"/> class.
		/// </summary>
		/// <param name="undoManager">The undo manager.</param>
		/// <exception cref="ArgumentNullException"><paramref name="undoManager"/> is a <see langword="null"/> reference.</exception>
		internal UndoRedoTransaction(UndoManager undoManager)
		{
			if (undoManager == null)
			{
				throw new ArgumentNullException("undoRedoContext");
			}

			this.owner = undoManager;
		}

		/// <summary>
		/// Registers an operation to the <see cref="UndoRedoTransaction"/>.
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <param name="target">The target of the undo operation.</param>
		/// <param name="selector">The undo operation which will be invoked on the target.</param>
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

			ActionInvocation<TSource> invocation = new ActionInvocation<TSource>(target, selector);
			this.invokables.Push(invocation);
		}

		/// <summary>
		/// Adds a child <see cref="UndoRedoTransaction"/> instance.
		/// </summary>
		/// <param name="transaction">The child <see cref="UndoRedoTransaction"/> to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="transaction"/> is a <see langword="null"/> reference.</exception>
		public void Add(UndoRedoTransaction transaction)
		{
			if (transaction == null)
			{
				throw new ArgumentNullException("transaction");
			}

			this.invokables.Push(transaction);
		}

		/// <summary>
		/// Gets the number of selectors registered in the <see cref="UndoRedoTransaction"/>.
		/// </summary>
		/// <value>The count.</value>
		public int Count
		{
			get { return this.invokables.Count; }
		}

		#region ITransaction members

		/// <summary>
		/// Commits the undo operation of this <see cref="ITransaction"/>.
		/// </summary>
		public void Commit()
		{
			this.owner.CommitTransaction();
		}

		/// <summary>
		/// Rollbacks the transaction and calls the undo operations to recover the state befor the <see cref="ITransaction"/> has been created.
		/// </summary>
		public void Rollback()
		{
			this.owner.RollbackTransaction();
		}

		#endregion

		#region IInvokable members

		/// <summary>
		/// Invokes all registered commands of this <see cref="UndoRedoTransaction"/>.
		/// </summary>
		public void Invoke()
		{
			while (this.invokables.Count > 0)
			{
				IInvokable invokable = this.invokables.Pop();
				invokable.Invoke();
			}
		}

		/// <summary>
		/// Name of the action, which is performed with this invocation.
		/// </summary>
		public string ActionName
		{
			get
			{
				return this.actionName;
			}

			set
			{
				this.actionName = value != null ? value : string.Empty;
			}
		}

		#endregion

		#region Dispose members

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
					this.Commit();
				}

				// if Finalize or IDisposable.Dispose, free your own state (unmanaged objects)
				this.disposed = true;
			}
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="UndoRedoTransaction"/> class.
		/// </summary>
		/// <remark>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="UndoRedoTransaction"/> is reclaimed by garbage collection.
		/// </remark>
		~UndoRedoTransaction()
		{
			this.Dispose(false);
		}

		#endregion
	}
}
