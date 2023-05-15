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

namespace Nofun.PIP2.Encoding
{
    public struct RangeRegEncoding
    {
        public byte opcode;
        public byte start;
        public byte count;

        public uint Instruction
        {
            set
            {
                opcode = (byte)(value & 0xFF);
                start = (byte)((value >> 8) & 0xFF);
                count = (byte)((value >> 16) & 0xFF);

                if (count == 0)
                {
                    throw new InvalidPIP2EncodingException("The register range count can not be 0!");
                }
            }
        }
    }
}