/*****************************************************************************
 * UndoManager. An easy to use undo API.
 * Copyright (C) 2009  Michael Lehmann 
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Diskordia.UndoRedo;
using Diskordia.UndoRedo.Transaction;

namespace UndoManagerTest
{
	[TestClass]
	public class UndoManagerTests
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RegisterInvocation_TargetNullReference_ArgumentNullException()
		{
			UndoManager target = new UndoManager();

			target.RegisterInvocation<ITarget>(null, p => p.UndoOperation());
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RegisterInvocation_SelectorNullReference_ArgumentNullException()
		{
			UndoManager target = new UndoManager();
			Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);
			target.RegisterInvocation<ITarget>(testMock.Object, null);
		}

		[TestMethod]
		public void RegisterInvocation_RegistersValidInvocation_TransactionCreatedAndInvocationRegistered()
		{
			UndoManager target = new UndoManager();

			Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);

			target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());

			Assert.IsTrue(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
		public void RegisterInvocation_RegistersValidInvocationIntoOpenTransaction_InvocationRegistered()
		{
			UndoManager target = new UndoManager();

			Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);

			target.CreateTransaction();
			target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());

			Assert.IsTrue(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Undo_NoUndoOperationRegistered_InvalidOperationException()
		{
			UndoManager target = new UndoManager();
			target.Undo();
		}

		[TestMethod]
		public void Undo_SingleOperation_UndoOperationCalled()
		{
			UndoManager target = new UndoManager();

			Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);
			testMock.Setup(p => p.UndoOperation());

			target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());

			target.Undo();

			testMock.Verify(p => p.UndoOperation(), Times.Once());
			Assert.IsFalse(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
		public void Undo_OperationsRecordedWithTransaction_UndoOperationsCalled()
		{
			UndoManager target = new UndoManager();

			Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);
			testMock.Setup(p => p.UndoOperation());

			using (target.CreateTransaction())
			{
				target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());
				target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());
			}

			target.Undo();

			testMock.Verify(p => p.UndoOperation(), Times.Exactly(2));
			Assert.IsFalse(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
		public void Undo_WithOpenTransaction_TransactionCommitedAndUndoOperationsCalled()
		{
			UndoManager target = new UndoManager();

			Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);
			testMock.Setup(p => p.UndoOperation());

			target.CreateTransaction();
			target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());
			target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());

			target.Undo();

			testMock.Verify(p => p.UndoOperation(), Times.Exactly(2));
			Assert.IsFalse(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
		public void Undo_UndoOperationRegistersRedoUperation_CanRedoAfterUndo()
		{
			UndoManager target = new UndoManager();

			Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);
			testMock.Setup(p => p.UndoOperation())
				.Callback(() => target.RegisterInvocation<ITarget>(testMock.Object, p => p.RedoOperation()));

			using (target.CreateTransaction())
			{
				target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());
				target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());
			}

			target.Undo();

			testMock.Verify(p => p.UndoOperation(), Times.Exactly(2));
			Assert.IsFalse(target.CanUndo);
			Assert.IsTrue(target.CanRedo);
		}

		[TestMethod]
		public void Undo_WithCascadedTransaction_UndoOperationsFromAllTransactionsCalled()
		{
			UndoManager target = new UndoManager();

			Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);
			testMock.Setup(p => p.UndoOperation());

			using (target.CreateTransaction())
			{
				target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());

				using (target.CreateTransaction())
				{
					target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());
				}
			}

			target.Undo();

			testMock.Verify(p => p.UndoOperation(), Times.Exactly(2));
			Assert.IsFalse(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Redo_NoRedoOperationAvailable_InvalidOperationException()
		{
			UndoManager target = new UndoManager();
			target.Redo();
		}

		[TestMethod]
		public void Redo_UndoOperationRegistersWithTransaction_RedoOperationsCaleld()
		{
			UndoManager target = new UndoManager();

			Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);
			testMock.Setup(p => p.UndoOperation())
				.Callback(() => target.RegisterInvocation<ITarget>(testMock.Object, p => p.RedoOperation()));
			testMock.Setup(p => p.RedoOperation());

			using (target.CreateTransaction())
			{
				target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());
				target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());
			}

			target.Undo();
			target.Redo();

			testMock.Verify(p => p.RedoOperation(), Times.Exactly(2));
			Assert.IsFalse(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
		public void Redo_RedoOperationRegistersUndoOperation_CanUndoAfterRedo()
		{
			UndoManager target = new UndoManager();

			Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);
			testMock.Setup(p => p.UndoOperation())
				.Callback(() => target.RegisterInvocation<ITarget>(testMock.Object, p => p.RedoOperation()));
			testMock.Setup(p => p.RedoOperation())
				.Callback(() => target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation()));

			using (target.CreateTransaction())
			{
				target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());
				target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());
			}

			target.Undo();
			target.Redo();

			testMock.Verify(p => p.RedoOperation(), Times.Exactly(2));
			Assert.IsTrue(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CommitTransaction_NoOpenTransactionAvailable_InvalidOperationException()
		{
			UndoManager target = new UndoManager();
			target.CommitTransaction();
		}

		[TestMethod]
		public void CommitTransaction_CommitOpenTransaction_TransactionRegisteredatUndoManager()
		{
			UndoManager target = new UndoManager();
			Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);

			target.CreateTransaction();
			target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());

			target.CommitTransaction();
			Assert.IsTrue(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
		public void CommitTransaction_CommitTopLevelOfCascadedTransactions_TransactionAddedToParentTransaction()
		{
			UndoManager target = new UndoManager();
			Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);

			ITransaction parent = target.CreateTransaction();
			using (target.CreateTransaction())
			{
				target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());
			}

			Assert.AreEqual(1, ((UndoRedoTransaction)parent).Count);
			Assert.IsTrue(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
		public void CommitTransaction_CommitAllCascadedTransactions_ChildAddedToParentTrasactionAndParentTransactionRegistered()
		{
			UndoManager target = new UndoManager();
			Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);

			ITransaction parent = target.CreateTransaction();
			using (target.CreateTransaction())
			{
				target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());
			}

			target.CommitTransaction();
			Assert.AreEqual(1, ((UndoRedoTransaction)parent).Count);
			Assert.IsTrue(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		//[TestMethod]
		//public void CommitTransaction_CommitParentLevelOfCascadedTransactions_ChildAddedToParentTrasactionAndParentTransactionRegistered()
		//{
		//    UndoManager target = new UndoManager();
		//    Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);
		//    testMock.Setup(p => p.UndoOperation());

		//    Transaction parent = target.CreateTransaction();
		//    target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());
		//    target.CreateTransaction();
		//    target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());

		//    parent.Commit();

		//    Assert.IsTrue(target.CanUndo);
		//    Assert.IsFalse(target.CanRedo);

		//    target.Undo();
		//    testMock.Verify(p => p.UndoOperation(), Times.Exactly(2));
		//}

		[TestMethod]
		public void RollbackTransaction_WithRegisteredUndoOperations_UndoOperationsCalled()
		{
			UndoManager target = new UndoManager();
			Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);
			testMock.Setup(p => p.UndoOperation());

			target.CreateTransaction();
			target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());

			target.RollbackTransaction();

			testMock.Verify(p => p.UndoOperation(), Times.Once());
			Assert.IsFalse(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
		public void RollbackTransaction_UndoOperationRegistersRedoOperation_RedoOperationNotRegistered()
		{
			UndoManager target = new UndoManager();
			Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);
			testMock.Setup(p => p.UndoOperation())
				.Callback(() => target.RegisterInvocation<ITarget>(testMock.Object, p => p.RedoOperation()));

			target.CreateTransaction();
			target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());

			target.RollbackTransaction();

			testMock.Verify(p => p.UndoOperation(), Times.Once());
			Assert.IsFalse(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
		public void RollbackTransaction_RollbackTopLevelOfCascadedTransactions_ParentTransactionStillOpen()
		{
			UndoManager target = new UndoManager();
			Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);
			testMock.Setup(p => p.UndoOperation());

			ITransaction parent = target.CreateTransaction();
			target.CreateTransaction();
			target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());

			target.RollbackTransaction();

			testMock.Verify(p => p.UndoOperation(), Times.Once());
			Assert.AreEqual(0, ((UndoRedoTransaction)parent).Count);
			Assert.IsTrue(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
		public void RollbackTransaction_RollbackAllCascadedTransactions_AllTransactionsRolledBack()
		{
			UndoManager target = new UndoManager();
			Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);
			testMock.Setup(p => p.UndoOperation());

			target.CreateTransaction();
			target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());
			target.CreateTransaction();
			target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());

			target.RollbackTransaction();
			target.RollbackTransaction();

			testMock.Verify(p => p.UndoOperation(), Times.Exactly(2));
			Assert.IsFalse(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		//[TestMethod]
		//public void RollbackTransaction_RollbackParentLevelOfCascadedTransactions_AllTransactionsRolledBack()
		//{
		//    UndoManager target = new UndoManager();
		//    Mock<ITarget> testMock = new Mock<ITarget>(MockBehavior.Strict);
		//    testMock.Setup(p => p.UndoOperation());

		//    Transaction parent = target.CreateTransaction();
		//    target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());
		//    target.CreateTransaction();
		//    target.RegisterInvocation<ITarget>(testMock.Object, p => p.UndoOperation());

		//    parent.Rollback();

		//    testMock.Verify(p => p.UndoOperation(), Times.Exactly(2));
		//    Assert.IsFalse(target.CanUndo);
		//    Assert.IsFalse(target.CanRedo);
		//}
	}
}