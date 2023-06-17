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

namespace Nofun.Driver.Graphics
{
    public struct SColor
    {
        public static readonly SColor Black = new SColor(0, 0, 0);
        public static readonly SColor White = new SColor(1, 1, 1);

        public float r;
        public float g;
        public float b;
        public float a;

        public bool FullBlack => (r == 0) && (g == 0) && (b == 0);

        public SColor(float r, float g, float b, float a = 1.0f)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public float Difference(SColor another)
        {
            return Math.Abs(r - another.r) + Math.Abs(g - another.g) + Math.Abs(b - another.b);
        }

        public uint ToRgb555()
        {
            return (uint)(b * 31) | ((uint)(g * 31) << 5) | ((uint)(r * 31) << 10);
        }

        public static SColor FromRgb555(uint value)
        {
            return new SColor(((value >> 10) & 0b11111) / 31.0f,
                    ((value >> 5) & 0b11111) / 31.0f,
                    (value & 0b11111) / 31.0f);
        }

        public static SColor FromArgb8888(uint value)
        {
            return new SColor(((value >> 16) & 0xFF) / 255.0f,
                    ((value >> 8) & 0xFF) / 255.0f,
                    (value & 0xFF) / 255.0f,
                    ((value >> 24) & 0xFF) / 255.0f);
        }

        public static SColor FromArgb8888SW(uint value)
        {
            uint alpha = (value >> 24) & 0xFF;
            if ((alpha & (1 << 7)) != 0)
            {
                alpha = 0;
            }
            else
            {
                alpha = 255;
            }

            return new SColor(((value >> 16) & 0xFF) / 255.0f,
                    ((value >> 8) & 0xFF) / 255.0f,
                    (value & 0xFF) / 255.0f,
                    alpha / 255.0f);
        }

        public static SColor FromRgb888(uint value)
        {
            return new SColor(((value >> 16) & 0xFF) / 255.0f,
                    ((value >> 8) & 0xFF) / 255.0f,
                    (value & 0xFF) / 255.0f);
        }
    };
}