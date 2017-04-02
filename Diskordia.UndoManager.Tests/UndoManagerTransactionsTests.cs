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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Diskordia.UndoRedo
{
	/// <summary>
	/// Summary description for UndoManagerTransactionsTests
	/// </summary>
	[TestClass]
	public class UndoManagerTransactionsTests
	{
		public UndoManagerTransactionsTests()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void RegisterInvocation_WithoutTransaction_RegistersInvocationWithPrivateTransaction()
		{
			TransactionFactoryStub factory = new TransactionFactoryStub();
			UndoManager target = new UndoManager(factory);

			factory.Transaction = new TransactionStub(target);

			Mock<IInvokable> invokableMock = new Mock<IInvokable>(MockBehavior.Strict);
			target.RegisterInvokation(invokableMock.Object);

			Assert.IsTrue(factory.TransactionCreated);
			Assert.IsTrue(((TransactionStub)factory.Transaction).InvokationRegistered);
			Assert.IsTrue(((TransactionStub)factory.Transaction).Commited);

			Assert.IsTrue(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
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

			Assert.IsTrue(factory.TransactionCreated);
			Assert.IsTrue(((TransactionStub)factory.Transaction).InvokationRegistered);
			Assert.IsTrue(((TransactionStub)factory.Transaction).Commited);

			Assert.IsTrue(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
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
			Assert.IsFalse(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
		public void Undo_TransactionNotCommited_CommitsTransactionAndInvokesUndoOperation()
		{
			UndoManager target = new UndoManager();

			Mock<IInvokable> invokableMock = new Mock<IInvokable>(MockBehavior.Loose);

			ITransaction transaction = target.CreateTransaction();
			target.RegisterInvokation(invokableMock.Object);

			target.Undo();

			invokableMock.Verify(i => i.Invoke(), Times.Once());
			Assert.IsFalse(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}
	}
}