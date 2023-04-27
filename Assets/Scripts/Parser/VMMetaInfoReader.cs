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

using Nofun.Util;
using Nofun.Util.Logging;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

namespace Nofun.Parser
{
    public class VMMetaInfoReader
    {
        public const int MagicLength = 4;

        private Dictionary<string, string> dict;

        public VMMetaInfoReader(BinaryReader reader)
        {
            dict = new();

            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            if (!IsMetadataStream(reader))
            {
                throw new InvalidDataException("Stream does not contain metadata info!");
            }

            reader.BaseStream.Seek(262, SeekOrigin.Current);

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                int keyLength = reader.ReadUInt16();
                int valueLength = reader.ReadUInt16();

                if (reader.BaseStream.Position == reader.BaseStream.Length)
                {
                    break;
                }

                // Two strings
                string key = reader.ReadUTF16String(keyLength);
                string value = reader.ReadUTF16String(valueLength);

                if (reader.BaseStream.Position == reader.BaseStream.Length)
                {
                    break;
                }

                // Pad byte
                reader.ReadByte();

                dict.Add(key, value);
            }
        }

        public string Get(string key)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            else
            {
                return null;
            }
        }

        private static bool IsMetadataStream(BinaryReader reader)
        {
            byte []magic = reader.ReadBytes(4);
            return IsMetadataMagic(magic);
        }

        public static bool IsMetadataMagic(Span<byte> magic)
        {
            return (magic[0] == 'M') && (magic[1] == 'E') && (magic[2] == 'T') && (magic[3] == 'A');
        }
    }
}