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
using System;

namespace Nofun.Module.VMGP3D
{
    [Module]
    public partial class VMGP3D
    {
        [ModuleCall]
        private int vMul(int fixedA, int fixedB)
        {
            return FixedUtil.FloatToFixed(FixedUtil.FixedToFloat(fixedA) * FixedUtil.FixedToFloat(fixedB));
        }

        [ModuleCall]
        private int vDiv(int fixedA, int fixedB)
        {
            return FixedUtil.FloatToFixed(FixedUtil.FixedToFloat(fixedA) / FixedUtil.FixedToFloat(fixedB));
        }

        [ModuleCall]
        private int vSqrt(int fixedV)
        {
            return FixedUtil.FloatToFixed((float)Math.Sqrt(FixedUtil.FixedToFloat(fixedV)));
        }

        [ModuleCall]
        private int vCos(int fixedV)
        {
            return FixedUtil.FloatToFixed((float)Math.Cos(FixedUtil.Fixed11PointToFloat((short)fixedV)));
        }

        [ModuleCall]
        private int vSin(int fixedV)
        {
            return FixedUtil.FloatToFixed((float)Math.Sin(FixedUtil.Fixed11PointToFloat((short)fixedV)));
        }
    }
}