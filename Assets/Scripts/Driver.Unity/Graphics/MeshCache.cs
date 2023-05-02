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
using Nofun.Driver.Graphics;
using NoAlloq;
using System.Collections.Generic;

namespace Nofun.Driver.Unity.Graphics
{
    public class MeshCacheEntry : ICacheEntry
    {
        public Mesh mesh;

        public DateTime LastAccessed { get; set; }
    }

    public class MeshCache : LTUHardPruneCache<MeshCacheEntry>
    {
        private class DeferredCreationInfo
        {
            public Vector3[] vertices;
            public Vector2[] uvs;
            public Vector3[] normals;
            public Color[] colors;
            public int[] triangles;
            public uint hash;
        };

        private List<DeferredCreationInfo> creationInfos;

        public MeshCache(int limit = 100, int cacheTimeout = 240)
            : base(limit, cacheTimeout)
        {
            creationInfos = new();
        }

        private void PostMeshes()
        {
            lock (creationInfos)
            {
                foreach (var info in creationInfos)
                {
                    Mesh newMesh = new Mesh();
                    newMesh.vertices = info.vertices;
                    newMesh.uv = info.uvs;
                    newMesh.normals = info.normals;
                    newMesh.colors = info.colors;
                    newMesh.triangles = info.triangles;

                    AddToCache(info.hash, new MeshCacheEntry()
                    {
                        mesh = newMesh,
                        LastAccessed = DateTime.Now
                    });
                }

                creationInfos.Clear();
            }
        }

        private void DeferMeshCreation(MpMesh mesh, uint targetHash)
        {
            var verticesTransformed = mesh.vertices.Select(vec => vec.ToUnity()).ToArray();
            var uvTransformed = mesh.uvs.Select(uv => uv.ToUnity()).ToArray();
            var normalTransformed = mesh.normals.Select(normal => normal.ToUnity()).ToArray();
            var colorTransformed = mesh.diffuses.Select(diffuseCol => diffuseCol.ToUnity()).ToArray();

            int[] triangles = null;

            if (!mesh.indices.IsEmpty)
            {
                switch (mesh.topology)
                {
                    case PrimitiveTopology.TriangleList:
                        triangles = mesh.indices.Select(indexShort => (int)indexShort).ToArray();
                        break;

                    case PrimitiveTopology.TriangleStrip:
                        {
                            List<int> indicies = new();
                            bool winding = false;

                            for (int i = 0; i < mesh.indices.Length - 2; i++)
                            {
                                if (mesh.indices[i + 2] < 0)
                                {
                                    // Restart the winding
                                    winding = false;
                                    i += 2;

                                    continue;
                                }

                                indicies.Add(winding ? mesh.indices[i + 1] : mesh.indices[i]);
                                indicies.Add(winding ? mesh.indices[i] : mesh.indices[i + 1]);
                                indicies.Add(mesh.indices[i + 2]);

                                winding = !winding;
                            }

                            triangles = indicies.ToArray();
                            break;
                        }

                    case PrimitiveTopology.TriangleFan:
                        {
                            List<int> indicies = new();
                            int fanRoot = -1;

                            for (int i = 0; i < mesh.indices.Length - 1; i++)
                            {
                                if (mesh.indices[i] < 0)
                                {
                                    fanRoot = -1;
                                    continue;
                                }

                                if (fanRoot == -1)
                                {
                                    fanRoot = mesh.indices[i];
                                    continue;
                                }

                                indicies.Add(fanRoot);
                                indicies.Add(mesh.indices[i]);
                                indicies.Add(mesh.indices[i + 1]);
                            }

                            triangles = indicies.ToArray();
                            break;
                        }

                    default:
                        throw new ArgumentException($"Unknown topology: {mesh.topology}");
                }
            }
            else
            {
                switch (mesh.topology)
                {
                    case PrimitiveTopology.TriangleList:
                        break;

                    case PrimitiveTopology.TriangleStrip:
                        {
                            int[] newIndicies = new int[(mesh.vertices.Length - 2) * 3];
                            bool winding = false;

                            for (int i = 0; i < mesh.indices.Length - 2; i++)
                            {
                                newIndicies[i * 3] = (winding ? i + 1 : i);
                                newIndicies[i * 3 + 1] = (winding ? i : i + 1);
                                newIndicies[i * 3 + 2] = i + 2;
                            }

                            triangles = newIndicies;
                            break;
                        }

                    case PrimitiveTopology.TriangleFan:
                        {
                            int[] newIndicies = new int[(mesh.indices.Length - 2) * 3];

                            for (int i = 1; i < mesh.indices.Length - 1; i++)
                            {
                                newIndicies[i * 3] = 0;
                                newIndicies[i * 3 + 1] = i;
                                newIndicies[i * 3 + 2] = i + 1;
                            }

                            triangles = newIndicies;
                            break;
                        }

                    default:
                        throw new ArgumentException($"Unknown topology: {mesh.topology}");
                }
            }

            lock (creationInfos)
            {
                creationInfos.Add(new DeferredCreationInfo()
                {
                    hash = targetHash,
                    triangles = triangles,
                    vertices = verticesTransformed,
                    uvs = uvTransformed,
                    normals = normalTransformed,
                    colors = colorTransformed
                });
            }
        }

        public uint GetMeshIdentifier(MpMesh mesh, out Mesh existingMesh)
        {
            existingMesh = null;
            XxHash32 hash = new();

            // Hash texture coordinates
            hash.Append(MemoryMarshal.Cast<NativeVector3D, byte>(mesh.vertices));
            hash.Append(MemoryMarshal.Cast<NativeUV, byte>(mesh.uvs));
            hash.Append(MemoryMarshal.Cast<NativeDiffuseColor, byte>(mesh.diffuses));
            hash.Append(MemoryMarshal.Cast<NativeVector3D, byte>(mesh.normals));
            hash.Append(MemoryMarshal.Cast<PrimitiveTopology, byte>(MemoryMarshal.CreateReadOnlySpan(ref mesh.topology, 1)));

            if (!mesh.indices.IsEmpty)
            {
                hash.Append(MemoryMarshal.Cast<short, byte>(mesh.indices));
            }

            uint hashValue = BitConverter.ToUInt32(hash.GetCurrentHash());

            MeshCacheEntry entry = GetFromCacheSafe(hashValue);
            if (entry != null)
            {
                existingMesh = entry.mesh;
            }
            else
            {
                DeferMeshCreation(mesh, hashValue);
            }

            return hashValue;
        }

        public Mesh GetMesh(uint indentifier)
        {
            lock (cache)
            {
                PostMeshes();

                MeshCacheEntry cache = GetFromCache(indentifier);
                if (cache == null)
                {
                    throw new ArgumentNullException($"Mesh entry is not supposed to be null!");
                }

                return cache.mesh;
            }
        }
    };
}