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

namespace Diskordia.UndoRedo.Transactions
{
	/// <summary>
	/// Interface for the ndo manager to encapsulate the transaction - undo manager interaction.
	/// </summary>
	internal interface ITransactionManager
	{
		/// <summary>
		/// Commits the provided transaction.
		/// </summary>
		/// <param name="transaction">The transaction to commit.</param>
		/// <exception cref="ArgumentNullException"><paramref name="transaction"/> is a <see langword="null"/> reference.</exception>
		/// <exception cref="ArgumentException">The <see cref="UndoManager"/> does not contain <paramref name="transaction"/>.</exception>
		void CommitTransaction(IInvokableTransaction transaction);

		/// <summary>
		/// Rollbacks the provided transaction.
		/// </summary>
		/// <param name="transaction">The transaction to roll back.</param>
		/// <exception cref="ArgumentNullException"><paramref name="transaction"/> is a <see langword="null"/> reference.</exception>
		/// <exception cref="ArgumentException">The <see cref="UndoManager"/> does not contain <paramref name="transaction"/>.</exception>
		/// <exception cref="Diskordia.UndoRedo.Invokations.ActionInvokationException">An error occured while invoking the undo operations within the transaction.</exception>
		void RollbackTransaction(IInvokableTransaction transaction);
	}
}