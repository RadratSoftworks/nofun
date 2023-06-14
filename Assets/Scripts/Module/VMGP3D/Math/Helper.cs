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
        private Vector4 MakePlane(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 edge1 = p2 - p1;
            Vector3 edge2 = p3 - p1;

            Vector3 normal = Vector3.Cross(edge1, edge2);
            float dist = -Vector3.Dot(normal, p1);

            return new Vector4(normal.x, normal.y, normal.z, dist);
        }

        [ModuleCall]
        private void vCreatePlaneFromPoly(VMPtr<NativePlane> destPlanePtr, VMPtr<NativeVector3D> polygonsPtr)
        {
            Span<NativeVector3D> polygons = polygonsPtr.AsSpan(system.Memory, 3);
            NativePlane plane = destPlanePtr.AsSpan(system.Memory)[0];

            Vector4 planeU = MakePlane(polygons[0].ToUnity(), polygons[1].ToUnity(), polygons[2].ToUnity());

            plane.normal = new NativeVector3D()
            {
                fixedX = FixedUtil.FloatToFixed(planeU.x),
                fixedY = FixedUtil.FloatToFixed(planeU.y),
                fixedZ = FixedUtil.FloatToFixed(planeU.z)
            };

            plane.fixedDistance = FixedUtil.FloatToFixed(planeU.w);
            destPlanePtr.Write(system.Memory, plane);
        }

        [ModuleCall]
        private short vCollisionVectorPoly(VMPtr<NativeVector3D> collisionPosPtr, VMPtr<NativeVector3D> lineVPtr,
            VMPtr<NativeVector3D> polygonVPtr, VMPtr<NativePlane> planePtr)
        {
            Vector4 plane;

            if (!planePtr.IsNull)
            {
                NativePlane planeCopy = planePtr.Read(system.Memory);
                plane = new Vector4(FixedUtil.FixedToFloat(planeCopy.normal.fixedX), FixedUtil.FixedToFloat(planeCopy.normal.fixedY),
                    FixedUtil.FixedToFloat(planeCopy.normal.fixedZ), FixedUtil.FixedToFloat(planeCopy.fixedDistance));
            }
            else
            {
                Span<NativeVector3D> polygonV = polygonVPtr.AsSpan(system.Memory, 3);
                plane = MakePlane(polygonV[0].ToUnity(), polygonV[1].ToUnity(), polygonV[2].ToUnity());
            }

            Span<NativeVector3D> lineVs = lineVPtr.AsSpan(system.Memory, 2);
            Vector3 dir = lineVs[1].ToUnity() - lineVs[0].ToUnity();
            Vector3 lineThrough = lineVs[0].ToUnity();

            float multiplier = Vector3.Dot(dir, plane);
            float otherSide = -plane.w - Vector3.Dot(plane, lineThrough);

            if (Math.Abs(multiplier) <= Mathf.Epsilon)
            {
                return 0;
            }

            float t = otherSide / multiplier;
            Vector3 collisionPoint = lineThrough + dir * t;

            collisionPosPtr.Write(system.Memory, collisionPoint.ToMophun());

            return 1;
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
            return vCollisionVectorPoly(collisionPointPtr, linePointsPtr, VMPtr<NativeVector3D>.Null, planePtr);
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

            Span<Vector4> pointChecks = stackalloc Vector4[]
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