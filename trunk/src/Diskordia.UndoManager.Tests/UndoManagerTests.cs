﻿/*****************************************************************************
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
using Diskordia.UndoRedo.Invokations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Diskordia.UndoRedo.Transactions;

namespace Diskordia.UndoRedo
{
	/// <summary>
	/// Summary description for UndoManagerTests
	/// </summary>
	[TestClass]
	public class UndoManagerTests
	{
		public UndoManagerTests()
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
		public void Constrcutor_TransactionFactoryNullReference_ThrowsException()
		{
			UndoManager target = new UndoManager(null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RegisterInvokation_InvokationNullReference_ThrowsException()
		{
			UndoManager target = new UndoManager();
			target.RegisterInvokation((IInvokable)null);
		}

		[TestMethod]
		public void RegisterInvokation_Invokatio_RegistersInvokation()
		{
			UndoManager target = new UndoManager();

			Mock<IInvokable> invokableMock = new Mock<IInvokable>(MockBehavior.Strict);
			target.RegisterInvokation(invokableMock.Object);

			Assert.IsTrue(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RegisterInvokation_DelegateNullReference_ThrowsException()
		{
			UndoManager target = new UndoManager();
			target.RegisterInvokation((Action<string>)null, string.Empty);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RegisterInvokation_ArgumentNullReference_ThrowsException()
		{
			UndoManager target = new UndoManager();

			Mock<ITarget> targetMock = new Mock<ITarget>();
			target.RegisterInvokation(targetMock.Object.Add, (string)null);
		}

		[TestMethod]
		public void RegisterInvokation_ActionDelegate_RegistersAction()
		{
			UndoManager target = new UndoManager();

			Mock<ITarget> targetMock = new Mock<ITarget>();
			target.RegisterInvokation(targetMock.Object.Add, "test");

			Assert.IsTrue(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RegisterInvokation_ExpressionNullReference_ThrowsException()
		{
			UndoManager target = new UndoManager();

			Mock<ITarget> targetMock = new Mock<ITarget>();
			target.RegisterInvokation(targetMock.Object, null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RegisterInvokation_TargetNullReference_ThrowsException()
		{
			UndoManager target = new UndoManager();

			Mock<ITarget> targetMock = new Mock<ITarget>();
			target.RegisterInvokation<ITarget>(null, t => t.UndoOperation());
		}

		[TestMethod]
		public void RegisterInvokation_LambdaExpression_RegistersLambdaExpression()
		{
			UndoManager target = new UndoManager();
			Mock<ITarget> targetMock = new Mock<ITarget>();
			target.RegisterInvokation(targetMock.Object, t => t.UndoOperation());

			Assert.IsTrue(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
		public void RegisterInvokation_WithoutTransaction_RegistersInvokationWithPrivateTransaction()
		{
			TransactionFactoryStub factory = new TransactionFactoryStub();
			UndoManager target = new UndoManager(factory);

			factory.Transaction = new TransactionStub(target); ;

			Mock<IInvokable> invokableMock = new Mock<IInvokable>(MockBehavior.Strict);
			target.RegisterInvokation(invokableMock.Object);

			Assert.IsTrue(factory.TransactionCreated);
			Assert.IsTrue(((TransactionStub)factory.Transaction).InvokationRegistered);
			Assert.IsTrue(((TransactionStub)factory.Transaction).Commited);

			Assert.IsTrue(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}

		[TestMethod]
		public void Undo_InvokationRegisteredWithoutTransactions_InvokesUndoOperation()
		{
			UndoManager target = new UndoManager();

			Mock<IInvokable> invokableMock = new Mock<IInvokable>(MockBehavior.Loose);
			target.RegisterInvokation(invokableMock.Object);

			target.Undo();

			invokableMock.Verify(i => i.Invoke(), Times.Once());
			Assert.IsFalse(target.CanUndo);
			Assert.IsFalse(target.CanRedo);
		}
	}
}