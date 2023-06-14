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
    // NOTE: The whole class is using UnityEngine math now. In the future, if someone wants to port
    // this to non-Unity (maybe?). They also have to make math function
    [Module]
    public partial class VMGP3D
    {
        private const int MatrixOrientationRowMajor = 0;
        private const int MatrixOrientationColumnMajor = 0x2000;
        private const int FullCircleDegrees = 360;
        private const double FullCircleRads = Math.PI * 2;

        private int currentMatrixOrientation = MatrixOrientationColumnMajor;

        private Matrix4x4 currentMatrix = Matrix4x4.identity;
        private Matrix4x4 projectionMatrix = Matrix4x4.identity;
        private Matrix4x4 lightMatrix = Matrix4x4.identity;

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


                matrix[0].m20 = FixedUtil.FloatToFixed(currentMatrix.m20);
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
        private void vMatrixInvert()
        {
            currentMatrix = Matrix4x4.Inverse(currentMatrix);
        }

        [ModuleCall]
        private void vMatrixMultiply(VMPtr<V3DMatrix> matrixPtr)
        {
            currentMatrix *= ReadMatrix(matrixPtr);
        }

        [ModuleCall]
        private void vMatrixTranslate(int xFixed, int yFixed, int zFixed)
        {
            currentMatrix *= Matrix4x4.Translate(new Vector3(FixedUtil.FixedToFloat(xFixed), FixedUtil.FixedToFloat(yFixed), FixedUtil.FixedToFloat(zFixed)));
        }

        [ModuleCall]
        private void vMatrixScale(int xFixed, int yFixed, int zFixed)
        {
            currentMatrix *= Matrix4x4.Scale(new Vector3(FixedUtil.FixedToFloat(xFixed), FixedUtil.FixedToFloat(yFixed), FixedUtil.FixedToFloat(zFixed)));
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
            currentMatrix = Matrix4x4.Perspective(fov, FixedUtil.FixedToFloat(widthFixed) / heightNearPlane, FixedUtil.FixedToFloat(zFarFixed), distNear);
            currentMatrix.SetColumn(2, -currentMatrix.GetColumn(2));
        }

        [ModuleCall]
        private void vMatrixOrtho(int widthFixed, int heightFixed, int zNearFixed, int zFarFixed)
        {
            currentMatrix = Matrix4x4.identity;
            currentMatrix.m00 = 2.0f / FixedUtil.FixedToFloat(widthFixed);
            currentMatrix.m11 = 2.0f / FixedUtil.FixedToFloat(heightFixed);
            currentMatrix.m22 = -2.0f / FixedUtil.FixedToFloat(zFarFixed - zNearFixed);
            currentMatrix.m23 = FixedUtil.FixedToFloat(zNearFixed + zFarFixed) / FixedUtil.FixedToFloat(zFarFixed - zNearFixed);
        }

        [ModuleCall]
        private void vMatrixLookAt(VMPtr<NativeVector3D> vEyePtr, VMPtr<NativeVector3D> vAtptr, VMPtr<NativeVector3D> vUpPtr)
        {
            Vector3 eye = vEyePtr.Read(system.Memory).ToUnity();
            Vector3 at = vAtptr.Read(system.Memory).ToUnity();
            Vector3 up = vUpPtr.Read(system.Memory).ToUnity();

            Matrix4x4 lookAtMatrix = Matrix4x4.identity;

            Vector3 zaxis = (at - eye).normalized;
            Vector3 xaxis = Vector3.Cross(up, zaxis).normalized;
            Vector3 yaxis = Vector3.Cross(zaxis, xaxis);

            lookAtMatrix.SetRow(0, xaxis);
            lookAtMatrix.SetRow(1, yaxis);
            lookAtMatrix.SetRow(2, zaxis);

            currentMatrix *= lookAtMatrix;
            currentMatrix *= Matrix4x4.Translate(-eye);
        }

        [ModuleCall]
        void vMatrixSetProjection(VMPtr<V3DMatrix> matrix)
        {
            projectionMatrix = ReadMatrix(matrix);
            system.GraphicDriver.ProjectionMatrix3D = projectionMatrix;
        }

        [ModuleCall]
        private void vMatrixSetLight(VMPtr<V3DMatrix> matrixPtr)
        {
            lightMatrix = ReadMatrix(matrixPtr);
        }
    }
}