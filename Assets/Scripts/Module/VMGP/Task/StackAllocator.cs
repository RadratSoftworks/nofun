using System;
using System.Collections.Generic;
using Nofun.Util.Allocator;
using UnityEngine;

namespace Nofun.Module.VMGP
{
    public class StackAllocator
    {
        public static readonly uint MaxTaskStackCount = 20;

        private uint stackSectionBase = 0;
        private uint defaultStackSize = 0;

        private ISpaceAllocator stackAllocatorImpl = null;
        private Dictionary<uint, uint> stackTopToStackBottom = new();

        /// <summary>
        /// Calculate the maximum amount of memory that all tasks can use as stack.
        /// </summary>
        /// <param name="defaultStackSize">The default stack size declared in program header.</param>
        /// <returns>The maximum total memory that will be used for stack.</returns>
        public static uint CalculateMaxTotalStackSize(uint defaultStackSize)
        {
            return Math.Min(0x10000, defaultStackSize * 4) * MaxTaskStackCount;
        }

        public StackAllocator(uint stackSectionBase, uint defaultStackSize, uint totalSize)
        {
            this.stackSectionBase = stackSectionBase;
            this.defaultStackSize = defaultStackSize;
            this.stackAllocatorImpl = new BlockAllocator(totalSize);
        }

        /// <summary>
        /// Allocate stack for a task.
        /// </summary>
        /// <param name="stackSize">The size of stack. If -1 is passed, the program stack size will be used.</param>
        /// <returns>Address of the stack top (which is at the end of the task stack section).</returns>
        /// <exception cref="Exception"></exception>
        public uint Allocate(long stackSize)
        {
            if (stackSize == -1)
            {
                stackSize = defaultStackSize;
            }

            if (stackSize > defaultStackSize)
            {
                throw new Exception($"Stack size is too large. Size={stackSize}");
            }

            long offset = stackAllocatorImpl.Allocate((uint) stackSize);
            if (offset < 0)
            {
                throw new Exception("Stack allocation failed.");
            }

            uint stackBottom = stackSectionBase + (uint) offset;
            uint stackTop = stackBottom + (uint) stackSize;

            stackTopToStackBottom.Add(stackTop, stackBottom);
            return stackTop;
        }

        /// <summary>
        /// Free stack that was allocated.
        /// </summary>
        /// <param name="addr">The address of the stack previously allocated</param>
        public void Free(uint addr)
        {
            if (!stackTopToStackBottom.ContainsKey(addr))
            {
                throw new Exception("Stack not found.");
            }

            uint stackBottom = stackTopToStackBottom[addr];

            stackTopToStackBottom.Remove(addr);
            stackAllocatorImpl.Free(stackBottom - stackSectionBase);
        }
    }
}