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

using System.IO;
using System.Buffers.Binary;

namespace Nofun.Util
{
    public static class StreamUtil
    {
        public static string ReadNullTerminatedString(BinaryReader reader)
        {
            string result = "";
            do
            {
                char value = reader.ReadChar();
                if (value == '\0')
                {
                    break;
                }
                result += value;
            } while (true);

            return result;
        }

        public static string ReadUTF16String(this BinaryReader reader, int lengthInBytes)
        {
            string result = "";

            for (int i = 0; i < lengthInBytes >> 1; i++)
            {
                ushort charValue = reader.ReadUInt16();
                if (charValue == 0)
                {
                    break;
                }
                result += (char)charValue;
            }

            return result;
        }
    }
}