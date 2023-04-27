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

using Nofun.Util;
using Nofun.VM;
using System;

using UnityEngine;

namespace Nofun.Module.VMGP3D
{
    // NOTE: The whole class is using UnityEngine math now. In the future, if someone wants to port
    // this to non-Unity (maybe?). They also have to make math function
    [Module]
    public partial class VMGP3D
    {
        private const int MatrixOrientationRowMajor = 0;
        private const int MatrixOrientationColumnMajor = 0x2000;
        private const int FullCircleDegrees = 360;

        private int currentMatrixOrientation = MatrixOrientationColumnMajor;

        private Matrix4x4 currentMatrix = Matrix4x4.identity;
        private Matrix4x4 projectionMatrix = Matrix4x4.identity;

        private Matrix4x4 ReadMatrix(VMPtr<V3DMatrix> matrix)
        {
            if (matrix.IsNull)
            {
                return currentMatrix;
            }

            Span<V3DMatrix> matrixValue = matrix.AsSpan(system.Memory);

            if (currentMatrixOrientation == MatrixOrientationColumnMajor)
            {
                return new Matrix4x4(
                    new Vector4(FixedUtil.FixedToFloat(matrixValue[0].m00), FixedUtil.FixedToFloat(matrixValue[0].m10), FixedUtil.FixedToFloat(matrixValue[0].m20), FixedUtil.FixedToFloat(matrixValue[0].m30)),
                    new Vector4(FixedUtil.FixedToFloat(matrixValue[0].m01), FixedUtil.FixedToFloat(matrixValue[0].m11), FixedUtil.FixedToFloat(matrixValue[0].m21), FixedUtil.FixedToFloat(matrixValue[0].m31)),
                    new Vector4(FixedUtil.FixedToFloat(matrixValue[0].m02), FixedUtil.FixedToFloat(matrixValue[0].m12), FixedUtil.FixedToFloat(matrixValue[0].m22), FixedUtil.FixedToFloat(matrixValue[0].m32)),
                    new Vector4(FixedUtil.FixedToFloat(matrixValue[0].m03), FixedUtil.FixedToFloat(matrixValue[0].m13), FixedUtil.FixedToFloat(matrixValue[0].m23), FixedUtil.FixedToFloat(matrixValue[0].m33))
                );
            }
            else
            {
                return new Matrix4x4(
                    new Vector4(FixedUtil.FixedToFloat(matrixValue[0].m00), FixedUtil.FixedToFloat(matrixValue[0].m01), FixedUtil.FixedToFloat(matrixValue[0].m02), FixedUtil.FixedToFloat(matrixValue[0].m03)),
                    new Vector4(FixedUtil.FixedToFloat(matrixValue[0].m10), FixedUtil.FixedToFloat(matrixValue[0].m11), FixedUtil.FixedToFloat(matrixValue[0].m12), FixedUtil.FixedToFloat(matrixValue[0].m13)),
                    new Vector4(FixedUtil.FixedToFloat(matrixValue[0].m20), FixedUtil.FixedToFloat(matrixValue[0].m21), FixedUtil.FixedToFloat(matrixValue[0].m22), FixedUtil.FixedToFloat(matrixValue[0].m23)),
                    new Vector4(FixedUtil.FixedToFloat(matrixValue[0].m30), FixedUtil.FixedToFloat(matrixValue[0].m31), FixedUtil.FixedToFloat(matrixValue[0].m32), FixedUtil.FixedToFloat(matrixValue[0].m33))
                );
            }
        }

        [ModuleCall]
        private void vMatrixSetCurrent(VMPtr<V3DMatrix> matrixPtr)
        {
            currentMatrix = ReadMatrix(matrixPtr);
        }

