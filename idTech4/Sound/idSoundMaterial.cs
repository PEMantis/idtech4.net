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
using System.IO;
using System.Linq;
using System.Text;

using idTech4.Services;
using idTech4.Text;

namespace idTech4.Sound
{
	public class idSoundMaterial : idDecl
	{
		#region Members		
		private bool _leadIn;
		private float _leadInVolume;
		private int _speakerMask;

		private idSoundMaterial _altSound;
		private SoundMaterialParameters _parameters = new SoundMaterialParameters();
		private List<object /*TODO: idList<idSoundSample*/> _entries = new List<object>();
		#endregion

		#region Constructor
		public idSoundMaterial()
			: base()
		{
			_parameters = new SoundMaterialParameters();
		}
		#endregion

		#region Methods
		#region Private
		private bool ParseMaterial(idLexer lexer)
		{
			IDeclManager declManager = idEngine.Instance.GetService<IDeclManager>();

			_parameters.MinDistance = 1;
			_parameters.MaxDistance = 10;
			_parameters.Volume = 1;
			_parameters.Shakes = 0;
			_parameters.Flags = 0;
			_parameters.SoundClass = 0;

			_speakerMask = 0;
			_altSound = null;

			idToken token;
			string tokenValue;

			_entries.Clear();

			while(true)
			{
				if((token = lexer.ExpectAnyToken()) == null)
				{
					return false;
				}

				tokenValue = token.ToString().ToLower();

				if(tokenValue == "}")
				{
					break;
				}
				// minimum number of sounds
				else if(tokenValue == "minsamples")
				{
					lexer.ParseInt();
				}
				else if(tokenValue == "description")
				{
					lexer.ReadTokenOnLine().ToString();
				}
				else if(tokenValue == "mindistance")
				{
					_parameters.MinDistance = lexer.ParseFloat();
				}
				else if(tokenValue == "maxdistance")
				{
					_parameters.MaxDistance = lexer.ParseFloat();
				}
				else if(tokenValue == "shakes")
				{
					token = lexer.ExpectAnyToken();

					if(token.Type == TokenType.Number)
					{
						_parameters.Shakes = token.ToFloat();
					}
					else
					{
						lexer.UnreadToken = token;
						_parameters.Shakes = 1.0f;
					}
				}
				else if(tokenValue == "reverb")
				{
					lexer.ParseFloat();

					if(lexer.ExpectTokenString(",") == false)
					{
						return false;
					}

					lexer.ParseFloat();
					// no longer supported
				}
				else if(tokenValue == "volume")
				{
					_parameters.Volume = lexer.ParseFloat();
				}
				// leadinVolume is used to allow light breaking leadin sounds to be much louder than the broken loop
				else if(tokenValue == "leadinvolume")
				{
					_leadInVolume = lexer.ParseFloat();
					_leadIn = true;
				}
				else if(tokenValue == "mask_center")
				{
					_speakerMask |= 1 << (int) Speakers.Center;
				}
				else if(tokenValue == "mask_left")
				{
					_speakerMask |= 1 << (int) Speakers.Left;
				}
				else if(tokenValue == "mask_right")
				{
					_speakerMask |= 1 << (int) Speakers.Right;
				}
				else if(tokenValue == "mask_backright")
				{
					_speakerMask |= 1 << (int) Speakers.BackRight;
				}
				else if(tokenValue == "mask_backleft")
				{
					_speakerMask |= 1 << (int) Speakers.BackLeft;
				}
				else if(tokenValue == "mask_lfe")
				{
					_speakerMask |= 1 << (int) Speakers.Lfe;
				}
				else if(tokenValue == "soundclass")
				{
					_parameters.SoundClass = lexer.ParseInt();

					if(_parameters.SoundClass < 0)
					{
						lexer.Warning("SoundClass out of range");
						return false;
					}
				}
				else if(tokenValue == "altsound")
				{
					if((token = lexer.ExpectAnyToken()) == null)
					{
						return false;
					}

					_altSound = declManager.FindSound(token.ToString());
				}
				else if(tokenValue == "ordered")
				{
					// no longer supported
				}
				else if(tokenValue == "no_dups")
				{
					_parameters.Flags |= SoundMaterialFlags.NoDuplicates;
				}
				else if(tokenValue == "no_flicker")
				{
					_parameters.Flags |= SoundMaterialFlags.NoFlicker;
				}
				else if(tokenValue == "plain")
				{
					// no longer supported
				}
				else if(tokenValue == "looping")
				{
					_parameters.Flags |= SoundMaterialFlags.Looping;
				}
				else if(tokenValue == "no_occlusion")
				{
					_parameters.Flags |= SoundMaterialFlags.NoOcclusion;
				}
				else if(tokenValue == "private")
				{
					_parameters.Flags |= SoundMaterialFlags.PrivateSound;
				}
				else if(tokenValue == "antiprivate")
				{
					_parameters.Flags |= SoundMaterialFlags.AntiPrivateSound;
				}
				else if(tokenValue == "playonce")
				{
					_parameters.Flags |= SoundMaterialFlags.PlayOnce;
				}
				else if(tokenValue == "global")
				{
					_parameters.Flags |= SoundMaterialFlags.Global;
				}
				else if(tokenValue == "unclamped")
				{
					_parameters.Flags |= SoundMaterialFlags.Unclamped;
				}
				else if(tokenValue == "omnidirectional")
				{
					_parameters.Flags |= SoundMaterialFlags.OmniDirectional;
				}
				// onDemand can't be a parms, because we must track all references and overrides would confuse it
				else if(tokenValue == "ondemand")
				{
					// no longer loading sounds on demand
				}
				// the wave files
				else if(tokenValue == "leadin")
				{
					_leadIn = true;
				}
				else if((tokenValue.EndsWith(".wav") == true) || (tokenValue.EndsWith(".ogg") == true))
				{
					idLog.Warning("TODO: .wav|.ogg");

					/*if ( token.IcmpPrefixPath( "sound/vo/" ) == 0 || token.IcmpPrefixPath( "sound/guis/" ) == 0 ) {
						parms.soundShaderFlags |= SSF_VO;
					}
					if ( token.IcmpPrefixPath( "sound/musical/" ) == 0 ) {
						parms.soundShaderFlags |= SSF_MUSIC;
					}
					// add to the wav list
					if ( s_maxSamples.GetInteger() == 0 || ( s_maxSamples.GetInteger() > 0 && entries.Num() < s_maxSamples.GetInteger() ) ) {
						entries.Append( soundSystemLocal.LoadSample( token.c_str() ) );
					}*/
				}
				else
				{
					lexer.Warning("unknown token '{0}'", token.ToString());
					return false;
				}
			}
			
			return true;
		}
		#endregion
		#endregion

