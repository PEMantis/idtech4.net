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
using Microsoft.Xna.Framework;

namespace idTech4.Content.Pipeline.Intermediate.Fonts
{
	public class FontContent
	{
		public short Ascender;
		public short Descender;

		public FontGlyph[] Glyphs;

		/// <summary>
		/// This is a sorted array of all characters in the font. 
		/// </summary>
		/// <remarks>
		/// This maps directly to glyphData, so if charIndex[0] is 42 then glyphData[0] is character 42.
		/// </remarks>
		public uint[] CharacterIndices;
		
		/// <summary>
		/// As an optimization, provide a direct mapping for the ascii character set. 
		/// </summary>
		public char[] Ascii;

		public string MaterialName;
	}

	public class FontGlyph
	{
		/// <summary>Width of glyph in pixels.</summary>
		public int Width;
		/// <summary>Height of glyph in pixels.</summary>
		public int Height;
		/// <summary>Distance in pixels from the base line to the top of the glyph.</summary>
		public int Top;
		/// <summary>Distance in pixels from the pen to the left edge of the glyph.</summary>
		public int Left;
		/// <summary>X adjustment after rendering this glyph.</summary>
		public int SkipX;

		/// <summary>
		/// Texture coordinates for the glyph.
		/// </summary>
		public Vector2 TextureCoordinates;
	}
}