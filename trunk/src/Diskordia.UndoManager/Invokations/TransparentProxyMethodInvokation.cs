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
using System.Runtime.Remoting.Messaging;

namespace Diskordia.UndoRedo.Invokations
{
	public class TransparentProxyMethodInvokation : IInvokable
	{
		private readonly object target;
		private readonly IMethodCallMessage methodCallMessage;

		public TransparentProxyMethodInvokation(object target, IMethodCallMessage methodCallMessage)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}

			if (methodCallMessage == null)
			{
				throw new ArgumentNullException("methodCallMessage");
			}

			this.target = target;
			this.methodCallMessage = methodCallMessage;
		}

		public void Invoke()
		{
			this.methodCallMessage.MethodBase.Invoke(target, this.methodCallMessage.Args);
		}
	}
}