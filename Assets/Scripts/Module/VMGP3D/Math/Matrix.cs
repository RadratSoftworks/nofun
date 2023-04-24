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
        
        private float FixedToFloat(int fixedValue)
        {
            return fixedValue / 65536.0f;
        }

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
        private void vSetMatrixMode(int mode)
        {
            if ((mode == MatrixOrientationRowMajor) || (mode == MatrixOrientationColumnMajor))
            {
                currentMatrixOrientation = mode;
            }
        }

        [ModuleCall]
        private void vMatrixPerspective(int widthFixed, int heightFixed, int zNearFixed, int zFarFixed)
        {
            currentMatrix = Matrix4x4.Perspective(1.0f, FixedToFloat(widthFixed) / FixedToFloat(heightFixed),
                FixedToFloat(zNearFixed), FixedToFloat(zFarFixed));
        }

        [ModuleCall]
        void vMatrixSetProjection(VMPtr<V3DMatrix> matrix)
        {
            projectionMatrix = ReadMatrix(matrix);
        }
    }
}