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
        private ComponentType _componentType;
        private ComponentTypeHelper _componentHelper;

        public ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public ComponentDataArray2(IAllocator allocator, ComponentType componentType, int length)
        {
            ElementSize = componentType.Size;
            Length = length;

            _componentType = componentType;
            _componentHelper = ComponentTypeHelper.Get(componentType);

            _allocator = allocator;
            _memoryHandle = allocator.Take(componentType.Size * length);
        }

        public ComponentDataArrayWriteLock AsSpan<T>(out Span<T> span)
            where T : unmanaged
        {
            var componentType = ComponentType<T>.Type;
            if (componentType.ID != _componentType.ID)
                throw new Exception("Invalid type.");

            if (!_lock.TryEnterWriteLock(0))
                throw new Exception("Could not take write lock on component data!");

            span = new Span<T>(_memoryHandle.Address, Length);
            return new ComponentDataArrayWriteLock(_lock);
        }

        public ComponentDataArrayReadLock AsReadOnlySpan<T>(out ReadOnlySpan<T> span)
          where T : unmanaged
        {
            var componentType = ComponentType<T>.Type;
            if (componentType.ID != _componentType.ID)
                throw new Exception("Invalid type.");

            if (!_lock.TryEnterReadLock(0))
                throw new Exception("Could not take write lock on component data!");

            span = new Span<T>(_memoryHandle.Address, Length);
            return new ComponentDataArrayReadLock(_lock);
        }

        public void Reset(int index)
        {
            Assert(index >= 0 && index < Length);
            if (!_lock.TryEnterWriteLock(0))
                throw new Exception("Could not take write lock on component data!");

            var addr = (byte*)_memoryHandle.Address;
            var dst = addr + index * ElementSize;

            _componentHelper.Reset(dst);
            _lock.ExitWriteLock();
        }

        public void Move(int srcIndex, int dstIndex)
        {
            if (srcIndex == dstIndex)
                return;

            Assert(srcIndex >= 0 && srcIndex < Length && dstIndex >= 0 && dstIndex < Length);
            if (!_lock.TryEnterWriteLock(0))
                throw new Exception("Could not take write lock on component data!");

            var addr = (byte*)_memoryHandle.Address;
            var src = addr + srcIndex * ElementSize;
            var dst = addr + dstIndex * ElementSize;

            _componentHelper.Copy(src, dst);
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