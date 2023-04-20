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
    };
}