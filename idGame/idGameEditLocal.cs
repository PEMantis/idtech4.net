﻿/*
===========================================================================

Doom 3 GPL Source Code
Copyright (C) 1999-2011 id Software LLC, a ZeniMax Media company. 

This file is part of the Doom 3 GPL Source Code (?Doom 3 Source Code?).  

Doom 3 Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Doom 3 Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Doom 3 Source Code.  If not, see <http://www.gnu.org/licenses/>.

In addition, the Doom 3 Source Code is also subject to certain additional terms. You should have received a copy of these additional terms immediately following the terms and conditions of the GNU General Public License which accompanied the Doom 3 Source Code.  If not, please request a copy in writing from id Software at the address below.

If you have questions concerning this license or the applicable additional terms, you may contact in writing id Software LLC, c/o ZeniMax Media Inc., Suite 120, Rockville, Maryland 20850 USA.

===========================================================================
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace idTech4.Game
{
	public class idGameEditLocal : idGameEdit
	{
		#region Constructor
		public idGameEditLocal()
		{
			idR.GameEdit = this;
		}
		#endregion

		#region Methods
		public void ParseSpawnArgsToRenderEntity(idDict args, idRenderEntity renderEntity)
		{
			renderEntity.Clear();
			
			string temp = args.GetString("model");

			//TODO
			/*modelDef = NULL;
			if ( temp[0] != '\0' ) {
				modelDef = static_cast<const idDeclModelDef *>( declManager->FindType( DECL_MODELDEF, temp, false ) );
				if ( modelDef ) {
					renderEntity->hModel = modelDef->ModelHandle();
				}
				if ( !renderEntity->hModel ) {
					renderEntity->hModel = renderModelManager->FindModel( temp );
				}
			}
			if ( renderEntity->hModel ) {
				renderEntity->bounds = renderEntity->hModel->Bounds( renderEntity );
			} else*/
			{
				// TODO: renderEntity.Bounds = new idBounds();
			}

			temp = args.GetString("skin");

			if(temp != null)
			{
				renderEntity.CustomSkin = idR.DeclManager.FindSkin(temp);
			}
			else if(1 == 0 /* modelDef != null*/)
			{
				// TODO
				//renderEntity->customSkin = modelDef->GetDefaultSkin();
			}

			temp = args.GetString("shader");

			if(temp != null)
			{
				renderEntity.CustomShader = idR.DeclManager.FindMaterial(temp);
			}

			renderEntity.Origin = args.GetVector("origin", "0 0 0");

			// get the rotation matrix in either full form, or single angle form
			renderEntity.Axis = args.GetMatrix("rotation", "1 0 0 0 1 0 0 0 1");

			if(renderEntity.Axis == Matrix.Identity)
			{
				float angle = args.GetFloat("angle");

				if(angle != 0.0f)
				{
					renderEntity.Axis = Matrix.CreateRotationY(angle); // TODO: this might fuck things up, upside down models and stuff
				}
				else
				{
					renderEntity.Axis = Matrix.Identity;
				}
			}

			// TODO
			//renderEntity.ReferencedSound = null;

			// get shader parms
			Vector3 color = args.GetVector("_color", "1 1 1");

			float[] shaderParms = renderEntity.ShaderParms;

			shaderParms[(int) ShaderParameter.Red] = color.X;
			shaderParms[(int) ShaderParameter.Green] = color.Y;
			shaderParms[(int) ShaderParameter.Blue] = color.Z;

			shaderParms[3] = args.GetFloat("shaderParm3", "1");
			shaderParms[4] = args.GetFloat("shaderParm4", "0");
			shaderParms[5] = args.GetFloat("shaderParm5", "0");
			shaderParms[6] = args.GetFloat("shaderParm6", "0");
			shaderParms[7] = args.GetFloat("shaderParm7", "0");
			shaderParms[8] = args.GetFloat("shaderParm8", "0");
			shaderParms[9] = args.GetFloat("shaderParm9", "0");
			shaderParms[10] = args.GetFloat("shaderParm10", "0");
			shaderParms[11] = args.GetFloat("shaderParm11", "0");

			renderEntity.ShaderParms = shaderParms;

			// check noDynamicInteractions flag
			renderEntity.NoDynamicInteractions = args.GetBool("noDynamicInteractions");

			// check noshadows flag
			renderEntity.NoShadow = args.GetBool("noshadows");

			// check noselfshadows flag
			renderEntity.NoSelfShadow = args.GetBool("noselfshadows");

			// TODO
			// init any guis, including entity-specific states
			/*for(int i = 0; i < renderEntity.Length; i++)
			{
				temp = args.GetString(i == 0 ? "gui" : string.Format("gui{0}", i + 1));

				if(temp != null)
				{
					renderEntity.Gui[i] = AddRenderGui(temp, args);
				}
			}*/
		}

		private idUserInterface AddRenderGui(string name, idDict args)
		{
			idKeyValue kv = args.MatchPrefix("gui_parm", null);
			idUserInterface gui = idR.UIManager.FindGui(name, true, (kv != null));

			UpdateGuiParams(gui, args);

			return gui;
		}

		private void UpdateGuiParams(idUserInterface gui, idDict args)
		{
			if((gui == null) || (args == null))
			{
				return;
			}

			idKeyValue kv = args.MatchPrefix("gui_parm", null);

			while(kv != null)
			{
				gui.SetState(kv.Key, kv.Value);
				kv = args.MatchPrefix("gui_parm", kv);
			}

			gui.SetState("noninteractive", args.GetBool("gui_noninteractive"));
			gui.StateChanged(idR.Game.Time);
		}
		#endregion
	}
}
