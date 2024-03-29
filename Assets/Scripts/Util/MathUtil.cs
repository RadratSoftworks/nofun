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
    public static class MathUtil
    {
        public const int FullCircleDegrees = 360;
        public const double FullCircleRads = Math.PI * 2;

        public static uint NextPowerOfTwo(uint value)
        {
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value++;

            return value;
        }

        public static float RadToDegs(float radians)
        {
            return radians * 180.0f / (float)System.Math.PI;
        }

        public static float Degs2Rad(float degs)
        {
            return degs / 180.0f * (float)System.Math.PI;
        }
    }
}