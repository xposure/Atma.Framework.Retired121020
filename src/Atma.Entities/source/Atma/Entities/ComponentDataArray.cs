﻿namespace Atma.Entities
{
    using Atma.Memory;
    using static Atma.Debug;

    using System;

    public sealed unsafe class ComponentDataArray : UnmanagedDispose//, IComponentDataArray2
    {
        public int ElementSize { get; }

        public int Length { get; }

        private IAllocator _allocator;
        private AllocationHandle _memoryHandle;
        private readonly ComponentType _componentType;
        private readonly ComponentTypeHelper _componentHelper;


        public ComponentDataArray(IAllocator allocator, ComponentType componentType, int length)
        {
            ElementSize = componentType.Size;
            Length = length;

            _componentType = componentType;
            _componentHelper = ComponentTypeHelper.Get(componentType);

            _allocator = allocator;
            _memoryHandle = allocator.Take(componentType.Size * length);
        }

        public Span<T> AsSpan<T>()
            where T : unmanaged
        {
            var componentType = ComponentType<T>.Type;
            if (componentType.ID != _componentType.ID)
                throw new Exception("Invalid type.");

            // if (!_lock.TryEnterWriteLock(0))
            //     throw new Exception("Could not take write lock on component data!");

            return new Span<T>((void*)_memoryHandle.Address, Length);
            //return new ComponentDataArrayWriteLock(_lock);
        }


        public T* AsPointer<T>()
            where T : unmanaged
        {
            var componentType = ComponentType<T>.Type;
            if (componentType.ID != _componentType.ID)
                throw new Exception("Invalid type.");

            // if (!_lock.TryEnterWriteLock(0))
            //     throw new Exception("Could not take write lock on component data!");

            return (T*)_memoryHandle.Address;
            //return new ComponentDataArrayWriteLock(_lock);
        }

        // public ComponentDataArrayReadLock AsReadOnlySpan<T>(out ReadOnlySpan<T> span)
        //   where T : unmanaged
        // {
        //     var componentType = ComponentType<T>.Type;
        //     if (componentType.ID != _componentType.ID)
        //         throw new Exception("Invalid type.");

        //     if (!_lock.TryEnterReadLock(0))
        //         throw new Exception("Could not take write lock on component data!");

        //     span = new Span<T>(_memoryHandle.Address, Length);
        //     return new ComponentDataArrayReadLock(_lock);
        // }

        public void Reset(int index)
        {
            Assert(index >= 0 && index < Length);
            // if (!_lock.TryEnterWriteLock(0))
            //     throw new Exception("Could not take write lock on component data!");

            var addr = (byte*)_memoryHandle.Address;
            var dst = addr + index * ElementSize;

            _componentHelper.Reset(dst);
            //_lock.ExitWriteLock();
        }

        public void Move(int srcIndex, int dstIndex)
        {
            if (srcIndex == dstIndex)
                return;

            Assert(srcIndex >= 0 && srcIndex < Length && dstIndex >= 0 && dstIndex < Length);
            // if (!_lock.TryEnterWriteLock(0))
            //     throw new Exception("Could not take write lock on component data!");

            var addr = (byte*)_memoryHandle.Address;
            var src = addr + srcIndex * ElementSize;
            var dst = addr + dstIndex * ElementSize;

            _componentHelper.Copy(src, dst);

#if DEBUG
            _componentHelper.Reset(src);
#endif
            //_lock.ExitWriteLock();
        }

        public static void CopyTo(ComponentDataArray srcArray, int srcIndex, ComponentDataArray dstArray, int dstIndex)
        {
            Assert(srcArray._componentType.ID == dstArray._componentType.ID);
            Assert(srcIndex >= 0 && srcIndex < srcArray.Length && dstIndex >= 0 && dstIndex < dstArray.Length);

            var srcAddr = (byte*)srcArray._memoryHandle.Address;
            var dstAddr = (byte*)dstArray._memoryHandle.Address;
            var src = srcAddr + srcIndex * srcArray.ElementSize;
            var dst = dstAddr + dstIndex * srcArray.ElementSize;

            srcArray._componentHelper.Copy(src, dst);

#if DEBUG
            srcArray._componentHelper.Reset(src);
#endif
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