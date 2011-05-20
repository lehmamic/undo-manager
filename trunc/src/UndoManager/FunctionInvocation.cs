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
	/// The function invocation class includes the selector, the receiver and the arguments to call a method of an object.
	/// </summary>
	/// <typeparam name="TSource">The type of the source.</typeparam>
	/// <typeparam name="TResult">The type of the result.</typeparam>
	internal class FunctionInvocation<TSource, TResult> : Invocation<TSource>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionInvocation&lt;TSource, TResult&gt;"/> class.
		/// </summary>
		/// <param name="target">The target on which the operation described by <paramref name="expression"/> has to be invoked.</param>
		/// <param name="expression">The LinQ expressio describing the action to invoke.</param>
		/// <exception cref="ArgumentNullException">
		///		<para><paramref name="target"/> is a <see langword="null"/> reference</para>
		///		<para>- or -</para>
		///		<para><paramref name="expression"/> is a <see langword="null"/> reference.</para>
		/// </exception>
		public FunctionInvocation(TSource target, Expression<Func<TSource, TResult>> expression)
		{
			if (target == null)
			{
				throw new ArgumentNullException("expression");
			}

			if (target == null)
			{
				throw new ArgumentNullException("target");
			}

			this.Target = target;
			this.Expression = expression;
		}

		/// <summary>
		/// Gets the expression required to invoke the operation.
		/// </summary>
		public Expression<Func<TSource, TResult>> Expression { get; private set; }

		/// <summary>
		/// Invokes this instance.
		/// </summary>
		public override void Invoke()
		{
			Func<TSource, TResult> func = this.Expression.Compile();
			func(Target);
		}
	}
}
