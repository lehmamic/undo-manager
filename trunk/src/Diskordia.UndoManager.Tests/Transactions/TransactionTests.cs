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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Diskordia.UndoRedo.Invocations;
using System.Linq.Expressions;
using UndoManagerTest;

namespace Diskordia.UndoRedo.Transactions
{
	/// <summary>
	/// Summary description for UndoRedoTransactionTests
	/// </summary>
	[TestClass]
	public class TransactionTests
	{
		public TransactionTests()
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
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_TransactionManagerNullReference_ThrowsException()
		{
			Transaction target = new Transaction(null);
		}

		[TestMethod]
		public void Commit_CommitsTransactionInTransactionManager()
		{
			TransactionManagerStub transactionManager = new TransactionManagerStub();
			Transaction target = new Transaction(transactionManager);

			target.Commit();

			Assert.IsTrue(transactionManager.CommitCalled);
		}

		[TestMethod]
		public void Rollback_RollbacksTransactionInTransactionManager()
		{
			TransactionManagerStub transactionManager = new TransactionManagerStub();
			Transaction target = new Transaction(transactionManager);

			target.Rollback();

			Assert.IsTrue(transactionManager.RollbackCalled);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RegisterInvokation_InvokationNullReference_ThrowsException()
		{
			TransactionManagerStub transactionManager = new TransactionManagerStub();
			Transaction target = new Transaction(transactionManager);

			target.RegisterInvokation((IInvokable)null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RegisterInvokation_ExpressionNullReference_ThrowsException()
		{
			TransactionManagerStub transactionManager = new TransactionManagerStub();
			Transaction target = new Transaction(transactionManager);

			target.RegisterInvokation<string>(string.Empty, (Expression<Action<string>>)null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RegisterInvokation_TargetNullReference_ThrowsException()
		{
			TransactionManagerStub transactionManager = new TransactionManagerStub();
			Transaction target = new Transaction(transactionManager);

			target.RegisterInvokation<string>(null, s => s.Clone());
		}

		[TestMethod]
		public void RegisterInvokation_AddsInvokationToTheInternalStack()
		{
			TransactionManagerStub transactionManager = new TransactionManagerStub();
			Transaction target = new Transaction(transactionManager);

			Mock<IInvokable> invokableMock = new Mock<IInvokable>(MockBehavior.Loose);
			target.RegisterInvokation(invokableMock.Object);

			target.Invoke();

			invokableMock.Verify(i => i.Invoke(), Times.Once());
		}

		[TestMethod]
		public void RegisterInvokation_AddsExpressionToInternalStack()
		{
			TransactionManagerStub transactionManager = new TransactionManagerStub();
			Transaction target = new Transaction(transactionManager);

			Mock<ITarget> targetMock = new Mock<ITarget>(MockBehavior.Loose);
			target.RegisterInvokation<ITarget>(targetMock.Object, t => t.UndoOperation());

			target.Invoke();

			targetMock.Verify(t => t.UndoOperation(), Times.Once());
		}

		[TestMethod]
		public void SetActionName_StringNullReference_SetsEmptyString()
		{
			TransactionManagerStub transactionManager = new TransactionManagerStub();
			Transaction target = new Transaction(transactionManager);

			target.ActionName = null;

			string expected = string.Empty;
			string actual = target.ActionName;

			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void SetActionName_ValidActionName_SetsEmptyString()
		{
			TransactionManagerStub transactionManager = new TransactionManagerStub();
			Transaction target = new Transaction(transactionManager);

			string expected = "Paste";
			target.ActionName = expected;

			string actual = target.ActionName;

			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void Invoke_InvokesAllRegisteredInvokations()
		{
			TransactionManagerStub transactionManager = new TransactionManagerStub();
			Transaction target = new Transaction(transactionManager);

			Mock<ITarget> targetMock = new Mock<ITarget>(MockBehavior.Loose);
			Mock<IInvokable> invokableMock = new Mock<IInvokable>(MockBehavior.Loose);

			targetMock.Setup(t => t.UndoOperation()).Callback(() => invokableMock.Verify(i => i.Invoke(), Times.Once()));
			invokableMock.Setup(i => i.Invoke()).Callback(() => targetMock.Verify(t => t.UndoOperation(), Times.Never()));

			target.RegisterInvokation<ITarget>(targetMock.Object, t => t.UndoOperation());
			target.RegisterInvokation(invokableMock.Object);

			target.Invoke();

			targetMock.Verify(t => t.UndoOperation(), Times.Once());
			invokableMock.Verify(i => i.Invoke(), Times.Once());
		}

		public void Dispose_CommitsTransaction()
		{
			TransactionManagerStub transactionManager = new TransactionManagerStub();
			Transaction target = new Transaction(transactionManager);

			target.Dispose();

			Assert.IsTrue(transactionManager.CommitCalled);
		}
	}
}