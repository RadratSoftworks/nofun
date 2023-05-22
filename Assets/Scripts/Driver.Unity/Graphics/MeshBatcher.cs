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

using NoAlloq;
using Nofun.Driver.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nofun.Driver.Unity.Graphics
{
    public class MeshBatcher
    {
        private class BatchedInfo
        {
            public List<Vector3> vertices;
            public List<Vector2> uvs;
            public List<Vector3> normals;
            public List<Color> colors;

            public List<int> indicies;

            public BatchedInfo()
            {
                normals = new();
                uvs = new();
                colors = new();
                indicies = new();
                vertices = new();
            }
        }

        private BatchedInfo currentBatch;
        private Queue<BatchedInfo> doneBatches;

        public MeshBatcher()
        {
            currentBatch = new();
            doneBatches = new();
        }

        public bool Batchable(MpMesh mesh)
        {
            return mesh.vertices.Length <= 128;
        }

        public void AddBasic(Span<Vector3> vertices, Span<Vector2> uvs, Span<Color> colors, Span<int> indicies)
        {
            int verticesCount = vertices.Length;

            currentBatch.indicies.AddRange(indicies.Select(i => i + currentBatch.vertices.Count).ToList());
            currentBatch.vertices.AddRange(vertices.ToArray());
            currentBatch.colors.AddRange(colors.ToArray());
            currentBatch.normals.AddRange(Enumerable.Repeat(Vector3.zero, verticesCount));
            currentBatch.uvs.AddRange(uvs.ToArray());
        }

        public void Add(MpMesh mesh)
        {
            int verticesCount = mesh.vertices.Length;

            if (mesh.indices.IsEmpty)
            {
                currentBatch.indicies.AddRange(IndiciesTransformer.Generate(verticesCount, mesh.topology, currentBatch.vertices.Count));
            }
            else
            {
                currentBatch.indicies.AddRange(IndiciesTransformer.Process(mesh.indices, mesh.topology, currentBatch.vertices.Count));
            }

            currentBatch.vertices.AddRange(mesh.vertices.Select(vertice => vertice.ToUnity()).ToList());
            
            if (mesh.uvs.IsEmpty)
            {
                currentBatch.uvs.AddRange(Enumerable.Repeat(Vector2.zero, verticesCount));
            }
            else
            {
                currentBatch.uvs.AddRange(mesh.uvs.Select(uv => uv.ToUnity()).ToList());
            }

            if (mesh.normals.IsEmpty)
            {
                currentBatch.normals.AddRange(Enumerable.Repeat(Vector3.zero, verticesCount));
            }
            else
            {
                currentBatch.normals.AddRange(mesh.normals.Select(normal => normal.ToUnity()).ToList());
            }

            if (mesh.diffuses.IsEmpty)
            {
                currentBatch.colors.AddRange(Enumerable.Repeat(Color.white, verticesCount));
            }
            else
            {
                currentBatch.colors.AddRange(mesh.diffuses.Select(color => color.ToUnity()).ToList());
            }
        }

        public bool Flush()
        {
            if (currentBatch.vertices.Count == 0)
            {
                return false;
            }

            lock (doneBatches)
            {
                doneBatches.Enqueue(currentBatch);
            }

            currentBatch = new();
            return true;
        }

        public int Pop(BufferPusher pusher)
        {
            BatchedInfo batch = null;

            lock (doneBatches)
            {
                batch = doneBatches.Peek();
            }

            int val = pusher.Push(batch.vertices, batch.uvs, batch.normals, batch.colors, batch.indicies);
            if (val >= 0)
            {
                lock (doneBatches)
                {
                    doneBatches.Dequeue();
                }
            }

            return val;
        }

        public void Reset()
        {
        }
    }
}