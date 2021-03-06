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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics.PackedVector;

using idTech4.Math;
using idTech4.Renderer;
using idTech4.Services;
using idTech4.UI.SWF.Scripting;

namespace idTech4.UI.SWF
{
	public class idSWF
	{
		#region Constants
		private const float AlphaEpsilon   = 0.001f;
		private const int StencilDecrement = -1;
		private const int StencilIncrement = -2;
		#endregion

		#region Properties
		public bool Crop
		{
			get
			{
				return _crop;
			}
			protected set
			{
				_crop = value;
			}
		}

		public idSWFScriptObject Globals
		{
			get
			{
				return _globals;
			}
		}

		public bool InhibitControl
		{
			get
			{
				return _inhibitControl;
			}
			set
			{
				_inhibitControl = value;
			}
		}
		
		public Random Random
		{
			get
			{
				return _random;
			}
		}

		public idSWFScriptObject RootObject
		{
			get
			{
				Debug.Assert(_mainSpriteInstance != null);

				return _mainSpriteInstance.ScriptObject;
			}
		}

		public idSWFScriptObject ShortcutKeys
		{
			get
			{
				return _shortcutKeys;
			}
		}

		public bool ShowBlackBars
		{
			get
			{
				return _blackbars;
			}
			protected set
			{
				_blackbars = value;
			}
		}

		public bool UseCircleForAccept
		{
			get
			{
				return false;
			}
		}

		public bool UseInhibitControl
		{
			get
			{
				return _useInhibitControl;
			}
			protected set
			{
				_useInhibitControl = value;
			}
		}
		#endregion

		#region Members
		// mouse coords for all flash files
		private static int _mouseX = -1;
		private static int _mouseY = -1;
		private static bool _isMouseInClientArea;

		private idSWFScriptObject _mouseObject;
		private idSWFScriptObject _hoverObject;
		
		private float _frameWidth;
		private float _frameHeight;
		private ushort _frameRate;
		private float _renderBorder;
		private float _swfScale;

		private Vector2	_scaleToVirtual;

		private long _lastRenderTime;

		private bool _isActive;
		private bool _inhibitControl;
		private bool _useInhibitControl;

		// certain screens need to be rendered when the pause menu is up so if this flag is
		// set on the gui we will allow it to render at a paused state;
		private bool _pausedRender;

		private bool _mouseEnabled;
		private bool _useMouse;

		private bool _blackbars;
		private bool _crop;
		private bool _paused;
		private bool _hasHitObject;

		private bool _forceNonPCPlatform;

		private idMaterial _atlasMaterial;
		private idMaterial _guiSolid;
		private idMaterial _guiCursorArrow;
		private idMaterial _guiCursorHand;
		private idMaterial _white;

		private Random _random;

		private idSWFSprite _mainSprite;
		private idSWFSpriteInstance _mainSpriteInstance;

		private idSWFScriptObject _globals;
		private idSWFScriptObject _shortcutKeys;

		private idSWFDictionaryEntry[] _dictionary;
		#endregion

		#region Constructor
		public idSWF()
		{			
			IDeclManager declManager = idEngine.Instance.GetService<IDeclManager>();
			ICVarSystem cvarSystem   = idEngine.Instance.GetService<ICVarSystem>();

			_atlasMaterial  = null;
			_swfScale       = 1.0f;
			_scaleToVirtual = new Vector2(1, 1);

			_random			= new Random();

			_guiSolid       = declManager.FindMaterial("guiSolid");
			_guiCursorArrow = declManager.FindMaterial("ui/assets/guicursor_arrow");
			_guiCursorHand  = declManager.FindMaterial("ui/assets/guicursor_hand");
			_white          = declManager.FindMaterial("_white");

			// TODO:
			/*tooltipButtonImage.Append( keyButtonImages_t( "<JOY1>", "guis/assets/hud/controller/xb360/a", "guis/assets/hud/controller/ps3/cross", 37, 37, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY2>", "guis/assets/hud/controller/xb360/b", "guis/assets/hud/controller/ps3/circle", 37, 37, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY3>", "guis/assets/hud/controller/xb360/x", "guis/assets/hud/controller/ps3/square", 37, 37, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY4>", "guis/assets/hud/controller/xb360/y", "guis/assets/hud/controller/ps3/triangle", 37, 37, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY_TRIGGER2>", "guis/assets/hud/controller/xb360/rt", "guis/assets/hud/controller/ps3/r2", 64, 52, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY_TRIGGER1>", "guis/assets/hud/controller/xb360/lt", "guis/assets/hud/controller/ps3/l2", 64, 52, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY5>", "guis/assets/hud/controller/xb360/lb", "guis/assets/hud/controller/ps3/l1", 52, 32, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY6>", "guis/assets/hud/controller/xb360/rb", "guis/assets/hud/controller/ps3/r1", 52, 32, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<MOUSE1>", "guis/assets/hud/controller/mouse1", "", 64, 52, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<MOUSE2>", "guis/assets/hud/controller/mouse2", "", 64, 52, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<MOUSE3>", "guis/assets/hud/controller/mouse3", "", 64, 52, 0 ) );
	 	
		   for ( int index = 0; index < tooltipButtonImage.Num(); index++ ) {
			   if ( ( tooltipButtonImage[index].xbImage != NULL ) && ( tooltipButtonImage[index].xbImage[0] != '\0' ) ) {
				   declManager->FindMaterial( tooltipButtonImage[index].xbImage );
			   }
			   if ( ( tooltipButtonImage[index].psImage != NULL ) && ( tooltipButtonImage[index].psImage[0] != '\0' ) ) {
				   declManager->FindMaterial( tooltipButtonImage[index].psImage );
			   }
		   }*/

			_useInhibitControl = true;
			_useMouse          = true;
											
			// TODO: soundWorld = soundWorld_;
		}

		// TODO: cleanup
		/*idSWF::~idSWF() {
			spriteInstanceAllocator.Free( mainspriteInstance );
			delete mainsprite;

			for ( int i = 0 ; i < dictionary.Num() ; i++ ) {
				if ( dictionary[i].sprite ) {
					delete dictionary[i].sprite;
					dictionary[i].sprite = NULL;
				}
				if ( dictionary[i].shape ) {
					delete dictionary[i].shape;
					dictionary[i].shape = NULL;
				}
				if ( dictionary[i].font ) {
					delete dictionary[i].font;
					dictionary[i].font = NULL;
				}
				if ( dictionary[i].text ) {
					delete dictionary[i].text;
					dictionary[i].text = NULL;
				}
				if ( dictionary[i].edittext ) {
					delete dictionary[i].edittext;
					dictionary[i].edittext = NULL;
				}
			}
	
			globals->Clear();
			tooltipButtonImage.Clear();
			globals->Release();

			shortcutKeys->Clear();
			shortcutKeys->Release();
		}*/
		#endregion

		#region Drawing
		public void Draw(IRenderSystem renderSystem, long time = 0, bool isSplitScreen = false)
		{
			if(this.IsActive == false)
			{
				return;
			}

			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			if(cvarSystem.GetInt("swf_stopat") > 0)
			{
				if(_mainSpriteInstance.CurrentFrame == cvarSystem.GetInt("swf_stopat"))
				{
					cvarSystem.Set("swf_timescale", 0.0f);
				}
			}

			long currentTime = idEngine.Instance.ElapsedTime;
			int framesToRun = 0;

			if(_paused == true)
			{
				_lastRenderTime = currentTime;
			}

			float swfTimeScale = cvarSystem.GetFloat("swf_timescale");

			if(swfTimeScale > 0.0f)
			{
				if(_lastRenderTime == 0)
				{
					_lastRenderTime = currentTime;
					framesToRun     = 1;
				}
				else
				{
					float deltaTime = (currentTime - _lastRenderTime);
					float fr        = ((float) _frameRate / 256.0f) * swfTimeScale;
					
					framesToRun      = (int) ((fr * deltaTime) / 1000.0f);
					_lastRenderTime += (long) (framesToRun * (1000.0f / fr));

					if(framesToRun > 10)
					{
						framesToRun = 10;
					}
				}

				for(int i = 0; i < framesToRun; i++) 
				{
					_mainSpriteInstance.Run();
					_mainSpriteInstance.RunActions();
				}
			}

			float pixelAspect = renderSystem.PixelAspect;
			float sysWidth    = renderSystem.Width * ((pixelAspect > 1.0f) ? pixelAspect : 1.0f);
			float sysHeight   = renderSystem.Height / ((pixelAspect < 1.0f) ? pixelAspect : 1.0f);
			float scale       = _swfScale * sysHeight / (float) _frameHeight;

			idSWFRenderState renderState = new idSWFRenderState();
			renderState.ColorXForm       = idSWFColorXForm.Default;
			renderState.StereoDepth      = _mainSpriteInstance.StereoDepth;
			renderState.Matrix.XX        = scale;
			renderState.Matrix.YY        = scale;
			renderState.Matrix.TX        = 0.5f * (sysWidth - (_frameWidth * scale));
			renderState.Matrix.TY        = 0.5f * (sysHeight - (_frameHeight * scale));

			_renderBorder = renderState.Matrix.TX / scale;

			_scaleToVirtual.X = (float) Constants.ScreenWidth / sysWidth;
			_scaleToVirtual.Y = (float) Constants.ScreenHeight / sysHeight;

			DrawSprite(renderSystem, _mainSpriteInstance, renderState, time, isSplitScreen);

			if(_blackbars == true)
			{
				float barWidth = renderState.Matrix.TX + 0.5f;
				float barHeight = renderState.Matrix.TY + 0.5f;

				if(barWidth > 0.0f)
				{
					renderSystem.Color = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

					DrawStretchPicture(0, 0, barWidth, sysHeight, 0, 0, 1, 1, _white);
					DrawStretchPicture(sysWidth - barWidth, 0, barWidth, sysHeight, 0, 0, 1, 1, _white);
				}

				if(barHeight > 0.0f)
				{
					renderSystem.Color = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

					DrawStretchPicture(0, 0, sysWidth, barHeight, 0, 0, 1, 1, _white);
					DrawStretchPicture(0, sysHeight - barHeight, sysWidth, barHeight, 0, 0, 1, 1, _white);
				}
			}

			if((_isMouseInClientArea == true) 
				&& ((_mouseEnabled == true) && (_useMouse == true)) 
				&& ((this.InhibitControl == true) || ((this.InhibitControl == false) && (_useInhibitControl == false))))
			{
				renderSystem.SetRenderState((ulong) (MaterialStates.SourceBlendSourceAlpha | MaterialStates.DestinationBlendOneMinusSourceAlpha));
				renderSystem.Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

				Vector2 mouse = renderState.Matrix.Transform(new Vector2(_mouseX - 1, _mouseY - 2));
				//idSWFScriptObject * hitObject = HitTest( mainspriteInstance, swfRenderState_t(), mouseX, mouseY, NULL );

				if(_hasHitObject == false) //hitObject == NULL ) {
				{ 
					DrawStretchPicture(mouse.X, mouse.Y, 32.0f, 32.0f, 0, 0, 1, 1, _guiCursorArrow);
				} 
				else
				{
					DrawStretchPicture(mouse.X, mouse.Y, 32.0f, 32.0f, 0, 0, 1, 1, _guiCursorHand);
				}
			}

			// restore the GL State
			renderSystem.SetRenderState(0);
		}

