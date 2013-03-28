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
using Diskordia.UndoRedo.Invokations;
using Diskordia.UndoRedo.Proxies;

namespace Diskordia.UndoRedo
{
	public static class UndoManagerExtensions
	{
		/// <summary>
		/// Registers an operation as lambda expression, which will be invoked when an undo is performed.
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <param name="undoManager">The <see cref="IUndoManager"/> instance.</param>
		/// <param name="target">The target instance.</param>
		/// <param name="selector">The invocation delegate of the undo operation.</param>
		/// <exception cref="ArgumentNullException">
		///		<para><paramref name="target"/> is a <see langword="null"/> reference</para>
		///		<para>- or -</para>
		///		<para><paramref name="selector"/> is a <see langword="null"/> reference.</para>
		/// </exception>
		public static void RegisterInvokation<TSource>(this IUndoManager undoManager, TSource target, Expression<Action<TSource>> selector)
		{
			if (undoManager == null)
			{
				throw new ArgumentNullException("undoManager");
			}

			if (target == null)
			{
				throw new ArgumentNullException("target");
			}

			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}

			var invokation = new LambdaExpressionInvokation<TSource>(target, selector);
			undoManager.RegisterInvokation(invokation);
		}

		/// <summary>
		/// Registers an operation with the provided argument, which will be invoked when an undo is performed.
		/// </summary>
		/// <typeparam name="TArgument">The type of the argument.</typeparam>
		/// <param name="undoManager">The <see cref="IUndoManager"/> instance.</param>
		/// <param name="selector">The invocation delegate of the undo operation.</param>
		/// <param name="argument">The argument to pass the teh method call while invoking the registered invokation.</param>
		/// <exception cref="ArgumentNullException">
		///		<para><paramref name="selector"/> is a <see langword="null"/> reference</para>
		///		<para>- or -</para>
		///		<para><paramref name="argument"/> is a <see langword="null"/> reference.</para>
		/// </exception>
		public static void RegisterInvokation<TArgument>(this IUndoManager undoManager, Action<TArgument> selector, TArgument argument)
		{
			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}

			if (argument == null)
			{
				throw new ArgumentNullException("argument");
			}

			var invokation = new ActionInvokation<TArgument>(selector, argument);
			undoManager.RegisterInvokation(invokation);
		}

		/// <summary>
		/// Prepares the target object as the subject for the dynamically invoked undo/redo operations.
		/// </summary>
		/// <param name="undoManager">The <see cref="IUndoManager"/> instance.</param>
		/// <param name="target">The target of the dynamic invokation.</param>
		/// <returns>The dynamic object targeting the provided <paramref name="target"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="target"/> is a <see langword="null"/> reference.</exception>
		public static dynamic PrepareWithInvocationTarget(this IUndoManager undoManager, object target)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}

			return new UndressedInvokationTarget(undoManager, target);
		}

		public static TTarget RegisterFor<TTarget>(this IUndoManager undoManager, TTarget target)
		{
			var proxy = new InvokationRegistrationProxy<TTarget>(undoManager, target);
			return (TTarget)proxy.GetTransparentProxy();
		}
	}
}