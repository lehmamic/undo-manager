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
using Xunit;

namespace Diskordia.UndoRedo.Invokations
{
    public class ActionInvokationTests
    {
        [Fact]
        public void Constructor_InvokationNullReference_ThrowsException()
        {
            string argument = "1234";

            Assert.Throws<ArgumentNullException>(() => new ActionInvokation<string>(null, argument));
        }

        [Fact]
        public void Constructor_ArgumentNullReference_ThrowsException()
        {
            Mock<ITarget> targetMock = new Mock<ITarget>(MockBehavior.Strict);

            Assert.Throws<ArgumentNullException>(() => new ActionInvokation<string>(targetMock.Object.Add, null));
        }

        [Fact]
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