using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpMik.Interfaces;

namespace SharpMik.Player
{
	/*
	 * Keeps tracks of user settings like pan separation and global volume.
	 * Also holds the selected driver and passes data to it.
	 * 
	 * Been trying to keep the user accessible variables hidden with accessors to access them.
	 * 
	 * Also strangly contains the MikMod_* functions that do alot of setting up.
	 * 
	 */
	public class ModDriver
	{
		#region private variables
		private IModDriver m_Driver;
		private SampleLoader m_sampleLoader;

		private ushort md_mode = SharpMikCommon.DMODE_STEREO | SharpMikCommon.DMODE_16BITS |
						SharpMikCommon.DMODE_SURROUND | SharpMikCommon.DMODE_SOFT_MUSIC |
						SharpMikCommon.DMODE_SOFT_SNDFX;


        internal byte md_numchn = 0;
        internal byte md_sngchn = 0;
        internal byte md_sfxchn = 0;

        private byte md_hardchn = 0;
        private byte md_softchn = 0;
		private ushort md_mixfreq = 44100;
		private ushort md_bpm = 125;
		private byte md_reverb = 0;
		private byte[] sfxinfo;

		private byte md_pansep         = 128;	/* 128 == 100% (full left/right) */
		private byte md_volume         = 128;	/* global sound volume (0-128) */
		private byte md_musicvolume    = 128;	/* volume of song */
		private byte md_sndfxvolume = 128;	/* volume of sound effects */

		private SAMPLE[] md_sample;
		#endregion
        

		#region Accessors
		public byte PanSeperation
		{
			get { return md_pansep; }
			set { md_pansep = value < 129 ? value: (byte)128; }
		}

		public byte SoftwareChannel
		{
			get { return md_softchn; }
		}


		public int ChannelCount
		{
			get { return md_numchn; }
			set { md_numchn = (byte)value; }
		}

		public ushort Mode
		{
			get { return md_mode; }
			set { md_mode = value; }
		}

		public ushort MixFrequency
		{
			get { return md_mixfreq; }
			set { md_mixfreq = value; }
		}
		public ushort Bpm
		{
			get { return md_bpm; }
			set { md_bpm = value; }
		}

		public byte Reverb
		{
			get { return md_reverb; }
			set { md_reverb = value; }
		}

		public byte SoundFXChannel
		{
			get { return md_sfxchn; }
			set { md_sfxchn = value; }
		}

		public byte GlobalVolume
		{
			get { return md_volume; }
			set { md_volume = value < 129 ? value : (byte)128; }
		}

		public byte MusicVolume
		{
			get { return md_musicvolume; }
			set { md_musicvolume = value < 129 ? value : (byte)128; }
		}

		public IModDriver Driver
		{
			get { return m_Driver; }
		}

		public SampleLoader SampleLoader
		{
			get { return m_sampleLoader; }
		}
		#endregion

		public ModDriver(IModDriver driver)
		{
			m_Driver = driver;
			m_sampleLoader = new SampleLoader(this);
		}

		internal short MD_SampleLoad(SAMPLOAD s, int type)
		{
			short result = -1;

			if (m_Driver != null)
			{
				if (type == (int)SharpMikCommon.MDTypes.MD_MUSIC)
				{
					type = (int)((md_mode & SharpMikCommon.DMODE_SOFT_MUSIC) == SharpMikCommon.DMODE_SOFT_MUSIC ? SharpMikCommon.MDDecodeTypes.MD_SOFTWARE : SharpMikCommon.MDDecodeTypes.MD_HARDWARE);
				}
				else if (type == (int)SharpMikCommon.MDTypes.MD_SNDFX)
				{
					type = (int)((md_mode & SharpMikCommon.DMODE_SOFT_SNDFX) == SharpMikCommon.DMODE_SOFT_SNDFX ? SharpMikCommon.MDDecodeTypes.MD_SOFTWARE : SharpMikCommon.MDDecodeTypes.MD_HARDWARE);
				}

				m_sampleLoader.SL_Init(s);
				result = m_Driver.SampleLoad(s, type);
				m_sampleLoader.SL_Exit(s);
			}

			return result;
		}

