/////*****************************************************************************
//// * UndoManager. An easy to use undo API.
//// * Copyright (C) 2009 Michael Lehmann 
//// ******************************************************************************
//// * This file is part of UndoManager.
//// *
//// * UndoManager is free software: you can redistribute it and/or modify
//// * it under the terms of the GNU Lesser General Public License as published by
//// * the Free Software Foundation, either version 3 of the License, or
//// * (at your option) any later version.
//// *
//// * UndoManager is distributed in the hope that it will be useful,
//// * but WITHOUT ANY WARRANTY; without even the implied warranty of
//// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//// * GNU Lesser General Public License for more details.
//// *
//// * You should have received a copy of the GNU Lesser General Public License
//// * along with UndoManager.  If not, see <http://www.gnu.org/licenses/>.
//// *****************************************************************************/

////using Diskordia.UndoRedo.Invokations;
////using Moq;
////using Xunit;

////namespace Diskordia.UndoRedo.Proxies
////{
////    public class InvokationRegistrationProxyTest
////    {
////        [Fact]
////        public void Invoke_TransparentProxyMethodCalled_RegistersMethodCall()
////        {
////            // arrange
////            var undoManager = new Mock<IUndoManager>();
////            var target = new Mock<ITarget>();

////            var proxy = new InvokationRegistrationProxy<ITarget>(undoManager.Object, target.Object);
////            var transparentProxy = (ITarget)proxy.GetTransparentProxy();

////            // act
////            transparentProxy.Add("TestItem");

////            // assert
////            undoManager.Verify(m => m.RegisterInvokation(It.IsAny<TransparentProxyMethodInvokation>()));
////        }
////    }
////}