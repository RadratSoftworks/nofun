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

namespace Nofun.Util
{
    public struct NRectangle
    {
        public int x;
        public int y;
        public int width;
        public int height;

        public int x1 => x + width;
        public int y1 => y + height;

        public NRectangle(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public bool Collide(NRectangle otherRect)
        {
            return (x < otherRect.x + otherRect.width) && (x + width > otherRect.x) && (y < otherRect.y + otherRect.height) && (y + height > otherRect.y);
        }
    }
}