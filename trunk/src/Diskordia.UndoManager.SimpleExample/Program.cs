/*****************************************************************************
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diskordia.UndoRedo;

namespace SimpleExample
{
	class Program
	{
		static void Main(string[] args)
		{
			ColoredLight light = new ColoredLight();
			Console.WriteLine("====== WORKING WITHOUT TRANSACTIONS ======");
			// switch on the light
			Console.WriteLine("Initial state");
			light.SwitchOn();

			// undo switch on => switch off
			Console.WriteLine("Undo operation");
			UndoManager.DefaultUndoManager.Undo();

			// redo undone operation => switch on again
			Console.WriteLine("Redo undone operation");
			UndoManager.DefaultUndoManager.Redo();

			Console.WriteLine("====== WORKING WITH TRANSACTIONS ======");
			// switch on the light
			Console.WriteLine("Initial state");
			using (UndoManager.DefaultUndoManager.CreateTransaction())
			{
				light.SwitchOn();
				light.SetColor("red");
			}

			// undo operation within the transaction
			Console.WriteLine("Undo Transaction");
			UndoManager.DefaultUndoManager.Undo();
		}
	}

	class ColoredLight
	{
		private string color = "white";

		public void SwitchOn()
		{
			Console.WriteLine("Switch on the light");
			UndoManager.DefaultUndoManager.PrepareWithInvocationTarget(this)
				.SwitchOff();
		}

		public void SwitchOff()
		{
			Console.WriteLine("Switch off the light");
			UndoManager.DefaultUndoManager.PrepareWithInvocationTarget(this)
				.SwitchOn();
		}

		public void SetColor(string color)
		{
			string backup = this.color;

			if (backup != color)
			{
				this.color = color;
				Console.WriteLine("Set color {0}.", color);

				UndoManager.DefaultUndoManager.PrepareWithInvocationTarget(this)
					.SetColor(backup);
			}
		}
	}
}
