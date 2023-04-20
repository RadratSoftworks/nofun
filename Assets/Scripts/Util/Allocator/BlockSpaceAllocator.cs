using System.Collections.Generic;
using System;

namespace Nofun.Util.Allocator
{
    public class BlockAllocator : ISpaceAllocator
    {
        private class BlockInfo
        {
            public long offset;
            public long size;

            public bool active;
        };

        private List<BlockInfo> blocks;
        private long maxSize;

        public BlockAllocator(long maxSize)
        {
            this.maxSize = maxSize;
            this.blocks = new();
        }

        public long Allocate(uint bytes)
        {
            long roundedSize = MemoryUtil.AlignUp(bytes, 4);
            long farthestEndOffset = 0;

            foreach (BlockInfo block in blocks)
            {
                farthestEndOffset = Math.Max(farthestEndOffset, block.offset + block.size);

                if (!block.active && block.size >= roundedSize)
                {
                    // Gonna use it right away
                    if (block.size == roundedSize)
                    {
                        block.active = true;
                        return block.offset;
                    }

                    long returnValue = block.offset;

                    // Divide it to two if we can
                    BlockInfo newBlock = new BlockInfo()
                    {
                        active = true,
                        offset = block.offset,
                        size = roundedSize
                    };

                    // Change our old block
                    block.size -= roundedSize;
                    block.offset += roundedSize;

                    blocks.Add(newBlock);
                    return returnValue;
                }
            }

            if (farthestEndOffset + roundedSize > maxSize)
            {
                return -1;
            }

            // We try our best, but we can't
            // We should alloc new block
            BlockInfo newBlockFin = new BlockInfo()
            {
                active = true,
                offset = farthestEndOffset,
                size = roundedSize
            };

            blocks.Add(newBlockFin);

            return farthestEndOffset;
        }

        public void Free(long offset)
        {
            BlockInfo block = blocks.Find(block => block.offset == offset);

            if (block != null)
            {
                block.active = false;
            }
        }
    }
}