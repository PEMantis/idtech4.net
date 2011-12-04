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

namespace idTech4.Renderer
{
	public sealed class idImage
	{
	}

	public enum TextureDepth
	{
		/// <summary>May be compressed, and always zeros the alpha channel.</summary>
		Specular,
		/// <summary>May be compressed.</summary>
		Diffuse,
		/// <summary>Will use compressed formats when possible.</summary>
		Default,
		/// <summary>May be compressed with 8 bit lookup.</summary>
		Bump,
		/// <summary>Either 32 bit or a component format, no loss at all.</summary>
		HighQuality
	}

	public enum TextureType
	{
		Disabled,
		TwoD,
		ThreeD,
		Cubix,
		Rectangle
	}

	public enum TextureFilter
	{
		Linear,
		Nearest,
		/// <summary>Use the user-specified r_textureFilter.</summary>
		Default
	}

	public enum TextureRepeat
	{
		Repeat,
		Clamp,
		/// <summary>This should replace TR_CLAMP_TO_ZERO and TR_CLAMP_TO_ZERO_ALPHA but I don't want to risk changing it right now.</summary>
		ClampToBorder,
		/// <summary>Guarantee 0,0,0,255 edge for projected textures, set AFTER image format selection</summary>
		ClampToZero,
		/// <summary>Guarantee 0 alpha edge for projected textures, set AFTER image format selection</summary>
		ClampToZeroAlpha
	}

	public enum CubeFiles
	{
		/// <summary>Not a cube map.</summary>
		TwoD,
		/// <summary>_px, _nx, _py, etc, directly sent to the renderer.</summary>
		Native,
		/// <summary>_forward, _back, etc, rotated and flipped as needed before sending to the renderer.</summary>
		Camera
	}

	public enum DynamicImageType
	{
		Static,
		Scratch, // video, screen wipe, etc.
		CubeRender,
		MirrorRender,
		XRayRender,
		RemoteRender
	}

	public enum TextureCoordinateGeneration
	{
		Explicit,
		DiffuseCube,
		ReflectCube,
		SkyboxCube,
		WobbleSkyCube,
		Screen, // screen aligned, for mirrorRenders and screen space temporaries.
		Screen2,
		GlassWarp
	}

	public enum StageLighting
	{
		Ambient, // execute after lighting.
		Bump,
		Diffuse,
		Specular
	}

	/// <summary>
	/// Cross-blended terrain textures need to modulate the color by the vertex color to smoothly blend between two textures.
	/// </summary>
	public enum StageVertexColor
	{
		Ignore,
		Modulate,
		InverseModulate
	}
}