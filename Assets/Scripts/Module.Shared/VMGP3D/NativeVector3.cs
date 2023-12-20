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

namespace Nofun.Module.VMGP3D
{
    public struct NativeVector3D
    {
        public int fixedX;
        public int fixedY;
        public int fixedZ;

        public NativeVector3D(int fixedX, int fixedY, int fixedZ)
        {
            this.fixedX = fixedX;
            this.fixedY = fixedY;
            this.fixedZ = fixedZ;
        }

        public static bool operator <= (NativeVector3D lhs, NativeVector3D rhs)
        {
            return (lhs.fixedX <= rhs.fixedX) && (lhs.fixedY <= rhs.fixedY) && (lhs.fixedZ <= rhs.fixedZ);
        }


        public static bool operator >=(NativeVector3D lhs, NativeVector3D rhs)
        {
            return (lhs.fixedX >= rhs.fixedX) && (lhs.fixedY >= rhs.fixedY) && (lhs.fixedZ >= rhs.fixedZ);
        }
    }
}