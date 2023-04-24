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

namespace Nofun.PIP2
{
    public static class Register
    {
        public const int Zero = 0;
        public const int SP = 4;
        public const int RA = 8;
        public const int FP = 12;

        public const int S0 = 16;
        public const int S1 = 20;
        public const int S2 = 24;
        public const int S3 = 28;
        public const int S4 = 32;
        public const int S5 = 36;
        public const int S6 = 40;
        public const int S7 = 44;

        public const int P0 = 48;
        public const int P1 = 52;
        public const int P2 = 56;
        public const int P3 = 60;

        public const int G0 = 64;
        public const int G1 = 68;
        public const int G2 = 72;
        public const int G3 = 76;
        public const int G4 = 80;
        public const int G5 = 84;
        public const int G6 = 88;
        public const int G7 = 92;
        public const int G8 = 96;
        public const int G9 = 100;
        public const int G10 = 104;
        public const int G11 = 108;
        public const int G12 = 112;
        public const int G13 = 116;

        public const int R0 = 120;
        public const int R1 = 124;

        public const int PC = 128;

        public const int PCIndex = 32;

        public const int TotalReg = 33;
    }
}