		internal short[] MD_GetSample(short handle)
		{
			if (m_Driver != null)
			{
				return m_Driver.GetSample(handle);
			}

			return null;
		}

		internal short MD_SetSample(short[] sample)
		{
			if (m_Driver != null)
			{
				return m_Driver.SetSample(sample);
			}

			return -1;
		}


		/* Note: 'type' indicates whether the returned value should be for music or for sound effects. */
		public uint MD_SampleSpace(int type)
		{
			if (type == (int)SharpMikCommon.MDTypes.MD_MUSIC)
				type = (int)((md_mode & SharpMikCommon.DMODE_SOFT_MUSIC) != 0 ? SharpMikCommon.MDDecodeTypes.MD_SOFTWARE : SharpMikCommon.MDDecodeTypes.MD_HARDWARE);
			else if (type == (int)SharpMikCommon.MDTypes.MD_SNDFX)
				type = (int)((md_mode & SharpMikCommon.DMODE_SOFT_SNDFX) != 0 ? SharpMikCommon.MDDecodeTypes.MD_SOFTWARE : SharpMikCommon.MDDecodeTypes.MD_HARDWARE);

			return m_Driver.FreeSampleSpace(type);
		}


		public void Voice_Stop_internal(byte voice)
		{
			if ((voice < 0) || (voice >= md_numchn)) return;
			if (voice >= md_sngchn)
			{
				/* It is a sound effects channel, so flag the voice as non-critical! */
				sfxinfo[voice - md_sngchn] = 0;
			}
			m_Driver.VoiceStop(voice);
		}

		internal bool Voice_Stopped_internal(sbyte voice)
		{
			if ((voice < 0) || (voice >= md_numchn)) 
				return false;

			return (m_Driver.VoiceStopped((byte)voice));
		}


		internal void Voice_Play_internal(sbyte voice, SAMPLE s, uint start)
		{
			uint repend;

			if ((voice < 0) || (voice >= md_numchn)) return;

			md_sample[voice] = s;
			repend = s.loopend;

			if ((s.flags & SharpMikCommon.SF_LOOP) == SharpMikCommon.SF_LOOP)
			{
				/* repend can't be bigger than size */
				if (repend > s.length) 
					repend = s.length;
			}
			
			m_Driver.VoicePlay((byte)voice, s.handle, start, s.length, s.loopstart, repend, s.flags);
		}

		internal void Voice_SetVolume_internal(sbyte voice, ushort vol)
		{
			uint tmp;

			if ((voice < 0) || (voice >= md_numchn)) return;

			/* range checks */
			if (md_musicvolume > 128) 
				md_musicvolume = 128;
			
			if (md_sndfxvolume > 128) 
				md_sndfxvolume = 128;
			
			if (md_volume > 128) 
				md_volume = 128;

			tmp =(uint)(vol * md_volume * ((voice < md_sngchn) ? md_musicvolume : md_sndfxvolume));
			m_Driver.VoiceSetVolume((byte)voice, (ushort)(tmp / 16384));
		}

		internal void Voice_SetPanning_internal(sbyte voice, uint pan)
		{
			if ((voice < 0) || (voice >= md_numchn)) 
				return;
			
			if (pan != SharpMikCommon.PAN_SURROUND)
			{
				if (md_pansep > 128) md_pansep = 128;
				if ((md_mode & SharpMikCommon.DMODE_REVERSE) == SharpMikCommon.DMODE_REVERSE)
					pan = 255 - pan;
				pan = (uint)((((short)(pan - 128) * md_pansep) / 128) + 128);
			}
			m_Driver.VoiceSetPanning((byte)voice, pan);
		}

		internal void Voice_SetFrequency_internal(sbyte voice, uint frq)
		{
			if ((voice < 0) || (voice >= md_numchn)) 
				return;
			
			if ((md_sample[voice] != null) && (md_sample[voice].divfactor != 0))
			{
				frq /= md_sample[voice].divfactor;
			}
			
			m_Driver.VoiceSetFrequency((byte)voice, frq);
		}

