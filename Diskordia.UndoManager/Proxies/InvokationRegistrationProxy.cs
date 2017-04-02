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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Security;
using Diskordia.UndoRedo.Invokations;

namespace Diskordia.UndoRedo.Proxies
{
	public class InvokationRegistrationProxy<TTarget> : RealProxy
	{
		private readonly IUndoManager undoManager;
		private readonly TTarget target;
		private string typeName;

		public InvokationRegistrationProxy(IUndoManager undoManager, TTarget target)
			: base(typeof(TTarget))
		{
			if (undoManager == null)
			{
				throw new ArgumentNullException("undoManager");
			}

			if (target == null)
			{
				throw new ArgumentNullException("target");
			}

			this.undoManager = undoManager;
			this.target = target;
		}

		#region IRemotingTypeInfo Members

		///<summary>
		///Checks whether the proxy that represents the specified object type can be cast to the type represented by the <see cref="T:System.Runtime.Remoting.IRemotingTypeInfo"></see> interface.
		///</summary>
		///<returns>
		///true if cast will succeed; otherwise, false.
		///</returns>
		///<param name="fromType">The type to cast to. </param>
		///<param name="o">The object for which to check casting. </param>
		///<exception cref="T:System.Security.SecurityException">The immediate caller makes the call through a reference to the interface and does not have infrastructure permission. </exception>
		[SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Validation done by Guard class.")]
		[SecurityCritical]
		public bool CanCastTo(Type fromType, object o)
		{
			if (fromType == null)
			{
				throw new ArgumentNullException("fromType");
			}
			if (o == null)
			{
				throw new ArgumentNullException("o");
			}

			if (fromType.IsAssignableFrom(o.GetType()))
			{
				return true;
			}

			return false;
		}

		///<summary>
		///Gets or sets the fully qualified type name of the server object in a <see cref="T:System.Runtime.Remoting.ObjRef"></see>.
		///</summary>
		///<value>
		///The fully qualified type name of the server object in a <see cref="T:System.Runtime.Remoting.ObjRef"></see>.
		///</value>
		///<exception cref="T:System.Security.SecurityException">The immediate caller makes the call through a reference to the interface and does not have infrastructure permission. </exception><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" /></PermissionSet>
		public string TypeName
		{
			[SecurityCritical] get { return this.typeName; }
			[SecurityCritical] set { this.typeName = value; }
		}

		#endregion

		#region Overrides from RealProxy

		public override IMessage Invoke(IMessage msg)
		{
			var callMessage = (IMethodCallMessage)msg;
			var invokation = new TransparentProxyMethodInvokation(this.target, callMessage);
			this.undoManager.RegisterInvokation(invokation);

			var arguments = new object[0];
			return new ReturnMessage(null, arguments, arguments.Length, callMessage.LogicalCallContext, callMessage);
		}

		#endregion
	}
}