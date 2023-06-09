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

using Nofun.Driver.Graphics;
using Nofun.Util;

using UnityEngine;

namespace Nofun.Driver.Unity.Graphics
{
    public class ClientState
    {
        public const int MaximumLight = 8;

        public MpCullMode cullMode = MpCullMode.CounterClockwise;
        public MpBlendMode blendMode = MpBlendMode.Replace;
        public MpTextureBlendMode textureBlendMode = MpTextureBlendMode.Modulate;
        public bool textureMode = false;
        public MpCompareFunc depthCompareFunc = MpCompareFunc.Always;
        public Rect scissorRect;
        public Rect viewportRect;
        public Matrix4x4 projectionMatrix3D;
        public Matrix4x4 viewMatrix3D;
        public Matrix4x4 lightMatrix3D;
        public Texture mainTexture;
        public MpExtendedMaterial extendedMaterial;
        public bool lighting = false;
        public bool specular = false;
        public bool fog = false;
        public SColor globalAmbient;
        public MpLight[] lights = new MpLight[MaximumLight];
        public Vector3 cameraPosition;
        public bool transparentTest = false;

        public ulong MaterialIdentifier
        {
            get
            {
                return
                    ((uint)BitUtil.BitScanForward((ulong)depthCompareFunc) << 1) |
                    ((ulong)blendMode << 4) |
                    ((ulong)cullMode << 7);
            }
        }
    }
}