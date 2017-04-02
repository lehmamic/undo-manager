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

namespace Diskordia.UndoRedo.State
{
	/// <summary>
	/// This class switchs the state of the <see cref="Diskordia.UndoRedo.UndoManager"/> exception save.
	/// </summary>
	internal class StateSwitcher : IDisposable
	{
		private readonly IStateHost owner;
		private readonly Diskordia.UndoRedo.State.UndoRedoState backup;

		private bool disposed = false;

		/// <summary>
		/// Initializes a new instance of the <see cref="StateSwitcher"/> class.
		/// </summary>
		/// <param name="target">The target <see cref="IStateHost"/> to switch the state.</param>
		/// <param name="state">The <see cref="UndoRedoState"/> to set on the <paramref name="target"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="target"/> is a <see langword="null"/> reference.</exception>
		public StateSwitcher(IStateHost target, Diskordia.UndoRedo.State.UndoRedoState state)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}

			this.owner = target;

			this.backup = this.owner.State;
			this.owner.State = state;
		}

		#region Dispose Members

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>True</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			// check to see if Dispose has already been called
			if (!this.disposed)
			{
				// if IDisposable.Dispose was called, dispose all managed resources
				if (disposing)
				{
					this.owner.State = this.backup;
				}

				// if Finalize or IDisposable.Dispose, free your own state (unmanaged objects)
				this.disposed = true;
			}
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="StateSwitcher"/> class.
		/// </summary>
		/// <remark>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="StateSwitcher"/> is reclaimed by garbage collection.
		/// </remark>
		~StateSwitcher()
		{
			this.Dispose(false);
		}

		#endregion
	}
}