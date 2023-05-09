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

using Nofun.Driver.Unity.Graphics;
using Nofun.Util;
using Nofun.VM;
using System;
using UnityEngine;

namespace Nofun.Module.VMGP3D
{
    [Module]
    public partial class VMGP3D
    {
        [ModuleCall]
        private void vCreatePlaneFromPoly(VMPtr<NativePlane> destPlanePtr, VMPtr<NativeVector3D> polygonsPtr)
        {
            Span<NativeVector3D> polygons = polygonsPtr.AsSpan(system.Memory, 3);
            NativePlane plane = destPlanePtr.AsSpan(system.Memory)[0];

            float x1 = FixedUtil.FixedToFloat(polygons[0].fixedX);
            float y1 = FixedUtil.FixedToFloat(polygons[0].fixedY);
            float z1 = FixedUtil.FixedToFloat(polygons[0].fixedZ);

            float x2 = FixedUtil.FixedToFloat(polygons[1].fixedX);
            float y2 = FixedUtil.FixedToFloat(polygons[1].fixedY);
            float z2 = FixedUtil.FixedToFloat(polygons[1].fixedZ);

            float x3 = FixedUtil.FixedToFloat(polygons[2].fixedX);
            float y3 = FixedUtil.FixedToFloat(polygons[2].fixedY);
            float z3 = FixedUtil.FixedToFloat(polygons[2].fixedZ);

            // Calculate first vector
            float v1x = x1 - x2;
            float v1y = y1 - y2;
            float v1z = z1 - z2;

            // Calculate second vector
            float v2x = x1 - x3;
            float v2y = y1 - y3;
            float v2z = z1 - z3;

            // Calculate normal by cross product
            float nx = (v1y * v2z - v1z * v2y);
            float ny = (v1z * v2x - v1x * v2z);
            float nz = (v1x * v2y - v1y * v2x);

            float dist = -(x1 * nx + x2 * ny + x3 * nz);

            plane.normal = new NativeVector3D()
            {
                fixedX = FixedUtil.FloatToFixed(nx),
                fixedY = FixedUtil.FloatToFixed(ny),
                fixedZ = FixedUtil.FloatToFixed(nz)
            };

            plane.fixedDistance = FixedUtil.FloatToFixed(dist);
            destPlanePtr.Write(system.Memory, plane);
        }

        [ModuleCall]
        private short vCollisionVectorPlane(VMPtr<NativeVector3D> collisionPointPtr, VMPtr<NativeVector3D> linePointsPtr, VMPtr<NativePlane> planePtr)
        {
            NativePlane plane = planePtr.Read(system.Memory);
            Span<NativeVector3D> points = linePointsPtr.AsSpan(system.Memory, 2);

            float pointX = FixedUtil.FixedToFloat(points[0].fixedX);
            float pointY = FixedUtil.FixedToFloat(points[0].fixedY);
            float pointZ = FixedUtil.FixedToFloat(points[0].fixedZ);

            float lineX = FixedUtil.FixedToFloat(points[0].fixedX - points[1].fixedX);
            float lineY = FixedUtil.FixedToFloat(points[0].fixedY - points[1].fixedY);
            float lineZ = FixedUtil.FixedToFloat(points[0].fixedZ - points[1].fixedZ);

            // Vector equation
            // x = x0 + lineX * t
            // y = y0 + lineY * t
            // z = z0 + lineZ * t
            // where (x0, y0, z0) is one of the point
            // solve t and apply to vector equation to get the intersection point

            // Plane equation
            // nx * x + ny * y + nz * z + d = 0
            // nx * x0 + nx * lineX * t + ny * y0 + ny * lineY * t + nz * z0 + nz * lineZ * t + d = 0
            // t * (nx * lineX + ny * lineY + nz * lineZ) = -d - nx * x0 - ny * y0 - nz * z0
            float normalX = FixedUtil.FixedToFloat(plane.normal.fixedX);
            float normalY = FixedUtil.FixedToFloat(plane.normal.fixedY);
            float normalZ = FixedUtil.FixedToFloat(plane.normal.fixedZ);

            float dist = FixedUtil.FixedToFloat(plane.fixedDistance);

            float rhs = -dist - normalX * pointX - normalY * pointY - normalZ * pointZ;
            float lhsMul = normalX * lineX * normalY * lineY * normalZ * lineZ;

            if (lhsMul == 0)
            {
                return 0;
            }

            float t = rhs / lhsMul;

            NativeVector3D collisionPoint = new NativeVector3D()
            {
                fixedX = FixedUtil.FloatToFixed(pointX + lineX * t),
                fixedY = FixedUtil.FloatToFixed(pointY + lineY * t),
                fixedZ = FixedUtil.FloatToFixed(pointZ + lineZ * t),
            };

            collisionPointPtr.Write(system.Memory, collisionPoint);
            return 1;
        }

        [ModuleCall]
        private short vBoxInViewFrustum(VMPtr<NativeBBox> boxPtr)
        {
            NativeBBox box = boxPtr.Read(system.Memory);
            Matrix4x4 pv = projectionMatrix * currentMatrix;

            // Calculate the frustum using Gribb/Hartmann
            // float[4] plane contains x,y,z and dist
            /*
            float[,] planes = new float[6, 4];

            for (int i = 3; i >= 0; i--) planes[0, i] = view.GetColumn(3)[i] + view.GetColumn(0)[i];    // Left
            for (int i = 3; i >= 0; i--) planes[1, i] = view.GetColumn(3)[i] - view.GetColumn(0)[i];    // Right
            for (int i = 3; i >= 0; i--) planes[2, i] = view.GetColumn(3)[i] + view.GetColumn(1)[i];    // Bottom
            for (int i = 3; i >= 0; i--) planes[3, i] = view.GetColumn(3)[i] - view.GetColumn(1)[i];    // Top 
            for (int i = 3; i >= 0; i--) planes[4, i] = view.GetColumn(3)[i] + view.GetColumn(2)[i];    // Near
            for (int i = 3; i >= 0; i--) planes[5, i] = view.GetColumn(3)[i] - view.GetColumn(2)[i];    // Far

            // https://gist.github.com/Kinwailo/d9a07f98d8511206182e50acda4fbc9b
            for (int i = 0; i < 6; i++)
            {
                Vector3 vmin = new Vector3(planes[i, 0] > 0 ? boxMin.x : boxMax.x, planes[i, 1] > 0 ? boxMin.y : boxMax.y, planes[i, 2] > 0 ? boxMin.z : boxMax.z);
                Vector3 vmax = new Vector3(planes[i, 0] > 0 ? boxMax.x : boxMin.x, planes[i, 1] > 0 ? boxMax.y : boxMin.y, planes[i, 2] > 0 ? boxMax.z : boxMin.z);
                Vector3 planeNormal = new Vector3(planes[i, 0], planes[i, 1], planes[i, 2]);

                if (Vector3.Dot(planeNormal, vmin) + planes[i, 3] > 0)
                {
                    // Outside
                    return 0;
                }

                if (Vector3.Dot(planeNormal, vmax) + planes[i, 3] >= 0)
                {
                    // Intersect
                    return 1;
                }
            }

            // Inside
            return 1;*/
            Vector3 boxMin = pv * box.min.ToUnity();
            Vector3 boxMax = pv * box.max.ToUnity();


        }
    }
}