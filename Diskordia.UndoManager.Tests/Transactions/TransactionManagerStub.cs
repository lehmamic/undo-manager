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

using Xunit;

namespace Diskordia.UndoRedo.Transactions
{
    internal class TransactionManagerStub : ITransactionManager
    {
        public bool CommitCalled { get; set; }

        public bool RollbackCalled { get; set; }

        public void CommitTransaction(IInvokableTransaction transaction)
        {
            Assert.NotNull(transaction);
            this.CommitCalled = true;
        }

        public void RollbackTransaction(IInvokableTransaction transaction)
        {
            Assert.NotNull(transaction);
            this.RollbackCalled = true;
        }
    }
}