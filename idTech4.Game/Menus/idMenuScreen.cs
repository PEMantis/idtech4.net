﻿/*
===========================================================================

Doom 3 BFG Edition GPL Source Code
Copyright (C) 1993-2012 id Software LLC, a ZeniMax Media company. 

This file is part of the Doom 3 BFG Edition GPL Source Code ("Doom 3 BFG Edition Source Code").  

Doom 3 BFG Edition Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Doom 3 BFG Edition Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Doom 3 BFG Edition Source Code.  If not, see <http://www.gnu.org/licenses/>.

In addition, the Doom 3 BFG Edition Source Code is also subject to certain additional terms. You should have received a copy of these additional terms immediately following the terms and conditions of the GNU General Public License which accompanied the Doom 3 BFG Edition Source Code.  If not, please request a copy in writing from id Software at the address below.

If you have questions concerning this license or the applicable additional terms, you may contact in writing id Software LLC, c/o ZeniMax Media Inc., Suite 120, Rockville, Maryland 20850 USA.

===========================================================================
*/
using System.Collections.Generic;

using idTech4.UI.SWF;

namespace idTech4.Game.Menus
{
	public abstract class idMenuScreen : idMenuWidget
	{
		#region Members
		private idSWF _menuGui;
		private MainMenuTransition _transition;
		#endregion

		#region Constructor
		public idMenuScreen()
			: base()
		{
			_transition = MainMenuTransition.Invalid;
		}
		#endregion

		#region idMenuWidget implementation
		#region Frame
		public override void Update()
		{
			if(_menuGui == null)
			{
				return;
			}

			//
			// Display
			//
			for(int childIndex = 0; childIndex < this.Children.Length; ++childIndex)
			{
				this.Children[childIndex].Update();
			}

			if(_menuData != null)
			{
				_menuData.UpdateChildren();
			}
		}
		#endregion
		#endregion
	}

	public enum MainMenuTransition
	{
		Invalid = -1,
		Simple,
		Advance,
		Back,
		Force
	}
}