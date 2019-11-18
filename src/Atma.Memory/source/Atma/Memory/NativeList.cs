namespace Atma.Memory
{
    using System;
    using System.Collections.Generic;
    internal struct NativeListData
    {
        public int Length;
        public int MaxLength;
        public AllocationHandle Handle;
    }

    [System.Diagnostics.DebuggerStepThrough]
    public unsafe struct NativeList<T> : IDisposable
        where T : unmanaged
    {



        internal IAllocator Allocator;

        //since this is resizable, we either have to force people to remember they
        //must pass this around by "ref", or we should double alloc
        //one for a persisent location of the handle info and one for the data
        //internal AllocationHandle Handle;

        //TODO: We are using a double alloc for NativeList to support it being passed around without ref        
        private NativeListData* _listInfo;
        internal AllocationHandle Handle;

        public int Length { get => _listInfo->Length; private set => _listInfo->Length = value; }
        public int MaxLength { get => _listInfo->MaxLength; private set => _listInfo->MaxLength = value; }

        public bool IsCreated => _listInfo->MaxLength > 0;
        public int ElementSize => SizeOf<T>.Size;

        public T* RawPointer => (T*)_listInfo->Handle.Address;
        public T* EndPointer => (T*)_listInfo->Handle.Address + _listInfo->Length;
        public T* MaxPointer => (T*)_listInfo->Handle.Address + _listInfo->MaxLength;

        public NativeList(IAllocator allocator, int length = 8)
        {
            Allocator = allocator;
            var sizeOfElement = SizeOf<T>.Size;

            Assert.GreatherThan(length, 0);
            Handle = Allocator.Take<NativeListData>(1);

            _listInfo = (NativeListData*)Handle.Address;
            _listInfo->Handle = Allocator.Take<T>(length);

            Length = 0;
            MaxLength = length;
        }

        public NativeList(ref NativeArray<T> array)
        {
            Allocator = array.Allocator;
            Handle = Allocator.Take<NativeListData>(1);
            _listInfo = (NativeListData*)Handle.Address;

            _listInfo->Handle = Allocator.Transfer(ref array.Handle);
            Length = array.Length;
            MaxLength = array.Length;
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
            var dst = (T*)Handle.Address;
            var len = Length;
            while (len-- > 0)
                *dst = default;

            //Unsafe.ClearAlign16(RawPointer, ElementSize * _maxLength);
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
        public void Add(in T item)
        {
            Assert.EqualTo(Handle.IsValid, true);
            var len = Length;
            var maxLen = MaxLength;
            if (len == maxLen)
            {
                maxLen = Math.Max(maxLen * 3, 16) / 2;
                Resize(maxLen);
            }
            RawPointer[len] = item;
            Length = len + 1;
        }

        /// <summary>
        /// removes the item from the list
        /// </summary>
        /// <param name="item">Item.</param>
        public void Remove(in T item)
        {
            Assert.EqualTo(Handle.IsValid, true);
            var comp = EqualityComparer<T>.Default;
            var len = Length;
            for (var i = 0; i < len; ++i)
            {
                if (comp.Equals(RawPointer[i], item))
                {
                    RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// removes the item at the given index from the list
        /// </summary>
        public void RemoveAt(int index)
        {
            Assert.EqualTo(Handle.IsValid, true);
            Assert.LessThan(index, Length);

            var len = --Length;
            for (var i = index; i < len; i++)
                RawPointer[i] = RawPointer[i + 1];
        }


        /// <summary>
        /// removes the item at the given index from the list but does NOT maintain list order
        /// </summary>
        /// <param name="index">Index.</param>
        public void RemoveAtWithSwap(int index)
        {
            Assert.EqualTo(Handle.IsValid, true);
            Assert.LessThan(index, Length);

            var len = Length--;
            RawPointer[index] = RawPointer[len];
            //RawPointer[Length - 1] = default;
            //--Length;
        }

        /// <summary>
        /// checks to see if item is in the FastList
        /// </summary>
        /// <param name="item">Item.</param>
        public bool Contains(in T item)
        {
            Assert.EqualTo(Handle.IsValid, true);
            var comp = EqualityComparer<T>.Default;
            var len = Length;
            for (var i = 0; i < len; ++i)
            {
                if (comp.Equals(RawPointer[i], item))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// if the buffer is at its max more space will be allocated to fit additionalItemCount
        /// </summary>
        public void EnsureCapacity(int additionalItemCount = 1)
        {
            Assert.EqualTo(Handle.IsValid, true);
            var len = Length;
            var maxLen = MaxLength;
            var neededLength = len + additionalItemCount;
            if (neededLength < maxLen)
                return;

            while (neededLength > maxLen)
                maxLen = Math.Max(maxLen * 3, 48) / 2;

            Resize(maxLen);
        }

        private void Resize(int newSize)
        {
            MaxLength = newSize;

            //copy data, get new handle, etc
            var newHandle = Allocator.Take(ElementSize * newSize);
            if (Handle.IsValid)
            {
                var src = RawPointer;
                var dst = (T*)newHandle.Address;
                var len = Length;
                for (var i = 0; i < len; i++)
                    dst[i] = src[i];

                Allocator.Free(ref _listInfo->Handle);
            }

            _listInfo->Handle = newHandle;
        }

        /// <summary>
        /// adds all items from array
        /// </summary>
        /// <param name="array">Array.</param>
        public void AddRange(IEnumerable<T> array)
        {
            Assert.EqualTo(Handle.IsValid, true);
            foreach (var item in array)
                Add(item);
        }

        /// <summary>
        /// sorts all items in the buffer up to length
        /// </summary>
        public void Sort(Comparison<T> comparison)
        {
            Assert.EqualTo(Handle.IsValid, true);
            var span = AsSpan();
            span.Sort(comparison);
        }

        /// <summary>
        /// sorts all items in the buffer up to length
        /// </summary>
        public void Sort(IComparer<T> comparer)
        {
            Assert.EqualTo(Handle.IsValid, true);
            var span = AsSpan();
            span.Sort(comparer);
        }

        public void Dispose()
        {
            Assert.EqualTo(Handle.IsValid, true);
            var copy = _listInfo->Handle;
            Allocator.Free(ref Handle);
            Allocator.Free(ref copy);

            //Length = 0;
            //MaxLength = 0;
        }

        public static implicit operator Span<T>(NativeList<T> arr) => arr.Slice();

        public Span<T> Slice() => Slice(0, Length);

        public Span<T> Slice(int start) => Slice(start, Length - start);

        public Span<T> Slice(int start, int length)
        {
            if (length == 0)
                return Span<T>.Empty;

            Assert.EqualTo(Handle.IsValid, true);
            Assert.Range(start, 0, Length);
            Assert.Range(start + length - 1, start, Length);
            return new Span<T>(&RawPointer[start], length);
        }

        public Span<T> AsSpan()
        {
            Assert.EqualTo(Handle.IsValid, true);
            return new Span<T>(RawPointer, Length);
        }

        public override string ToString()
        {
            return Slice().ToString();
        }
    }
}
