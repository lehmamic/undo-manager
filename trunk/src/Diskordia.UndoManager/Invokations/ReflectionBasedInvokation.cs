/*****************************************************************************
 * UndoManager. An easy to use undo API.
 * Copyright (C) 2009  Michael Lehmann 
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
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Diskordia.UndoRedo.Invokations
{
	/// <summary>
	/// An <see cref="IInvokable"/> implementation that invokes an operation based on reflection informations.
	/// </summary>
	internal class ReflectionBasedInvokation : IInvokable
	{
		private const BindingFlags MEMBERSFLAGS =
		BindingFlags.Instance | BindingFlags.Static |
		BindingFlags.Public | BindingFlags.NonPublic;

		private readonly object target;
		private readonly InvokeMemberBinder binder;
		private readonly object[] args;

		/// <summary>
		/// Initializes a new instance of the <see cref="ReflectionBasedInvokation"/> class.
		/// </summary>
		/// <param name="target">The target object.</param>
		/// <param name="binder">The binder carrying the reflection information.</param>
		/// <param name="args">The arguments to pass to the method.</param>
		/// <exception cref="ArgumentNullException">
		///		<para><paramref name="target"/> is a <see langword="null"/> reference</para>
		///		<para>- or -</para>
		///		<para><paramref name="binder"/> is a <see langword="null"/> reference</para>
		///		<para>- or -</para>
		///		<para><paramref name="args"/> is a <see langword="null"/> reference.</para>
		/// </exception>
		public ReflectionBasedInvokation(object target, InvokeMemberBinder binder, object[] args)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}

			if (binder == null)
			{
				throw new ArgumentNullException("binder");
			}

			if (args == null)
			{
				throw new ArgumentNullException("args");
			}

			this.target = target;
			this.binder = binder;
			this.args = args;
		}

		#region IInvokable members

		/// <summary>
		/// Invokes the operation(s) of this <see cref="IInvokable"/> instance.
		/// </summary>
		public void Invoke()
		{
			var method = this.TypeOfDressed()
				.GetMethod(this.binder.Name, MEMBERSFLAGS, null, this.GetParameterTypes(this.args).ToArray(), null);

			method.Invoke(this.target, this.args);
		}

		#endregion

		#region Private members

		private IEnumerable<Type> GetParameterTypes(object[] args)
		{
			return from a in args select this.GetActualType(a);
		}

		private Type GetActualType(object t)
		{
			return (t is ParameterInfo) ? (t as ParameterInfo).ParameterType
										: t.GetType();
		}

		private Type TypeOfDressed()
		{
			return (this.target is Type) ? (Type)this.target
											: this.target.GetType();
		}

		#endregion
	}
}