		private void LimitHardVoices(int limit)
		{
			int t = 0;

			if (!((md_mode & SharpMikCommon.DMODE_SOFT_SNDFX) == SharpMikCommon.DMODE_SOFT_SNDFX) && (md_sfxchn > limit)) 
				md_sfxchn = (byte)limit;

			if (!((md_mode & SharpMikCommon.DMODE_SOFT_MUSIC) == SharpMikCommon.DMODE_SOFT_MUSIC) && (md_sngchn > limit)) 
				md_sngchn = (byte)limit;

			if (!((md_mode & SharpMikCommon.DMODE_SOFT_SNDFX) == SharpMikCommon.DMODE_SOFT_SNDFX))
			{
				md_hardchn = md_sfxchn;
			}
			else
			{
				md_hardchn = 0;
			}

			if (!((md_mode & SharpMikCommon.DMODE_SOFT_MUSIC) == SharpMikCommon.DMODE_SOFT_MUSIC))
			{
				md_hardchn += md_sngchn;
			}

			while (md_hardchn > limit)
			{
				if ((++t & 1) == 1)
				{
					if (!((md_mode & SharpMikCommon.DMODE_SOFT_SNDFX) == SharpMikCommon.DMODE_SOFT_SNDFX) && (md_sfxchn > 4))
					{
						md_sfxchn--;
					}
				}
				else
				{
					if (!((md_mode & SharpMikCommon.DMODE_SOFT_MUSIC) == SharpMikCommon.DMODE_SOFT_MUSIC) && (md_sngchn > 8))
					{
						md_sngchn--;
					}
				}

				if (!((md_mode & SharpMikCommon.DMODE_SOFT_SNDFX) == SharpMikCommon.DMODE_SOFT_SNDFX))
				{
					md_hardchn = md_sfxchn;
				}
				else
				{
					md_hardchn = 0;
				}

				if (!((md_mode & SharpMikCommon.DMODE_SOFT_MUSIC) == SharpMikCommon.DMODE_SOFT_MUSIC))
				{
					md_hardchn += md_sngchn;
				}
			}

			md_numchn = (byte)(md_hardchn + md_softchn);
		}




		private void LimitSoftVoices(int limit)
		{
			int t = 0;

			if ((md_mode & SharpMikCommon.DMODE_SOFT_SNDFX) == SharpMikCommon.DMODE_SOFT_SNDFX && (md_sfxchn > limit)) 
				md_sfxchn = (byte)limit;

			if ((md_mode & SharpMikCommon.DMODE_SOFT_MUSIC) == SharpMikCommon.DMODE_SOFT_MUSIC && (md_sngchn > limit))
				md_sngchn = (byte)limit;

			if ((md_mode & SharpMikCommon.DMODE_SOFT_SNDFX) == SharpMikCommon.DMODE_SOFT_SNDFX)
				md_softchn = md_sfxchn;
			else
				md_softchn = 0;

			if ((md_mode & SharpMikCommon.DMODE_SOFT_MUSIC) == SharpMikCommon.DMODE_SOFT_MUSIC)
				md_softchn += md_sngchn;

			while (md_softchn > limit)
			{
				if ((++t & 1) == 1)
				{
					if (((md_mode & SharpMikCommon.DMODE_SOFT_SNDFX) == SharpMikCommon.DMODE_SOFT_SNDFX) && (md_sfxchn > 4)) 
						md_sfxchn--;
				}
				else
				{
					if (((md_mode & SharpMikCommon.DMODE_SOFT_MUSIC) == SharpMikCommon.DMODE_SOFT_MUSIC) && (md_sngchn > 8)) 
						md_sngchn--;
				}

				if (!((md_mode & SharpMikCommon.DMODE_SOFT_SNDFX) == SharpMikCommon.DMODE_SOFT_SNDFX))
					md_softchn = md_sfxchn;
				else
					md_softchn = 0;

				if (!((md_mode & SharpMikCommon.DMODE_SOFT_MUSIC) == SharpMikCommon.DMODE_SOFT_MUSIC))
					md_softchn += md_sngchn;
			}
			md_numchn = (byte)(md_hardchn + md_softchn);
		}







		// should be moved into own file..
		#region milkmod stuff 
		internal bool isplaying = false;
		private bool initialized = false;

		private bool MikMod_Active_internal()
		{
			return isplaying;
		}

