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
using Nofun.Util.Logging;
using Nofun.VM;

namespace Nofun.Module.VMGP3D
{
    [Module]
    public partial class VMGP3D
    {
        private VMSystem system;

        /// <summary>
        /// Texture cache, used to manage indefinite texture uploaded using vSetTexture.
        /// </summary>
        private TextureCache textureCache;

        /// <summary>
        /// Current active texture. A single texture is used to draw over primitives.
        /// </summary>
        private ITexture activeTexture;

        private MpCompareFunc previousCompareFunc = MpCompareFunc.Less;

        public VMGP3D(VMSystem system)
        {
            this.system = system;
            this.textureCache = new();
            this.managedTextures = new();
        }

        [ModuleCall]
        private void vInit3D()
        {
        }

        [ModuleCall]
        private void vSetViewport(int left, int top, int width, int height)
        {
            system.GraphicDriver.Viewport = new NRectangle(left, top, width, height);
        }

        [ModuleCall]
        private void vSetRenderState(RenderState state, uint value)
        {
            switch (state)
            {
                case RenderState.CullMode:
                    system.GraphicDriver.Cull = (MpCullMode)value;
                    break;

                case RenderState.ZFunction:
                    system.GraphicDriver.DepthFunction = (MpCompareFunc)value;
                    previousCompareFunc = (MpCompareFunc)value;

                    break;

                case RenderState.TextureEnable:
                    system.GraphicDriver.TextureMode = (value != 0);
                    break;

                case RenderState.ColorBufferBlendMode:
                    system.GraphicDriver.ColorBufferBlend = (MpBlendMode)value;
                    break;

                case RenderState.ShadeMode:
                    // Literally ignore
                    break;

                case RenderState.ZEnable:
                    {
                        if (value == 0)
                        {
                            system.GraphicDriver.DepthFunction = MpCompareFunc.Always;
                        }
                        else
                        {
                            system.GraphicDriver.DepthFunction = previousCompareFunc;
                        }

                        break;
                    }

                case RenderState.FilterMode:
                    if (activeTexture != null)
                    {
                        activeTexture.Filter = (MpFilterMode)value;
                    }

                    break;

                case RenderState.WrapMode:
                    if (activeTexture != null)
                    {
                        activeTexture.Wrap = (MpTextureWrapMode)value;
                    }

                    break;

                case RenderState.TextureBlendMode:
                    system.GraphicDriver.TextureBlendMode = (MpTextureBlendMode)value;
                    break;

                case RenderState.LightingEnable:
                    system.GraphicDriver.Lighting = (value != 0);
                    break;

                case RenderState.SpecularEnable:
                    system.GraphicDriver.Specular = (value != 0);
                    break;

                case RenderState.FogEnable:
                    system.GraphicDriver.Fog = (value != 0);
                    break;

                case RenderState.TransparentEnable:
                    system.GraphicDriver.TransparentTest = (value != 0);
                    break;

                // These are ignorable
                case RenderState.PerspectiveEnable:
                case RenderState.AlphaEnable:
                    break;

                default:
                    Logger.Warning(LogClass.VMGP3D, $"Unhandled render state={state}, value={value}");
                    break;
            }
        }

        [ModuleCall]
        private void vSetZBuffer(ushort value)
        {
            system.GraphicDriver.ClearDepth(value / ushort.MaxValue);
        }

        [ModuleCall]
        private int vSetTexture(VMPtr<byte> textureData, uint format, uint lods, uint mipCount)
        {
            try
            {
                activeTexture = textureCache.Get(system.GraphicDriver, textureData, system.Memory, (TextureFormat)format,
                    lods, mipCount, system.VMGPModule.ScreenPalette);

                system.GraphicDriver.MainTexture = activeTexture;
                return 1;
            }
            catch (System.Exception ex)
            {
                Logger.Error(LogClass.VMGP3D, $"Set texture failed with exception: {ex}");
                return 0;
            }
        }

        [ModuleCall]
        private void vFreeTexture(int handle)
        {
        }

        [ModuleCall]
        private void vDeleteAllTextures()
        {
            textureCache.Clear();
        }

        [ModuleCall]
        private void vDrawBillboard(VMPtr<NativeBillboard> billboardPtr)
        {
            NativeBillboard billboard = billboardPtr.Read(system.Memory);

            system.GraphicDriver.ViewMatrix3D = currentMatrix;
            system.GraphicDriver.DrawBillboard(billboard);
        }

