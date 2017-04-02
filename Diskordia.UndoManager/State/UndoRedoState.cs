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

namespace Diskordia.UndoRedo.State
{
	/// <summary>
	/// Indicates the state of the <see cref="Diskordia.UndoRedo.UndoManager"/>.
	/// </summary>
	internal enum UndoRedoState
	{
		/// <summary>
		/// The <see cref="Diskordia.UndoRedo.UndoManager"/> is idle.
		/// </summary>
		Idle,

		/// <summary>
		/// The <see cref="Diskordia.UndoRedo.UndoManager"/> is undoing.
		/// </summary>
		Undoing,

		/// <summary>
		/// The <see cref="Diskordia.UndoRedo.UndoManager"/> is redoing.
		/// </summary>
		Redoing,

		/// <summary>
		/// The <see cref="Diskordia.UndoRedo.UndoManager"/> is committing an open <see cref="Diskordia.UndoRedo.Transactions.ITransaction"/>.
		/// </summary>
		Committing,

		/// <summary>
		/// The <see cref="Diskordia.UndoRedo.UndoManager"/> is rolling back an open <see cref="Diskordia.UndoRedo.Transactions.ITransaction"/>.
		/// </summary>
		RollingBack
	}
}