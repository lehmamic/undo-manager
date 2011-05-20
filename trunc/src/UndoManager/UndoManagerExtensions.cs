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
using System.Linq.Expressions;

namespace UndoRedo
{
	/// <summary>
	/// This class provides extension methods to support the usage of the <see cref="UndoManager"/>.
	/// </summary>
	public static class UndoManagerExtensions
	{
		/// <summary>
		/// Registers an operation into the undo history of the default <see cref="UndoManager"/>.
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <param name="target">The target instance.</param>
		/// <param name="selector">The invocation delegate of the undo operation.</param>
		/// <exception cref="ArgumentNullException">
		///		<para><paramref name="target"/> is a <see langword="null"/> reference</para>
		///		<para>- or -</para>
		///		<para><paramref name="selector"/> is a <see langword="null"/> reference.</para>
		/// </exception>
		public static void RegisterUndoInvocation<TSource>(this TSource target, Expression<Action<TSource>> selector)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}

			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}

			UndoManager.DefaultUndoManager.RegisterInvocation(target, selector);
		}
	}
}
