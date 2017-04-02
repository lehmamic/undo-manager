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
using Diskordia.UndoRedo.Invokations;
using Moq;
using Xunit;

namespace Diskordia.UndoRedo
{
    public class UndoManagerTests
    {
        [Fact]
        public void Constrcutor_TransactionFactoryNullReference_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new UndoManager(null));
        }

        [Fact]
        public void RegisterInvocation_InvokationNullReference_ThrowsException()
        {
            var target = new UndoManager();

            Assert.Throws<ArgumentNullException>(() => target.RegisterInvokation((IInvokable)null));
        }

        [Fact]
        public void RegisterInvocation_Invokation_RegistersInvokation()
        {
            var target = new UndoManager();

            var invokableMock = new Mock<IInvokable>(MockBehavior.Strict);
            target.RegisterInvokation(invokableMock.Object);

            Assert.True(target.CanUndo);
            Assert.False(target.CanRedo);
        }

        [Fact]
        public void RegisterInvocation_DelegateNullReference_ThrowsException()
        {
            var target = new UndoManager();

            Assert.Throws<ArgumentNullException>(() => target.RegisterInvokation((Action<string>)null, string.Empty));
        }

        [Fact]
        public void RegisterInvocation_ActionArgumentNullReference_ThrowsException()
        {
            var target = new UndoManager();

            var targetMock = new Mock<ITarget>();

            Assert.Throws<ArgumentNullException>(() => target.RegisterInvokation(targetMock.Object.Add, (string)null));
        }

        [Fact]
        public void RegisterInvocation_ActionDelegate_RegistersAction()
        {
            var target = new UndoManager();

            var targetMock = new Mock<ITarget>();
            target.RegisterInvokation(targetMock.Object.Add, "test");

            Assert.True(target.CanUndo);
            Assert.False(target.CanRedo);
        }

        [Fact]
        public void RegisterInvocation_ExpressionNullReference_ThrowsException()
        {
            var target = new UndoManager();

            var targetMock = new Mock<ITarget>();

            Assert.Throws<ArgumentNullException>(() => target.RegisterInvokation(targetMock.Object, null));
        }

        [Fact]
        public void RegisterInvocation_TargetNullReference_ThrowsException()
        {
            var target = new UndoManager();

            var targetMock = new Mock<ITarget>();

            Assert.Throws<ArgumentNullException>(() => target.RegisterInvokation<ITarget>(null, t => t.UndoOperation()));
        }

        [Fact]
        public void RegisterInvocation_LambdaExpression_RegistersLambdaExpression()
        {
            var target = new UndoManager();
            var targetMock = new Mock<ITarget>();
            target.RegisterInvokation(targetMock.Object, t => t.UndoOperation());

            Assert.True(target.CanUndo);
            Assert.False(target.CanRedo);
        }

        [Fact]
        public void Undo_NoInvocationsRegistered_ThrowsException()
        {
            var target = new UndoManager();

            Assert.Throws<InvalidOperationException>(() => target.Undo());
        }

        [Fact]
        public void Undo_InvocationRegistered_InvokesUndoOperation()
        {
            var target = new UndoManager();

            var invokableMock = new Mock<IInvokable>(MockBehavior.Loose);
            target.RegisterInvokation(invokableMock.Object);

            target.Undo();

            invokableMock.Verify(i => i.Invoke(), Times.Once());
            Assert.False(target.CanUndo);
            Assert.False(target.CanRedo);
        }

        [Fact]
        public void Undo_UndoOperationRegistersInvocations_InvocationsAreRegisteredInRedoStack()
        {
            var target = new UndoManager();

            var invokableMock = new Mock<ITarget>(MockBehavior.Strict);
            invokableMock.Setup(i => i.UndoOperation())
                .Callback(() => target.RegisterInvokation(invokableMock.Object, i => i.RedoOperation()));

            target.RegisterInvokation(invokableMock.Object, i => i.UndoOperation());

            target.Undo();

            invokableMock.Verify(i => i.UndoOperation(), Times.Once());
            invokableMock.Verify(i => i.RedoOperation(), Times.Never());
            Assert.False(target.CanUndo);
            Assert.True(target.CanRedo);
        }

        [Fact]
        public void Redo_NoRedoInvocationsRegistered_ThrowsException()
        {
            var target = new UndoManager();

            Assert.Throws<InvalidOperationException>(() => target.Redo());
        }

        [Fact]
        public void Redo_RedoOperationAvailable_InvokesRedoOperation()
        {
            var target = new UndoManager();

            var invokableMock = new Mock<ITarget>(MockBehavior.Strict);
            invokableMock.Setup(i => i.UndoOperation())
                .Callback(() => target.RegisterInvokation(invokableMock.Object, i => i.RedoOperation()));
            invokableMock.Setup(i => i.RedoOperation());

            target.RegisterInvokation(invokableMock.Object, i => i.UndoOperation());

            target.Undo();
            target.Redo();

            invokableMock.Verify(i => i.UndoOperation(), Times.Once());
            invokableMock.Verify(i => i.RedoOperation(), Times.Once());
            Assert.False(target.CanUndo);
            Assert.False(target.CanRedo);
        }

        [Fact]
        public void Redo_RedoRegistersInvocation_InvocationsAreRegisteredInUndoStack()
        {
            var target = new UndoManager();

            var invokableMock = new Mock<ITarget>(MockBehavior.Strict);
            invokableMock.Setup(i => i.UndoOperation())
                .Callback(() => target.RegisterInvokation(invokableMock.Object, i => i.RedoOperation()));
            invokableMock.Setup(i => i.RedoOperation())
                .Callback(() => target.RegisterInvokation(invokableMock.Object, i => i.UndoOperation()));

            target.RegisterInvokation(invokableMock.Object, i => i.UndoOperation());

            target.Undo();
            target.Redo();

            invokableMock.Verify(i => i.UndoOperation(), Times.Once());
            invokableMock.Verify(i => i.RedoOperation(), Times.Once());
            Assert.True(target.CanUndo);
            Assert.False(target.CanRedo);
        }
    }
}