		#region idDecl implementation
		#region Properties
		public override string DefaultDefinition
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return "{\n\t_default.wav\n}";
			}
		}

		public override int MemoryUsage
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idLog.Warning("TODO: idSoundMaterial.MemoryUsage");

				return 0;
			}
		}
		#endregion

		#region Methods
		#region Public
		public override bool Parse(string text)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idLexer lexer = new idLexer(idDeclFile.LexerOptions);
			lexer.LoadMemory(text, this.FileName, this.LineNumber);
			lexer.SkipUntilString("{");
			
			if(ParseMaterial(lexer) == false)
			{
				MakeDefault();
				return false;
			}

			return true;
		}
		#endregion

		#region Protected
		protected override bool GenerateDefaultText()
		{
			string name = this.Name;

			if(Path.GetExtension(this.Name) == "")
			{
				name += ".wav";
			}

			// if there exists a wav file with the same name
			if(true) //fileSystem->ReadFile( wavname, NULL ) != -1 )
			{
				this.SourceText = string.Format("sound {0} // IMPLICITLY GENERATED\n{{\n{1}\n}}\n", this.Name, name);
			}
			else
			{
				return false;
			}

			return true;
		}
		#endregion
		#endregion
		#endregion
	}

	[Flags]
	public enum SoundMaterialFlags
	{
		/// <summary>Only plays for the current listenerId.</summary>
		PrivateSound = 1 << 0,
		/// <summary>Plays for everyone but the current listenerId.</summary>
		AntiPrivateSound = 1 << 1,
		/// <summary>Don't flow through portals, only use straight line.</summary>
		NoOcclusion = 1 << 2,
		/// <summary>Play full volume to all speakers and all listeners.</summary>
		Global = 1 << 3,
		/// <summary>Fall off with distance, but play same volume in all speakers.</summary>
		OmniDirectional = 1 << 4,
		/// <summary>Repeat the sound continuously.</summary>
		Looping = 1 << 5,
		/// <summary>Never restart if already playing on any channel of a given emitter.</summary>
		PlayOnce = 1 << 6,
		/// <summary>Don't clamp calculated volumes at 1.0.</summary>
		Unclamped = 1 << 7,
		/// <summary>Always return 1.0 for volume queries.</summary>
		NoFlicker = 1 << 8,
		/// <summary>Try not to play the same sound twice in a row.</summary>
		NoDuplicates = 1 << 9
	}

	[Flags]
	public enum Speakers
	{
		Left = 0,
		Right,
		Center,
		Lfe,
		BackLeft,
		BackRight
	}

	/// <summary>
	/// These options can be overriden from sound shader defaults on a per-emitter and per-channel basis.
	/// </summary>
	public class SoundMaterialParameters
	{
		public float MinDistance;
		public float MaxDistance;

		/// <summary>
		/// In dB, negative values get quieter.
		/// </summary>
		public float Volume;

		public float Shakes;

		public SoundMaterialFlags Flags;

		/// <summary>
		/// For global fading of sounds.
		/// </summary>
		public int SoundClass;
	}
}