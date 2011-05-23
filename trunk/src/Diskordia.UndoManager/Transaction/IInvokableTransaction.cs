﻿/*****************************************************************************
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
using System.Linq.Expressions;
using Diskordia.UndoRedo.Invocation;

namespace Diskordia.UndoRedo.Transaction
{
	/// <summary>
	/// Inernal interface for transaction - undo manager interaction.
	/// </summary>
	internal interface IInvokableTransaction : ITransaction, IInvokable
	{
		/// <summary>
		/// Registers an operation to the <see cref="ITransaction"/>.
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <param name="target">The target of the undo operation.</param>
		/// <param name="selector">The undo operation which will be invoked on the target.</param>
		/// <exception cref="ArgumentNullException">
		///		<para><paramref name="target"/> is a <see langword="null"/> reference</para>
		///		<para>- or -</para>
		///		<para><paramref name="selector"/> is a <see langword="null"/> reference.</para>
		/// </exception>
		void RegisterInvokation<TSource>(TSource target, Expression<Action<TSource>> selector);

		/// <summary>
		/// Registers an <see cref="IInvokable"/> implementation to the <see cref="ITransaction"/>.
		/// </summary>
		/// <param name="invokation">The invokation to register.</param>
		/// <exception cref="ArgumentNullException"><paramref name="invokation"/> is a <see langword="null"/> reference.</exception>
		void RegisterInvocation(IInvokable invokation);

		/// <summary>
		/// Name of the action, which is performed with this invocation.
		/// </summary>
		string ActionName { get; set; }
	}
}