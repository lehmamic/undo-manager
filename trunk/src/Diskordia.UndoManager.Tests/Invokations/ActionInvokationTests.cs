using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Diskordia.UndoRedo.Invokations;
using Diskordia.UndoRedo;
using Moq;

namespace Diskordia.UndoRedo.Invokations
{
	/// <summary>
	/// Summary description for InvocationTests
	/// </summary>
	[TestClass]
	public class ActionInvokationTests
	{
		public ActionInvokationTests()
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
		public void Constructor_InvokationNullReference_ThrowsException()
		{
			string argument = "1234";

			ActionInvokation<string> target = new ActionInvokation<string>(null, argument);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_ArgumentNullReference_ThrowsException()
		{
			Mock<ITarget> targetMock = new Mock<ITarget>(MockBehavior.Strict);

			ActionInvokation<string> target = new ActionInvokation<string>(targetMock.Object.Add, null);
		}

		[TestMethod]
		public void Invoke_InvokesPassedDelegate()
		{
			string argument = "1234";
			Mock<ITarget> targetMock = new Mock<ITarget>(MockBehavior.Strict);
			targetMock.Setup(t => t.Add(argument));

			ActionInvokation<string> target = new ActionInvokation<string>(targetMock.Object.Add, argument);
			target.Invoke();

			targetMock.Verify(t => t.Add(argument), Times.Once());
		}
	}
}