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
        private void vVectorTransformV3(VMPtr<NativeVector3D> source, VMPtr<NativeVector3D> destination, int count)
        {
            Span<NativeVector3D> sourceSpan = source.AsSpan(system.Memory, count);
            Span<NativeVector3D> destSpan = source.AsSpan(system.Memory, count);
        
            for (int i = 0; i < count; i++)
            {
                destSpan[i] = currentMatrix.MultiplyPoint3x4(sourceSpan[i].ToUnity()).ToMophun();
            }
        }

        [ModuleCall]
        private void vVectorAdd(VMPtr<NativeVector3D> dest, VMPtr<NativeVector3D> lhs, VMPtr<NativeVector3D> rhs)
        {
            NativeVector3D lhsValue = lhs.Read(system.Memory);
            NativeVector3D rhsValue = rhs.Read(system.Memory);

            dest.Write(system.Memory, new NativeVector3D()
            {
                fixedX = lhsValue.fixedX + rhsValue.fixedX,
                fixedY = lhsValue.fixedY + rhsValue.fixedY,
                fixedZ = lhsValue.fixedZ + rhsValue.fixedZ,
            });
        }

        [ModuleCall]
        private void vVectorSub(VMPtr<NativeVector3D> dest, VMPtr<NativeVector3D> lhs, VMPtr<NativeVector3D> rhs)
        {
            NativeVector3D lhsValue = lhs.Read(system.Memory);
            NativeVector3D rhsValue = rhs.Read(system.Memory);

            dest.Write(system.Memory, new NativeVector3D()
            {
                fixedX = lhsValue.fixedX - rhsValue.fixedX,
                fixedY = lhsValue.fixedY - rhsValue.fixedY,
                fixedZ = lhsValue.fixedZ - rhsValue.fixedZ,
            });
        }

        // It just multiplies. Dot and product is separate function
        [ModuleCall]
        private void vVectorMul(VMPtr<NativeVector3D> dest, VMPtr<NativeVector3D> lhs, VMPtr<NativeVector3D> rhs)
        {
            NativeVector3D lhsValue = lhs.Read(system.Memory);
            NativeVector3D rhsValue = rhs.Read(system.Memory);

            dest.Write(system.Memory, new NativeVector3D()
            {
                fixedX = FixedUtil.FloatToFixed(FixedUtil.FixedToFloat(lhsValue.fixedX) * FixedUtil.FixedToFloat(rhsValue.fixedX)),
                fixedY = FixedUtil.FloatToFixed(FixedUtil.FixedToFloat(lhsValue.fixedY) * FixedUtil.FixedToFloat(rhsValue.fixedY)),
                fixedZ = FixedUtil.FloatToFixed(FixedUtil.FixedToFloat(lhsValue.fixedZ) * FixedUtil.FixedToFloat(rhsValue.fixedZ)),
            });
        }
    }
}