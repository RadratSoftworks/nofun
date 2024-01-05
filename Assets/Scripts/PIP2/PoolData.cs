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

using System;

namespace Nofun.PIP2
{
    public enum PoolDataType
    {
        None,
        Import,
        ImmInteger,
        ImmFloat
    };

    public class PoolData
    {
        private PoolDataType dataType;
        private Action function;
        private uint immInt;
        private float immFloat;
        private string name;

        public PoolData()
        {
            this.dataType = PoolDataType.None;
        }

        public PoolData(uint immInt, string name = "")
        {
            this.immInt = immInt;
            this.name = name;
            this.dataType = PoolDataType.ImmInteger;
        }

        public PoolData(Action function, string name)
        {
            this.function = function;
            this.name = name;
            this.dataType = PoolDataType.Import;
        }

        public PoolData(float immFloat)
        {
            this.immFloat = immFloat;
            this.dataType = PoolDataType.ImmFloat;
        }

        public PoolDataType DataType => dataType;

        public uint? ImmediateInteger => (dataType == PoolDataType.ImmInteger) ? immInt : null;
        public float? ImmediateFloat => (dataType == PoolDataType.ImmFloat) ? immFloat : null;
        public Action Function => (dataType == PoolDataType.Import) ? function : null;
        public string Name => name;
        public bool IsInCode { get; set; }
        public bool IsCodePointerRelocatedInData { get; set; }
    }
}
