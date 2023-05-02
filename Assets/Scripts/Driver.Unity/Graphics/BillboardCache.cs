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

using Nofun.Module.VMGP3D;
using System;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using UnityEngine;

using Nofun.Util;

namespace Nofun.Driver.Unity.Graphics
{
    public class BillboardCacheEntry : ICacheEntry
    {
        public Mesh mesh;

        public DateTime LastAccessed { get; set; }
    }

    public class BillboardCache : LTUHardPruneCache<BillboardCacheEntry>
    {
        public BillboardCache(int limit = 100, int cacheTimeout = 240)
            : base(limit, cacheTimeout)
        {
        }

        public Mesh GetBillboardMesh(NativeBillboard billboard)
        {
            XxHash32 hash = new();

            // Hash texture coordinates
            hash.Append(MemoryMarshal.Cast<NativeUV, byte>(MemoryMarshal.CreateReadOnlySpan(ref billboard.uv0, 1)));
            hash.Append(MemoryMarshal.Cast<NativeUV, byte>(MemoryMarshal.CreateReadOnlySpan(ref billboard.uv1, 1)));
            hash.Append(MemoryMarshal.Cast<NativeUV, byte>(MemoryMarshal.CreateReadOnlySpan(ref billboard.uv2, 1)));
            hash.Append(MemoryMarshal.Cast<NativeUV, byte>(MemoryMarshal.CreateReadOnlySpan(ref billboard.uv3, 1)));

            // Hash colors
            hash.Append(MemoryMarshal.Cast<NativeDiffuseColor, byte>(MemoryMarshal.CreateReadOnlySpan(ref billboard.color0, 1)));
            hash.Append(MemoryMarshal.Cast<NativeDiffuseColor, byte>(MemoryMarshal.CreateReadOnlySpan(ref billboard.color1, 1)));
            hash.Append(MemoryMarshal.Cast<NativeDiffuseColor, byte>(MemoryMarshal.CreateReadOnlySpan(ref billboard.color2, 1)));
            hash.Append(MemoryMarshal.Cast<NativeDiffuseColor, byte>(MemoryMarshal.CreateReadOnlySpan(ref billboard.color3, 1)));

            uint hashValue = BitConverter.ToUInt32(hash.GetCurrentHash());

            BillboardCacheEntry entry = GetFromCache(hashValue);
            if (entry != null)
            {
                return entry.mesh;
            }

            // Create new mesh
            Mesh newBillboardMesh = new Mesh();
            newBillboardMesh.vertices = new Vector3[]
            {
                new Vector3(0, 1, 0),
                new Vector3(1, 1, 0),
                new Vector3(1, 0, 0),
                new Vector3(0, 0, 0)
            };

            newBillboardMesh.triangles = new int[]
            {
                0, 1, 2,
                0, 2, 3
            };

            newBillboardMesh.uv = new Vector2[]
            {
                billboard.uv0.ToUnity(),
                billboard.uv1.ToUnity(),
                billboard.uv2.ToUnity(),
                billboard.uv3.ToUnity()
            };

            newBillboardMesh.colors = new Color[]
            {
                billboard.color0.ToUnity(),
                billboard.color1.ToUnity(),
                billboard.color2.ToUnity(),
                billboard.color3.ToUnity()
            };

            newBillboardMesh.RecalculateNormals();

            AddToCache(hashValue, new BillboardCacheEntry()
            {
                LastAccessed = DateTime.Now,
                mesh = newBillboardMesh
            });

            return newBillboardMesh;
        }
    };
}