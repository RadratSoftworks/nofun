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

namespace Nofun.Driver.Graphics
{
    public enum TextureFormat
    {
        Monochrome = 0,
        Gray4 = 1,
        Gray16 = 2,
        Palette2 = 3,
        Palette4 = 4,
        Palette16 = 5,
        Palette256 = 6,
        RGB332 = 7,
        RGB565 = 8,
        RGB555 = 9,
        RGB888 = 10,
        ARGB8888 = 11,
        RGB444 = 12,
        ARGB4444 = 13,
        ARGB1555 = 14,
        Palette16_Alt = 16,
        Palette256_Alt = 17,
        Palette16_ARGB = 18,
        Palette256_ARGB = 19
    };
}