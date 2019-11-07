namespace Atma.Entities
{
    using Atma.Memory;
    using static Atma.Debug;

    using System;
    using System.Threading;
    using Atma.Common;


    public unsafe class ComponentDataArray2 : UnmanagedDispose, IComponentDataArray2
    {
        public int ElementSize { get; }

        public int Length { get; }

        private IAllocator _allocator;
        private AllocationHandle _memoryHandle;

        public ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public ComponentDataArray2(IAllocator allocator, ComponentType type, int length)
        {
            ElementSize = type.Size;
            Length = length;

            _allocator = allocator;
            _memoryHandle = allocator.Take(type.Size * length);
        }

        public ComponentDataArrayWriteLock AsSpan<T>(out Span<T> span)
            where T : unmanaged
        {
            Assert(UnmanagedType<T>.Type.Size == ElementSize);

            if (!_lock.TryEnterWriteLock(0))
                throw new Exception("Could not take write lock on component data!");

            span = new Span<T>(_memoryHandle.Address, Length);
            return new ComponentDataArrayWriteLock(_lock);
        }

        public ComponentDataArrayReadLock AsReadOnlySpan<T>(out ReadOnlySpan<T> span)
          where T : unmanaged
        {
            Assert(UnmanagedType<T>.Type.Size == ElementSize);
            if (!_lock.TryEnterReadLock(0))
                throw new Exception("Could not take write lock on component data!");

            span = new Span<T>(_memoryHandle.Address, Length);
            return new ComponentDataArrayReadLock(_lock);
        }

        public void Move<T>(int src, int dst)
            where T : unmanaged
        {
            Assert(UnmanagedType<T>.Type.Size == ElementSize);
            Assert(src >= 0 && src < Length && dst >= 0 && dst < Length);
            if (!_lock.TryEnterWriteLock(0))
                throw new Exception("Could not take write lock on component data!");

            var data = (T*)_memoryHandle.Address;
            data[dst] = data[src];
            _lock.ExitWriteLock();
        }

        protected override void OnUnmanagedDispose()
        {
            _allocator.Free(ref _memoryHandle);
        }

        protected override void OnManagedDispose()
        {
            _allocator = null;
        }
    }
}