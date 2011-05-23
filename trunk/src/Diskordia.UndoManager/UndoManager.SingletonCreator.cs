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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diskordia.UndoRedo
{
	public sealed partial class UndoManager
	{
		#region Signleton Creator

		/// <summary>
		/// Internal class to support lazy initialization.
		/// </summary>
		private class SingletonCreator
		{
			/// <summary>
			/// Initializes static members of the <see cref="SingletonCreator"/> class.
			/// </summary>
			static SingletonCreator()
			{
			}

			/// <summary>
			/// Prevents a default instance of the <see cref="SingletonCreator"/> class from being created.
			/// </summary>
			private SingletonCreator()
			{
			}

			/// <summary>
			/// Threadsafe implementation (compiler guaranteed whith static initializatrion).
			/// This implementation uses an inner class to make the .NET instantiation fully lazy.
			/// </summary>
			internal static readonly UndoManager Instance = new UndoManager();
		}

		/// <summary>
		/// Gets the default undo manager.
		/// </summary>
		/// <value>The default undo manager.</value>
		public static IUndoManager DefaultUndoManager
		{
			get
			{
				return SingletonCreator.Instance;
			}
		}

		#endregion
	}
}