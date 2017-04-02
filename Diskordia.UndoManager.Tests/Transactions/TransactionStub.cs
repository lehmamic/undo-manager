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
using Diskordia.UndoRedo.Invokations;
using System.Collections;

namespace Diskordia.UndoRedo.Transactions
{
	internal class TransactionStub : IInvokableTransaction
	{
		public bool InvokationRegistered { get; set; }

		public bool Commited { get; set; }

		private readonly Stack<IInvokable> invokables = new Stack<IInvokable>();
		private readonly ITransactionManager owner;

		private string actionName = string.Empty;
		private bool disposed = false;

		internal TransactionStub(ITransactionManager transactionManager)
		{
			if (transactionManager == null)
			{
				throw new ArgumentNullException("undoRedoContext");
			}

			this.owner = transactionManager;
		}

		#region IInvokableTransaction members

		public void RegisterInvokation(IInvokable invokation)
		{
			if (invokation == null)
			{
				throw new ArgumentNullException("transaction");
			}

			this.invokables.Push(invokation);

			this.InvokationRegistered = true;
		}

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

		public void Commit()
		{
			this.owner.CommitTransaction(this);

			this.Commited = true;
		}

		public void Rollback()
		{
			this.owner.RollbackTransaction(this);
		}

		#endregion

		#region IInvokable members

		public void Invoke()
		{
			while (this.invokables.Any())
			{
				IInvokable invokable = this.invokables.Pop();
				invokable.Invoke();
			}
		}

		#endregion

		#region IEnumerable<IInvokable> members

		public IEnumerator<IInvokable> GetEnumerator()
		{
			return this.invokables.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion

		#region Dispose members

		#region IDisposable Members

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

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

		~TransactionStub()
		{
			this.Dispose(false);
		}

		#endregion
	}
}