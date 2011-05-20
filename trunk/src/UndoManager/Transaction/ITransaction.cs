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

namespace UndoRedo.Transaction
{
	/// <summary>
	/// Transaction interface for handling the undo redo transactions.
	/// </summary>
	public interface ITransaction : IDisposable
	{
		/// <summary>
		/// Commits the undo operation of this <see cref="ITransaction"/>.
		/// </summary>
		void Commit();

		/// <summary>
		/// Rollbacks the transaction and calls the undo operations to recover the state befor the <see cref="ITransaction"/> has been created.
		/// </summary>
		void Rollback();
	}
}