        [ModuleCall]
        private void vMatrixGetCurrent(VMPtr<V3DMatrix> matrixPtr)
        {
            Span<V3DMatrix> matrix = matrixPtr.AsSpan(system.Memory);

            if (currentMatrixOrientation == MatrixOrientationColumnMajor)
            {
                matrix[0].m00 = FixedUtil.FloatToFixed(currentMatrix.m00);
                matrix[0].m01 = FixedUtil.FloatToFixed(currentMatrix.m01);
                matrix[0].m02 = FixedUtil.FloatToFixed(currentMatrix.m02);
                matrix[0].m03 = FixedUtil.FloatToFixed(currentMatrix.m03);


                matrix[0].m10 = FixedUtil.FloatToFixed(currentMatrix.m10);
                matrix[0].m11 = FixedUtil.FloatToFixed(currentMatrix.m11);
                matrix[0].m12 = FixedUtil.FloatToFixed(currentMatrix.m12);
                matrix[0].m13 = FixedUtil.FloatToFixed(currentMatrix.m13);


                matrix[0].m21 = FixedUtil.FloatToFixed(currentMatrix.m20);
                matrix[0].m21 = FixedUtil.FloatToFixed(currentMatrix.m21);
                matrix[0].m22 = FixedUtil.FloatToFixed(currentMatrix.m22);
                matrix[0].m23 = FixedUtil.FloatToFixed(currentMatrix.m23);


                matrix[0].m30 = FixedUtil.FloatToFixed(currentMatrix.m30);
                matrix[0].m31 = FixedUtil.FloatToFixed(currentMatrix.m31);
                matrix[0].m32 = FixedUtil.FloatToFixed(currentMatrix.m32);
                matrix[0].m33 = FixedUtil.FloatToFixed(currentMatrix.m33);
            }
            else
            {
                matrix[0].m00 = FixedUtil.FloatToFixed(currentMatrix.m00);
                matrix[0].m10 = FixedUtil.FloatToFixed(currentMatrix.m01);
                matrix[0].m20 = FixedUtil.FloatToFixed(currentMatrix.m02);
                matrix[0].m30 = FixedUtil.FloatToFixed(currentMatrix.m03);


                matrix[0].m01 = FixedUtil.FloatToFixed(currentMatrix.m10);
                matrix[0].m11 = FixedUtil.FloatToFixed(currentMatrix.m11);
                matrix[0].m21 = FixedUtil.FloatToFixed(currentMatrix.m12);
                matrix[0].m31 = FixedUtil.FloatToFixed(currentMatrix.m13);


                matrix[0].m02 = FixedUtil.FloatToFixed(currentMatrix.m20);
                matrix[0].m12 = FixedUtil.FloatToFixed(currentMatrix.m21);
                matrix[0].m22 = FixedUtil.FloatToFixed(currentMatrix.m22);
                matrix[0].m32 = FixedUtil.FloatToFixed(currentMatrix.m23);


                matrix[0].m03 = FixedUtil.FloatToFixed(currentMatrix.m30);
                matrix[0].m13 = FixedUtil.FloatToFixed(currentMatrix.m31);
                matrix[0].m23 = FixedUtil.FloatToFixed(currentMatrix.m32);
                matrix[0].m33 = FixedUtil.FloatToFixed(currentMatrix.m33);
            }
        }

        [ModuleCall]
        private void vSetMatrixMode(int mode)
        {
            if ((mode == MatrixOrientationRowMajor) || (mode == MatrixOrientationColumnMajor))
            {
                currentMatrixOrientation = mode;
            }
        }

        [ModuleCall]
        private void vMatrixIdentity()
        {
            currentMatrix = Matrix4x4.identity;
        }

        [ModuleCall]
        private void vMatrixTranslate(int xFixed, int yFixed, int zFixed)
        {
            currentMatrix *= Matrix4x4.Translate(new Vector3(FixedUtil.FixedToFloat(xFixed), FixedUtil.FixedToFloat(yFixed), FixedUtil.FixedToFloat(zFixed)));
        }

        [ModuleCall]
        private void vMatrixRotateX(short d)
        {
            currentMatrix *= Matrix4x4.Rotate(Quaternion.AngleAxis(FixedUtil.Fixed11PointToFloat(d) * FullCircleDegrees, Vector3.right));
        }

        [ModuleCall]
        private void vMatrixRotateY(short d)
        {
            currentMatrix *= Matrix4x4.Rotate(Quaternion.AngleAxis(FixedUtil.Fixed11PointToFloat(d) * FullCircleDegrees, Vector3.up));
        }

        [ModuleCall]
        private void vMatrixRotateZ(short d)
        {
            currentMatrix *= Matrix4x4.Rotate(Quaternion.AngleAxis(FixedUtil.Fixed11PointToFloat(d) * FullCircleDegrees, Vector3.forward));
        }

        [ModuleCall]
        private void vMatrixPerspective(int widthFixed, int heightFixed, int zNearFixed, int zFarFixed)
        {
            float heightNearPlane = FixedUtil.FixedToFloat(heightFixed);
            float distNear = FixedUtil.FixedToFloat(zNearFixed);

            float fov = MathUtil.RadToDegs(Mathf.Atan(heightNearPlane / distNear / 2) * 2.0f);

            // Matrix column 2 in Unity already flipping Z, because of reversing view matrix flip
            // But we don't need that, so reverse it
            currentMatrix = Matrix4x4.Perspective(fov, FixedUtil.FixedToFloat(widthFixed) / heightNearPlane, distNear, FixedUtil.FixedToFloat(zFarFixed));
            currentMatrix.SetColumn(2, -currentMatrix.GetColumn(2));
        }

        [ModuleCall]
        void vMatrixSetProjection(VMPtr<V3DMatrix> matrix)
        {
            projectionMatrix = ReadMatrix(matrix);
            system.GraphicDriver.Set3DProjectionMatrix(projectionMatrix);
        }
    }
}