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
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace SharpMik.Player
{
    // https://wiki.multimedia.cx/index.php/IMA_ADPCM
    // Thanks to the authors of this wiki page!
    public static class ADPCMToPcm
    {
        private static readonly int[] ImaStepTable = {
            7, 8, 9, 10, 11, 12, 13, 14, 16, 17,
            19, 21, 23, 25, 28, 31, 34, 37, 41, 45,
            50, 55, 60, 66, 73, 80, 88, 97, 107, 118,
            130, 143, 157, 173, 190, 209, 230, 253, 279, 307,
            337, 371, 408, 449, 494, 544, 598, 658, 724, 796,
            876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066,
            2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358,
            5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
            15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767
        };

        private static readonly int[] ImaIndexTable =
        {
            -1, -1, -1, -1, 2, 4, 6, 8,
            -1, -1, -1, -1, 2, 4, 6, 8
        };

        public static void Convert(Span<byte> source, Span<short> value)
        {
            // All zero beforehand as seen in MORC
            int predictor = 0;
            int stepIndex = 0;
            int step = ImaStepTable[stepIndex];

            int nibbleCounter = 0;
            int sampleDecoded = 0;

            while ((nibbleCounter >> 3) < source.Length)
            {
                int nibble = (source[nibbleCounter >> 3] >> (nibbleCounter & 7)) & 0xF;

                stepIndex += ImaIndexTable[nibble];
                stepIndex = Math.Clamp(stepIndex, 0, 88);

                // It's encoded this way and should be decoded this way, else there will be annoying audio spike
                int diff = 0;

                if ((nibble & 0x04) != 0) diff += step;
                if ((nibble & 0x02) != 0) diff += step >> 1;
                if ((nibble & 0x01) != 0) diff += step >> 2;

                bool subtract = ((nibble & 0x08) != 0);

                predictor = Math.Clamp(predictor + (subtract ? -diff : diff), short.MinValue, short.MaxValue);
                step = ImaStepTable[stepIndex];

                value[sampleDecoded++] = (short)predictor;
                nibbleCounter += 4;
            }
        }
    }
}