namespace Atma.Memory
{
    using System;
    internal struct NativeStackData
    {
        public int Length;
        public int MaxLength;
        public AllocationHandle Handle;
    }

    public unsafe struct NativeStack<T> : IDisposable
        where T : unmanaged
    {


        internal IAllocator Allocator;

        //since this is resizable, we either have to force people to remember they
        //must pass this around by "ref", or we should double alloc
        //one for a persisent location of the handle info and one for the data
        //internal AllocationHandle Handle;

        //TODO: We are using a double alloc for NativeList to support it being passed around without ref        
        private NativeStackData* _listInfo;
        internal AllocationHandle Handle;

        public int Length { get => _listInfo->Length; private set => _listInfo->Length = value; }
        public int MaxLength { get => _listInfo->MaxLength; private set => _listInfo->MaxLength = value; }

        public bool IsCreated => _listInfo->MaxLength > 0;
        public int ElementSize => SizeOf<T>.Size;

        public T* RawPointer => (T*)_listInfo->Handle.Address;
        public T* EndPointer => (T*)_listInfo->Handle.Address + _listInfo->Length;
        public T* MaxPointer => (T*)_listInfo->Handle.Address + _listInfo->MaxLength;

        public NativeStack(IAllocator allocator, int length = 8)
        {
            Allocator = allocator;
            var sizeOfElement = SizeOf<T>.Size;

            Assert.GreatherThan(length, 0);
            Handle = Allocator.Take<NativeStackData>(1);

            _listInfo = (NativeStackData*)Handle.Address;
            _listInfo->Handle = Allocator.Take<T>(length);

            Length = 0;
            MaxLength = length;
        }

        /// <summary>
        /// provided for ease of access though it is recommended to just access the buffer directly.
        /// </summary>
        /// <param name="index">Index.</param>
        public ref T this[int index]
        {
            get
            {
                Assert.EqualTo(Handle.IsValid, true);
                Assert.Range(index, 0, Length);
                return ref RawPointer[index];
            }
        }

        /// <summary>
        /// clears the list and nulls out all items in the buffer
        /// </summary>
        public void Clear()
        {
            Assert.EqualTo(Handle.IsValid, true);
            Unsafe.ClearAlign16(RawPointer, ElementSize * MaxLength);
            Length = 0;
        }

        /// <summary>
        /// works just like clear except it does not null our all the items in the buffer. Useful when dealing with structs.
        /// </summary>
        public void Reset()
        {
            Assert.EqualTo(Handle.IsValid, true);
            Length = 0;
        }

        /// <summary>
        /// adds the item to the list
        /// </summary>
        public void Push(in T item)
        {
            Assert.EqualTo(Handle.IsValid, true);
            if (Length == MaxLength)
            {
                MaxLength = Math.Max(MaxLength * 3, 16) / 2;
                Resize(MaxLength);
            }
            RawPointer[Length++] = item;
        }

        /// <summary>
        /// removes the item from the list
        /// </summary>
        /// <param name="item">Item.</param>
        public T Pop()
        {
            Assert.EqualTo(Handle.IsValid, true);
            Assert.GreatherThan(Length, 0);
            return RawPointer[--Length];
        }


        public T Peek()
        {
            Assert.EqualTo(Handle.IsValid, true);
            Assert.GreatherThan(Length, 0);
            return RawPointer[Length];
        }

        /// <summary>
        /// if the buffer is at its max more space will be allocated to fit additionalItemCount
        /// </summary>
        public void EnsureCapacity(int additionalItemCount = 1)
        {
            Assert.EqualTo(Handle.IsValid, true);
            var neededLength = Length + additionalItemCount;
            if (neededLength < MaxLength)
                return;

            while (neededLength > MaxLength)
                MaxLength = Math.Max(MaxLength * 3, 48) / 2;

            Resize(MaxLength);
        }

        private void Resize(int newSize)
        {
            MaxLength = newSize;
            //copy data, get new handle, etc
            var newHandle = Allocator.Take(ElementSize * MaxLength);
            if (Handle.IsValid)
            {
                var src = RawPointer;
                var dst = (T*)newHandle.Address;
                for (var i = 0; i < Length; i++)
                    dst[i] = src[i];

                Allocator.Free(ref _listInfo->Handle);
            }
            _listInfo->Handle = newHandle;
        }

        ///// <summary>
        ///// adds all items from array
        ///// </summary>
        ///// <param name="array">Array.</param>
        //public void AddRange(IEnumerable<T> array)
        //{
        //.EqualTo H Assert(_handle.IsValid, true);
        //    foreach (var item in array)
        //        Add(item);
        //}

        ///// <summary>
        ///// sorts all items in the buffer up to length
        ///// </summary>
        //public void Sort(Comparison<T> comparison)
        //{
        //.EqualTo H Assert(_handle.IsValid, true);
        //    Span.Sort(comparison);
        //}

        ///// <summary>
        ///// sorts all items in the buffer up to length
        ///// </summary>
        //public void Sort(IComparer<T> comparer)
        //{
        //.EqualTo H Assert(_handle.IsValid, true);
        //    Span.Sort(comparer);
        //}

        public void Dispose()
        {
            Assert.EqualTo(Handle.IsValid, true);
            var copy = _listInfo->Handle;
            Allocator.Free(ref Handle);
            Allocator.Free(ref copy);

            //Length = 0;
            //MaxLength = 0;
        }

        //public static implicit operator NativeSlice<T>(NativeList<T> arr) => arr.Slice();
        //public NativeSlice<T> Slice() => Slice(0, _length);

        //public NativeSlice<T> Slice(int start) => Slice(start, _length - start);

        //public NativeSlice<T> Slice(int start, int length)
        //{
        //.EqualTo H Assert(_handle.IsValid, true);
        //    Assert(start >= 0);
        //    Assert(length > 0);
        //    Assert(start + length <= _length);
        //    return new NativeSlice<T>(_handle, start, length);
        //}

        //public Span<T> Span
        //{
        //    get
        //    {
        //.EqualTo H     Assert(_handle.IsValid, true);
        //        return new Span<T>(RawPointer, _length);
        //    }
        //}
    }
}