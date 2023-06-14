/*
 * (C) 2023 Radrat Softworks
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
 
using System;

using SharpMik.Interfaces;
using SharpMik.Extentions;
using System.IO;
using SharpMik.Attributes;

using Nofun.Loader.MFXM;
using Nofun.Util;

using System.Runtime.InteropServices;

using NoAlloq;
using System.Linq;
using System.Collections.Generic;

namespace SharpMik.Loaders
{
    [ModFileExtentions(".mfxm")]
	public class MFXMLoader : IModLoader
	{
		private const int XMENVCNT = (12*2);
		private const int XMNOTECNT = (8*SharpMikCommon.Octave);

		/*========== Loader variables */
		private MFXMHeader mh;
        private ushort[] patternRowCount = null;
        private sbyte[] relativeNoteNumbers = null;
        static	int sampHeader=0;

		public MFXMLoader()
			: base()
		{
			m_ModuleType = "MFXM";
			m_ModuleVersion = "MFXM (Mophun XM)";
		}

		public override bool Test()
		{
            byte[] magic = new byte[4];

            if (m_Reader.Read_bytes(magic, 4))
            {
                if ((magic[0] == 'M') && (magic[1] == 'H') && (magic[2] == 'D') && (magic[3] == 'R'))
                {
                    return true;
                }
            }

            return false;
        }

		public override bool Init()
		{
			mh = new MFXMHeader();
			return true;
		}

		public override void Cleanup()
		{
		}

		byte[] XM_Convert(MFXMNote[] xmtracks,int place,ushort rows)
		{
			int t;
			byte note,ins,vol,eff,dat;

            UniReset();
			for(t=0;t<rows;t++) 
			{
				MFXMNote xmtrack = xmtracks[place++];
				note = xmtrack.note;
				ins  = xmtrack.ins;
				vol  = xmtrack.vol;
				eff  = xmtrack.eff;
				dat  = xmtrack.dat;

				if(note != 0)
				{
					if(note > XMNOTECNT)
						UniEffect(SharpMikCommon.Commands.UNI_KEYFADE,0);
					else
						UniNote(note-1);
				}
				if(ins != 0) 
					UniInstrument(ins-1);

				switch(vol>>4) {
					case 0x6: /* volslide down */
						if((vol&0xf)!=0 ) UniEffect(SharpMikCommon.Commands.UNI_XMEFFECTA,vol&0xf);
						break;
					case 0x7: /* volslide up */
						if((vol&0xf)!=0) UniEffect(SharpMikCommon.Commands.UNI_XMEFFECTA,vol<<4);
						break;

						/* volume-row fine volume slide is compatible with protracker
						   EBx and EAx effects i.e. a zero nibble means DO NOT SLIDE, as
						   opposed to 'take the last sliding value'. */
					case 0x8: /* finevol down */
						UniPTEffect(0xe,0xb0|(vol&0xf));
						break;
					case 0x9: /* finevol up */
						UniPTEffect(0xe,0xa0|(vol&0xf));
						break;
					case 0xa: /* set vibrato speed */
						UniEffect(SharpMikCommon.Commands.UNI_XMEFFECT4,vol<<4);
						break;
					case 0xb: /* vibrato */
						UniEffect(SharpMikCommon.Commands.UNI_XMEFFECT4,vol&0xf);
						break;
					case 0xc: /* set panning */
						UniPTEffect(0x8,vol<<4);
						break;
					case 0xd: /* panning slide left (only slide when data not zero) */
						if((vol&0xf)!=0) UniEffect(SharpMikCommon.Commands.UNI_XMEFFECTP,vol&0xf);
						break;
					case 0xe: /* panning slide right (only slide when data not zero) */
						if((vol&0xf)!=0) UniEffect(SharpMikCommon.Commands.UNI_XMEFFECTP,vol<<4);
						break;
					case 0xf: /* tone porta */
						UniPTEffect(0x3,vol<<4);
						break;
					default:
						if((vol>=0x10)&&(vol<=0x50))
							UniPTEffect(0xc,vol-0x10);
						break;
				}

				if (eff != 0xFF)
				{
					switch(eff) {
						// These are same
						case 0x0:
						case 0x1:
						case 0x2:
						case 0x3:
						case 0x5:
						case 0x7:
							UniPTEffect(eff,dat);
							break;

						case 0x4:
							UniEffect(SharpMikCommon.Commands.UNI_XMEFFECT4,dat);
							break;
						case 0x6:
							UniEffect(SharpMikCommon.Commands.UNI_XMEFFECT6,dat);
							break;

                        // Can't find it in Symbian runtime, skip
                        case 0x1A:
                            break;

                        // Panning is dropped, and extensions are flatten into effect number
                        default:
							/* the pattern jump destination is written in decimal,
								but it seems some poor tracker software writes them
								in hexadecimal... (sigh) */
							if (eff==0xc) {
								/* don't change anything if we're sure it's in hexa */
								if ((((dat&0xf0)>>4)<=9)&&((dat&0xf)<=9))
									/* otherwise, convert from dec to hex */
									dat=(byte)((((dat&0xf0)>>4)*10)+(dat&0xf));
							}
							if ((eff == 0x14) || (eff > 0x16))
							{
								throw new Exception($"Unimplemented effect {eff}");
							}
							UniMFXMEffect(eff, dat);
							break;
					}
				}
				UniNewline();				
			}
            return UniDup();
		}

		bool LoadPatterns(bool dummypat)
		{
			m_Module.AllocTracks();
			m_Module.AllocPatterns();

            // Decrypt pattern data
            Span<short> maxAndExtendedOffsetBits = stackalloc short[2];
            uint compressedSize = 0;

            if (m_Reader.Read(MemoryMarshal.Cast<short, byte>(maxAndExtendedOffsetBits)) != 4)
            {
                throw new InvalidDataException("Failed to read max and extended offset bits!");
            }

            if (m_Reader.Read(MemoryMarshal.Cast<uint, byte>(MemoryMarshal.CreateSpan(ref compressedSize, 1))) != 4)
            {
                throw new InvalidDataException("Failed to read compressed data size!");
            }

            Memory<byte> compressedPatternData = new byte[compressedSize];
            if (m_Reader.Read(compressedPatternData.Span) != compressedSize)
            {
                throw new InvalidDataException("Failed to read unknown compressed data");
            }

            // Calculate destination size for extract
            // Each pattern note is 5 bytes
            int finalDestSize = patternRowCount.Aggregate(0, (acc, x) => acc + (int)x * mh.channelCount * 5);
            byte[] destPatternData = new byte[finalDestSize];

            if (CompressionUtil.TryLZDecompressContent(new MemoryBitStream(compressedPatternData), destPatternData,
                (byte)maxAndExtendedOffsetBits[1], (byte)maxAndExtendedOffsetBits[0]) != finalDestSize)
            {
                throw new Exception("Failed to decompress pattern data!");
            }

            int numtrk = 0;
            int offset = 0;
            int noteSize = Marshal.SizeOf<MFXMNote>();

            int t = 0;

            for(t = 0; t < mh.patternCount; t++) 
			{
                ushort rowCount = patternRowCount[t];
                
				m_Module.pattrows[t] = rowCount;

                if (rowCount != 0) 
				{
					MFXMNote[] xmpat = new MFXMNote[rowCount * m_Module.numchn];

					for (int u = 0; u < rowCount; u++) 
					{
						for(int v = 0; v < m_Module.numchn; v++) 
						{
							// Can't cast directly due to how these notes are laid out (plane channel, not interleaved)
							xmpat[v * rowCount + u] = MemoryMarshal.Cast<byte, MFXMNote>(destPatternData.AsSpan(offset, noteSize))[0];
							offset += noteSize;
						}
					}

					if (m_Reader.isEOF()) 
					{
						m_LoadError = MMERR_LOADING_PATTERN;
						return false;
					}

					for (int v=0; v < m_Module.numchn; v++)
                    {
						m_Module.tracks[numtrk++] = XM_Convert(xmpat, v * rowCount, rowCount);
                    }

					xmpat = null;
				} 
				else 
				{
					for (int v=0; v<m_Module.numchn; v++)
                    {
						m_Module.tracks[numtrk++] = XM_Convert(null, 0, rowCount);
                    }
				}
			}

			if(dummypat) 
			{
				m_Module.pattrows[t] = 64;
				MFXMNote[] xmpat = new MFXMNote[64 * m_Module.numchn];

				for(int v = 0; v < m_Module.numchn; v++)
				{
					m_Module.tracks[numtrk++]=XM_Convert(xmpat, v * 64, 64);
				}
			}

			return true;
		}

        bool LoadSamples()
        {
            m_Module.AllocSamples();
            relativeNoteNumbers = new sbyte[mh.sampleCount];

            for (int i = 0; i < mh.sampleCount; i++)
            {
                MFXMSampleHeader header = new MFXMSampleHeader();
                if (m_Reader.Read(MemoryMarshal.Cast<MFXMSampleHeader, byte>(MemoryMarshal.CreateSpan<MFXMSampleHeader>(ref header, 1))) != Marshal.SizeOf<MFXMSampleHeader>())
                {
                    m_LoadError = MMERR_LOADING_SAMPLEINFO;
                    throw new InvalidDataException("Failed to read sample header!");
                }

                byte relativeNoteNumber = 0;

				if (header.headerSize > 18)
				{
					int left = header.headerSize - 18;
					// Usually they pad one more byte for alignment, we dont need 2
					if (left >= 1)
					{
						if (m_Reader.Read(MemoryMarshal.CreateSpan(ref relativeNoteNumber, 1)) != 1)
						{
							m_LoadError = MMERR_LOADING_SAMPLEINFO;
							throw new InvalidDataException("Failed to read unknown word!");
						}
						left -= 1;
					}
					m_Reader.Seek(left, SeekOrigin.Current);
				}

                SAMPLE sample = m_Module.samples[i];

                sample.loopstart = header.loopStart;
                sample.loopend = header.loopEnd;
				sample.length = header.sampleLengthInBytes;
                sample.samplename = $"Sample {i}";
                sample.seekpos = (uint)m_Reader.Tell();
				sample.volume = header.volume;
				sample.speed = (uint)(header.finetune + 128);

				// No panning on where technically mono is used on phones
                sample.panning = 127;

				// Put it temporarily in here
                relativeNoteNumbers[i] = (sbyte)relativeNoteNumber;

                if (sample.length > 0)
				{
					bool is16bit = ((header.flags & 4) != 0);
					
					sample.flags |= SharpMikCommon.SF_OWNPAN | SharpMikCommon.SF_SIGNED;
					if (is16bit)
					{
						sample.flags |= SharpMikCommon.SF_16BITS;
                    }

					if ((header.flags & 0x3) != 0)
					{
						sample.flags |= SharpMikCommon.SF_LOOP;
					}

					if ((header.flags & 0x2) != 0)
					{
						sample.flags |= SharpMikCommon.SF_BIDI;
					}

					if ((header.flags & 8) != 0)
					{
						sample.flags |= SharpMikCommon.SF_ADPCM;
                        sample.length <<= 1;
                    }
					else if (is16bit)
					{
						// If not ADPCM, we will sort out the length (for reading)
                        sample.length >>= 1;
					}

                    m_Reader.Seek((int)header.sampleLengthInBytes, SeekOrigin.Current);
                }
            }

            return true;
        }

		void FixEnvelope(EnvPt[] cur, int pts)
		{
			int u, old, tmp;
			EnvPt prev;
			int place = 0;
			/* Some broken XM editing program will only save the low byte
				of the position value. Try to compensate by adding the
				missing high byte. */

			prev = cur[place++];
			old = prev.pos;

			for (u = 1; u < pts; u++) 
			{
				if (cur[place].pos < prev.pos) 
				{
					if (cur[place].pos < 0x100) 
					{
						if (cur[place].pos > old)	/* same hex century */
							tmp = cur[place].pos + (prev.pos - old);
						else
						{
							int temp = cur[place].pos;
							temp = temp | ((prev.pos + 0x100) & 0xff00);
							tmp = temp;
						}
						old = cur[place].pos;
						cur[place].pos = (short)tmp;
					} 
					else 
					{
						old = cur[place].pos;
					}
				} 
				else
					old = cur[place].pos;

				prev = cur[place++];
			}
		}

		void XM_ProcessEnvelopeVolume(ref INSTRUMENT d, MFXMPointsHeader header, short[] envelopes)
		{
			for (int u = 0; u < envelopes.Length >> 1; u++) 
			{					
				d.volenv[u].pos = envelopes[u << 1];		
				d.volenv[u].val = envelopes[(u << 1)+ 1];	
			}

			// 0x80 type is filter envelope. From IT probably
			if ((header.type & 1) != 0) d.volflg|=SharpMikCommon.EF_ON;				
			if ((header.type & 2) != 0) d.volflg|=SharpMikCommon.EF_SUSTAIN;		
			if ((header.type & 4) != 0) d.volflg|=SharpMikCommon.EF_LOOP;

            d.volsusbeg = d.volsusend = header.sustainPoint;
            d.volbeg = header.loopStart;							
			d.volend = header.loopEnd;							
			d.volpts = header.pointCount;							
																				
			/* scale envelope */									
			for (int p=0;p<XMENVCNT/2;p++)								
				d.volenv[p].val<<=2;							

			if ((d.volflg&SharpMikCommon.EF_ON) != 0&&(d.volpts<2))
			{
				int flag = d.volflg;
				flag &=~SharpMikCommon.EF_ON;
				d.volflg = (byte)flag;
			}
		}

		void XM_ProcessEnvelopePan(ref INSTRUMENT d, MFXMPointsHeader header, short[] envelopes)
		{
			for (int u = 0; u < envelopes.Length >> 1; u++) 
			{					
				d. panenv[u].pos = envelopes[u << 1];		
				d. panenv[u].val = envelopes[(u << 1)+ 1];	
			}								
						
			if ((header.type&1) != 0) d. panflg|=SharpMikCommon.EF_ON;				
			if ((header.type&2) != 0) d. panflg|=SharpMikCommon.EF_SUSTAIN;		
			if ((header.type&4) != 0) d. panflg|=SharpMikCommon.EF_LOOP;
	
			d.pansusbeg=d.pansusend=header.sustainPoint;		
			d.panbeg=header.loopStart;							
			d.panend=header.loopEnd;							
			d.panpts=header.pointCount;							

			/* scale envelope */									
			for (int p=0;p<XMENVCNT/2;p++)								
				d.panenv[p].val<<=2;							

			if ((d.panflg&SharpMikCommon.EF_ON) != 0&&(d.panpts<2))
			{
				int flag = d.panflg;
				flag &=~SharpMikCommon.EF_ON;
				d.panflg = (byte)flag;
			}
		}

		bool LoadInstruments()
		{
			if(!m_Module.AllocInstruments())
			{
				m_LoadError = MMERR_NOT_A_MODULE;
				return false;
			}

            for (int t = 0; t < m_Module.numins; t++)
            {
                INSTRUMENT d = m_Module.instruments[t];
                MFXMInstrumentHeader instrumentHeader = new MFXMInstrumentHeader();

                if (m_Reader.Read(MemoryMarshal.Cast<MFXMInstrumentHeader, byte>(MemoryMarshal.CreateSpan(ref instrumentHeader, 1))) != Marshal.SizeOf<MFXMInstrumentHeader>())
                {
                    m_LoadError = MMERR_LOADING_HEADER;
                    return false;
                }

                d.samplenumber.Memset(0xff, SharpMikCommon.INSTNOTES);

                d.insname = $"Instrument {t}";
                d.volfade = instrumentHeader.volumeFade;

                byte[] sampleOrders = new byte[instrumentHeader.sampleNumbersCount];
                if (m_Reader.Read(sampleOrders) != instrumentHeader.sampleNumbersCount)
                {
                    m_LoadError = MMERR_LOADING_HEADER;
                    return false;
                }

                for (int i = 0; i < instrumentHeader.sampleNumbersCount; i++)
                {
                    d.samplenumber[i] = sampleOrders[i];

					// Previously it's temporarily put in handle
					if (sampleOrders[i] > m_Module.numsmp)
					{
                        d.samplenote[i] = 255;
						d.samplenumber[i] = 255;
                    }
					else
					{
						int realNote = relativeNoteNumbers[sampleOrders[i]] + i;

                        realNote = Math.Clamp(realNote, 0, 255);
                        d.samplenote[i] = (byte)realNote;
					}
                }

                if ((instrumentHeader.flag & 4) != 0)
                {
                    byte vibFlag = m_Reader.Read_byte();
                    byte vibSweep = m_Reader.Read_byte();
                    byte vibDepth = m_Reader.Read_byte();
                    byte vibRate = m_Reader.Read_byte();

                    // Assign vibrator attributes to sample
                    for (int i = 0; i < instrumentHeader.sampleNumbersCount; i++)
                    {
                        SAMPLE referredSample = m_Module.samples[sampleOrders[i]];

                        referredSample.vibflags = vibFlag;
                        referredSample.vibsweep = vibSweep;
                        referredSample.vibdepth = vibDepth;
                        referredSample.vibrate = vibRate;
                    }
                }

                if ((instrumentHeader.flag & 1) != 0)
                {
                    MFXMPointsHeader pointsHeader = new MFXMPointsHeader();
                    if (m_Reader.Read(MemoryMarshal.Cast<MFXMPointsHeader, byte>(MemoryMarshal.CreateSpan(ref pointsHeader, 1))) != Marshal.SizeOf<MFXMPointsHeader>())
                    {
                        m_LoadError = MMERR_LOADING_HEADER;
                        return false;
                    }

                    short[] envelopes = new short[2 * pointsHeader.pointCount];
                    if (m_Reader.Read(MemoryMarshal.Cast<short, byte>(envelopes)) != pointsHeader.pointCount * 4)
                    {
                        m_LoadError = MMERR_LOADING_HEADER;
                        return false;
                    }

                    XM_ProcessEnvelopeVolume(ref d, pointsHeader, envelopes);
                }

                if ((instrumentHeader.flag & 2) != 0)
                {
                    MFXMPointsHeader pointsHeader = new MFXMPointsHeader();
                    if (m_Reader.Read(MemoryMarshal.Cast<MFXMPointsHeader, byte>(MemoryMarshal.CreateSpan(ref pointsHeader, 1))) != Marshal.SizeOf<MFXMPointsHeader>())
                    {
                        m_LoadError = MMERR_LOADING_HEADER;
                        return false;
                    }

                    short[] envelopes = new short[2 * pointsHeader.pointCount];
                    if (m_Reader.Read(MemoryMarshal.Cast<short, byte>(envelopes)) != pointsHeader.pointCount * 4)
                    {
                        m_LoadError = MMERR_LOADING_HEADER;
                        return false;
                    }

                    XM_ProcessEnvelopePan(ref d, pointsHeader, envelopes);
                }

                if ((d.volflg & SharpMikCommon.EF_ON) != 0)
                    FixEnvelope(d.volenv, d.volpts);
                if ((d.panflg & SharpMikCommon.EF_ON) != 0)
                    FixEnvelope(d.panenv, d.panpts);
            }

			return true;
		}

		public override bool Load(int curious)
		{
            int headerSize = Marshal.SizeOf<MFXMHeader>();

            // Reader header
            if (m_Reader.Read(MemoryMarshal.Cast<MFXMHeader, byte>(MemoryMarshal.CreateSpan<MFXMHeader>(ref mh, 1))) != headerSize)
            {
                m_LoadError = MMERR_LOADING_HEADER;
                return false;
            }

            // Now read pattern length each
            int dataLeft = mh.totalHeaderSectionSize - mh.songLength - headerSize;
            patternRowCount = new ushort[mh.patternCount];

            if (dataLeft > 0)
            {
                if (m_Reader.Read(MemoryMarshal.Cast<ushort, byte>(MemoryMarshal.CreateSpan<ushort>(ref patternRowCount[0], mh.patternCount))) != dataLeft)
                {
                    m_LoadError = MMERR_LOADING_HEADER;
                    return false;
                }
            }
            else
            {
                patternRowCount.AsSpan().Fill(mh.defaultRowCount);
            }

            Span<byte> orders = new byte[mh.songLength];
            if (m_Reader.Read(orders) != mh.songLength)
            {
                m_LoadError = MMERR_LOADING_HEADER;
                return false;
            }

			/* set module variables */
			m_Module.initspeed = (byte)5;
            m_Module.sngspd = mh.defaultSongSpeed;
            m_Module.inittempo = (ushort)mh.defaultBPM;

			m_Module.modtype = "Mophun tracker (probably from MPC2)";
			m_Module.numchn = (byte)mh.channelCount;
			m_Module.numpat = mh.patternCount;
			m_Module.numtrk  = (ushort)(m_Module.numpat*m_Module.numchn);   /* get number of channels */
			m_Module.songname = "Buon i buon dai buon ngu buon doi";        /* make a title up */
			m_Module.numpos = mh.songLength;               /* copy the songlength */
			m_Module.reppos = (ushort)(/*mh.restart<mh.songlength?mh.restart:*/0);
			m_Module.numins = mh.instrumentCount;
            m_Module.numsmp = mh.sampleCount;
            m_Module.flags |= SharpMikCommon.UF_XMPERIODS | SharpMikCommon.UF_INST | SharpMikCommon.UF_NOWRAP | SharpMikCommon.UF_FT2QUIRKS | SharpMikCommon.UF_PANNING;
			if((mh.flags & 1) != 0)
				m_Module.flags |= SharpMikCommon.UF_LINEAR;

			m_Module.bpmlimit = 32;
			m_Module.chanvol.Memset(64, m_Module.numchn);			/* store channel volumes */

			m_Module.positions = orders.Select(x => (ushort)x).ToArray();

            /* We have to check for any pattern numbers in the order list greater than
			   the number of patterns total. If one or more is found, we set it equal to
			   the pattern total and make a dummy pattern to workaround the problem */
            bool dummypat = false;
            
            for(int t = 0; t < m_Module.numpos; t++) {
				if (m_Module.positions[t] >= m_Module.numpat)
                {
					m_Module.positions[t] = m_Module.numpat;
					dummypat = true;
				}
			}

			if (dummypat)
            {
				m_Module.numpat++;
                m_Module.numtrk += m_Module.numchn;
			}

            if (!LoadPatterns(dummypat))
            {
                return false;
            }

            if (!LoadSamples())
            {
                return false;
            }

            if(!LoadInstruments())
			{
 				return false;
			}

			return true;
        }

		public override string LoadTitle()
		{
			throw new NotImplementedException();
		}
	}
}
