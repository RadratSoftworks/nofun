using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Nofun.VM
{
    public struct VMPtr<T> where T : struct
    {
        public UInt32 address;

        public UInt32 Value => address;

        public VMPtr(UInt32 address)
        {
            this.address = address;
        }

        public static VMPtr<T> operator + (VMPtr<T> lhs, int rhs)
        {
            return new VMPtr<T>(lhs.address + (uint)(rhs * Marshal.SizeOf<T>()));
        }

        public static bool operator == (VMPtr<T> lhs, VMPtr<T> rhs)
        {
            return lhs.address == rhs.address;
        }

        public static bool operator != (VMPtr<T> lhs, VMPtr<T> rhs)
        {
            return lhs.address != rhs.address;
        }

        public VMPtr<T> this[int rhs]
        {
            get
            {
                return this + rhs;
            }
        }

        public T Read(VMMemory memory)
        {
            return AsSpan(memory, 1)[0];
        }

        public Span<T> AsSpan(VMMemory memory, int count, int startOffset = 0)
        {
            return MemoryMarshal.Cast<byte, T>(memory.GetMemorySpan((int)address + startOffset, Marshal.SizeOf<T>() * count));
        }

        public void Write(VMMemory memory, T value)
        {
            MemoryMarshal.Write(memory.GetMemorySpan((int)address, Marshal.SizeOf<T>()), ref value);
        }

        public static VMPtr<T> Null => new VMPtr<T>(0);
    }
}