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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Diskordia.UndoRedo.Invokations
{
	/// <summary>
	/// Summary description for LambdaExpressionInvokationTests
	/// </summary>
	[TestClass]
	public class LambdaExpressionInvokationTests
	{
		public LambdaExpressionInvokationTests()
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
		public void Constructor_ExpressionNullReference_ThrowsException()
		{
			Mock<ITarget> targetMock = new Mock<ITarget>(MockBehavior.Strict);

			LambdaExpressionInvokation<ITarget> target = new LambdaExpressionInvokation<ITarget>(targetMock.Object, null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_TargetNullReference_ThrowsException()
		{
			string argument = "1234";
			Mock<ITarget> targetMock = new Mock<ITarget>(MockBehavior.Strict);

			LambdaExpressionInvokation<ITarget> target = new LambdaExpressionInvokation<ITarget>(null, t => t.Add(argument));
		}

		[TestMethod]
		public void Invoke_InvokesPassedDelegate()
		{
			string argument = "1234";
			Mock<ITarget> targetMock = new Mock<ITarget>(MockBehavior.Strict);
			targetMock.Setup(t => t.Add(argument));

			LambdaExpressionInvokation<ITarget> target = new LambdaExpressionInvokation<ITarget>(targetMock.Object, t => t.Add(argument));
			target.Invoke();

			targetMock.Verify(t => t.Add(argument), Times.Once());
		}
	}
}