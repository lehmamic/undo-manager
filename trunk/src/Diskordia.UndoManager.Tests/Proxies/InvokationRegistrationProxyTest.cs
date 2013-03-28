using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Diskordia.UndoRedo.Proxies
{
	[TestClass]
	public class InvokationRegistrationProxyTest
	{
		[TestMethod]
		public void TestMethod1()
		{
			var target = new Mock<ITarget>();
			var proxy = new InvokationRegistrationProxy<ITarget>(TODO, target.Object);

			var transparentProxy = (ITarget) proxy.GetTransparentProxy();

			transparentProxy.Add("TestItem");
		}
	}
}
