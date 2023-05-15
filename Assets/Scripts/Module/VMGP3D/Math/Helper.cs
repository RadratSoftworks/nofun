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

            float lengthn = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
            float dist = -(x1 * nx + x2 * ny + x3 * nz);

            plane.normal = new NativeVector3D()
            {
                fixedX = FixedUtil.FloatToFixed(nx / lengthn),
                fixedY = FixedUtil.FloatToFixed(ny / lengthn),
                fixedZ = FixedUtil.FloatToFixed(nz / lengthn)
            };

            plane.fixedDistance = FixedUtil.FloatToFixed(dist / lengthn);
            destPlanePtr.Write(system.Memory, plane);
        }

        [ModuleCall]
        private short vCollisionPointBox(VMPtr<NativeVector3D> point, VMPtr<NativeBBox> box)
        {
            NativeVector3D pointValue = point.Read(system.Memory);
            NativeBBox boxValue = box.Read(system.Memory);

            bool hit = (boxValue.min <= pointValue) && (pointValue <= boxValue.max);
            return (short)(hit ? 1 : 0);
        }

        [ModuleCall]
        private short vCollisionBoxBox(VMPtr<NativeBBox> box1, VMPtr<NativeBBox> box2)
        {
            NativeBBox box1Value = box1.Read(system.Memory);
            NativeBBox box2Value = box2.Read(system.Memory);

            bool collide = ((box1Value.min.fixedX <= box2Value.max.fixedX) && (box1Value.max.fixedX >= box2Value.min.fixedX) &&
                (box1Value.min.fixedY <= box2Value.max.fixedY) && (box1Value.max.fixedY >= box2Value.min.fixedY) &&
                (box1Value.min.fixedZ <= box2Value.max.fixedZ) && (box1Value.max.fixedZ >= box2Value.min.fixedZ));
            
            return (short)(collide ? 1 : 0);
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

            // Can use Gribb/Hartman too. But this is more easy to read for me
            // Basically, transform the box into world space, after that check if at least one vertice's x,y,z is in near plane distance range or not (in w range)
            // Of course, if it's outside the w range, we would not even need to draw it at all. This means that is in view frustum would also consider intersect a win
            Vector3 boxMin = box.min.ToUnity();
            Vector3 boxMax = box.max.ToUnity();

            Vector4[] pointChecks = new Vector4[]
            {
                pv * new Vector4(boxMin.x, boxMin.y, boxMin.z, 1.0f),
                pv * new Vector4(boxMin.x, boxMin.y, boxMax.z, 1.0f),
                pv * new Vector4(boxMax.x, boxMin.y, boxMax.z, 1.0f),
                pv * new Vector4(boxMax.x, boxMin.y, boxMin.z, 1.0f),
                pv * new Vector4(boxMax.x, boxMax.y, boxMax.z, 1.0f),
                pv * new Vector4(boxMax.x, boxMax.y, boxMin.z, 1.0f),
                pv * new Vector4(boxMin.x, boxMax.y, boxMin.z, 1.0f),
                pv * new Vector4(boxMin.x, boxMax.y, boxMax.z, 1.0f)
            };

            for (int c = 0; c < 3; c++)
            {
                bool fullInside = false;

                for (int i = 0; i < pointChecks.Length; i++)
                {
                    if (pointChecks[i][c] > -pointChecks[i][3])
                    {
                        fullInside = true;
                        break;
                    }
                }

                if (!fullInside)
                {
                    return 0;
                }

                fullInside = false;

                for (int i = 0; i < pointChecks.Length; i++)
                {
                    if (pointChecks[i][c] < pointChecks[i][3])
                    {
                        fullInside = true;
                        break;
                    }
                }

                if (!fullInside)
                {
                    return 0;
                }
            }

            return 1;
        }
    }
}