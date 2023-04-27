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
        public MeshCache(int limit = 100, int cacheTimeout = 240)
            : base(limit, cacheTimeout)
        {
        }

        public Mesh GetMesh(MpMesh mesh)
        {
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

            MeshCacheEntry entry = GetFromCache(hashValue);
            if (entry != null)
            {
                return entry.mesh;
            }

            // Create new mesh. Translate as normal, but if we encountered topology other than list, we would take action
            Mesh newBillboardMesh = new Mesh();
            newBillboardMesh.vertices = mesh.vertices.Select(vec => Struct3DToUnity.MophunVector3ToUnity(vec)).ToArray();
            newBillboardMesh.uv = mesh.uvs.Select(uv => Struct3DToUnity.MophunUVToUnity(uv)).ToArray();
            newBillboardMesh.normals = mesh.normals.Select(normal => Struct3DToUnity.MophunVector3ToUnity(normal)).ToArray();
            newBillboardMesh.colors = mesh.diffuses.Select(diffuseCol => Struct3DToUnity.MophunDColorToUnity(diffuseCol)).ToArray();

            if (!mesh.indices.IsEmpty)
            {
                switch (mesh.topology)
                {
                    case PrimitiveTopology.TriangleList:
                        newBillboardMesh.triangles = mesh.indices.Select(indexShort => (int)indexShort).ToArray();
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

                            newBillboardMesh.triangles = indicies.ToArray();
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

                            newBillboardMesh.triangles = indicies.ToArray();
                            break;
                        }

                    default:
                        throw new ArgumentException($"Unknown topology: {mesh.topology}");
                }
            } else
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

                            newBillboardMesh.triangles = newIndicies;
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

                            newBillboardMesh.triangles = newIndicies;
                            break;
                        }

                    default:
                        throw new ArgumentException($"Unknown topology: {mesh.topology}");
                }
            }

            AddToCache(hashValue, new MeshCacheEntry()
            {
                LastAccessed = DateTime.Now,
                mesh = newBillboardMesh
            });

            return newBillboardMesh;
        }
    };
}