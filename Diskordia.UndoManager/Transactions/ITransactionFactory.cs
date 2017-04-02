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

namespace Diskordia.UndoRedo.Transactions
{
	/// <summary>
	/// Interface to decouple the factor whichis creating instances of <see cref="IInvokableTransaction"/> implementations.
	/// </summary>
	public interface ITransactionFactory
	{
		/// <summary>
		/// Create an instance of the <see cref="IInvokableTransaction"/>.
		/// </summary>
		/// <param name="transactionManager">the transaction manager handling the transactions.</param>
		/// <returns>An factory implementing the <see cref="IInvokableTransaction"/> interface.</returns>
		/// <exception cref="System.ArgumentNullException"><paramref name="transactionManager"/> is a <see langword="null"/> reference.</exception>
		IInvokableTransaction CreateTransaction(ITransactionManager transactionManager);
	}
}