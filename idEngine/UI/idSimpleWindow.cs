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

using idTech4.Renderer;

namespace idTech4.UI
{
	public sealed class idSimpleWindow
	{
		#region Properties
		public string Name
		{
			get
			{
				return _name;
			}
		}

		public idWindow Parent
		{
			get
			{
				return _parent;
			}
		}
		#endregion

		#region Members
		private string _name;

		private idUserInterface _gui;
		private idDeviceContext _context;
		private WindowFlags _flags;

		private idWindow _parent;

		private Rectangle _drawRect; // overall rect
		private Rectangle _clientRect; // client area
		private Rectangle _textRect;

		private Vector2 _origin;
		private int _fontNumber;

		private float _materialScaleX;
		private float _materialScaleY;
		private int _borderSize;
		private TextAlign _textAlign;
		private int _textAlignX;
		private int _textAlignY;
		private int _textShadow;

		private idWinString _text = new idWinString("text");
		private idWinBool _visible = new idWinBool("visible");
		private idWinRectangle _rect = new idWinRectangle("rect");
		private idWinVector4 _backColor = new idWinVector4("backColor");
		private idWinVector4 _foreColor = new idWinVector4("foreColor");
		private idWinVector4 _materialColor = new idWinVector4("matColor");
		private idWinVector4 _borderColor = new idWinVector4("borderColor");
		private idWinFloat _textScale = new idWinFloat("textScale");
		private idWinFloat _rotate = new idWinFloat("rotate");
		private idWinVector2 _shear = new idWinVector2("shear");
		private idWinBackground _backgroundName = new idWinBackground("background");
		private idWinBool _hideCursor = new idWinBool("hideCursor");

		private idMaterial _background;
		#endregion

		#region Constructor
		public idSimpleWindow(idWindow win)
		{
			_gui = win.UserInterface;
			_context = win.DeviceContext;

			_drawRect = win.DrawRectangle;
			_clientRect = win.ClientRectangle;
			_textRect = win.TextRectangle;

			_origin = win.Origin;
			// TODO: _fontNumber = win.FontNumber;
			_name = win.Name;

			_materialScaleX = win.MaterialScaleX;
			_materialScaleY = win.MaterialScaleY;

			_borderSize = win.BorderSize;
			_textAlign = win.TextAlign;
			_textAlignX = win.TextAlignX;
			_textAlignY = win.TextAlignY;
			_background = win.Background;
			_flags = win.Flags;
			_textShadow = win.TextShadow;

			_visible.Set(win.IsVisible);
			_text.Set(win.Text);
			_rect.Set(win.Rectangle);
			_backColor.Set(win.BackColor);
			_foreColor.Set(win.ForeColor);
			_borderColor.Set(win.BorderColor);
			_textScale.Set(win.TextScale);
			_rotate.Set(win.Rotate);
			_shear.Set(win.Shear);
			_backgroundName.Set(win.BackgroundName);

			if(_backgroundName != string.Empty)
			{
				_background = idE.DeclManager.FindMaterial(_backgroundName);
				_background.Sort = (float) MaterialSort.Gui; ;
				_background.ImageClassification = 1; // just for resource tracking
			}

			_backgroundName.Material = _background;

			_parent = win.Parent;
			_hideCursor.Set(win.HideCursor);

			if(_parent != null)
			{
				if(_text.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_text);
				}

				if(_visible.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_visible);
				}

				if(_rect.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_rect);
				}

