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

namespace Diskordia.UndoRedo
{
	/// <summary>
	/// This class provides extension methods to support the usage of the <see cref="UndoManager"/>.
	/// </summary>
	public static class UndoManagerExtensions
	{
		/// <summary>
		/// Prepares the target object as the subject for the dynamically invoked undo/redo operations.
		/// </summary>
		/// <param name="undoManager">The undo manager.</param>
		/// <param name="target">The target of the dynamic invokation.</param>
		/// <returns>The dynamic object targeting the provided <paramref name="target"/>.</returns>
		public static dynamic PrepareWithInvocationTarget(this IUndoManager undoManager, object target)
		{
			if (undoManager == null)
			{
				throw new ArgumentNullException("undoManager");
			}

			if (target == null)
			{
				throw new ArgumentNullException("target");
			}

			return new UndressedInvokationTarget(undoManager, target);
		}
	}
}