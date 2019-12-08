namespace Atma.Entities
{
    using Atma.Memory;
    using Microsoft.Extensions.Logging;
    using System;

    internal sealed unsafe class ComponentDataArray : UnmanagedDispose
    {
        public int ElementSize { get; }

        public int Length { get; }

        private ILogger _logger;
        private ILoggerFactory _logFactory;
        private IAllocator _allocator;
        private AllocationHandle _memoryHandle;
        internal void* Memory => (void*)_memoryHandle.Address;
        private readonly ComponentType _componentType;
        private readonly ComponentTypeHelper _componentHelper;



        internal ComponentDataArray(ILoggerFactory logFactory, IAllocator allocator, ComponentType componentType, int length)
        {
            _logFactory = logFactory;
            _logger = _logFactory.CreateLogger<ComponentDataArray>();

            ElementSize = componentType.Size;
            Length = length;

            _componentType = componentType;
            _componentHelper = ComponentTypeHelper.Get(componentType);

            _allocator = allocator;
            _memoryHandle = allocator.Take(componentType.Size * length);
        }

        public Span<T> AsSpan<T>(int start = 0, int length = -1)
            where T : unmanaged
        {
            var componentType = ComponentType<T>.Type;
            Contract.EqualTo(componentType.ID, _componentType.ID);
            return AsSpan<T>(componentType, start, length);
        }

        internal Span<T> AsSpan<T>(ComponentType componentType, int start = 0, int length = -1)
            where T : unmanaged
        {
            if (length == -1)
                length = Length;

            Assert.Range(start, 0, Length);
            Assert.Range(start + length - 1, start, Length);
            Contract.EqualTo(componentType.ID, _componentType.ID);
            var src = (T*)_memoryHandle.Address;
            return new Span<T>((void*)(src + start), length);
        }

        internal void Reset(int index)
        {
            Assert.Range(index, 0, Length);

            var addr = (byte*)_memoryHandle.Address;
            var dst = addr + index * ElementSize;

            _componentHelper.Reset(dst);
        }

        internal void Move(int srcIndex, int dstIndex)
        {
            if (srcIndex == dstIndex)
                return;

            Assert.Range(srcIndex, 0, Length);
            Assert.Range(dstIndex, 0, Length);

            var addr = (byte*)_memoryHandle.Address;
            var src = addr + srcIndex * ElementSize;
            var dst = addr + dstIndex * ElementSize;

            _componentHelper.Copy(src, dst);

#if DEBUG
            _componentHelper.Reset(src);
#endif
        }

        internal void Copy(ref void* ptr, int dstIndex, int length, bool incrementSrc)
        {
            Assert.Range(dstIndex, 0, Length - length + 1);
            var addr = (byte*)_memoryHandle.Address;
            var dst = (void*)(addr + dstIndex * ElementSize);

            if (!incrementSrc)
            {
                while (length-- > 0)
                {
                    var savePtr = ptr;
                    _componentHelper.CopyAndMoveNext(ref savePtr, ref dst);
                }
            }
            else
            {
                while (length-- > 0)
                    _componentHelper.CopyAndMoveNext(ref ptr, ref dst);
            }
        }

        internal void Copy(void* ptr, int dstIndex)
        {
            Assert.Range(dstIndex, 0, Length);
            var addr = (byte*)_memoryHandle.Address;
            var dst = addr + dstIndex * ElementSize;

            _componentHelper.Copy(ptr, dst);
        }

        internal static void CopyTo(ComponentDataArray srcArray, int srcIndex, ComponentDataArray dstArray, int dstIndex)
        {
            Assert.EqualTo(srcArray._componentType.ID, dstArray._componentType.ID);
            Assert.Range(srcIndex, 0, srcArray.Length);
            Assert.Range(dstIndex, 0, dstArray.Length);

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
            _logger = null;
            _logFactory = null;
        }
    }
}