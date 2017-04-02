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
using Moq;
using Diskordia.UndoRedo.Invokations;
using Xunit;

namespace Diskordia.UndoRedo.Transactions
{
    public class TransactionTests
    {
        [Fact]
        public void Constructor_TransactionManagerNullReference_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new Transaction(null));
        }

        [Fact]
        public void Commit_CommitsTransactionInTransactionManager()
        {
            TransactionManagerStub transactionManager = new TransactionManagerStub();
            Transaction target = new Transaction(transactionManager);

            target.Commit();

            Assert.True(transactionManager.CommitCalled);
        }

        [Fact]
        public void Rollback_RollbacksTransactionInTransactionManager()
        {
            TransactionManagerStub transactionManager = new TransactionManagerStub();
            Transaction target = new Transaction(transactionManager);

            target.Rollback();

            Assert.True(transactionManager.RollbackCalled);
        }

        [Fact]
        public void RegisterInvokation_InvokationNullReference_ThrowsException()
        {
            TransactionManagerStub transactionManager = new TransactionManagerStub();
            Transaction target = new Transaction(transactionManager);

            Assert.Throws<ArgumentNullException>(() => target.RegisterInvokation((IInvokable)null));
        }

        [Fact]
        public void RegisterInvokation_AddsInvokationToTheInternalStack()
        {
            TransactionManagerStub transactionManager = new TransactionManagerStub();
            Transaction target = new Transaction(transactionManager);

            Mock<IInvokable> invokableMock = new Mock<IInvokable>(MockBehavior.Loose);
            target.RegisterInvokation(invokableMock.Object);

            target.Invoke();

            invokableMock.Verify(i => i.Invoke(), Times.Once());
        }

        [Fact]
        public void SetActionName_StringNullReference_SetsEmptyString()
        {
            TransactionManagerStub transactionManager = new TransactionManagerStub();
            Transaction target = new Transaction(transactionManager);

            target.ActionName = null;

            string expected = string.Empty;
            string actual = target.ActionName;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SetActionName_ValidActionName_SetsEmptyString()
        {
            TransactionManagerStub transactionManager = new TransactionManagerStub();
            Transaction target = new Transaction(transactionManager);

            string expected = "Paste";
            target.ActionName = expected;

            string actual = target.ActionName;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Invoke_InvokesAllRegisteredInvokations()
        {
            TransactionManagerStub transactionManager = new TransactionManagerStub();
            Transaction target = new Transaction(transactionManager);

            Mock<IInvokable> invokableMock1 = new Mock<IInvokable>(MockBehavior.Loose);
            Mock<IInvokable> invokableMock2 = new Mock<IInvokable>(MockBehavior.Loose);

            invokableMock1.Setup(i => i.Invoke()).Callback(() => invokableMock2.Verify(t => t.Invoke(), Times.Once()));
            invokableMock2.Setup(i => i.Invoke()).Callback(() => invokableMock1.Verify(t => t.Invoke(), Times.Never()));

            target.RegisterInvokation(invokableMock1.Object);
            target.RegisterInvokation(invokableMock2.Object);

            target.Invoke();

            invokableMock1.Verify(i => i.Invoke(), Times.Once());
            invokableMock2.Verify(i => i.Invoke(), Times.Once());
        }

        public void Dispose_CommitsTransaction()
        {
            TransactionManagerStub transactionManager = new TransactionManagerStub();
            Transaction target = new Transaction(transactionManager);

            target.Dispose();

            Assert.True(transactionManager.CommitCalled);
        }
    }
}