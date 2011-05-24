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
using System.Dynamic;

namespace Diskordia.UndoRedo.Invokations
{
	/// <summary>
	/// An implementation of <see cref="DynamicObject"/> that registers the invokations at the <see cref="IUndoManager"/>.
	/// </summary>
	internal class UndressedInvokationTarget : DynamicObject
	{
		private readonly IUndoManager undoManager;
		private readonly object target;

		/// <summary>
		/// Initializes a new instance of the <see cref="UndressedInvokationTarget"/> class.
		/// </summary>
		/// <param name="undoManager">The undo manager.</param>
		/// <param name="target">The target of the invokation.</param>
		/// <exception cref="ArgumentNullException">
		///		<para><paramref name="undoManager"/> is a <see langword="null"/> reference</para>
		///		<para>- or -</para>
		///		<para><paramref name="target"/> is a <see langword="null"/> reference.</para>
		/// </exception>
		public UndressedInvokationTarget(IUndoManager undoManager, object target)
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

		/// <summary>
		/// Provides the implementation for operations that invoke a member. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as calling a method.
		/// </summary>
		/// <param name="binder">Provides information about the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the statement sampleObject.SampleMethod(100), where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleMethod". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
		/// <param name="args">The arguments that are passed to the object member during the invoke operation. For example, for the statement sampleObject.SampleMethod(100), where sampleObject is derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, <paramref name="args"/> is equal to 100.</param>
		/// <param name="result">The result of the member invocation.</param>
		/// <returns>
		/// True if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.).
		/// </returns>
		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			result = null;

			ReflectionBasedInvokation invokation = new ReflectionBasedInvokation(this.target, binder, args);
			this.undoManager.RegisterInvokation(invokation);

			return true;
		}
	}
}