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
    public struct NativeBillboard
    {
        public NativeVector3D position;
        public int fixedWidth;
        public int fixedHeight;
        public NativeUV uv0;
        public NativeUV uv1;
        public NativeUV uv2;
        public NativeUV uv3;
        public NativeDiffuseColor color0;
        public NativeDiffuseColor color1;
        public NativeDiffuseColor color2;
        public NativeDiffuseColor color3;
        public ushort rotation;
        public ushort rotationPointFlag;
    }
}