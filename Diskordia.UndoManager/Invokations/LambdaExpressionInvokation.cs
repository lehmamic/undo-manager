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

namespace Diskordia.UndoRedo.Invokations
{
	/// <summary>
	/// The action invocation class includes the selector, the receiver and the arguments to call a method of an object.
	/// </summary>
	/// <typeparam name="TSource">The type of the source.</typeparam>
	public sealed class LambdaExpressionInvokation<TSource> : IInvokable
	{
		private readonly Expression<Action<TSource>> expression;
		private readonly TSource target;

		/// <summary>
		/// Initializes a new instance of the <see cref="LambdaExpressionInvokation&lt;TSource&gt;"/> class.
		/// </summary>
		/// <param name="target">The target on which the operation described by <paramref name="expression"/> has to be invoked.</param>
		/// <param name="expression">The LinQ expressio describing the action to invoke.</param>
		/// <exception cref="ArgumentNullException">
		///		<para><paramref name="target"/> is a <see langword="null"/> reference</para>
		///		<para>- or -</para>
		///		<para><paramref name="expression"/> is a <see langword="null"/> reference.</para>
		/// </exception>
		public LambdaExpressionInvokation(TSource target, Expression<Action<TSource>> expression)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}

			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}

			this.target = target;
			this.expression = expression;
		}

		#region IInvokable members

		/// <summary>
		/// Invokes the operation(s) of this <see cref="IInvokable"/> instance.
		/// </summary>
		public void Invoke()
		{
			Action<TSource> action = this.expression.Compile();
			action(this.target);
		}

		#endregion
	}
}