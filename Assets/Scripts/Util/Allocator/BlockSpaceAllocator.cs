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

using System.Collections.Generic;
using System;
using Nofun.Util.Logging;

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
        private long allocated;

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

                    allocated += roundedSize;

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
            allocated += roundedSize;

            return farthestEndOffset;
        }

        public void Free(long offset)
        {
            BlockInfo block = blocks.Find(block => block.offset == offset);

            if (block != null)
            {
                block.active = false;
                allocated -= block.size;
            }
            else
            {
                Logger.Error(LogClass.VMGP3D, $"Can't find offset {offset}");
            }
        }

        public long AmountFree => Math.Max(maxSize - allocated, 0);
    }
}