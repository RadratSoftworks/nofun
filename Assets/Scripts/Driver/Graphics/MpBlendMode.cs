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
    public enum MpBlendMode
    {
        /// <summary>
        /// Source pixel replaces color buffer pixel.
        /// </summary>
        Replace,

        /// <summary>
        /// Source pixel is multiplied with alpha, then add to color buffer.
        /// </summary>
        AlphaAdd,

        /// <summary>
        /// Source pixel is multiplied with source alpha, while color buffer pixel is multiplied with one minus source alpha.
        /// </summary>
        /// 
        Alpha,

        /// <summary>
        /// Multiply source pixel with color buffer pixel.
        /// </summary>
        Modulate
    }
}