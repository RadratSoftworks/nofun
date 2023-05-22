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
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nofun.Driver.Graphics
{
    public static class IndiciesTransformer
    {
        public static List<int> Generate(int verticesCount, PrimitiveTopology topology, int addOffset = 0)
        {
            List<int> newIndicies = new();
            
            switch (topology)
            {
                case PrimitiveTopology.TriangleList:
                    newIndicies = Enumerable.Range(addOffset, verticesCount).ToList();
                    break;

                case PrimitiveTopology.TriangleStrip:
                    {
                        bool winding = false;

                        for (int i = 0; i < verticesCount - 2; i++)
                        {
                            newIndicies.Add(addOffset + (winding ? i + 1 : i));
                            newIndicies.Add(addOffset + (winding ? i : i + 1));
                            newIndicies.Add(addOffset + i + 2);

                            winding = !winding;
                        }

                        break;
                    }

                case PrimitiveTopology.TriangleFan:
                    {
                        for (int i = 1; i < verticesCount - 1; i++)
                        {
                            newIndicies.Add(addOffset);
                            newIndicies.Add(addOffset + i);
                            newIndicies.Add(addOffset + i + 1);
                        }

                        break;
                    }

                default:
                    throw new ArgumentException($"Unknown topology: {topology}");
            }

            return newIndicies;
        }

        public static int Estimate(Span<short> indices, PrimitiveTopology topology)
        {
            switch (topology)
            {
                case PrimitiveTopology.TriangleList:
                    return indices.Length;


                case PrimitiveTopology.TriangleStrip:
                    {
                        bool winding = false;
                        int totalCount = 0;

                        for (int i = 0; i < indices.Length - 2; i++)
                        {
                            if (indices[i + 2] < 0)
                            {
                                // Restart the winding
                                winding = false;
                                i += 2;

                                continue;
                            }

                            totalCount += 3;
                            winding = !winding;
                        }

                        return totalCount;
                    }

                case PrimitiveTopology.TriangleFan:
                    {
                        int fanRoot = -1;
                        int totalCount = 0;

                        for (int i = 0; i < indices.Length - 1; i++)
                        {
                            if (indices[i] < 0)
                            {
                                fanRoot = -1;
                                continue;
                            }

                            if (fanRoot == -1)
                            {
                                fanRoot = indices[i];
                                continue;
                            }

                            totalCount += 3;
                        }

                        return totalCount;
                    }

                default:
                    throw new ArgumentException($"Unknown topology: {topology}");
            }
        }

        public static List<int> Process(Span<short> indices, PrimitiveTopology topology, int addOffset)
        {
            List<int> triangles = new();
            switch (topology)
            {
                case PrimitiveTopology.TriangleList:
                    triangles = indices.Select(indexShort => addOffset + indexShort).ToList();
                    break;

                case PrimitiveTopology.TriangleStrip:
                    {
                        bool winding = false;

                        for (int i = 0; i < indices.Length - 2; i++)
                        {
                            if (indices[i + 2] < 0)
                            {
                                // Restart the winding
                                winding = false;
                                i += 2;

                                continue;
                            }

                            triangles.Add(addOffset + (winding ? indices[i + 1] : indices[i]));
                            triangles.Add(addOffset + (winding ? indices[i] : indices[i + 1]));
                            triangles.Add(addOffset + indices[i + 2]);

                            winding = !winding;
                        }

                        break;
                    }

                case PrimitiveTopology.TriangleFan:
                    {
                        int fanRoot = -1;

                        for (int i = 0; i < indices.Length - 1; i++)
                        {
                            if (indices[i] < 0)
                            {
                                fanRoot = -1;
                                continue;
                            }

                            if (fanRoot == -1)
                            {
                                fanRoot = indices[i];
                                continue;
                            }

                            triangles.Add(addOffset + fanRoot);
                            triangles.Add(addOffset + indices[i]);
                            triangles.Add(addOffset + indices[i + 1]);
                        }

                        break;
                    }

                default:
                    throw new ArgumentException($"Unknown topology: {topology}");
            }

            return triangles;
        }
    }
}