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

namespace Nofun.Util.Allocator
{
    public interface ISpaceAllocator
    {
        /// <summary>
        /// Allocate memory from the space.
        /// 
        /// The memory offset should be aligned to 4 bytes.
        /// </summary>
        /// <param name="size">Size of memory to allocate.</param>
        /// <returns>-1 on full, else offset of the memory in the space.</returns>
        long Allocate(uint size);

        /// <summary>
        /// Free an allocated memory.
        /// </summary>
        /// <param name="offset">Offset returned by allocate. If this offset does not point to
        /// an existing memory, the allocator should do nothing.</param>
        void Free(long offset);

        /// <summary>
        /// Get the amount of free memory available.
        /// </summary>
        /// <returns>Free amount of memory available in bytes unit.</returns>
        long AmountFree { get; }
    };
}