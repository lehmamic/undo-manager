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
using System.Linq;
using System.Linq.Expressions;
using Diskordia.UndoRedo.Invocations;

namespace Diskordia.UndoRedo.Transactions
{
	/// <summary>
	/// A <see cref="Transaction"/> records the undo operations, which are registered in the <see cref="UndoManager"/> while commiting the <see cref="Transaction"/>.
	/// </summary>
	public sealed class Transaction : IInvokableTransaction
	{
		private readonly Stack<IInvokable> invokables = new Stack<IInvokable>();
		private readonly ITransactionManager owner;

		private string actionName = string.Empty;
		private bool disposed = false;

		/// <summary>
		/// Initializes a new instance of the <see cref="Transaction"/> class.
		/// </summary>
		/// <param name="transactionManager">The <see cref="ITransactionManager"/> controlling this transaction.</param>
		/// <exception cref="ArgumentNullException"><paramref name="transactionManager"/> is a <see langword="null"/> reference.</exception>
		internal Transaction(ITransactionManager transactionManager)
		{
			if (transactionManager == null)
			{
				throw new ArgumentNullException("undoRedoContext");
			}

			this.owner = transactionManager;
		}

		#region IInvokableTransaction members

		/// <summary>
		/// Registers an operation to the <see cref="Transaction"/>.
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <param name="target">The target of the undo operation.</param>
		/// <param name="selector">The undo operation which will be invoked on the target.</param>
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

			ActionInvokation<TSource> invokation = new ActionInvokation<TSource>(target, selector);
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
				throw new ArgumentNullException("transaction");
			}

			this.invokables.Push(invokation);
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

		#region ITransaction members

		/// <summary>
		/// Commits the undo operation of this <see cref="ITransaction"/>.
		/// </summary>
		public void Commit()
		{
			this.owner.CommitTransaction(this);
		}

		/// <summary>
		/// Rollbacks the transaction and calls the undo operations to recover the state befor the <see cref="ITransaction"/> has been created.
		/// </summary>
		public void Rollback()
		{
			this.owner.RollbackTransaction(this);
		}

		#endregion

		#region IInvokable members

		/// <summary>
		/// Invokes all registered commands of this <see cref="Transaction"/>.
		/// </summary>
		public void Invoke()
		{
			while (this.invokables.Any())
			{
				IInvokable invokable = this.invokables.Pop();
				invokable.Invoke();
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
		/// Finalizes an instance of the <see cref="Transaction"/> class.
		/// </summary>
		/// <remark>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="Transaction"/> is reclaimed by garbage collection.
		/// </remark>
		~Transaction()
		{
			this.Dispose(false);
		}

		#endregion
	}
}