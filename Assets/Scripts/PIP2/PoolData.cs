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

        public PoolData()
        {
            this.dataType = PoolDataType.None;
        }

        public PoolData(uint immInt)
        {
            this.immInt = immInt;
            this.dataType = PoolDataType.ImmInteger;
        }

        public PoolData(Action function)
        {
            this.function = function;
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
    }
}