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
                    new Vector4(FixedToFloat(matrixValue[0].m00), FixedToFloat(matrixValue[0].m10), FixedToFloat(matrixValue[0].m20), FixedToFloat(matrixValue[0].m30)),
                    new Vector4(FixedToFloat(matrixValue[0].m01), FixedToFloat(matrixValue[0].m11), FixedToFloat(matrixValue[0].m21), FixedToFloat(matrixValue[0].m31)),
                    new Vector4(FixedToFloat(matrixValue[0].m02), FixedToFloat(matrixValue[0].m12), FixedToFloat(matrixValue[0].m22), FixedToFloat(matrixValue[0].m32)),
                    new Vector4(FixedToFloat(matrixValue[0].m03), FixedToFloat(matrixValue[0].m13), FixedToFloat(matrixValue[0].m23), FixedToFloat(matrixValue[0].m33))
                );
            }
            else
            {
                return new Matrix4x4(
                    new Vector4(FixedToFloat(matrixValue[0].m00), FixedToFloat(matrixValue[0].m01), FixedToFloat(matrixValue[0].m02), FixedToFloat(matrixValue[0].m03)),
                    new Vector4(FixedToFloat(matrixValue[0].m10), FixedToFloat(matrixValue[0].m11), FixedToFloat(matrixValue[0].m12), FixedToFloat(matrixValue[0].m13)),
                    new Vector4(FixedToFloat(matrixValue[0].m20), FixedToFloat(matrixValue[0].m21), FixedToFloat(matrixValue[0].m22), FixedToFloat(matrixValue[0].m23)),
                    new Vector4(FixedToFloat(matrixValue[0].m30), FixedToFloat(matrixValue[0].m31), FixedToFloat(matrixValue[0].m32), FixedToFloat(matrixValue[0].m33))
                );
            }
        }

        [ModuleCall]
        private void vMatrixGetCurrent(VMPtr<V3DMatrix> matrixPtr)
        {
            Span<V3DMatrix> matrix = matrixPtr.AsSpan(system.Memory);

            if (currentMatrixOrientation == MatrixOrientationColumnMajor)
            {
                matrix[0].m00 = FloatToFixed(currentMatrix.m00);
                matrix[0].m01 = FloatToFixed(currentMatrix.m01);
                matrix[0].m02 = FloatToFixed(currentMatrix.m02);
                matrix[0].m03 = FloatToFixed(currentMatrix.m03);


                matrix[0].m10 = FloatToFixed(currentMatrix.m10);
                matrix[0].m11 = FloatToFixed(currentMatrix.m11);
                matrix[0].m12 = FloatToFixed(currentMatrix.m12);
                matrix[0].m13 = FloatToFixed(currentMatrix.m13);


                matrix[0].m21 = FloatToFixed(currentMatrix.m20);
                matrix[0].m21 = FloatToFixed(currentMatrix.m21);
                matrix[0].m22 = FloatToFixed(currentMatrix.m22);
                matrix[0].m23 = FloatToFixed(currentMatrix.m23);


                matrix[0].m30 = FloatToFixed(currentMatrix.m30);
                matrix[0].m31 = FloatToFixed(currentMatrix.m31);
                matrix[0].m32 = FloatToFixed(currentMatrix.m32);
                matrix[0].m33 = FloatToFixed(currentMatrix.m33);
            }
            else
            {
                matrix[0].m00 = FloatToFixed(currentMatrix.m00);
                matrix[0].m10 = FloatToFixed(currentMatrix.m01);
                matrix[0].m20 = FloatToFixed(currentMatrix.m02);
                matrix[0].m30 = FloatToFixed(currentMatrix.m03);


                matrix[0].m01 = FloatToFixed(currentMatrix.m10);
                matrix[0].m11 = FloatToFixed(currentMatrix.m11);
                matrix[0].m21 = FloatToFixed(currentMatrix.m12);
                matrix[0].m31 = FloatToFixed(currentMatrix.m13);


                matrix[0].m02 = FloatToFixed(currentMatrix.m20);
                matrix[0].m12 = FloatToFixed(currentMatrix.m21);
                matrix[0].m22 = FloatToFixed(currentMatrix.m22);
                matrix[0].m32 = FloatToFixed(currentMatrix.m23);


                matrix[0].m03 = FloatToFixed(currentMatrix.m30);
                matrix[0].m13 = FloatToFixed(currentMatrix.m31);
                matrix[0].m23 = FloatToFixed(currentMatrix.m32);
                matrix[0].m33 = FloatToFixed(currentMatrix.m33);
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
            currentMatrix *= Matrix4x4.Translate(new Vector3(FixedToFloat(xFixed), FixedToFloat(yFixed), FixedToFloat(zFixed)));
        }

        [ModuleCall]
        private void vMatrixRotateX(int d)
        {
            currentMatrix *= Matrix4x4.Rotate(Quaternion.AngleAxis(d % 4096, Vector3.right));
        }

        [ModuleCall]
        private void vMatrixRotateY(int d)
        {
            currentMatrix *= Matrix4x4.Rotate(Quaternion.AngleAxis(d % 4096, Vector3.up));
        }

        [ModuleCall]
        private void vMatrixRotateZ(int d)
        {
            currentMatrix *= Matrix4x4.Rotate(Quaternion.AngleAxis(d % 4096, Vector3.forward));
        }

        [ModuleCall]
        private void vMatrixPerspective(int widthFixed, int heightFixed, int zNearFixed, int zFarFixed)
        {
            float heightNearPlane = FixedToFloat(heightFixed);
            float distNear = FixedToFloat(zNearFixed);

            float fov = MathUtil.RadToDegs(Mathf.Atan(heightNearPlane / distNear / 2) * 2.0f);

            // Matrix column 2 in Unity already flipping Z, because of reversing view matrix flip
            // But we don't need that, so reverse it
            currentMatrix = Matrix4x4.Perspective(fov, FixedToFloat(widthFixed) / heightNearPlane, distNear, FixedToFloat(zFarFixed));
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