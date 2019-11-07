namespace Atma.Memory
{
    using System;

    using static Atma.Debug;

    public unsafe struct NativeBuffer : IDisposable
    {
        private int _length, _maxLength;
        private AllocationHandle _handle;

        public int Length => _length;

        public int MaxLength => _maxLength;

        //public int ElementSize => SizeOf<T>.Size;

        public bool IsCreated => _maxLength > 0;

        public byte* RawPointer => (byte*)_handle.Address;
        public byte* EndPointer => (byte*)_handle.Address + _length;
        public byte* MaxPointer => (byte*)_handle.Address + _maxLength;

        public NativeBuffer(Allocator allocator, int length = 256)
        {
            Assert(length > 0);
            _handle = MemoryManager.Take(allocator, length);
            _length = 0;
            _maxLength = length;

        }

        /// <summary>
        /// clears the list and nulls out all items in the buffer
        /// </summary>
        public void Clear()
        {
            Assert(_handle.IsValid);
            Unsafe.ClearAlign16(RawPointer, _maxLength);
            _length = 0;
        }

        /// <summary>
        /// works just like clear except it does not null our all the items in the buffer. Useful when dealing with structs.
        /// </summary>
        public void Reset()
        {
            Assert(_handle.IsValid);
            _length = 0;
        }

        /// <summary>
        /// adds the item to the list
        /// </summary>
        public T* Add<T>(in T item)
            where T : unmanaged
        {
            Assert(_handle.IsValid);
            var sizeInBytes = SizeOf<T>.Size;

            EnsureCapacity(sizeInBytes);

            var len = _length;
            var location = (T*)EndPointer;
            *location = item;
            _length += sizeInBytes;
            return location;
        }


        /// <summary>
        /// if the buffer is at its max more space will be allocated to fit additionalItemCount
        /// </summary>
        public void EnsureCapacity(int additionalItemCount = 1)
        {
            Assert(_handle.IsValid);
            var neededLength = _length + additionalItemCount;
            if (neededLength < _maxLength)
                return;

            while (neededLength > _maxLength)
                _maxLength = Math.Max(_maxLength * 3, 48) / 2;

            Resize(_maxLength);
        }

        private void Resize(int newSize)
        {
            _maxLength = newSize;
            //copy data, get new handle, etc
            var newHandle = MemoryManager.Take(_handle.Allocator, _maxLength);
            if (_handle.IsValid)
            {
                var src = RawPointer;
                var dst = newHandle.Address;
                Unsafe.CopyAlign16(src, dst, _length);
                MemoryManager.Free(ref _handle);
            }
            _handle = newHandle;
        }


        public void Dispose()
        {
            Assert(_handle.IsValid);
            MemoryManager.Free(ref _handle);
            _length = 0;
            _maxLength = 0;
        }

        //public NativeSlice<T> Slice<T>() where T : unmanaged => Slice<T>(0, _length);

        //public NativeSlice<T> Slice<T>(int start) where T : unmanaged => Slice<T>(start, _length - start);

        //public NativeSlice<T> Slice<T>(int start, int length)
        //    where T : unmanaged
        //{

        //    length *= SizeOf<T>.Size;
        //    Assert(_handle.IsValid);
        //    Assert(start >= 0);
        //    Assert(length > 0);
        //    Assert(start + length <= _length);
        //    return new NativeSlice<T>(_handle, start, length);
        //}
    }
}
