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

using Diskordia.UndoRedo.Invokations;
using Diskordia.UndoRedo.Transactions;
using Moq;
using Xunit;

namespace Diskordia.UndoRedo
{
    public class UndoManagerTransactionsTests
    {

        [Fact]
        public void RegisterInvocation_WithoutTransaction_RegistersInvocationWithPrivateTransaction()
        {
            TransactionFactoryStub factory = new TransactionFactoryStub();
            UndoManager target = new UndoManager(factory);

            factory.Transaction = new TransactionStub(target);

            Mock<IInvokable> invokableMock = new Mock<IInvokable>(MockBehavior.Strict);
            target.RegisterInvokation(invokableMock.Object);

            Assert.True(factory.TransactionCreated);
            Assert.True(((TransactionStub)factory.Transaction).InvokationRegistered);
            Assert.True(((TransactionStub)factory.Transaction).Commited);

            Assert.True(target.CanUndo);
            Assert.False(target.CanRedo);
        }

        [Fact]
        public void RegisterInvocation_WithTransaction_RegistersInvocationWithPublicTransaction()
        {
            TransactionFactoryStub factory = new TransactionFactoryStub();
            UndoManager target = new UndoManager(factory);

            factory.Transaction = new TransactionStub(target);

            Mock<IInvokable> invokableMock = new Mock<IInvokable>(MockBehavior.Strict);

            using (ITransaction transaction = target.CreateTransaction())
            {
                target.RegisterInvokation(invokableMock.Object);
            }

            Assert.True(factory.TransactionCreated);
            Assert.True(((TransactionStub)factory.Transaction).InvokationRegistered);
            Assert.True(((TransactionStub)factory.Transaction).Commited);

            Assert.True(target.CanUndo);
            Assert.False(target.CanRedo);
        }

        [Fact]
        public void Undo_InvocationRegisteredWithTransaction_InvokesUndoOperation()
        {
            UndoManager target = new UndoManager();

            Mock<IInvokable> invokableMock = new Mock<IInvokable>(MockBehavior.Loose);

            using (ITransaction transaction = target.CreateTransaction())
            {
                target.RegisterInvokation(invokableMock.Object);
            }

            target.Undo();

            invokableMock.Verify(i => i.Invoke(), Times.Once());
            Assert.False(target.CanUndo);
            Assert.False(target.CanRedo);
        }

        [Fact]
        public void Undo_TransactionNotCommited_CommitsTransactionAndInvokesUndoOperation()
        {
            UndoManager target = new UndoManager();

            Mock<IInvokable> invokableMock = new Mock<IInvokable>(MockBehavior.Loose);

            ITransaction transaction = target.CreateTransaction();
            target.RegisterInvokation(invokableMock.Object);

            target.Undo();

            invokableMock.Verify(i => i.Invoke(), Times.Once());
            Assert.False(target.CanUndo);
            Assert.False(target.CanRedo);
        }
    }
}