		private void DrawSprite(IRenderSystem renderSystem, idSWFSpriteInstance spriteInstance, idSWFRenderState renderState, long time, bool isSplitScreen)
		{
			if(spriteInstance == null)
			{
				idLog.Warning("RenderSprite: spriteInstance == null");
				return;
			}

			if(spriteInstance.IsVisible == false)
			{
				return;
			}
						
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			if(((renderState.ColorXForm.Mul.W + renderState.ColorXForm.Add.W) <= AlphaEpsilon) && (cvarSystem.GetFloat("swf_forceAlpha") <= 0.0f))
			{
				return;
			}

			List<idSWFDisplayEntry> activeMasks = new List<idSWFDisplayEntry>();
			
			for(int i = 0; i < spriteInstance.DisplayList.Count; i++)
			{
				idSWFDisplayEntry display = spriteInstance.DisplayList[i];

				for(int j = 0; j < activeMasks.Count; j++)
				{
					idSWFDisplayEntry mask = activeMasks[j];

					if(display.Depth > mask.ClipDepth)
					{
						DrawMask(renderSystem, mask, renderState, StencilDecrement);
						activeMasks.RemoveAt(j);
					}
				}

				if(display.ClipDepth > 0)
				{
					activeMasks.Add(display);
					DrawMask(renderSystem, display, renderState, StencilIncrement);
					continue;
				}

				idSWFDictionaryEntry entry = FindDictionaryEntry(display.CharacterID);

				if(entry == null)
				{
					continue;
				}

				idSWFRenderState renderState2 = new idSWFRenderState();
				renderState2.ColorXForm       = idSWFColorXForm.Default;

				if(spriteInstance.StereoDepth != StereoDepthType.None)
				{
					renderState2.StereoDepth = spriteInstance.StereoDepth;
				}
				else if(renderState.StereoDepth != StereoDepthType.None)
				{
					renderState2.StereoDepth = renderState.StereoDepth;
				}

				renderState2.Matrix     = display.Matrix.Multiply(renderState.Matrix);
				renderState2.ColorXForm = display.ColorXForm.Multiply(renderState.ColorXForm);
				renderState2.Ratio      = display.Ratio;

				if(display.BlendMode != 0)
				{
					renderState2.BlendMode = (byte) display.BlendMode;
				}
				else
				{
					renderState2.BlendMode = renderState.BlendMode;
				}

				renderState2.ActiveMasks = renderState.ActiveMasks + activeMasks.Count;

				if(spriteInstance.MaterialOverride != null)
				{
					renderState2.Material       = spriteInstance.MaterialOverride;
					renderState2.MaterialWidth  = spriteInstance.MaterialWidth;
					renderState2.MaterialHeight = spriteInstance.MaterialHeight;
				}
				else
				{
					renderState2.Material       = renderState.Material;
					renderState2.MaterialWidth  = renderState.MaterialWidth;
					renderState2.MaterialHeight = renderState.MaterialHeight;
				}

				float xOffset = 0.0f;
				float yOffset = 0.0f;

				if(entry is idSWFSprite)
				{
					display.SpriteInstance.SetAlignment(spriteInstance.OffsetX, spriteInstance.OffsetY);

					if(display.SpriteInstance.Name.StartsWith("_") == true)
					{
						//if ( display.spriteInstance->name.Icmp( "_leftAlign" ) == 0 ) {
						//	float adj = (float)frameWidth  * 0.10;
						//	renderState2.matrix.tx = ( display.matrix.tx - adj ) * renderState.matrix.xx;
						//}
						//if ( display.spriteInstance->name.Icmp( "_rightAlign" ) == 0 ) {
						//	renderState2.matrix.tx = ( (float)renderSystem->GetWidth() - ( ( (float)frameWidth - display.matrix.tx - adj ) * renderState.matrix.xx ) );
						//}
						float titleSafe = cvarSystem.GetFloat("swf_titleSafe");
						float widthAdj  = titleSafe * _frameWidth;
						float heightAdj = titleSafe * _frameHeight;

						float pixelAspect = renderSystem.PixelAspect;
						float sysWidth    = renderSystem.Width * ((pixelAspect > 1.0f) ? pixelAspect : 1.0f);
						float sysHeight   = renderSystem.Height / ((pixelAspect < 1.0f) ? pixelAspect : 1.0f);

						if(display.SpriteInstance.Name.Equals("_fullScreen", StringComparison.OrdinalIgnoreCase) == true)
						{
							float xScale = sysWidth / (float) _frameWidth;
							float yScale = sysHeight / (float) _frameHeight;

							renderState2.Matrix.TX = display.Matrix.TX * renderState.Matrix.XX;
							renderState2.Matrix.TY = display.Matrix.TY * renderState.Matrix.YY;
							renderState2.Matrix.XX = xScale;
							renderState2.Matrix.YY = yScale;
						}

						if(display.SpriteInstance.Name.Equals("_absTop", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TY = display.Matrix.TY * renderState2.Matrix.YY;
							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_top", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TY = (display.Matrix.TY + heightAdj) * renderState.Matrix.YY;
							display.SpriteInstance.SetAlignment(spriteInstance.OffsetY + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_topLeft", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TX = (display.Matrix.TX + widthAdj) * renderState.Matrix.XX;
							renderState2.Matrix.TY = (display.Matrix.TY + heightAdj) * renderState.Matrix.YY;

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_left") == true)
						{
							float prevX = renderState2.Matrix.TX;
							renderState2.Matrix.TX = (display.Matrix.TX + widthAdj) * renderState.Matrix.XX;
							xOffset = ((renderState2.Matrix.TX - prevX) / renderState.Matrix.XX);

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.ToLower().Contains("_absleft") == true)
						{
							float prevX = renderState2.Matrix.TX;
							renderState2.Matrix.TX = display.Matrix.TX * renderState.Matrix.XX;
							xOffset = ((renderState2.Matrix.TX - prevX) / renderState.Matrix.XX);

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.ToLower().Contains("_bottomleft") == true)
						{
							float prevX = renderState2.Matrix.TX;
							renderState2.Matrix.TX = (display.Matrix.TX + widthAdj) * renderState.Matrix.XX;
							xOffset = ((renderState2.Matrix.TX - prevX) / renderState.Matrix.XX);

							float prevY = renderState2.Matrix.TY;
							renderState2.Matrix.TY = ((float) sysHeight - (((float) _frameHeight - display.Matrix.TY + heightAdj) * renderState.Matrix.YY));
							yOffset = ((renderState2.Matrix.TY - prevY) / renderState.Matrix.YY);

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_absBottom", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TY = ((float) sysHeight - (((float) _frameHeight - display.Matrix.TY) * renderState.Matrix.YY));
							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_bottom", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TY = ((float) sysHeight - (((float) _frameHeight - display.Matrix.TY + heightAdj) * renderState.Matrix.YY));
							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_topRight", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TX = ((float) sysWidth - (((float) _frameWidth - display.Matrix.TX + widthAdj) * renderState.Matrix.XX));
							renderState2.Matrix.TY = (display.Matrix.TY + heightAdj) * renderState.Matrix.YY;

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_right", StringComparison.OrdinalIgnoreCase) == true)
						{
							float prevX = renderState2.Matrix.TX;
							renderState2.Matrix.TX = ((float) sysWidth - (((float) _frameWidth - display.Matrix.TX + widthAdj) * renderState.Matrix.XX));
							xOffset = ((renderState2.Matrix.TX - prevX) / renderState.Matrix.XX);

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.ToLower().Contains("_absright") == true)
						{
							float prevX = renderState2.Matrix.TX;
							renderState2.Matrix.TX = ((float) sysWidth - (((float) _frameWidth - display.Matrix.TX) * renderState.Matrix.XX));
							xOffset = ((renderState2.Matrix.TX - prevX) / renderState.Matrix.XX);

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_bottomRight", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TX = ((float) sysWidth - (((float) _frameWidth - display.Matrix.TX + widthAdj) * renderState.Matrix.XX));
							renderState2.Matrix.TY = ((float) sysHeight - (((float) _frameHeight - display.Matrix.TY + heightAdj) * renderState.Matrix.YY));

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						// ABSOLUTE CORNERS OF SCREEN
						else if(display.SpriteInstance.Name.Equals("_absTopLeft", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TX = display.Matrix.TX * renderState.Matrix.XX;
							renderState2.Matrix.TY = display.Matrix.TY * renderState.Matrix.YY;

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_absTopRight", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TX = ((float) sysWidth - (((float) _frameWidth - display.Matrix.TX) * renderState.Matrix.XX));
							renderState2.Matrix.TY = display.Matrix.TY * renderState.Matrix.YY;

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_absBottomLeft", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TX = display.Matrix.TX * renderState.Matrix.XX;
							renderState2.Matrix.TY = ((float) sysHeight - (((float) _frameHeight - display.Matrix.TY) * renderState.Matrix.YY));

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_absBottomRight", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TX = ((float) sysWidth - (((float) _frameWidth - display.Matrix.TX) * renderState.Matrix.XX));
							renderState2.Matrix.TY = ((float) sysHeight - (((float) _frameHeight - display.Matrix.TY) * renderState.Matrix.YY));

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
					}

					DrawSprite(renderSystem, display.SpriteInstance, renderState2, time, isSplitScreen);
				}
				else if(entry is idSWFMorphShape)
				{
					idLog.Warning("TODO: RenderMorphShape(renderSystem, (idSWFShape) entry, renderState2);");
				}
				else if(entry is idSWFShape)
				{
					DrawShape(renderSystem, (idSWFShape) entry, renderState2);
				}
				else if(entry is idSWFEditText)
				{
					DrawEditText(renderSystem, display.TextInstance, renderState2, time, isSplitScreen);
				}
				else
				{
					//idLib::Warning( "%s: Tried to render an unrenderable character %d", filename.c_str(), entry->type );
				}
			}

			for(int j = 0; j < activeMasks.Count; j++)
			{
				DrawMask(renderSystem, activeMasks[j], renderState, StencilDecrement);
			}
		}

		private void DrawEditText(IRenderSystem renderSystem, idSWFTextInstance textInstance, idSWFRenderState renderState, long time, bool isSplitScreen)
		{
			if(textInstance == null)
			{
				idLog.Warning("RenderEditText: textInstance == null");
				return;
			}

			if(textInstance.IsVisible == false)
			{
				return;
			}

			ILocalization localization = idEngine.Instance.GetService<ILocalization>();
			ICVarSystem cvarSystem     = idEngine.Instance.GetService<ICVarSystem>();

			idSWFEditText shape = textInstance.EditText;

			string text = string.Empty;

			if(textInstance.Variable == string.Empty)
			{
				if(textInstance.RenderMode == TextRenderMode.Paragraph)
				{
					idLog.Warning("TODO: text render paragraph");
			
					/*if ( textInstance->NeedsGenerateRandomText() ) {
						textInstance->StartParagraphText( Sys_Milliseconds() );
					}
					text = textInstance->GetParagraphText( Sys_Milliseconds() );*/
				} 
				else if((textInstance.RenderMode == TextRenderMode.RandomAppear) || (textInstance.RenderMode == TextRenderMode.RandomAppearCapitals))
				{
					idLog.Warning("TODO: text render random");

					/*if ( textInstance->NeedsGenerateRandomText() ) {
						textInstance->StartRandomText( Sys_Milliseconds() );
					}
					text = textInstance->GetRandomText( Sys_Milliseconds() );*/
				} 
				else 
				{
					text = localization.Get(textInstance.Text);
				}
			} 
			else 
			{
				idSWFScriptVariable var = _globals.Get(textInstance.Variable);

				if(var.IsUndefined == true)
				{
					text = localization.Get(textInstance.Text);
				}
				else
				{
					text = localization.Get(var.ToString());
				}
			}

			if(text.Length == 0)
			{
				textInstance.SelectionStart = -1;
				textInstance.SelectionEnd   = -1;				
			}

			// TODO: play sound
			/*if ( textInstance->NeedsSoundPlayed() ) {
				PlaySound( textInstance->GetSoundClip() );
				textInstance->ClearPlaySound();
			}*/	

			// TODO: tooltips
			/*if ( textInstance->tooltip ) {
				FindTooltipIcons( &text );
			} else {
				tooltipIconList.Clear();
			}*/

			int selectionStart = textInstance.SelectionStart;
			int selectionEnd   = textInstance.SelectionEnd;
			int cursorPosition = selectionEnd;

			bool inputField = false;
			bool drawCursor = false;

			idSWFScriptVariable focusWindow = _globals.Get("focusWindow");

			if((focusWindow.IsObject == true) && (focusWindow.Object == textInstance.ScriptObject))
			{
				inputField = true;
			}

			if((inputField == true) && (((idEngine.Instance.FrameNumber >> 4) & 1) == 0))
			{
				cursorPosition = selectionEnd;
				drawCursor     = true;
			}

			if(selectionStart > selectionEnd)
			{
				int tmp        = selectionEnd;
				selectionEnd   = selectionStart;
				selectionStart = tmp;
			}

			Vector2 xScaleVector = renderState.Matrix.Scale(new Vector2(1, 0));
			Vector2 yScaleVector = renderState.Matrix.Scale(new Vector2(0, 1));

			float xScale = xScaleVector.Length();
			float yScale = yScaleVector.Length();

			if(isSplitScreen == true)
			{
				yScaleVector *= 0.5f;
			}

			float invXScale = 1.0f / xScale;
			float invYScale = 1.0f / yScale;

			idSWFMatrix matrix = renderState.Matrix;
			matrix.XX *= invXScale;
			matrix.XY *= invXScale;
			matrix.YY *= invYScale;
			matrix.YX *= invYScale;

			idSWFFont fontEntry = FindDictionaryEntry(shape.FontID, typeof(idSWFFont)) as idSWFFont;

			if(fontEntry == null)
			{
				idLog.Warning("idSWF::DrawEditText: NULL font");
				return;
			}

			idFont fontInfo = fontEntry.Font;

			float postTransformHeight = idSWFHelper.Twip(shape.FontHeight) * yScale;
			float glyphScale          = postTransformHeight / 48.0f;
			float imageScale          = postTransformHeight / 24.0f;
	
			textInstance.GlyphScale   = glyphScale;

			Vector4 defaultColor = textInstance.Color.ToVector4();
			defaultColor         = Vector4.Multiply(defaultColor, renderState.ColorXForm.Mul) + renderState.ColorXForm.Add;

			if(cvarSystem.GetFloat("swf_forceAlpha") > 0.0f)
			{
				defaultColor.W = cvarSystem.GetFloat("swf_forceAlpha");
			}

			if(defaultColor.W <= AlphaEpsilon) 
			{
				return;
			}

			Vector4 selectionColor = defaultColor;
			selectionColor.W      *= 0.5f;

			renderSystem.Color = defaultColor;
			renderSystem.SetRenderState(StateForRenderState(renderState));

			idSWFRect bounds     = new idSWFRect();
			bounds.TopLeft.X     = xScale * (shape.Bounds.TopLeft.X + idSWFHelper.Twip(shape.LeftMargin));
			bounds.BottomRight.X = xScale * (shape.Bounds.BottomRight.X - idSWFHelper.Twip(shape.RightMargin));

			float lineSpacing = fontInfo.GetAscender(1.15f * glyphScale);
	
			if(shape.Leading != 0) 
			{
				lineSpacing += idSWFHelper.Twip(shape.Leading);
			}

			bounds.TopLeft.Y     = yScale * (shape.Bounds.TopLeft.Y + (1.15f * glyphScale));
			bounds.BottomRight.Y = yScale * (shape.Bounds.BottomRight.Y);

			textInstance.LineSpacing = lineSpacing;
			textInstance.Bounds      = bounds;
				
			if((shape.Flags & EditTextFlags.AutoSize) == EditTextFlags.AutoSize)
			{
				bounds.BottomRight.X = _frameWidth;
				bounds.BottomRight.Y = _frameHeight;
			}
	
			if((drawCursor == true) && (cursorPosition <= 0))
			{
				idLog.Warning("TODO: cursor position");
	
				/*float yPos = 0.0f;
				scaledGlyphInfo_t glyph;
				fontInfo->GetScaledGlyph( glyphScale, ' ', glyph );
				yPos = glyph.height / 2.0f;
				DrawEditCursor( gui, bounds.tl.x, yPos, 1.0f, linespacing, matrix );*/
			}

			if(textInstance.IsSubtitle == true)
			{
				if((text == string.Empty) && (textInstance.SubtitleText == string.Empty))
				{
					return;
				}
			}
			else if(text == string.Empty)
			{
				return;
			}

			float x = bounds.TopLeft.X;
			float y = bounds.TopLeft.Y;

			int maxLines = (int) ((bounds.BottomRight.Y - bounds.TopLeft.Y) / lineSpacing);

			if(maxLines == 0) 
			{
				maxLines = 1;
			}

			textInstance.MaxLines = maxLines;

			List<string> textLines = new List<string>();
			textLines.Add(string.Empty);

			int currentLine = 0;

			// tracks the last breakable character we found
			int lastBreak    = 0;
			float lastBreakX = 0;

			bool insertingImage = false;
			int iconIndex       = 0;

			int charIndex = 0;

			if(textInstance.IsSubtitle == true) 
			{
				charIndex = textInstance.SubtitleStartIndex;
			}

			while(charIndex < text.Length)
			{
				if(text[charIndex] == '\n')
				{
					if((shape.Flags & EditTextFlags.MultiLine) == EditTextFlags.MultiLine)
					{
						textLines[currentLine] += '\n';
						x = bounds.TopLeft.X;
						y += lineSpacing;

						textLines.Add(string.Empty);
						currentLine++;
						lastBreak = 0;
						charIndex++;
				
						continue;
					} 
					else 
					{
						break;
					}
				}
		
				int glyphStart = charIndex;
				char character = text[charIndex++];

				ScaledGlyph glyph = fontInfo.GetScaledGlyph(glyphScale, character);
				float glyphSkip = glyph.SkipX;

				if(textInstance.HasStroke == true) 
				{
					glyphSkip += (cvarSystem.GetFloat("swf_textStrokeSizeGlyphSpacer") * textInstance.StrokeWeight * glyphScale);
				}

				// TODO: tooltips
				/*tooltipIcon_t iconCheck;
		
				if ( iconIndex < tooltipIconList.Num() ) {
					iconCheck = tooltipIconList[iconIndex];
				}*/

				float imageSkip = 0.0f;

				/*if ( charIndex - 1 == iconCheck.startIndex ) {
					insertingImage = true;
					imageSkip = iconCheck.imageWidth * imageScale;
				} else if ( charIndex - 1 == iconCheck.endIndex ) {
					insertingImage = false;
					iconIndex++;
					glyphSkip = 0.0f;
				}*/

				if(insertingImage == true)
				{
					glyphSkip = 0.0f;
				}

				 // only break lines of text when we are not inputting data
				if(inputField == false)
				{
					if(((x + glyphSkip) > bounds.BottomRight.X) || ((x + imageSkip) > bounds.BottomRight.X)) 
					{
						if((shape.Flags & (EditTextFlags.MultiLine | EditTextFlags.WordWrap)) != 0)
						{
							if(lastBreak > 0) 
							{
								int curLineIndex = currentLine;
								currentLine++;
								textLines.Add(string.Empty);

								if(maxLines == 1) 
								{
									textLines[curLineIndex] = textLines[curLineIndex].Substring(0, textLines[curLineIndex].Length - 3);
									textLines[curLineIndex] += "...";

									break;
								} 
								else
								{
									textLines[curLineIndex] = textLines[curLineIndex].Substring(0, lastBreak);

									x -= lastBreakX;
								}
							} 
							else 
							{
								textLines.Add(string.Empty);

								currentLine++;
								x = bounds.TopLeft.X;
							}

							lastBreak = 0;
						} 
						else 
						{
							break;
						}
					}
				}

				while((glyphStart < charIndex) && (glyphStart < text.Length)) 
				{
					textLines[currentLine] += text[ glyphStart++];
				}

				x += glyphSkip + imageSkip;

				if((character == ' ') || (character == '-')) 
				{
					lastBreak  = textLines[currentLine].Length;
					lastBreakX = x;
				}
			}

			// subtitle functionality
			if((textInstance.IsSubtitle == true) && (textInstance.IsUpdatingSubtitle == true))
			{
				idLog.Warning("TODO: subtitle functionality");
	
				/*if ( textLines.Num() > 0 && textInstance->SubNeedsSwitch() ) {

					int lastWordIndex = textInstance->GetApporoximateSubtitleBreak( time );	
					int newEndChar = textInstance->GetSubStartIndex() + textLines[0].Length();

					int wordCount = 0;
					bool earlyOut = false;
					for ( int index = 0; index < textLines[0].Length(); ++index ) {
						if ( textLines[0][index] == ' ' || textLines[0][index] == '-' ) {
							if ( index != 0 ) {
								if ( wordCount == lastWordIndex ) {
									newEndChar = textInstance->GetSubStartIndex() + index;
									earlyOut = true;
									break;
								}

								// cover the double space at the beginning of sentences
								if ( index > 0 && textLines[0][index - 1 ] != ' ' ) {
									wordCount++;
								}
							}
						} else if ( index == textLines[0].Length() ) {
							if ( wordCount == lastWordIndex ) {
								newEndChar = textInstance->GetSubStartIndex() + index;
								earlyOut = true;
								break;
							}
							wordCount++;
						}
					}

					if ( wordCount <= 0 && textLines[0].Length() > 0 ) {
						wordCount = 1;
					}
			
					if ( !earlyOut ) {
						textInstance->LastWordChanged( wordCount, time );
					}

					textInstance->SetSubEndIndex( newEndChar, time );
				
					idStr subText = textLines[0].Left( newEndChar - textInstance->GetSubStartIndex() );
					idSWFParmList parms;
					parms.Append( subText );
					parms.Append( textInstance->GetSpeaker().c_str() );
					parms.Append( textInstance->GetSubAlignment() );
					Invoke( "subtitleChanged", parms );
					parms.Clear();

					textInstance->SetSubNextStartIndex( textInstance->GetSubEndIndex() );
					textInstance->SwitchSubtitleText( time );
				}

				if ( !textInstance->UpdateSubtitle( time ) ) {			
					textInstance->SubtitleComplete();
					idSWFParmList parms;
					parms.Append( textInstance->GetSubAlignment() );
					Invoke( "subtitleComplete", parms );
					parms.Clear();
					textInstance->SubtitleCleanup();
				}*/
			}

			//*************************************************
			// CALCULATE THE NUMBER OF SCROLLS LINES LEFT
			//*************************************************

			idLog.Warning("TODO: scroll lines");

			/*textInstance->CalcMaxScroll( textLines.Num() - maxLines );*/

			int c = 1;
			int textLine = 0; //textInstance->scroll;	

			/*if ( textLine + maxLines > textLines.Num() && maxLines < textLines.Num() ) {
				textLine = textLines.Num() - maxLines;
				textInstance->scroll = textLine;
			} else if ( textLine < 0 || textLines.Num() <= maxLines ) {
				textLine = 0;
				textInstance->scroll = textLine;
			} else if ( textInstance->renderMode == SWF_TEXT_RENDER_AUTOSCROLL ) {
				textLine = textLines.Num() - maxLines;
				textInstance->scroll = textInstance->maxscroll;
			}*/

			// END SCROLL CALCULATION
			//*************************************************

			int index = 0;

			int startCharacter      = 0;
			int endCharacter        = 0;
			int inputEndChar        = 0;
			int overallIndex        = 0;
			int curIcon             = 0;
			float yPrevBottomOffset = 0.0f;
			float yOffset           = 0;

			int[] strokeXOffsets    = { -1, 1, -1, 1 };
			int[] strokeYOffsets    = { -1, -1, 1, 1 };

			iconIndex               = 0;
			
			string inputText;

			if(inputField == true)
			{
				idLog.Warning("TODO: input field");

				/*if ( textLines.Num() > 0 ) {
					idStr & text = textLines[0];
					float left = bounds.tl.x;

					int startCheckIndex = textInstance->GetInputStartChar();

					if ( startCheckIndex >= text.Length() ) {
						startCheckIndex = 0;
					}

					if ( cursorPos < startCheckIndex && cursorPos >= 0 ) {
						startCheckIndex = cursorPos;
					}

					bool endFound = false;
					int c = startCheckIndex;
					while ( c < text.Length() ) {
						uint32 tc = text.UTF8Char( c++ );
						scaledGlyphInfo_t glyph;
						fontInfo->GetScaledGlyph( glyphScale, tc, glyph );
						float glyphSkip = glyph.xSkip;
						if ( textInstance->HasStroke() ) {
							glyphSkip += ( swf_textStrokeSizeGlyphSpacer.GetFloat() * textInstance->GetStrokeWeight() * glyphScale );
						}

						if ( left + glyphSkip > bounds.br.x ) {
							if ( cursorPos > c && cursorPos != endCharacter ) {

								float removeSize = 0.0f;

								while ( removeSize < glyphSkip ) {
									if ( endCharacter == c ) {
										break;
									}
									scaledGlyphInfo_t removeGlyph;
									fontInfo->GetScaledGlyph( glyphScale, inputText[ endCharacter++ ], removeGlyph );
									removeSize += removeGlyph.xSkip;
								}

								left -= removeSize;
							} else {
								inputEndChar = c;
								endFound = true;
								break;
							}
						} 
						inputText.AppendUTF8Char( tc++ );
						left += glyphSkip;
					}

					if ( !endFound ) {
						inputEndChar = text.Length();
					}

					startCheckIndex += endCharacter;
					textInstance->SetInputStartCharacter( startCheckIndex );
					endCharacter = startCheckIndex;
				}*/
			}
	
			for(int t = 0; t < textLines.Count; t++)
			{
				if((textInstance.IsSubtitle == true) && (t > 0))
				{
					break;
				}
		
				if(t < textLine)
				{
					text = textLines[t];
					c    += text.Length;
			
					startCharacter = endCharacter;
					endCharacter   = startCharacter + text.Length;
					overallIndex   += text.Length;

					idLog.Warning("TODO: tooltip icons");

					// find the right icon index if we scrolled passed the previous ones
					/*for ( int iconChar = curIcon; iconChar < tooltipIconList.Num(); ++iconChar ) {
						if ( endCharacter > tooltipIconList[iconChar].startIndex ) {
							curIcon++;
						} else {
							break;
						}
					}*/

					continue;
				}

				if(index == maxLines) 
				{
					break;
				}

				startCharacter = endCharacter;
				text           = textLines[textLine];
				int lastChar   = text.Length;

				if(textInstance.IsSubtitle == true) 
				{
					lastChar = textInstance.SubtitleEndIndex;
				}

				textLine++;

				if(inputField == true) 
				{
					if(inputEndChar == 0) 
					{
						inputEndChar += 1;
					}

					selectionStart -= startCharacter;
					selectionEnd   -= startCharacter;
					cursorPosition -= startCharacter;
					endCharacter    = inputEndChar;
					lastChar        = endCharacter;
					text            = text.Substring(startCharacter, endCharacter - startCharacter);
				} 
				else 
				{
					if(lastChar == 0) 
					{
						// blank line so add space char
						endCharacter = startCharacter + 1;
					} 
					else 
					{
						endCharacter = startCharacter + lastChar;
					}
				}

				float width    = 0.0f;
				insertingImage = false;
				int i          = 0;

				while(i < lastChar) 
				{
					// TODO: tooltips
					/*if ( curIcon < tooltipIconList.Num() && tooltipIconList[curIcon].startIndex == startCharacter + i ) {
						width += tooltipIconList[curIcon].imageWidth * imageScale;
						i += tooltipIconList[curIcon].endIndex - tooltipIconList[curIcon].startIndex - 1;
						curIcon++;
					} else*/
					{
						if(i < text.Length) 
						{
							ScaledGlyph glyph = fontInfo.GetScaledGlyph(glyphScale, text[i++]);
							width            += glyph.SkipX;

							if(textInstance.HasStroke == true) 
							{
								width += (cvarSystem.GetFloat("swf_textStrokeSizeGlyphSpacer") * textInstance.StrokeWeight * glyphScale);
							}
						} 
						else 
						{
							i++;
						}
					}
				}

				y = bounds.TopLeft.Y + (index * lineSpacing);

				float biggestGlyphHeight = 0.0f;		

				/*for ( int image = 0; image < tooltipIconList.Num(); ++image ) {
					if ( tooltipIconList[image].startIndex >= startCharacter && tooltipIconList[image].endIndex < endCharacter ) {
						biggestGlyphHeight = tooltipIconList[image].imageHeight > biggestGlyphHeight ? tooltipIconList[image].imageHeight : biggestGlyphHeight;
					}
				}*/

				float yBottomOffset = 0.0f;
				float yTopOffset    = 0.0f;

				if(biggestGlyphHeight > 0.0f) 
				{			
					float topSpace    = 0.0f;
					float bottomSpace = 0.0f;
					int idx           = 0;

					ScaledGlyph glyph = fontInfo.GetScaledGlyph(glyphScale, text[idx++]);
					topSpace          = ((biggestGlyphHeight * imageScale) - glyph.Height) / 2.0f;
					bottomSpace       = topSpace;

					if((topSpace > 0.0f) && (t != 0)) 
					{
						yTopOffset += topSpace;
					}

					if(bottomSpace > 0.0f) 
					{
						yBottomOffset += bottomSpace;
					}
				} 
				else 
				{
					yBottomOffset = 0.0f;
				}

				if(t != 0) 
				{
					if((yPrevBottomOffset > 0) || (yTopOffset > 0)) 
					{
						yOffset += (yTopOffset > yPrevBottomOffset) ? yTopOffset : yPrevBottomOffset;
					}
				}

				y                += yOffset;		
				yPrevBottomOffset = yBottomOffset;		

				float extraSpace = 0.0f;

				switch(shape.Align)
				{
					case TextAlign.Left:
						x = bounds.TopLeft.X;
						break;

					case TextAlign.Right:
						x = bounds.BottomRight.X - width;
						break;

					case TextAlign.Center:
						x = (bounds.TopLeft.X + bounds.BottomRight.X - width) * 0.5f;
						break;

					case TextAlign.Justify:
						x = bounds.TopLeft.X;

						if((width > (bounds.BottomRight.X - bounds.TopLeft.X) * 0.5f) && (index < textLines.Count - 1)) 
						{
							extraSpace = ((bounds.BottomRight.X - bounds.TopLeft.X ) - width ) / ((float) lastChar - 1.0f);
						}
						break;
				}

				// TODO: tooltipIcon_t icon;
				insertingImage = false;

				// find the right icon index if we scrolled passed the previous ones
				// TODO: tooltips
				/*for ( int iconChar = iconIndex; iconChar < tooltipIconList.Num(); ++iconChar ) {
					if ( overallIndex > tooltipIconList[iconChar].startIndex ) {
						iconIndex++;
					} else {
						break;
					}
				}*/

				float baseLine       = y + fontInfo.GetAscender(glyphScale);
				int overallLineIndex = 0;
				Vector4 textColor    = defaultColor;
				i                    = 0;
				
				while(i < lastChar) 
				{		
					if(i >= text.Length) 
					{
						break;
					}

					// support colors
					if(textInstance.IgnoreColor == false) 
					{
						if(text[i] == (int) idColorIndex.Escape)
						{
							if(idColor.IsColor(text, i++) == true)
							{
								if(text[i] == (int) idColorIndex.Default)
								{
									i++;
									textColor = defaultColor;
								} 
								else 
								{
									textColor   = idColor.FromIndex(text[i++]);
									textColor.W = defaultColor.W;
								}
						
								continue;
							}
						}
					}

					char character = text[i++];

					if(character == '\n') 
					{
						c++;
				
						overallIndex    += i - overallLineIndex;
						overallLineIndex = i;
				
						continue;
					}

					// skip a single leading space
					if((character == ' ') && (i == 1))
					{
						c++;

						overallIndex    += i - overallLineIndex;
						overallLineIndex = i;
						
						continue;
					}

					// TODO: tooltips
					/*if ( iconIndex <  tooltipIconList.Num() ) {
						icon = tooltipIconList[iconIndex];
					}*/
			
					/*if(overallIndex == icon.startIndex) 
					{
						insertingImage = true;

						scaledGlyphInfo_t glyph;
						fontInfo->GetScaledGlyph( glyphScale, character, glyph );

						float imageHeight = icon.imageHeight * imageScale;
						float glyphHeight = glyph.height;

						float imageY = 0.0f;
						if ( icon.baseline == 0 ) {
							imageY = baseLine - glyph.top;
							imageY += ( glyphHeight - imageHeight ) * 0.5f;
							imageY += 2.0f;
						} else {
							imageY = ( y + glyphHeight ) - ( ( icon.imageHeight * imageScale ) - ( glyphHeight ) );	
						} 
				
						float imageX = x + glyph.left;
						float imageW = icon.imageWidth * imageScale;
						float imageH = icon.imageHeight * imageScale;

						idVec2 topl = matrix.Transform( idVec2( imageX, imageY ) );
						idVec2 topr = matrix.Transform( idVec2( imageX + imageW, imageY ) );
						idVec2 br = matrix.Transform( idVec2( imageX + imageW, imageY + imageH ) );
						idVec2 bl = matrix.Transform( idVec2( imageX, imageY + imageH ) );

						float s1 = 0.0f;
						float t1 = 0.0f;
						float s2 = 1.0f;
						float t2 = 1.0f;

						//uint32 color = gui->GetColor();
						idVec4 imgColor = colorWhite;
						imgColor.w = defaultColor.w;
						gui->SetColor( imgColor );
						DrawStretchPic( idVec4( topl.x, topl.y, s1, t1 ), idVec4( topr.x, topr.y, s2, t1 ), idVec4( br.x, br.y, s2, t2 ), idVec4( bl.x, bl.y, s1, t2 ), icon.material );
						gui->SetColor( defaultColor );

						x += icon.imageWidth * imageScale;
						x += extraSpace;
					
					} else if ( overallIndex == icon.endIndex ) {
						insertingImage = false;
						iconIndex++;	
					}*/

					if(insertingImage == true) 
					{
						overallIndex    += i - overallLineIndex;
						overallLineIndex = i;

						continue;
					}

					// the glyphs texcoords assume nearest filtering, to get proper
					// bilinear support we need to go an extra half texel on each side
					ScaledGlyph glyph = fontInfo.GetScaledGlyph(glyphScale, character);

					float glyphSkip = glyph.SkipX;

					if(textInstance.HasStroke == true) 
					{
						glyphSkip += (cvarSystem.GetFloat("swf_textStrokeSizeGlyphSpacer") * textInstance.StrokeWeight * glyphScale);
					}

					float glyphW = glyph.Width + 1.0f;	// +1 for bilinear half texel on each side
					float glyphH = glyph.Height + 1.0f;

					float glyphY = baseLine - glyph.Top;
					float glyphX = x + glyph.Left;

					Vector2 topLeft     = matrix.Transform(new Vector2(glyphX, glyphY));
					Vector2 topRight    = matrix.Transform(new Vector2(glyphX + glyphW, glyphY));
					Vector2 bottomRight = matrix.Transform(new Vector2(glyphX + glyphW, glyphY + glyphH));
					Vector2 bottomLeft  = matrix.Transform(new Vector2(glyphX, glyphY + glyphH));

					float s1 = glyph.S1;
					float t1 = glyph.T1;
					float s2 = glyph.S2;
					float t2 = glyph.T2;

					if((c > selectionStart) && (c <= selectionEnd))
					{
						Vector2 sTopLeft     = matrix.Transform(new Vector2(x, y));
						Vector2 sTopRight    = matrix.Transform(new Vector2(x + glyphSkip, y));
						Vector2 sBottomRight = matrix.Transform(new Vector2(x + glyphSkip, y + lineSpacing));
						Vector2 sBottomLeft  = matrix.Transform(new Vector2(x, y + lineSpacing));

						renderSystem.Color = selectionColor;
						DrawStretchPicture(new Vector4(sTopLeft.X, sTopLeft.Y, 0, 0), new Vector4(sTopRight.X, sTopRight.Y, 1, 0), new Vector4(sBottomRight.X, sBottomRight.Y, 1, 1), new Vector4(sBottomLeft.X, sBottomLeft.Y, 0, 1), _white);
						renderSystem.Color = textColor;
					}

					if(textInstance.HasDropShadow == true) 
					{			
						float dsY = glyphY + glyphScale * 2.0f;
						float dsX = glyphX + glyphScale * 2.0f;

						Vector2 dsTopLeft = matrix.Transform(new Vector2(dsX, dsY));
						Vector2 dsTopRight = matrix.Transform(new Vector2(dsX + glyphW, dsY));
						Vector2 dsBottomRight   = matrix.Transform(new Vector2(dsX + glyphW, dsY + glyphH));
						Vector2 dsBottomLeft   = matrix.Transform(new Vector2(dsX, dsY + glyphH));

						Vector4 dsColor = idColor.Black;
						dsColor.W       = defaultColor.W;

						renderSystem.Color = dsColor;
						DrawStretchPicture(new Vector4(dsTopLeft.X, dsTopLeft.Y, s1, t1), new Vector4(dsTopRight.X, dsTopRight.Y, s2, t1), new Vector4(dsBottomRight.X, dsBottomRight.Y, s2, t2), new Vector4(dsBottomLeft.X, dsBottomLeft.Y, s1, t2), glyph.Material);
						renderSystem.Color = textColor;
					} 
					else if(textInstance.HasStroke == true) 
					{
						Vector4 strokeColor = idColor.Black;
						strokeColor.W       = textInstance.StrokeStrength * defaultColor.W;

						renderSystem.Color = strokeColor;

						for(index = 0; index < 4; ++index) 
						{
							float xPos = glyphX + ((strokeXOffsets[index] * textInstance.StrokeWeight) * glyphScale);
							float yPos = glyphY + ((strokeYOffsets[index] * textInstance.StrokeWeight) * glyphScale);
							
							Vector2 sTopLeft     = matrix.Transform(new Vector2(xPos, yPos));
							Vector2 sTopRight    = matrix.Transform(new Vector2(xPos + glyphW, yPos));
							Vector2 sBottomRight = matrix.Transform(new Vector2(xPos + glyphW, yPos + glyphH));
							Vector2 sBottomLeft  = matrix.Transform(new Vector2(xPos, yPos + glyphH));

							DrawStretchPicture(new Vector4(sTopLeft.X, sTopLeft.Y, s1, t1), new Vector4(sTopRight.X, sTopRight.Y, s2, t1), new Vector4(sBottomRight.X, sBottomRight.Y, s2, t2), new Vector4(sBottomLeft.X, sBottomLeft.Y, s1, t2), glyph.Material);
						}

						renderSystem.Color = textColor;
					}

					DrawStretchPicture(new Vector4(topLeft.X, topLeft.Y, s1, t1), new Vector4(topRight.X, topRight.Y, s2, t1), new Vector4(bottomRight.X, bottomRight.Y, s2, t2), new Vector4(bottomLeft.X, bottomLeft.Y, s1, t2), glyph.Material);

					x += glyphSkip;
					x += extraSpace;

					if(cursorPosition == c) 
					{
						idLog.Warning("TODO: DrawEditCursor( gui, x - 1.0f, y, 1.0f, linespacing, matrix );");
					}

					c++;

					overallIndex    += i - overallLineIndex;
					overallLineIndex = i;
				}

				index++;
			}
		}

		private void DrawMask(IRenderSystem renderSystem, idSWFDisplayEntry mask, idSWFRenderState renderState, int stencilMode)
		{
			idSWFRenderState renderState2 = new idSWFRenderState();
			renderState2.StereoDepth      = renderState.StereoDepth;
			renderState2.Matrix           = mask.Matrix.Multiply(renderState.Matrix);
			renderState2.ColorXForm       = mask.ColorXForm.Multiply(renderState.ColorXForm);
			renderState2.Ratio            = mask.Ratio;
			renderState2.Material         = _guiSolid;
			renderState2.ActiveMasks      = stencilMode;

			idSWFDictionaryEntry entry = _dictionary[mask.CharacterID];

			if(entry is idSWFMorphShape)
			{
				idLog.Warning("TODO: RenderMorphShape( gui, entry.shape, renderState2 );");
			}
			else
			{
				DrawShape(renderSystem, (idSWFShape) entry, renderState2);
			}
		}

		private void DrawShape(IRenderSystem renderSystem, idSWFShape shape, idSWFRenderState renderState)
		{
			if(shape == null)
			{
				idLog.Warning("RenderShape: shape == null");
				return;
			}

			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			foreach(idSWFShapeDrawFill fill  in shape.Fills)
			{
				idMaterial material    = null;
				idSWFMatrix invMatrix  = idSWFMatrix.Default;
				idSWFColorXForm color  = idSWFColorXForm.Default;
				
				Vector2 atlasScale = Vector2.Zero;
				Vector2 atlasBias  = Vector2.Zero;
				bool useAtlas      = false;

				Vector2 size = new Vector2(1, 1);

				if(renderState.Material != null)
				{
					material = renderState.Material;
					invMatrix.XX = invMatrix.YY = (1.0f / 20.0f);
				}
				else if(fill.Style.Type == 0)
				{
					material  = _guiSolid;
					color.Mul = fill.Style.StartColor.ToVector4();
				} 
				else if((fill.Style.Type == 4) && (fill.Style.BitmapID != 65535)) 
				{
					// everything in a single image atlas
					idSWFImage entry  = _dictionary[fill.Style.BitmapID] as idSWFImage;
					material          = _atlasMaterial;
					Vector2 atlasSize = new Vector2(material.ImageWidth, material.ImageHeight);

					size = entry.ImageSize;

					atlasScale = size / atlasSize;
					atlasBias  = entry.ImageAtlasOffset / atlasSize;
					
					// de-normalize color channels after DXT decompression
					color.Mul = entry.ChannelScale;
					useAtlas  = true;
					invMatrix = fill.Style.StartMatrix.Inverse();
				} 
				else 
				{
					material = _guiSolid;
				}

				color = color.Multiply(renderState.ColorXForm);

				if(cvarSystem.GetFloat("swf_forceAlpha") > 0.0f)
				{
					color.Mul.W = cvarSystem.GetFloat("swf_forceAlpha");
					color.Add.W = 0.0f;
				}

				if((color.Mul.W + color.Add.W) <= AlphaEpsilon)
				{
					continue;
				}

				Vector4 colorMul   = color.Mul;
				Vector4 colorAdd   = color.Add;
				idSWFRect bounds = shape.StartBounds;

				if(renderState.MaterialWidth > 0)
				{
					size.X = renderState.MaterialWidth;
				}

				if(renderState.MaterialHeight > 0)
				{
					size.Y = renderState.MaterialHeight;
				}

				Vector2 oneOverSize = new Vector2(1.0f / size.X, 1.0f / size.Y);

				renderSystem.SetRenderState(StateForRenderState(renderState));

				idVertex[] verts = new idVertex[fill.StartVertices.Length];
				ushort[] indexes = fill.Indices;

				for(int j = 0; j < fill.StartVertices.Length; j++)
				{
					Vector2 xy = fill.StartVertices[j];
				
					verts[j].Clear();
					verts[j].Position = new Vector3(renderState.Matrix.Transform(xy) * _scaleToVirtual, 0);
					verts[j].Color    = colorMul;
					verts[j].Color2   = colorAdd;

					// for some reason I don't understand, having texcoords
					// in the range of 2000 or so causes what should be solid
					// fill areas to have horizontal bands on nvidia, but not 360.
					// forcing the texcoords to zero fixes it.
					if(fill.Style.Type != 0)
					{
						Vector2 st;
						
						// all the swf vertexes have an implicit scale of 1/20 for some reason...
						st.X = ((xy.X - bounds.TopLeft.X) * oneOverSize.X) * 20.0f;
						st.Y = ((xy.Y - bounds.TopLeft.Y) * oneOverSize.Y) * 20.0f;
						st = invMatrix.Transform(st);
						
						if(useAtlas == true) 
						{
							st = (st * atlasScale) + atlasBias;
						}

						// inset the tc - the gui may use a vmtr and the tc might end up
						// crossing page boundaries if using [0.0, 1.0]
						st.X = MathHelper.Clamp(st.X, 0.001f, 0.999f);
						st.Y = MathHelper.Clamp(st.Y, 0.001f, 0.999f);

						verts[j].TextureCoordinates = new HalfVector2(st);
					}
				}
				
				renderSystem.AddPrimitive(verts, indexes, material, renderState.StereoDepth);
			}
			
			for(int i = 0; i < shape.Lines.Length; i++)
			{
				idSWFShapeDrawLine line = shape.Lines[i];
				
				idSWFColorXForm color = new idSWFColorXForm();
				color.Mul             = line.Style.StartColor.ToVector4();
				color                 = color.Multiply(renderState.ColorXForm);

				if(cvarSystem.GetFloat("swf_forceAlpha") > 0.0f)
				{
					color.Mul.W = cvarSystem.GetFloat("swf_forceAlpha");
					color.Add.W = 0.0f;
				}

				if((color.Mul.W + color.Add.W) <= AlphaEpsilon)
				{
					continue;
				}
								
				renderSystem.SetRenderState(StateForRenderState(renderState) | (ulong) MaterialStates.PolygonLineMode);

				idVertex[] verts = new idVertex[line.StartVertices.Length];
				ushort[] indexes = line.Indices;

				for(int j = 0; j < line.StartVertices.Length; j++)
				{
					Vector2 xy = line.StartVertices[j];
				
					verts[j].Clear();
					verts[j].Position = new Vector3(renderState.Matrix.Transform(xy) * _scaleToVirtual, 0);
					verts[j].Color    = color.Mul;
					verts[j].Color2 =   new Vector4(color.Add.X * 0.5f + 0.5f, 
						color.Add.Y * 0.5f + 0.5f, 
						color.Add.Z * 0.5f + 0.5f, 
						color.Add.W * 0.5f + 0.5f);
					
					renderSystem.AddPrimitive(verts, indexes, _white, renderState.StereoDepth);
				}
			}
		}

		private void DrawStretchPicture(float x, float y, float width, float height, float s1, float t1, float s2, float t2, idMaterial material)
		{
			idEngine.Instance.GetService<IRenderSystem>().DrawStretchPicture(x * _scaleToVirtual.X, y * _scaleToVirtual.Y, width * _scaleToVirtual.X, height * _scaleToVirtual.Y, s1, t1, s2, t2, material);
		}

		private void DrawStretchPicture(Vector4 topLeft, Vector4 topRight, Vector4 bottomRight, Vector4 bottomLeft, idMaterial material)
		{
			idEngine.Instance.GetService<IRenderSystem>().DrawStretchPicture(	
				new Vector4(topLeft.X * _scaleToVirtual.X,     topLeft.Y * _scaleToVirtual.Y,     topLeft.Z,     topLeft.W),
				new Vector4(topRight.X * _scaleToVirtual.X,    topRight.Y * _scaleToVirtual.Y,    topRight.Z,    topRight.W),
				new Vector4(bottomRight.X * _scaleToVirtual.X, bottomRight.Y * _scaleToVirtual.Y, bottomRight.Z, bottomRight.W),
				new Vector4(bottomLeft.X * _scaleToVirtual.X,  bottomLeft.Y * _scaleToVirtual.Y,  bottomLeft.Z,  bottomLeft.W),
				material);
		}

		private ulong StateForRenderState(idSWFRenderState renderState)
		{
			MaterialStates extraState = MaterialStates.Override | MaterialStates.DepthFunctionLess | MaterialStates.DepthMask; // SWF State always overrides what's set in the material

			if(renderState.ActiveMasks > 0)
			{
				extraState |= MaterialStates.StencilFunctionEqual | idHelper.MakeStencilReference((ulong) (128 + renderState.ActiveMasks)) | idHelper.MakeStencilMask(255);
			} 
			else if(renderState.ActiveMasks == StencilIncrement)
			{
				return (ulong) (MaterialStates.ColorMask | MaterialStates.AlphaMask | MaterialStates.StencilOperationFailKeep | MaterialStates.StencilOperationZFailKeep | MaterialStates.StencilOperationPassIncrement);
			}
			else if(renderState.ActiveMasks == StencilDecrement)
			{
				return (ulong) (MaterialStates.ColorMask | MaterialStates.AlphaMask | MaterialStates.StencilOperationFailKeep | MaterialStates.StencilOperationZFailKeep | MaterialStates.StencilOperationPassDecrement);
			}

			switch(renderState.BlendMode)
			{
				case 7: // difference : dst = abs( dst - src )
				case 9: // subtract : dst = dst - src
					return (ulong) (extraState | (MaterialStates.SourceBlendOne | MaterialStates.DestinationBlendOne | MaterialStates.BlendOperationSubtract));
				case 8: // add : dst = dst + src
					return (ulong) (extraState | (MaterialStates.SourceBlendOne | MaterialStates.DestinationBlendOne));
				case 6: // darken : dst = min( dst, src )
					return (ulong) (extraState | (MaterialStates.SourceBlendOne | MaterialStates.DestinationBlendOne | MaterialStates.BlendOperationMin));
				case 5: // lighten : dst = max( dst, src )
					return (ulong) (extraState | (MaterialStates.SourceBlendOne | MaterialStates.DestinationBlendOne | MaterialStates.BlendOperationMax));
				case 4: // screen : dst = dst + src - dst*src ( we only do dst - dst * src, we could do the extra + src with another pass if we need to)
					return (ulong) (extraState | (MaterialStates.SourceBlendDestinationColor | MaterialStates.DestinationBlendOne | MaterialStates.BlendOperationSubtract));
				case 14: // hardlight : src < 0.5 ? multiply : screen
				case 13: // overlay : dst < 0.5 ? multiply  : screen
				case 3: // multiply : dst = ( dst * src ) + ( dst * (1-src.a) )
					return (ulong) (extraState | (MaterialStates.SourceBlendDestinationColor | MaterialStates.DestinationBlendOneMinusSourceAlpha));
				case 12: // erase
				case 11: // alpha
				case 10: // invert
				case 2: // layer
				case 1: // normal
				case 0: // normaler
				default:
					return (ulong) (extraState | (MaterialStates.SourceBlendOne | MaterialStates.DestinationBlendOneMinusSourceAlpha));
			}
		}
		#endregion

		#region Events
		public bool HandleEvent(SystemEvent ev)
		{
			if((this.IsActive == false) || (((_inhibitControl == false) && (_useInhibitControl == true)) == true))
			{
				return false;
			}

			IInputSystem inputSystem = idEngine.Instance.GetService<IInputSystem>();

			if(ev.Type == SystemEventType.Key)
			{
				return HandleKeyEvent(ev);
			}
			else if(ev.Type == SystemEventType.Char)
			{
				return HandleCharEvent(ev);
			}
			else if(ev.Type == SystemEventType.Mouse)
			{
				return HandleMouseEvent(ev);
			}
			else if(ev.Type == SystemEventType.MouseLeave)
			{
				_isMouseInClientArea = false;
			}
			else if(ev.Type == SystemEventType.Joystick)
			{
				idSWFParameterList parms = new idSWFParameterList();
				parms.Add(ev.Value);
				parms.Add(ev.Value2 / 32.0f);

				Invoke("onJoystick", parms);
			}
	
			return false;
		}

		private bool HandleKeyEvent(SystemEvent ev)
		{
			IInputSystem inputSystem = idEngine.Instance.GetService<IInputSystem>();
			Keys value               = (Keys) ev.Value;
			idSWFScriptVariable var  = new idSWFScriptVariable();

			if(value == Keys.Mouse1)
			{
				_mouseEnabled = true;

				idSWFParameterList parms;

				if(ev.Value2 != 0)
				{
					idSWFScriptVariable waitInput = _globals.Get("waitInput");

					if(waitInput.IsFunction == true)
					{
						_useMouse = false;

						idSWFParameterList waitParms = new idSWFParameterList();
						waitParms.Add(ev.Value);

						waitInput.Function.Invoke(null, waitParms);
						waitParms.Clear();
					}
					else
					{
						_useMouse = true;
					}

					idSWFScriptObject hitObject = HitTest(_mainSpriteInstance, new idSWFRenderState(), _mouseX, _mouseY, null);
				
					if(hitObject != null) 
					{
						_mouseObject = hitObject;
						var          = hitObject.Get("onPress");

						if(var.IsFunction == true)
						{
							parms = new idSWFParameterList();
							parms.Add(ev.DeviceNumber);

							var.Function.Invoke(hitObject, parms);
							parms.Clear();

							return true;
						}


						var = hitObject.Get("onDrag");

						if(var.IsFunction == true)
						{
							parms = new idSWFParameterList();
							parms.Add(_mouseX);
							parms.Add(_mouseY);
							parms.Add(true);

							var.Function.Invoke(hitObject, parms);

							parms.Clear();

							return true;
						}
					}

					parms = new idSWFParameterList();
					parms.Add(hitObject);

					Invoke("setHitObject", parms);
				} 
				else 
				{
					if(_mouseObject != null) 
					{
						var = _mouseObject.Get("onRelease");

						if(var.IsFunction == true)
						{
							parms = new idSWFParameterList();
							parms.Add(_mouseObject); // FIXME: Remove this

							var.Function.Invoke(_mouseObject, parms);
						}					

						_mouseObject = null;
					}

					if(_hoverObject != null)
					{
						_hoverObject = null;
					}

					if(var.IsFunction == true)
					{
						return true;
					}
				}
			
				return false;
			}

			string keyName = inputSystem.GetStringFromKey((Keys) ev.Value);
			var            = _shortcutKeys.Get(keyName);

			// anything more than 32 levels of indirection we can be pretty sure is an infinite loop
			for(int runaway = 0; runaway < 32; runaway++)
			{
				idSWFParameterList eventParms = new idSWFParameterList();
				eventParms.Add(ev.DeviceNumber);

				if(var.IsString == true)
				{
					// alias to another key
					var = _shortcutKeys.Get(var.ToString());
					continue;
				}
				else if(var.IsObject == true)
				{
					// if this object is a sprite, send fake mouse events to it
					idSWFScriptObject obj = var.Object;
				
					// make sure we don't send an onRelease event unless we have already sent that object an onPress
					bool wasPressed = obj.Get("_pressed").ToBool();

					obj.Set("_pressed", ev.Value2);

					if(ev.Value2 != 0)
					{
						var = obj.Get("onPress");
					}
					else if(wasPressed == true)
					{
						var = obj.Get("onRelease");
					}

					if(var.IsFunction == true)
					{
						var.Function.Invoke(obj, eventParms);
						return true;
					}
				} 
				else if(var.IsFunction == true)
				{
					if(ev.Value2 != 0)
					{
						// anonymous functions only respond to key down events
						var.Function.Invoke(null, eventParms);
						return true;
					}
				
					return false;
				}

				idSWFScriptVariable useFunction = _globals.Get("useFunction");

				if((useFunction.IsFunction == true) && (ev.Value2 != 0))
				{
					string action = inputSystem.GetBinding((Keys) ev.Value);

					if(action == "_use")
					{
						useFunction.Function.Invoke(null, new idSWFParameterList());
					}
				}

				idSWFScriptVariable waitInput = _globals.Get("waitInput");

				if(waitInput.IsFunction == true)
				{
					_useMouse = false;

					if(ev.Value2 != 0)
					{
						idSWFParameterList waitParms = new idSWFParameterList();
						waitParms.Add(ev.Value);

						waitInput.Function.Invoke(null, waitParms);
					}
				} 
				else 
				{
					_useMouse = true;
				}

				idSWFScriptVariable focusWindow = _globals.Get("focusWindow");

				if(focusWindow.IsFunction == true)
				{
					idSWFScriptVariable onKey = focusWindow.Object.Get("onKey");

					if(onKey.IsFunction == true)
					{
						// make sure we don't send an onRelease event unless we have already sent that object an onPress
						idSWFScriptObject obj = focusWindow.Object;
						bool wasPressed       = obj.Get("_kpressed").ToBool();

						obj.Set("_kpressed", ev.Value2);

						if((ev.Value2 != 0) || (wasPressed == true))
						{
							idSWFParameterList parms = new idSWFParameterList();
							parms.Add(ev.Value);
							parms.Add(ev.Value2);

							onKey.Function.Invoke(focusWindow.Object, parms).ToBool();

							return true;
						} 
						else if((value == Keys.LeftShift) || (value == Keys.RightShift))
						{
							idSWFParameterList parms = new idSWFParameterList();
							parms.Add(ev.Value);
							parms.Add(ev.Value2);

							onKey.Function.Invoke(focusWindow.Object, parms).ToBool();
						}
					}
				}
			
				return false;
			}

			throw new Exception(string.Format("Circular reference in shortcutKeys.{0}", keyName));
		} 

		private bool HandleCharEvent(SystemEvent ev)
		{	
			IInputSystem inputSystem        = idEngine.Instance.GetService<IInputSystem>();
			idSWFScriptVariable focusWindow = _globals.Get("focusWindow");

			if(focusWindow.IsObject == true)
			{
				idSWFScriptVariable onChar = focusWindow.Object.Get("onChar");

				if(onChar.IsFunction == true)
				{
					idSWFParameterList parms = new idSWFParameterList();
					parms.Add(ev.Value);
					parms.Add(inputSystem.GetStringFromKey((Keys) ev.Value));

					onChar.Function.Invoke(focusWindow.Object, parms).ToBool();

					return true;
				}
			}

			return false;
		}
	
		private bool HandleMouseEvent(SystemEvent ev)
		{
			_mouseEnabled        = true;
			_isMouseInClientArea = true;
			
			_mouseX += ev.Value;
			_mouseY += ev.Value2;

			_mouseX = (int) idMath.Max(idMath.Min(_mouseX, (int) (_frameWidth + _renderBorder)), (int) (0.0f - _renderBorder));
			_mouseY = (int) idMath.Max(idMath.Min(_mouseY, (int) _frameHeight), 0);
			
			bool retVal = false;

			idSWFScriptObject hitObject = HitTest(_mainSpriteInstance, new idSWFRenderState(), _mouseX, _mouseY, null);

			if(hitObject != null)
			{
				_hasHitObject = true;
			}
			else
			{
				_hasHitObject = false;
			}

			if(hitObject != _hoverObject)
			{
				// first check to see if we should call onRollOut on our previous hoverObject
				if(_hoverObject != null)
				{
					idSWFScriptVariable var = _hoverObject.Get("onRollOut");

					if(var.IsFunction == true)
					{
						var.Function.Invoke(_hoverObject, new idSWFParameterList());
						retVal = true;
					}

					_hoverObject = null;
				}

				// then call onRollOver on our hitObject
				if(hitObject != null)
				{
					_hoverObject            = hitObject;
					idSWFScriptVariable var = hitObject.Get("onRollOver");

					if(var.IsFunction == true)
					{
						var.Function.Invoke(hitObject, new idSWFParameterList());
						retVal = true;
					}
				}
			}

			if(_mouseObject != null)
			{
				idSWFScriptVariable var = _mouseObject.Get("onDrag");

				if(var.IsFunction == true)
				{
					idSWFParameterList parms = new idSWFParameterList();
					parms.Add(_mouseX);
					parms.Add(_mouseY);
					parms.Add(false);

					var.Function.Invoke(_mouseObject, parms);

					return true;
				}
			}

			return retVal;
		}
		#endregion

		#region Loading
		private void CreateGlobals()
		{
			_globals = new idSWFScriptObject();
			_globals.Set("_global", _globals);

			_globals.Set("Object",             new idSWFScriptFunction_Object());

			_shortcutKeys = new idSWFScriptObject();

			ScriptFunction_clearShortcutKeys(_shortcutKeys, this, new idSWFParameterList());

			_globals.Set("shortcutKeys",       _shortcutKeys);

			_globals.Set("shortcutKeys_clear", new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_clearShortcutKeys, this));
			_globals.Set("deactivate",         new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_deactivate, this));
			_globals.Set("inhibitControl",     new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_inhibitControl, this));
			_globals.Set("useInhibit",         new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_useInhibit, this));
			_globals.Set("precacheSound",      new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_precacheSound, this));
			_globals.Set("playSound",          new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_playSound, this));
			_globals.Set("stopSounds",         new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_stopSounds, this));
			_globals.Set("getPlatform",        new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_getPlatform, this));
			_globals.Set("getTruePlatform",    new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_getTruePlatform, this));
			_globals.Set("getLocalString",     new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_getLocalString, this));
			_globals.Set("swapPS3Buttons",     new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_swapPS3Buttons, this));
			_globals.Set("strReplace",         new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_strReplace, this));
			_globals.Set("getCVarInteger",     new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_getCVarInteger, this));
			_globals.Set("setCVarInteger",     new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_setCVarInteger, this));

			_globals.Set("acos",               new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_acos, this));
			_globals.Set("cos",	               new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_cos, this));
			_globals.Set("sin",	               new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_sin, this));
			_globals.Set("round",              new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_round, this));
			_globals.Set("pow",	               new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_pow, this));
			_globals.Set("sqrt",               new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_sqrt, this));
			_globals.Set("abs",                new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_abs, this));
			_globals.Set("rand",               new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_rand, this));
			_globals.Set("floor",              new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_floor, this));
			_globals.Set("ceil",               new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_ceil, this));
			_globals.Set("toUpper",	           new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_toUpper, this));

			_globals.SetNative("platform",     new idSWFScriptNativeVariable_NestedReadonly<idSWF>(ScriptVariable_getPlatform, this));
			_globals.SetNative("blackbars",    new idSWFScriptNativeVariable_Nested<idSWF>(ScriptVariable_getBlackbars, ScriptVariable_setBlackbars, this));
			_globals.SetNative("cropToHeight", new idSWFScriptNativeVariable_Nested<idSWF>(ScriptVariable_getCrop, ScriptVariable_setCrop, this));
			_globals.SetNative("cropToFit",    new idSWFScriptNativeVariable_Nested<idSWF>(ScriptVariable_getCrop, ScriptVariable_setCrop, this));
			_globals.SetNative("crop",         new idSWFScriptNativeVariable_Nested<idSWF>(ScriptVariable_getCrop, ScriptVariable_setCrop, this));
		}

		public int GetPlatform()
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			if((cvarSystem.GetBool("in_useJoystick") == true) || (_forceNonPCPlatform == true))
			{
				_forceNonPCPlatform = false;
				return 0;
			}

			return 2;
		}

		public idSWFScriptVariable GetGlobal(string name)
		{
			return _globals.Get(name);
		}

		public void SetGlobal(string name, int value)
		{
			SetGlobal(name, new idSWFScriptVariable(value));
		}

		public void SetGlobal(string name, idSWFScriptFunction value)
		{
			_globals.Set(name, value);
		}

		public void SetGlobal(string name, idSWFScriptVariable value)
		{
			_globals.Set(name, value);
		}

		internal void LoadFrom(ContentReader input)
		{
			IDeclManager declManager = idEngine.Instance.GetService<IDeclManager>();
			ICVarSystem cvarSystem   = idEngine.Instance.GetService<ICVarSystem>();

			_mainSpriteInstance = null;

			// ------------------------------
			// BEGIN XNB LOAD
			_frameWidth  = input.ReadSingle();
			_frameHeight = input.ReadSingle();
			_frameRate   = input.ReadUInt16();

			input.ReadInt32(); // dict type - sprite

			_mainSprite = new idSWFSprite(this);
			_mainSprite.LoadFrom(input);

			_dictionary = new idSWFDictionaryEntry[input.ReadInt32()];

			for(int i = 0; i < _dictionary.Length; i++)
			{
				_dictionary[i] = CreateDictionaryEntry((idSWFDictionaryType) input.ReadInt32());
				_dictionary[i].LoadFrom(input);
			}

			// END XNB LOAD
			// ------------------------------

			_atlasMaterial = declManager.FindMaterial(input.AssetName.Replace('\\', '/'));

			CreateGlobals();

			_mainSpriteInstance = new idSWFSpriteInstance();
			_mainSpriteInstance.Initialize(_mainSprite, null, 0);

			_globals.Set("_root", _mainSpriteInstance.ScriptObject);
			
			// Do this to touch any external references (like sounds)
			// But disable script warnings because many globals won't have been created yet
			int debug = cvarSystem.GetInt("swf_debug");
			cvarSystem.Set("swf_debug", 0);						

			_mainSpriteInstance.Run();
			_mainSpriteInstance.RunActions();
			_mainSpriteInstance.RunTo(0);

			cvarSystem.Set("swf_debug", debug);

			if(_mouseX == -1)
			{
				_mouseX = (int) (_frameWidth / 2);
			}

			if(_mouseY == -1)
			{
				_mouseY = (int) (_frameHeight / 2);
			}
		}

		private idSWFDictionaryEntry CreateDictionaryEntry(idSWFDictionaryType type)
		{
			switch(type)
			{
				case idSWFDictionaryType.Null:
					return new idSWFNull();

				case idSWFDictionaryType.Image:
					return new idSWFImage();

				case idSWFDictionaryType.Shape:
					return new idSWFShape();

				case idSWFDictionaryType.Morph:
					return new idSWFMorphShape();

				case idSWFDictionaryType.Sprite:
					return new idSWFSprite(this);

				case idSWFDictionaryType.Font:
					return new idSWFFont();

				case idSWFDictionaryType.Text:
					return new idSWFText();

				case idSWFDictionaryType.EditText:
					return new idSWFEditText();

				default:
					idEngine.Instance.Error("Unknown SWF dictionary type");
					break;
			}

			return null;
		}
		#endregion

		#region Misc.
		public idSWFDictionaryEntry FindDictionaryEntry(int characterID)
		{
			if(_dictionary.Length < (characterID + 1))
			{
				idLog.Warning("could not find character {0}", characterID);
				return null;
			}

			return _dictionary[characterID];
		}

		public idSWFDictionaryEntry FindDictionaryEntry(int characterID, Type type)
		{
			if(_dictionary.Length < (characterID + 1))
			{
				idLog.Warning("could not find character {0}", characterID);
				return null;
			}

			if(_dictionary[characterID].GetType() != type)
			{
				idLog.Warning("character {0} is the wrong type", characterID);
				return null;
			}

			return _dictionary[characterID];
		}

		public idSWFScriptObject HitTest(idSWFSpriteInstance spriteInstance, idSWFRenderState renderState, int x, int y, idSWFScriptObject parentObject)
		{
			if(spriteInstance.Parent != null)
			{
				idSWFDisplayEntry displayEntry = spriteInstance.Parent.FindDisplayEntry(spriteInstance.Depth);

				if((displayEntry.ColorXForm.Mul.W + displayEntry.ColorXForm.Add.W) < 0.001f)
				{
					return null;
				}
			}

			if(spriteInstance.IsVisible == false)
			{
				return null;
			}

			if((spriteInstance.ScriptObject.HasValidProperty("onRelease") == true)
				|| (spriteInstance.ScriptObject.HasValidProperty("onPress") == true)
				|| (spriteInstance.ScriptObject.HasValidProperty("onRollOver") == true)
				|| (spriteInstance.ScriptObject.HasValidProperty("onRollOut") == true)
				|| (spriteInstance.ScriptObject.HasValidProperty("onDrag") == true))
			{
				parentObject = spriteInstance.ScriptObject;
			}
			
			// rather than returning the first object we find, we actually want to return the last object we find
			idSWFScriptObject returnObject = null;
	
			float xOffset = spriteInstance.OffsetX;
			float yOffset = spriteInstance.OffsetY;

			Matrix edgeEquations, edgeEquationsTemp;
	
			for(int i = 0; i < spriteInstance.DisplayList.Count; i++)
			{
				idSWFDisplayEntry display  = spriteInstance.DisplayList[i];
				idSWFDictionaryEntry entry = FindDictionaryEntry(display.CharacterID);

				if(entry == null)
				{
					continue;
				}

				idSWFRenderState renderState2 = new idSWFRenderState();
				renderState2.Matrix           = display.Matrix.Multiply(renderState.Matrix);
				renderState2.Ratio            = display.Ratio;

				if(entry is idSWFMorphShape)
				{
					// FIXME: this should be roughly the same as SWF_DICT_SHAPE
				} 
				else if(entry is idSWFEditText)
				{
					// FIXME: this should be roughly the same as SWF_DICT_SHAPE
				}
				else if(entry is idSWFSprite)
				{
					idSWFScriptObject obj = HitTest(display.SpriteInstance, renderState2, x, y, parentObject);

					if((obj != null) && (obj.Get("_visible").ToBool() == true))
					{
						returnObject = obj;
					}
				}
				else if((entry is idSWFShape) && (parentObject != null))
				{
					idSWFShape shape = (idSWFShape) entry;

					for(i = 0; i < shape.Fills.Length; i++)
					{
						idSWFShapeDrawFill fill = shape.Fills[i];

						for(int j = 0; j < fill.Indices.Length; j += 3)
						{
							Vector2 xy1 = renderState2.Matrix.Transform(fill.StartVertices[fill.Indices[j + 0]]);
							Vector2 xy2 = renderState2.Matrix.Transform(fill.StartVertices[fill.Indices[j + 1]]);
							Vector2 xy3 = renderState2.Matrix.Transform(fill.StartVertices[fill.Indices[j + 2]]);

							edgeEquationsTemp     = new Matrix();
							edgeEquationsTemp.M11 = xy1.X + xOffset;
							edgeEquationsTemp.M12 = xy1.Y + yOffset;
							edgeEquationsTemp.M13 = 1.0f; ;

							edgeEquationsTemp.M21 = xy2.X + xOffset;
							edgeEquationsTemp.M22 = xy2.Y + yOffset;
							edgeEquationsTemp.M23 = 1.0f;

							edgeEquationsTemp.M31 = xy3.X + xOffset;
							edgeEquationsTemp.M32 = xy3.Y + yOffset;
							edgeEquationsTemp.M33 = 1.0f;

							edgeEquations = Matrix.Invert(edgeEquationsTemp);

							Vector3 p     = new Vector3(x, y, 1.0f);
							Vector3 signs = Vector3.Transform(p, edgeEquations);

							bool bx = signs.X > 0;
							bool by = signs.Y > 0;
							bool bz = signs.Z > 0;

							if((bx == by) && (bx == bz))
							{
								// point inside
								returnObject = parentObject;
							}
						}
					}
				} 
				else if(entry is idSWFEditText)
				{
					idSWFScriptObject editObject = null;

					if((display.TextInstance.ScriptObject.HasProperty("onRelease") == true)
						|| (display.TextInstance.ScriptObject.HasProperty("onPress") == true))
					{
						// if the edit box itself can be clicked, then we want to return it when it's clicked on
						editObject = display.TextInstance.ScriptObject;
					} 
					else if(parentObject != null) 
					{
						// otherwise, we want to return the parent object
						editObject = parentObject;
					}

					if(editObject == null) 
					{
						continue;
					}

					if(display.TextInstance.Text == string.Empty)
					{
						continue;
					}

					idSWFEditText shape = entry as idSWFEditText;
					idSWFEditText text  = display.TextInstance.EditText;
					float textLength    = display.TextInstance.TextLength;

					float lengthDiff = idMath.Abs(shape.Bounds.BottomRight.X - shape.Bounds.TopLeft.X) - textLength;
			
					Vector3 topLeft; 
					Vector3 topRight; 
					Vector3 bottomRight; 
					Vector3 bottomLeft;

					float topOffset = 0.0f;
					xOffset         = spriteInstance.OffsetX;
					yOffset         = spriteInstance.OffsetY;
					
					if(text.Align == TextAlign.Left)
					{
						topLeft     = renderState2.Matrix.Transform(new Vector2(shape.Bounds.TopLeft.X + xOffset, shape.Bounds.TopLeft.Y + topOffset + yOffset)).ToVector3();
						topRight    = renderState2.Matrix.Transform(new Vector2(shape.Bounds.BottomRight.X - lengthDiff + xOffset, shape.Bounds.TopLeft.Y + topOffset + yOffset)).ToVector3();
						bottomRight = renderState2.Matrix.Transform(new Vector2(shape.Bounds.BottomRight.X - lengthDiff + xOffset, shape.Bounds.BottomRight.Y + topOffset + yOffset)).ToVector3();
						bottomLeft  = renderState2.Matrix.Transform(new Vector2(shape.Bounds.TopLeft.X + xOffset, shape.Bounds.BottomRight.Y + topOffset + yOffset)).ToVector3();				
					} 
					else if(text.Align == TextAlign.Right)
					{
						topLeft     = renderState2.Matrix.Transform(new Vector2(shape.Bounds.TopLeft.X + lengthDiff + xOffset, shape.Bounds.TopLeft.Y + topOffset + yOffset)).ToVector3();
						topRight    = renderState2.Matrix.Transform(new Vector2(shape.Bounds.BottomRight.X + xOffset, shape.Bounds.TopLeft.Y + topOffset + yOffset)).ToVector3();
						bottomRight = renderState2.Matrix.Transform(new Vector2(shape.Bounds.BottomRight.X + xOffset, shape.Bounds.BottomRight.Y + topOffset + yOffset)).ToVector3();
						bottomLeft  = renderState2.Matrix.Transform(new Vector2(shape.Bounds.TopLeft.X + lengthDiff + xOffset, shape.Bounds.BottomRight.Y + topOffset + yOffset)).ToVector3();
					}
					else if(text.Align == TextAlign.Center)
					{
						float middle = ((shape.Bounds.BottomRight.X + xOffset) + (shape.Bounds.TopLeft.X + xOffset)) / 2.0f;

						topLeft     = renderState2.Matrix.Transform(new Vector2(middle - (textLength / 2.0f), shape.Bounds.TopLeft.Y + topOffset + yOffset)).ToVector3();
						topRight    = renderState2.Matrix.Transform(new Vector2(middle + (textLength / 2.0f), shape.Bounds.TopLeft.Y + topOffset + yOffset)).ToVector3();
						bottomRight = renderState2.Matrix.Transform(new Vector2(middle + (textLength / 2.0f), shape.Bounds.BottomRight.Y + topOffset + yOffset)).ToVector3();
						bottomLeft  = renderState2.Matrix.Transform(new Vector2(middle - (textLength / 2.0f), shape.Bounds.BottomRight.Y + topOffset + yOffset)).ToVector3();
					} 
					else
					{
						topLeft     = renderState2.Matrix.Transform(new Vector2(shape.Bounds.TopLeft.X + xOffset, shape.Bounds.TopLeft.Y + topOffset + yOffset)).ToVector3();
						topRight    = renderState2.Matrix.Transform(new Vector2(shape.Bounds.BottomRight.X + xOffset, shape.Bounds.TopLeft.Y + topOffset + yOffset)).ToVector3();
						bottomRight = renderState2.Matrix.Transform(new Vector2(shape.Bounds.BottomRight.X + xOffset, shape.Bounds.BottomRight.Y + topOffset + yOffset)).ToVector3();
						bottomLeft  = renderState2.Matrix.Transform(new Vector2(shape.Bounds.TopLeft.X + xOffset, shape.Bounds.BottomRight.Y + topOffset + yOffset)).ToVector3();
					}

					topLeft.Z   = 1.0f;
					topRight.Z  = 1.0f;
					bottomRight.Z = 1.0f;
					bottomLeft.Z  = 1.0f;

					edgeEquationsTemp = new Matrix();
					edgeEquationsTemp.M11 = topLeft.X;
					edgeEquationsTemp.M12 = topLeft.Y;
					edgeEquationsTemp.M13 = topLeft.Z;

					edgeEquationsTemp.M21 = topRight.X;
					edgeEquationsTemp.M22 = topRight.Y;
					edgeEquationsTemp.M23 = topRight.Z;

					edgeEquationsTemp.M31 = bottomRight.X;
					edgeEquationsTemp.M32 = bottomRight.Y;
					edgeEquationsTemp.M33 = bottomRight.Z;

					edgeEquations = Matrix.Invert(edgeEquationsTemp);

					Vector3 p     = new Vector3(x, y, 1.0f);
					Vector3 signs = Vector3.Transform(p, edgeEquations);

					bool bx = signs.X > 0;
					bool by = signs.Y > 0;
					bool bz = signs.Z > 0;

					if((bx == by) && (bx == bz))
					{
						// point inside top right triangle
						returnObject = editObject;
					}

					edgeEquationsTemp = new Matrix();
					edgeEquationsTemp.M11 = topLeft.X;
					edgeEquationsTemp.M12 = topLeft.Y;
					edgeEquationsTemp.M13 = topLeft.Z;

					edgeEquationsTemp.M21 = bottomRight.X;
					edgeEquationsTemp.M22 = bottomRight.Y;
					edgeEquationsTemp.M23 = bottomRight.Z;

					edgeEquationsTemp.M31 = bottomLeft.X;
					edgeEquationsTemp.M32 = bottomLeft.Y;
					edgeEquationsTemp.M33 = bottomLeft.Z;

					edgeEquations = Matrix.Invert(edgeEquationsTemp);
					
					signs = Vector3.Transform(p, edgeEquations);

					bx = signs.X > 0;
					by = signs.Y > 0;
					bz = signs.Z > 0;

					if((bx == by) && (bx == bz))
					{
						// point inside bottom left triangle
						returnObject = editObject;
					}
				}
			}

			return returnObject;
		}

		public void Invoke(string function, idSWFParameterList parms)
		{
			ICVarSystem cvarSystem  = idEngine.Instance.GetService<ICVarSystem>();
			idSWFScriptObject obj   = _mainSpriteInstance.ScriptObject;
			idSWFScriptVariable var = obj.Get(function);

			if(cvarSystem.GetBool("swf_debugInvoke") == true)
			{
				idLog.WriteLine("SWF: Invoke {0} with {1} parms", function, parms.Count);
			}

			if(var.IsFunction == true)
			{
				var.Function.Invoke(null, parms);
			}
		}
		#endregion

		#region State
		#region Properties
		public bool IsActive
		{
			get
			{
				return _isActive;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// When a SWF is deactivated, it rewinds the timeline back to the start.
		/// </remarks>
		/// <param name="show"></param>
		public void Activate(bool show)
		{
			if((_isActive == false) && (show == true))
			{
				_inhibitControl = false;
				_lastRenderTime = idEngine.Instance.ElapsedTime;

				_mainSpriteInstance.ClearDisplayList();
				_mainSpriteInstance.Play();
				_mainSpriteInstance.Run();
				_mainSpriteInstance.RunActions();
			}
			
			_isActive = show;
		}
		#endregion
		#endregion

		#region Script Functions
		private idSWFScriptVariable ScriptFunction_abs(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			if(parms.Count != 1)
			{
				return new idSWFScriptVariable();
			}

			return new idSWFScriptVariable(idMath.Abs(parms[0].ToFloat()));
		}

		private idSWFScriptVariable ScriptFunction_acos(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			if(parms.Count != 1)
			{
				return new idSWFScriptVariable();
			}

			return new idSWFScriptVariable(idMath.Acos(parms[0].ToFloat()));
		}

		private idSWFScriptVariable ScriptFunction_ceil(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			if((parms.Count != 1) || (parms[0].IsNumeric == false))
			{
				idLog.Warning("Invalid parameters specified for ceil");
				return new idSWFScriptVariable();
			}

			return new idSWFScriptVariable(idMath.Ceil(parms[0].ToFloat()));
		}

		private idSWFScriptVariable ScriptFunction_clearShortcutKeys(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			idSWFScriptObject obj = context.ShortcutKeys;
			obj.Clear();

			obj.Set("clear",            new idSWFScriptFunction_Nested<idSWF>(ScriptFunction_clearShortcutKeys, this));
			obj.Set("JOY1",             "ENTER");
			obj.Set("JOY2",             "BACKSPACE");
			obj.Set("JOY3",             "START");
			obj.Set("JOY5",             "LB");
			obj.Set("JOY6",             "RB");
			obj.Set("JOY9",             "START");
			obj.Set("JOY10",            "BACKSPACE");
			obj.Set("JOY_DPAD_UP",      "UP");
			obj.Set("JOY_DPAD_DOWN",    "DOWN");
			obj.Set("JOY_DPAD_LEFT",    "LEFT");
			obj.Set("JOY_DPAD_RIGHT",   "RIGHT");
			obj.Set("JOY_STICK1_UP",    "STICK1_UP");
			obj.Set("JOY_STICK1_DOWN",  "STICK1_DOWN");
			obj.Set("JOY_STICK1_LEFT",  "STICK1_LEFT");
			obj.Set("JOY_STICK1_RIGHT", "STICK1_RIGHT");
			obj.Set("JOY_STICK2_UP",    "STICK2_UP");
			obj.Set("JOY_STICK2_DOWN",  "STICK2_DOWN");
			obj.Set("JOY_STICK2_LEFT",  "STICK2_LEFT");
			obj.Set("JOY_STICK2_RIGHT", "STICK2_RIGHT");
			obj.Set("KP_ENTER",         "ENTER");
			obj.Set("MWHEELDOWN",       "MWHEEL_DOWN");
			obj.Set("MWHEELUP",         "MWHEEL_UP");
			obj.Set("K_TAB",            "TAB");
			
			// FIXME: I'm an RTARD and didn't realize the keys all have "ARROW" after them
			obj.Set("LEFTARROW",        "LEFT");
			obj.Set("RIGHTARROW",       "RIGHT");
			obj.Set("UPARROW",          "UP");
			obj.Set("DOWNARROW",        "DOWN");

			return new idSWFScriptVariable();
		}

		private idSWFScriptVariable ScriptFunction_cos(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			if(parms.Count != 1)
			{
				return new idSWFScriptVariable();
			}

			return new idSWFScriptVariable(idMath.Cos(parms[0].ToFloat()));
		}

		private idSWFScriptVariable ScriptFunction_deactivate(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			context.Activate(false);

			return new idSWFScriptVariable();
		}

		private idSWFScriptVariable ScriptFunction_floor(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			if((parms.Count != 1) || (parms[0].IsNumeric == false))
			{
				idLog.Warning("Invalid parameters specified for floor");
				return new idSWFScriptVariable();
			}

			return new idSWFScriptVariable(idMath.Floor(parms[0].ToFloat()));
		}

		private idSWFScriptVariable ScriptFunction_inhibitControl(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			context.InhibitControl = parms[0].ToBool();

			return new idSWFScriptVariable();
		}

		private idSWFScriptVariable ScriptFunction_toUpper(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			if((parms.Count != 1) || (parms[0].IsString == false))
			{
				idLog.Warning("Invalid parameters specified for toUpper");
				return new idSWFScriptVariable();
			}

			return new idSWFScriptVariable(idEngine.Instance.GetService<ILocalization>().Get(parms[0].ToString()).ToUpper());
		}

		private idSWFScriptVariable ScriptFunction_useInhibit(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			context.UseInhibitControl = parms[0].ToBool();
			return new idSWFScriptVariable();
		}

		private idSWFScriptVariable ScriptFunction_precacheSound(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_playSound(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_pow(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			if(parms.Count != 2)
			{
				return new idSWFScriptVariable();
			}

			float value = parms[0].ToFloat();
			float power = parms[0].ToFloat();

			return new idSWFScriptVariable(idMath.Pow(value, power));
		}

		private idSWFScriptVariable ScriptFunction_rand(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			float min = 0.0f;
			float max = 1.0f;

			switch(parms.Count)
			{
				case 0:
					break;

				case 1:
					max = parms[0].ToFloat();
					break;

				default:
					min = parms[0].ToFloat();
					max = parms[1].ToFloat();
					break;
			}

			return new idSWFScriptVariable(min + context.Random.Next() * (max - min));
		}

		private idSWFScriptVariable ScriptFunction_round(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			if(parms.Count != 1)
			{
				return new idSWFScriptVariable();
			}

			return new idSWFScriptVariable((int) (parms[0].ToFloat() + 0.5f));
		}

		private idSWFScriptVariable ScriptFunction_sin(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			if(parms.Count != 1)
			{
				return new idSWFScriptVariable();
			}

			return new idSWFScriptVariable(idMath.Sin(parms[0].ToFloat()));
		}

		private idSWFScriptVariable ScriptFunction_sqrt(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			if(parms.Count != 1)
			{
				return new idSWFScriptVariable();
			}

			return new idSWFScriptVariable(idMath.Sqrt(parms[0].ToFloat()));
		}

		private idSWFScriptVariable ScriptFunction_stopSounds(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_getPlatform(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			return new idSWFScriptVariable(context.GetPlatform());
		}

		private idSWFScriptVariable ScriptFunction_getTruePlatform(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			return new idSWFScriptVariable(2);
		}

		private idSWFScriptVariable ScriptFunction_getLocalString(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			if(parms.Count == 0)
			{
				return new idSWFScriptVariable();
			}

			return new idSWFScriptVariable(idEngine.Instance.GetService<ILocalization>().Get(parms[0].ToString()));
		}

		private idSWFScriptVariable ScriptFunction_swapPS3Buttons(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			return new idSWFScriptVariable(context.UseCircleForAccept);
		}

		private idSWFScriptVariable ScriptFunction_strReplace(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			if(parms.Count != 3)
			{
				return new idSWFScriptVariable(string.Empty);
			}

			string str    = parms[0].ToString();
			string repStr = parms[1].ToString();
			string val    = parms[2].ToString();

			return new idSWFScriptVariable(str.Replace(repStr, val));
		}

		private idSWFScriptVariable ScriptFunction_getCVarInteger(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			return new idSWFScriptVariable(idEngine.Instance.GetService<ICVarSystem>().GetInt(parms[0].ToString()));
		}

		private idSWFScriptVariable ScriptFunction_setCVarInteger(idSWFScriptObject scriptObj, idSWF context, idSWFParameterList parms)
		{
			idEngine.Instance.GetService<ICVarSystem>().Set(parms[0].ToString(), parms[1].ToInt32());
			return new idSWFScriptVariable();
		}
		#endregion

		#region Script Variables
		private idSWFScriptVariable ScriptVariable_getBlackbars(idSWFScriptObject scriptObj, idSWF context)
		{
			return new idSWFScriptVariable(context.ShowBlackBars);
		}

		private void ScriptVariable_setBlackbars(idSWFScriptObject scriptObj, idSWF context, idSWFScriptVariable value)
		{
			context.ShowBlackBars = value.ToBool();
		}

		private idSWFScriptVariable ScriptVariable_getCrop(idSWFScriptObject scriptObj, idSWF context)
		{
			return new idSWFScriptVariable(context.Crop);
		}

		private void ScriptVariable_setCrop(idSWFScriptObject scriptObj, idSWF context, idSWFScriptVariable value)
		{
			context.Crop = value.ToBool();
		}

		private idSWFScriptVariable ScriptVariable_getPlatform(idSWFScriptObject scriptObj, idSWF context)
		{
			return new idSWFScriptVariable(context.GetPlatform());
		}
		#endregion

		#region ScriptFunction_object
		private class idSWFScriptFunction_Object : idSWFScriptFunction
		{
			#region Members
			private idSWFScriptObject _object;
			#endregion

			#region Constructor
			public idSWFScriptFunction_Object() 
				: base()
			{
				_object = new idSWFScriptObject();
			}
			#endregion

			#region idSWFScriptFunction implementation
			public override idSWFScriptObject Prototype
			{
				get
				{
					return _object;
				}
				set
				{
					Debug.Assert(false);
				}
			}

			public override idSWFScriptVariable Invoke(idSWFScriptObject scriptObj, idSWFParameterList parms)
			{
				return new idSWFScriptVariable();
			}
			#endregion
		}
		#endregion
	}

	public enum idSWFTag
	{
		End                          = 0,
		ShowFrame                    = 1,
		DefineShape                  = 2,
		PlaceObject                  = 4,
		RemoveObject                 = 5,
		DefineBits                   = 6,
		DefineButton                 = 7,
		JpegTables                   = 8,
		SetBackgroundColor           = 9,
		DefineFont                   = 10,
		DefineText                   = 11,
		DoAction                     = 12,
		DefineFontInfo               = 13,
		DefineSound                  = 14,
		StartSound                   = 15,
		DefineButtonSound            = 17,
		SoundStreamHead              = 18,
		SoundStreamBlock             = 19,
		DefineBitsLossless           = 20,
		DefineBitsJpeg2              = 21,
		DefineShape2                 = 22,
		DefineButtonCxForm           = 23,
		Protect                      = 24,
		PlaceObject2                 = 26,
		RemoveObject2                = 28,
		DefineShape3                 = 32,
		DefineText2                  = 33,
		DefineButton2                = 34,
		DefineBitsJpeg3              = 35,
		DefineBitsLossless2          = 36,
		DefineEditText               = 37,
		DefineSprite                 = 39,
		FrameLabel                   = 43,
		SoundStreamHead2             = 45,
		DefineMorphShape             = 46,
		DefineFont2                  = 48,
		ExportAssets                 = 57,
		EnableDebugger               = 58,
		DoInitAction                 = 59,
		DefineVideoStream            = 60,
		VideoFrame                   = 61,
		DefineFontInfo2              = 62,
		EnableDebugger2              = 64,
		ScriptLimits                 = 65,
		SetTabIndex                  = 66,
		FileAttributes               = 69,
		PlaceObject3                 = 70,
		ImportAssets2                = 71,
		DefineFontAlignZones         = 73,
		CsmTextSettings              = 74,
		DefineFont3                  = 75,
		SymbolClass                  = 76,
		Metadata                     = 77,
		DefineScalingGrid            = 78,
		DoAbc                        = 82,
		DefineShape4                 = 83,
		DefineMorphShape2            = 84,
		DefineSceneAndFrameLabelData = 86,
		DefineBinaryData             = 87,
		DefineFontName               = 88,
		StartSound2                  = 89
	}

	public enum idSWFAction
	{
		End             = 0,

		// swf 3
		NextFrame       = 0x04,
		PrevFrame       = 0x05,
		Play            = 0x06,
		Stop            = 0x07,
		ToggleQuality   = 0x08,
		StopSounds      = 0x09,

		GotoFrame       = 0x81,
		GetURL          = 0x83,
		WaitForFrame    = 0x8A,
		SetTarget       = 0x8B,
		GoToLabel       = 0x8C,
	
		// swf 4
		Add             = 0x0A,
		Subtract        = 0x0B,
		Multiply        = 0x0C,
		Divide          = 0x0D,
		Equals          = 0x0E,
		Less            = 0x0F,
		And             = 0x10,
		Or              = 0x11,
		Not             = 0x12,
		StringEquals    = 0x13,
		StringLength    = 0x14,
		StringExtract   = 0x15,
		Pop             = 0x17,
		ToInteger       = 0x18,
		GetVariable     = 0x1C,
		SetVariable     = 0x1D,
		SetTarget2      = 0x20,
		StringAdd       = 0x21,
		GetProperty     = 0x22,
		SetProperty     = 0x23,
		CloneSprite     = 0x24,
		RemoveSprite    = 0x25,
		Trace           = 0x26,
		StartDrag       = 0x27,
		EndDrag         = 0x28,
		StringLess      = 0x29,
		RandomNumber    = 0x30,
		MBStringLength  = 0x31,
		CharToAscii     = 0x32,
		AsciiToChar     = 0x33,
		GetTime         = 0x34,
		MBStringExtract = 0x35,
		MBCharToAscii   = 0x36,
		MBAsciiToChar   = 0x37,

		WaitForFrame2   = 0x8D,
		Push            = 0x96,
		Jump            = 0x99,
		GetURL2         = 0x9A,
		If              = 0x9D,
		Call            = 0x9E,
		GotoFrame2      = 0x9F,
	
		// swf 5
		Delete          = 0x3A,
		Delete2         = 0x3B,
		DefineLocal     = 0x3C,
		CallFunction    = 0x3D,
		Return          = 0x3E,
		Modulo          = 0x3F,
		NewObject       = 0x40,
		DefineLocal2    = 0x41,
		InitArray       = 0x42,
		InitObject      = 0x43,
		TypeOf          = 0x44,
		TargetPath      = 0x45,
		Enumerate       = 0x46,
		Add2            = 0x47,
		Less2           = 0x48,
		Equals2         = 0x49,
		ToNumber        = 0x4A,
		ToString        = 0x4B,
		PushDuplicate   = 0x4C,
		StackSwap       = 0x4D,
		GetMember       = 0x4E,
		SetMember       = 0x4F,
		Increment       = 0x50,
		Decrement       = 0x51,
		CallMethod      = 0x52,
		NewMethod       = 0x53,
		BitAnd          = 0x60,
		BitOr           = 0x61,
		BitXor          = 0x62,
		BitLShift       = 0x63,
		BitRShift       = 0x64,
		BitURShift      = 0x65,

		StoreRegister   = 0x87,
		ConstantPool    = 0x88,
		With            = 0x94,
		DefineFunction  = 0x9B,
	
		// swf 6
		InstanceOf      = 0x54,
		Enumerate2      = 0x55,
		StrictEquals    = 0x66,
		Greater         = 0x67,
		StringGreater   = 0x68,
	
		// swf 7
		Extends         = 0x69,
		CastOp          = 0x2B,
		ImplementsOp    = 0x2C,
		Throw           = 0x2A,
		Try             = 0x8F,

		DefineFunction2 = 0x8E,
	};

	public enum idSWFDictionaryType
	{
		Null,
		Image,
		Shape,
		Morph,
		Sprite,
		Font,
		Text,
		EditText
	}

	public struct idSWFRect
	{
		public Vector2 TopLeft;
		public Vector2 BottomRight;

		internal void LoadFrom(ContentReader input)
		{
			this.TopLeft     = input.ReadVector2();
			this.BottomRight = input.ReadVector2();
		}
	}

	public struct idSWFColorRGBA
	{
		public byte R;
		public byte G;
		public byte B;
		public byte A;

		public idSWFColorRGBA(byte r, byte g, byte b, byte a)
		{
			this.R = r;
			this.G = g;
			this.B = b;
			this.A = a;
		}

		internal void LoadFrom(ContentReader input)
		{
			this.R = input.ReadByte();
			this.G = input.ReadByte();
			this.B = input.ReadByte();
			this.A = input.ReadByte();
		}

		public Vector4 ToVector4()
		{
			return new Vector4(this.R * (1.0f / 255.0f), this.G * (1.0f / 255.0f), this.B * (1.0f / 255.0f), this.A * (1.0f / 255.0f));
		}

		public static idSWFColorRGBA Default = new idSWFColorRGBA(255, 255, 255, 255);
	}

	public struct idSWFRenderState
	{
		public idSWFMatrix Matrix;
		public idSWFColorXForm ColorXForm;

		public idMaterial Material;
		public int MaterialWidth;
		public int MaterialHeight;

		public int ActiveMasks;
		public byte BlendMode;
		public float Ratio;
		public StereoDepthType StereoDepth;
	}
}