        [ModuleCall]
        private short vRenderPrimitive(VMPtr<NativeMesh> mesh, uint topology)
        {
            NativeMesh meshCopy = mesh.Read(system.Memory);

            MpMesh meshMp = new MpMesh()
            {
                vertices = meshCopy.vertices.AsSpan(system.Memory, meshCopy.count),
                uvs = meshCopy.uvs.AsSpan(system.Memory, meshCopy.count),
                diffuses = meshCopy.diffuses.AsSpan(system.Memory, meshCopy.count),
                speculars = meshCopy.speculars.AsSpan(system.Memory, meshCopy.count),
                normals = meshCopy.normal.AsSpan(system.Memory, meshCopy.count),
                topology = (PrimitiveTopology)topology
            };

            system.GraphicDriver.ViewMatrix3D = currentMatrix;
            system.GraphicDriver.DrawPrimitives(meshMp);

            return 1;
        }

        [ModuleCall]
        private short vRenderPrimitiveIndexed(VMPtr<short> indexList, short indexCount, VMPtr<NativeMesh> mesh, uint topology)
        {
            NativeMesh meshCopy = mesh.Read(system.Memory);

            MpMesh meshMp = new MpMesh()
            {
                vertices = meshCopy.vertices.AsSpan(system.Memory, meshCopy.count),
                uvs = meshCopy.uvs.AsSpan(system.Memory, meshCopy.count),
                diffuses = meshCopy.diffuses.AsSpan(system.Memory, meshCopy.count),
                speculars = meshCopy.speculars.AsSpan(system.Memory, meshCopy.count),
                normals = meshCopy.normal.AsSpan(system.Memory, meshCopy.count),
                indices = indexList.AsSpan(system.Memory, indexCount),
                topology = (PrimitiveTopology)topology
            };

            system.GraphicDriver.ViewMatrix3D = currentMatrix;
            system.GraphicDriver.DrawPrimitives(meshMp);

            return 1;
        }

        [ModuleCall]
        private uint vGetInteger(uint key, VMPtr<uint> value)
        {
            switch ((InternalStateType)key)
            {
                case InternalStateType.MaxLights:
                    value.Write(system.Memory, (uint)system.GraphicDriver.MaxLights);
                    break;

                case InternalStateType.MaxVertices:
                    value.Write(system.Memory, 2048);
                    break;

                case InternalStateType.MaxTextureCount:
                    value.Write(system.Memory, 64);
                    break;

                case InternalStateType.MaxTextureSize:
                    value.Write(system.Memory, 1024);
                    break;

                case InternalStateType.TextureCacheSize:
                    value.Write(system.Memory, 16 * 1024 * 1024);
                    break;

                case InternalStateType.DepthBits:
                    value.Write(system.Memory, 32);
                    break;

                default:
                    Logger.Trace(LogClass.VMGP3D, $"Unknown internal state type {key}");
                    return 0;
            }

            return 1;
        }

        [ModuleCall]
        private void vDrawPolygon(VMPtr<NativeVertexGST> v1Ptr, VMPtr<NativeVertexGST> v2Ptr, VMPtr<NativeVertexGST> v3Ptr)
        {
            NativeVertexGST v1 = v1Ptr.Read(system.Memory);
            NativeVertexGST v2 = v2Ptr.Read(system.Memory);
            NativeVertexGST v3 = v3Ptr.Read(system.Memory);

            MpMesh meshMp = new MpMesh();

            meshMp.vertices = new NativeVector3D[]
            {
                new NativeVector3D(v1.position.fixedX, v1.position.fixedY, v1.position.fixedZ),
                new NativeVector3D(v2.position.fixedX, v2.position.fixedY, v2.position.fixedZ),
                new NativeVector3D(v3.position.fixedX, v3.position.fixedY, v3.position.fixedZ),
            };

            meshMp.uvs = new NativeUV[]
            {
                v1.uv,
                v2.uv,
                v3.uv
            };

            meshMp.diffuses = new NativeDiffuseColor[]
            {
                v1.diffuse,
                v2.diffuse,
                v3.diffuse
            };

            meshMp.speculars = new NativeSpecularColor[]
            {
                v1.specular,
                v2.specular,
                v3.specular
            };

            meshMp.topology = PrimitiveTopology.TriangleList;

            system.GraphicDriver.ViewMatrix3D = currentMatrix;
            system.GraphicDriver.DrawPrimitives(meshMp);
        }
    }
}