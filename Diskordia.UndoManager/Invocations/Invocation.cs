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

namespace Diskordia.UndoRedo.Invocation
{
	/// <summary>
	/// The invocation class includes the selector, the receiver and the arguments to call a method of an object.
	/// </summary>
	/// <typeparam name="TSource">The type of the source.</typeparam>
	internal abstract class Invocation<TSource> : IInvokable
	{
		private string actionName = string.Empty;

		/// <summary>
		/// Gets or sets the target of the invocation.
		/// </summary>
		public TSource Target { get; protected set; }

		#region IInvokable Members

		/// <summary>
		/// Invokes the operation(s) of this <see cref="IInvokable"/> instance.
		/// </summary>
		public abstract void Invoke();

		/// <summary>
		/// Name of the action, which is performed with this invocation.
		/// </summary>
		public string ActionName
		{
			get
			{
				return this.actionName;
			}

			set
			{
				this.actionName = value != null ? value : string.Empty;
			}
		}

		#endregion
	}
}
