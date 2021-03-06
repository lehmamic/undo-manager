﻿/*****************************************************************************
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
using System.Linq.Expressions;
using Diskordia.UndoRedo.Invokations;
using Diskordia.UndoRedo.Transactions;

namespace Diskordia.UndoRedo
{
	/// <summary>
	/// Undo manager interface for recording undo and redo operations.
	/// </summary>
	public interface IUndoManager
	{
		/// <summary>
		/// Gets a value indicating whether the <see cref="IUndoManager"/> can perform an redo operation.
		/// </summary>
		/// <value><c>True</c> if the <see cref="IUndoManager"/> can perform an redo operation, <c>false</c>.</value>
		bool CanRedo { get; }

		/// <summary>
		/// Gets a value indicating whether the <see cref="IUndoManager"/> can perform an undo operation.
		/// </summary>
		/// <value><c>True</c> if the <see cref="IUndoManager"/> can perform an undo operation; otherwise, <c>false</c>.</value>
		bool CanUndo { get; }

		/// <summary>
		/// Commits the open transactions and records the registered undo operations in the <see cref="IUndoManager"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">No open <see cref="ITransaction"/> to commit available.</exception>
		void CommitTransactions();

		/// <summary>
		/// Creates an <see cref="ITransaction"/> for recording the undo operations. Undo operations
		/// which are recorded while a <see cref="ITransaction"/> is opened get recorded by the <see cref="IUndoManager"/> only if the <see cref="ITransaction"/>
		/// is commited. A rollback will execute the undo operations which where registered while the <see cref="ITransaction"/> has been open.
		/// </summary>
		/// <returns>A new object implementing the <see cref="ITransaction"/> interface.</returns>
		ITransaction CreateTransaction();

		/// <summary>
		/// Gets a value indicating whether the <see cref="IUndoManager"/> is redoing.
		/// </summary>
		/// <value>
		/// 	<c>True</c> if the <see cref="IUndoManager"/> is redoing; otherwise, <c>false</c>.
		/// </value>
		bool IsRedoing { get; }

		/// <summary>
		/// Gets a value indicating whether the <see cref="IUndoManager"/> is undoing.
		/// </summary>
		/// <value>
		/// 	<c>True</c> if the <see cref="IUndoManager"/> is undoing; otherwise, <c>false</c>.
		/// </value>
		bool IsUndoing { get; }

		/// <summary>
		/// Invokes the last recorded redo operation or transaction.
		/// </summary>
		/// <exception cref="ActionInvokationException">An error occured while invoking the registered redo operation.</exception>
		void Redo();

		/// <summary>
		/// Registers an <see cref="IInvokable"/> implementation to the <see cref="ITransaction"/>.
		/// </summary>
		/// <param name="invokation">The invokation to register.</param>
		/// <exception cref="ArgumentNullException"><paramref name="invokation"/> is a <see langword="null"/> reference.</exception>
		void RegisterInvokation(IInvokable invokation);

		/// <summary>
		/// Rollbacks the open transaction and invokes the regsitered undo operations.
		/// </summary>
		/// <exception cref="ActionInvokationException">An error occured while invoking the undo operations within the open transaction.</exception>
		void RollbackTransactions();

		/// <summary>
		/// Invokes the last recorded undo operation or transaction.
		/// </summary>
		/// <exception cref="ActionInvokationException">An error occured while invoking the registered undo operation.</exception>
		void Undo();
	}
}