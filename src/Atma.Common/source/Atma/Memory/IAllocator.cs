namespace Atma.Memory
{
    using System;

    public enum AllocatorBounds
    {
        Front,
        Back
    }

    public interface IAllocator : IDisposable
    {
        AllocationHandle Take(int size, AllocatorBounds bounds = AllocatorBounds.Front);
        void Free(ref AllocationHandle handle);
    }

    public unsafe struct AllocationHandle //: IDisposable
    {
        public readonly void* Address;
        public readonly uint Id;
        public readonly uint Length;

        public AllocationHandle(void* address, uint id, uint length)
        {
            Address = address;
            Id = id;
            Length = length;
        }
    }
}