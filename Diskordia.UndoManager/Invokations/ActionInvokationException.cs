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

namespace Diskordia.UndoRedo.Invokations
{
	/// <summary>
	/// Represents an error occur during invokation of an undo or redo operation.
	/// </summary>
	public sealed class ActionInvokationException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ActionInvokationException"/> class.
		/// </summary>
		/// <param name="message">Message embedded in the exception.</param>
		public ActionInvokationException(string message)
			: this(message, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ActionInvokationException"/> class.
		/// </summary>
		/// <param name="message">Message embedded in the exception.</param>
		/// <param name="innerException">The <see cref="Exception"/> embedded in the now constructed <see cref="ActionInvokationException"/> instance.</param>
		public ActionInvokationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}