namespace Atma.Memory
{
    using System;
    using System.Collections.Generic;

    using static Atma.Debug;

    public unsafe struct NativeStack<T> : IDisposable
        where T : unmanaged
    {
        private int _length, _maxLength;
        private AllocationHandle _handle;
        //private AllocatorBounds _bounds;
        private IAllocator _allocator;

        public int Length => _length;

        public int MaxLength => _maxLength;

        public int ElementSize => SizeOf<T>.Size;

        public bool IsCreated => _maxLength > 0;

        public T* RawPointer => (T*)_handle.Address;
        public T* EndPointer => (T*)_handle.Address + _length;
        public T* MaxPointer => (T*)_handle.Address + _maxLength;

        public NativeStack(IAllocator allocator, int length = 8)
        {
            _allocator = allocator;
            var sizeOfElement = SizeOf<T>.Size;

            Assert(length > 0);
            _handle = allocator.Take<T>(length, AllocatorBounds.Front);// MemoryManager.Take(allocator, sizeOfElement * length);
            _length = 0;
            _maxLength = length;
        }

        /// <summary>
        /// provided for ease of access though it is recommended to just access the buffer directly.
        /// </summary>
        /// <param name="index">Index.</param>
        public ref T this[int index]
        {
            get
            {
                //Assert(_handle.IsValid);
                Assert(index >= 0 && index < _length);
                return ref RawPointer[index];
            }
        }

        /// <summary>
        /// clears the list and nulls out all items in the buffer
        /// </summary>
        public void Clear()
        {
            //Assert(_handle.IsValid);
            Unsafe.ClearAlign16(RawPointer, ElementSize * _maxLength);
            _length = 0;
        }

        /// <summary>
        /// works just like clear except it does not null our all the items in the buffer. Useful when dealing with structs.
        /// </summary>
        public void Reset()
        {
            //Assert(_handle.IsValid);
            _length = 0;
        }

        /// <summary>
        /// adds the item to the list
        /// </summary>
        public void Push(in T item)
        {
            //Assert(_handle.IsValid);
            if (_length == _maxLength)
            {
                _maxLength = Math.Max(_maxLength * 3, 16) / 2;
                Resize(_maxLength);
            }
            RawPointer[_length++] = item;
        }

        /// <summary>
        /// removes the item from the list
        /// </summary>
        /// <param name="item">Item.</param>
        public T Pop()
        {
            //Assert(_handle.IsValid);
            Assert(_length > 0);
            return RawPointer[--_length];
        }


        public T Peek()
        {
            //Assert(_handle.IsValid);
            Assert(_length > 0);
            return RawPointer[_length];
        }

        /// <summary>
        /// if the buffer is at its max more space will be allocated to fit additionalItemCount
        /// </summary>
        public void EnsureCapacity(int additionalItemCount = 1)
        {
            //Assert(_handle.IsValid);
            var neededLength = _length + additionalItemCount;
            if (neededLength < _maxLength)
                return;

            while (neededLength > _maxLength)
                _maxLength = Math.Max(_maxLength * 3, 48) / 2;

            Resize(_maxLength);
        }

        private void Resize(int newSize)
        {
            // _maxLength = newSize;
            // //copy data, get new handle, etc
            // //_allocator.Free(_handle);

            // Assert(newSize > 0);
            // var sizeOfElement = SizeOf<T>.Size;
            // var newHandle = _allocator.Take<T>(_maxLength);// MemoryManager.Take(allocator, sizeOfElement * length);

            // var src = RawPointer;
            // var dst = (T*)newHandle.Address;
            // for (var i = 0; i < _length; i++)
            //     dst[i] = src[i];

            // _allocator.Free(ref _handle);
            // _handle = newHandle;
        }

        ///// <summary>
        ///// adds all items from array
        ///// </summary>
        ///// <param name="array">Array.</param>
        //public void AddRange(IEnumerable<T> array)
        //{
        //    Assert(_handle.IsValid);
        //    foreach (var item in array)
        //        Add(item);
        //}

        ///// <summary>
        ///// sorts all items in the buffer up to length
        ///// </summary>
        //public void Sort(Comparison<T> comparison)
        //{
        //    Assert(_handle.IsValid);
        //    Span.Sort(comparison);
        //}

        ///// <summary>
        ///// sorts all items in the buffer up to length
        ///// </summary>
        //public void Sort(IComparer<T> comparer)
        //{
        //    Assert(_handle.IsValid);
        //    Span.Sort(comparer);
        //}

        public void Dispose()
        {
            //Assert(_handle.IsValid);
            //MemoryManager.Free(ref _handle);
            _allocator.Free(ref _handle);
            _length = 0;
            _maxLength = 0;
        }

        //public static implicit operator NativeSlice<T>(NativeList<T> arr) => arr.Slice();
        //public NativeSlice<T> Slice() => Slice(0, _length);

        //public NativeSlice<T> Slice(int start) => Slice(start, _length - start);

        //public NativeSlice<T> Slice(int start, int length)
        //{
        //    Assert(_handle.IsValid);
        //    Assert(start >= 0);
        //    Assert(length > 0);
        //    Assert(start + length <= _length);
        //    return new NativeSlice<T>(_handle, start, length);
        //}

        //public Span<T> Span
        //{
        //    get
        //    {
        //        Assert(_handle.IsValid);
        //        return new Span<T>(RawPointer, _length);
        //    }
        //}
    }
}