				if(_backColor.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_backColor);
				}

				if(_materialColor.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_materialColor);
				}

				if(_foreColor.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_foreColor);
				}

				if(_borderColor.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_borderColor);
				}

				if(_textScale.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_textScale);
				}

				if(_rotate.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_rotate);
				}

				if(_shear.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_shear);
				}

				if(_backgroundName.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_backgroundName);
				}
			}
		}
		#endregion

		#region Methods
		#region Public
		public void Draw(int x, int y)
		{
			if(_visible == false)
			{
				return;
			}

			CalculateClientRectangle(0, 0);

			// TODO: font
			// dc->SetFont(fontNum);

			_drawRect.Offset(x, y);
			_clientRect.Offset(x, y);
			_textRect.Offset(x, y);

			SetupTransforms(x, y);

			if((_flags & WindowFlags.NoClip) != 0)
			{
				_context.ClippingEnabled = false;
			}

			DrawBackground(_drawRect);

			// TODO: DrawBorderAndCaption(drawRect);

			// TODO: textShadow
			/*if ( textShadow ) {
				idStr shadowText = text;
				idRectangle shadowRect = textRect;

				shadowText.RemoveColors();
				shadowRect.x += textShadow;
				shadowRect.y += textShadow;

				dc->DrawText( shadowText, textScale, textAlign, colorBlack, shadowRect, !( flags & WIN_NOWRAP ), -1 );
			}
			 
			 // TODO: DrawText
			dc->DrawText(text, textScale, textAlign, foreColor, textRect, !( flags & WIN_NOWRAP ), -1);*/

			_context.SetTransformInformation(Vector3.Zero, Matrix.Identity);

			if((_flags & WindowFlags.NoClip) != 0)
			{
				_context.ClippingEnabled = true;
			}

			_drawRect.Offset(-x, -y);
			_clientRect.Offset(-x, -y);
			_textRect.Offset(-x, -y);
		}

		public idWindowVariable GetVariableByName(string name)
		{
			idWindowVariable ret = null;
			string nameLower = name.ToLower();

			if(nameLower == "background")
			{
				ret = _backgroundName;
			}
			else if(nameLower == "visible")
			{
				ret = _visible;
			}
			else if(nameLower == "rect")
			{
				ret = _rect;
			}
			else if(nameLower == "backcolor")
			{
				ret = _backColor;
			}
			else if(nameLower == "matcolor")
			{
				ret = _materialColor;
			}
			else if(nameLower == "forecolor")
			{
				ret = _foreColor;
			}
			else if(nameLower == "bordercolor")
			{
				ret = _borderColor;
			}
			else if(nameLower == "textscale")
			{
				ret = _textScale;
			}
			else if(nameLower == "rotate")
			{
				ret = _rotate;
			}
			else if(nameLower == "text")
			{
				ret = _text;
			}
			else if(nameLower == "backgroundname")
			{
				ret = _backgroundName;
			}
			else if(nameLower == "hidecursor")
			{
				ret = _hideCursor;
			}

			return ret;
		}
		#endregion

		#region Private
		private void CalculateClientRectangle(int offsetX, int offsetY)
		{
			_drawRect = _rect.Data;

			if((_flags & WindowFlags.InvertRectangle) != 0)
			{
				_drawRect.X = (int) (_rect.X - _rect.Width);
				_drawRect.Y = (int) (_rect.Y - _rect.Height);
			}

			_drawRect.X += offsetX;
			_drawRect.Y += offsetY;

			_clientRect = _drawRect;

			if((_rect.Height > 0.0f) && (_rect.Width > 0.0f))
			{
				if(((_flags & WindowFlags.Border) != 0) && (_borderSize != 0.0f))
				{
					_clientRect.X += _borderSize;
					_clientRect.Y += _borderSize;
					_clientRect.Width -= _borderSize;
					_clientRect.Height -= _borderSize;
				}

				_textRect = _clientRect;
				_textRect.X += 2;
				_textRect.Y += 2;
				_textRect.Width -= 2;
				_textRect.Height -= 2;

				_textRect.X += _textAlignX;
				_textRect.Y += _textAlignY;
			}

			_origin = new Vector2(_rect.X + (_rect.Width / 2), _rect.Y + (_rect.Height / 2));
		}

		private void DrawBackground(Rectangle drawRect)
		{
			if(_backColor.W > 0)
			{
				idConsole.WriteLine("DrawFilled");
			}
			/*if (backColor.w() > 0) {
		dc->DrawFilledRect(drawRect.x, drawRect.y, drawRect.w, drawRect.h, backColor);
	}*/

			if(_background != null)
			{
				if(_materialColor.W > 0)
				{
					idConsole.WriteLine("DrawMaterial");
				}
			}

			/*if (background) {
				if (matColor.w() > 0) {
					float scalex, scaley;
					if ( flags & WIN_NATURALMAT ) {
						scalex = drawRect.w / background->GetImageWidth();
						scaley = drawRect.h / background->GetImageHeight();
					} else {
						scalex = matScalex;
						scaley = matScaley;
					}
					dc->DrawMaterial(drawRect.x, drawRect.y, drawRect.w, drawRect.h, background, matColor, scalex, scaley);
				}*/
		}

		private void SetupTransforms(float x, float y)
		{
			Matrix transform = Matrix.Identity;
			Vector3 origin = new Vector3(_origin.X + x, _origin.Y + y, 0);

			// TODO: rotate
			/*if ( rotate ) {
				static idRotation rot;
				static idVec3 vec(0, 0, 1);
				rot.Set( org, vec, rotate );
				trans = rot.ToMat3();
			}*/

			// TODO: shear
			/*if ( shear.x || shear.y ) {
				static idMat3 smat;
				smat.Identity();
				smat[0][1] = shear.x;
				smat[1][0] = shear.y;
				trans *= smat;
			}*/

			if(transform != Matrix.Identity)
			{
				_context.SetTransformInformation(origin, transform);
			}
		}
		#endregion
		#endregion
	}
}