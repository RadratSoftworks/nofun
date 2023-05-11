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

namespace Nofun.Util
{
    public static class CompressionUtil
    {
        public static int TryLZDecompressContent(BitStream source, Span<byte> dest, byte extendedOffsetBits,
            byte maxOffsetBits)
        {
            uint destPointer = 0;

            while (true)
            {
                ulong flags = source.ReadBits(1);

                if (flags == 1)
                {
                    int v2 = 0;
                    while (v2 < maxOffsetBits && (source.ReadBits(1) == 1))
                    {
                        v2++;
                    }

                    uint copyLength = 2;

                    if (v2 != 0)
                    {
                        copyLength = ((uint)source.ReadBits(v2) | (1u << v2)) + 1;
                    }

                    uint backOffset = 0;

                    if (copyLength == 2)
                    {
                        backOffset = (uint)source.ReadBits(8) + 2;
                    }
                    else
                    {
                        backOffset = (uint)source.ReadBits(extendedOffsetBits) + copyLength;
                    }

                    copyLength = Math.Min(copyLength, (uint)(dest.Length - destPointer));

                    dest.Slice((int)(destPointer - backOffset), (int)copyLength).CopyTo(
                        dest.Slice((int)destPointer, (int)copyLength));

                    destPointer += copyLength;
                }
                else
                {
                    ulong result = source.ReadBits(8);
                    dest[(int)destPointer++] = (byte)(result & 0xFF);
                }

                if (destPointer >= dest.Length)
                {
                    break;
                }

                if (!source.Valid)
                {
                    break;
                }
            }

            return (int)destPointer;
        }

    }
}