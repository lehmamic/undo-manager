using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Diskordia.UndoRedo.Transactions
{
	internal class TransactionManagerStub : ITransactionManager
	{
		public bool CommitCalled { get; set; }

		public bool RollbackCalled { get; set; }

		public void CommitTransaction(IInvokableTransaction transaction)
		{
			Assert.IsNotNull(transaction, "The commited transaction shouldnot be null.");
			this.CommitCalled = true;
		}

		public void RollbackTransaction(IInvokableTransaction transaction)
		{
			Assert.IsNotNull(transaction, "The commited transaction shouldnot be null.");
			this.RollbackCalled = true;
		}
	}
}