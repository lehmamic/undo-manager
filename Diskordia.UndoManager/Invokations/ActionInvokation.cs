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

namespace Diskordia.UndoRedo.Invokations
{
	/// <summary>
	/// The action invocation class includes the selector and the arguments to call a method of an object.
	/// </summary>
	/// <typeparam name="TArgument">The type of the argument.</typeparam>
	public sealed class ActionInvokation<TArgument> : IInvokable
	{
		private readonly Action<TArgument> selector;
		private readonly TArgument argument;

		/// <summary>
		/// Initializes a new instance of the <see cref="ActionInvokation&lt;TArgument&gt;"/> class.
		/// </summary>
		/// <param name="selector">The delegate to invoke.</param>
		/// <param name="argument">The argument of the delegate method.</param>
		/// <exception cref="ArgumentNullException">
		///		<para><paramref name="selector"/> is a <see langword="null"/> reference</para>
		///		<para>- or -</para>
		///		<para><paramref name="argument"/> is a <see langword="null"/> reference.</para>
		/// </exception>
		public ActionInvokation(Action<TArgument> selector, TArgument argument)
		{
			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}

			if (argument == null)
			{
				throw new ArgumentNullException("argument");
			}

			this.selector = selector;
			this.argument = argument;
		}

		#region IInvokable members

		/// <summary>
		/// Invokes the operation(s) of this <see cref="IInvokable"/> instance.
		/// </summary>
		public void Invoke()
		{
			this.selector(this.argument);
		}

		#endregion
	}
}