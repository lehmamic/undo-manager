using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Security;

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
		[SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
			Justification = "Validation done by Guard class.")]
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
			return msg;
		}

		#endregion
	}
}