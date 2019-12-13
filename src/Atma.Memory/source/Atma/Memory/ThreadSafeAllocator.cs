namespace Atma.Memory
{
    internal class ThreadSafeAllocator : IAllocator
    {
        private readonly IAllocator _allocator;
        public ThreadSafeAllocator(IAllocator allocator)
        {
            _allocator = allocator;
        }

        public void Dispose()
        {
            lock (_allocator)
                _allocator.Dispose();
        }

        public void Free(ref AllocationHandle handle)
        {
            lock (_allocator)
                _allocator.Free(ref handle);
        }

        public AllocationHandle Take(int size)
        {
            lock (_allocator)
                return _allocator.Take(size);
        }

        public AllocationHandle Transfer(ref AllocationHandle handle)
        {
            lock (_allocator)
                return _allocator.Transfer(ref handle);
        }
    }
}