		public bool MikMod_Active()
		{
			return MikMod_Active_internal();
		}


		private bool MikMod_EnableOutput_internal()
		{
			if(!isplaying) 
			{
				if (m_Driver.PlayStart())
				{
					return true;
				}

				isplaying = true;
			}

			return false;
		}

		public bool MikMod_EnableOutput()
		{
			return MikMod_EnableOutput_internal();
		}



		public void MikMod_Update()
		{
			if (isplaying)
			{
				m_Driver.Update();
			}
		}


		public void Driver_Pause(bool pause)
		{
			if (isplaying)
			{
				if (pause)
				{
					m_Driver.Pause();
				}
				else
				{
					m_Driver.Resume();
				}
			}
		}

		internal void MikMod_DisableOutput_internal()
		{
			if(isplaying && m_Driver != null) 
			{
				isplaying = false;
				m_Driver.PlayStop();
			}
		}

		public bool MikMod_SetNumVoices_internal(int music, int sfx)
		{
			bool resume = false;
			int t, oldchn = 0;

			if ((music == 0) && (sfx == 0))
			{
				return true;
			}

			
			if (isplaying)
			{
				MikMod_DisableOutput_internal();
				oldchn = md_numchn;
				resume = true;
			}

			if (sfxinfo != null)
			{
				sfxinfo = null;
			}

			if (md_sample != null)
			{
				md_sample = null;
			}

			if (music != -1)
			{
				md_sngchn = (byte)music;
			}

			if (sfx != -1)
			{
				md_sfxchn = (byte)sfx;
			}

			md_numchn = (byte)(md_sngchn + md_sfxchn);

			LimitHardVoices(m_Driver.HardVoiceLimit);
			LimitSoftVoices(m_Driver.SoftVoiceLimit);

			if (m_Driver.SetNumVoices())
			{
				MikMod_Exit_internal();
				md_numchn = md_softchn = md_hardchn = md_sfxchn = md_sngchn = 0;
				return true;
			}

			if ((md_sngchn + md_sfxchn) != 0)
			{
				md_sample = new SAMPLE[md_sngchn + md_sfxchn];
				for (int i = 0; i < md_sngchn + md_sfxchn; i++)
				{
					md_sample[i] = new SAMPLE();
				}
			}

			if (md_sfxchn != 0)
			{
				sfxinfo = new byte[md_sfxchn];
			}

			/* make sure the player doesn't start with garbage */
			for (t = oldchn; t < md_numchn; t++)
			{
				Voice_Stop_internal((byte)t);
			}
			
			if (resume)
			{
				MikMod_EnableOutput_internal();
			}

			return false;
		}


		private void MikMod_Exit_internal()
		{
			MikMod_DisableOutput_internal();
			m_Driver.Exit();
			md_numchn = md_sfxchn = md_sngchn = 0;

			if (sfxinfo != null)
				sfxinfo = null;

			if (md_sample != null)
				md_sample = null;

			initialized = false;
		}

		public void MikMod_Exit()
		{	
			MikMod_Exit_internal();
		}


		private bool _mm_init(ModPlayer player, string cmdline)
		{
			m_Driver.CommandLine(cmdline);

			if (m_Driver.IsPresent())
			{
				if (m_Driver.Init(player))
				{
					MikMod_Exit_internal();
					m_Driver = null;
					throw new Exception("Failed to init driver!");
				}
			}
			else
			{
				throw new Exception("Driver not present!");
			}

			initialized = true;

			return false;
		}

		public bool MikMod_Init(ModPlayer player, string cmdline)
		{
			return _mm_init(player, cmdline);
		}

		public bool MikMod_Reset(ModPlayer player, string cmdline)
		{
			return _mm_reset(player, cmdline);
		}

		private bool _mm_reset(ModPlayer player, string cmdline)
		{
			bool wasplaying = false;

			if (!initialized) 
				return _mm_init(player, cmdline);

			if (isplaying)
			{
				wasplaying = true;
				m_Driver.PlayStop();
			}

			if (m_Driver.Reset())
			{
				MikMod_Exit_internal();
				return true;
			}

			if (wasplaying) 
				m_Driver.PlayStart();
			return false;
		}

		#endregion
	}
}
