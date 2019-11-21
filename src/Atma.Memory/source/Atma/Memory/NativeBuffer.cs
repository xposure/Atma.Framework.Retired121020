namespace Atma.Memory
{
    using System;

    [System.Diagnostics.DebuggerStepThrough]
    internal struct NativeBufferData
    {
        public int Length;
        public int MaxLength;
        public AllocationHandle Handle;
    }

    [System.Diagnostics.DebuggerStepThrough]
    public unsafe readonly ref struct NativeBufferPtr<T>
        where T : unmanaged
    {
        private readonly NativeBuffer _buffer;
        public readonly int Offset;

        public T* Value => (T*)(_buffer.RawPointer + Offset);

        public readonly int Count;
        public readonly int SizeInBytes;

        internal NativeBufferPtr(NativeBuffer buffer, int offset, int count, int sizeInBytes)
        {
            _buffer = buffer;
            Offset = offset;
            Count = count;
            SizeInBytes = sizeInBytes;
        }
    }

    //[System.Diagnostics.DebuggerStepThrough]
    public unsafe struct NativeBuffer : IDisposable
    {


        internal IAllocator Allocator;

        //since this is resizable, we either have to force people to remember they
        //must pass this around by "ref", or we should double alloc
        //one for a persisent location of the handle info and one for the data
        //internal AllocationHandle Handle;

        //TODO: We are using a double alloc for NativeList to support it being passed around without ref        
        private NativeBufferData* _listInfo;
        internal AllocationHandle Handle;

        public int Length { get => _listInfo->Length; private set => _listInfo->Length = value; }
        public int MaxLength { get => _listInfo->MaxLength; private set => _listInfo->MaxLength = value; }

        public bool IsCreated => _listInfo->MaxLength > 0;
        //public int ElementSize => SizeOf<T>.Size;

        public byte* RawPointer => (byte*)_listInfo->Handle.Address;
        public byte* EndPointer => (byte*)_listInfo->Handle.Address + _listInfo->Length;
        public byte* MaxPointer => (byte*)_listInfo->Handle.Address + _listInfo->MaxLength;

        public NativeBuffer(IAllocator allocator, int length = 256)
        {
            Allocator = allocator;
            //var sizeOfElement = SizeOf<T>.Size;

            Assert.GreatherThan(length, 0);
            Handle = Allocator.Take<NativeBufferData>(1);

            _listInfo = (NativeBufferData*)Handle.Address;
            _listInfo->Handle = Allocator.Take(length);

            Length = 0;
            MaxLength = length;

        }

        /// <summary>
        /// clears the list and nulls out all items in the buffer
        /// </summary>
        public void Clear()
        {
            Assert.EqualTo(Handle.IsValid, true);
            Unsafe.ClearAlign16(RawPointer, MaxLength);
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
        public NativeBufferPtr<T> Add<T>(in T item)
            where T : unmanaged
        {
            Assert.EqualTo(Handle.IsValid, true);
            var sizeInBytes = SizeOf<T>.Size;

            EnsureCapacity(sizeInBytes);

            var len = Length;
            var location = (T*)EndPointer;
            *location = item;
            Length += sizeInBytes;
            return new NativeBufferPtr<T>(this, len, 1, sizeInBytes);

        }

        public unsafe NativeBufferPtr<T> Take<T>(int count)
            where T : unmanaged
        {
            var sizeInBytes = SizeOf<T>.Size * count;
            EnsureCapacity(sizeInBytes);

            var len = Length;
            var location = (T*)EndPointer;
            Length += sizeInBytes;
            return new NativeBufferPtr<T>(this, len, count, sizeInBytes);
        }

        internal unsafe T* Get<T>(int offset)
            where T : unmanaged
        {
            return (T*)RawPointer[offset];
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

            var newSize = MaxLength;
            while (neededLength > newSize)
                newSize = Unsafe.Align16(Math.Max(newSize * 3, 48) / 2);

            Resize(newSize);
        }

        private void Resize(int newSize)
        {
            //var oldLength = MaxLength;
            MaxLength = newSize;
            //copy data, get new handle, etc
            var newHandle = Allocator.Take(MaxLength);
            if (Handle.IsValid)
            {
                var src = RawPointer;
                var dst = (byte*)newHandle.Address;
                Unsafe.CopyAlign16(src, dst, Length);
                Allocator.Free(ref _listInfo->Handle);
            }
            _listInfo->Handle = newHandle;
        }


        public void Dispose()
        {
            Assert.EqualTo(Handle.IsValid, true);
            var copy = _listInfo->Handle;
            Allocator.Free(ref Handle);
            Allocator.Free(ref copy);

            Length = 0;
            MaxLength = 0;
        }

        //public NativeSlice<T> Slice<T>() where T : unmanaged => Slice<T>(0, _length);

        //public NativeSlice<T> Slice<T>(int start) where T : unmanaged => Slice<T>(start, _length - start);

        //public NativeSlice<T> Slice<T>(int start, int length)
        //    where T : unmanaged
        //{

        //    length *= SizeOf<T>.Size;
        //.EqualTo H Assert(_handle.IsValid, true);
        //    Assert(start >= 0);
        //    Assert(length > 0);
        //    Assert(start + length <= _length);
        //    return new NativeSlice<T>(_handle, start, length);
        //}
    }
}
