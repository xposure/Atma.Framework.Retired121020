namespace Atma.Memory
{
    using System;
    using System.Collections.Generic;

    [System.Diagnostics.DebuggerStepThrough]
    public unsafe ref struct NativeFixedList<T>
        where T : unmanaged
    {
        private readonly void* _addr;
        private readonly int _size;
        private int _length;
        public int Length => _length;
        public int Free => _size - _length;
        public int MaxLength => _size;

        public bool IsValid => _addr != null;

        public int ElementSize => SizeOf<T>.Size;

        public T* RawPointer => (T*)_addr;
        public T* EndPointer => (T*)_addr + _length;
        public T* MaxPointer => (T*)_addr + _size;

        public NativeFixedList(void* addr, int size, int length = 0)
        {
            Assert.GreatherThanEqualTo(size, 0);
            Assert.Range(length, 0, size);
            _addr = addr;
            _size = size;
            _length = length;
        }

        /// <summary>
        /// provided for ease of access though it is recommended to just access the buffer directly.
        /// </summary>
        /// <param name="index">Index.</param>
        public ref T this[int index]
        {
            get
            {
                Assert.EqualTo(IsValid, true);
                Assert.Range(index, 0, _length);
                return ref RawPointer[index];
            }
        }

        /// <summary>
        /// clears the list and nulls out all items in the buffer
        /// </summary>
        public void Clear()
        {
            Assert.EqualTo(IsValid, true);
            var dst = RawPointer;
            var len = Length;
            while (len-- > 0)
                *dst = default;

            _length = 0;
        }

        /// <summary>
        /// works just like clear except it does not null our all the items in the buffer. Useful when dealing with structs.
        /// </summary>
        public void Reset()
        {
            Assert.EqualTo(IsValid, true);
            _length = 0;
        }

        /// <summary>
        /// adds the item to the list
        /// </summary>
        public void Add(in T item)
        {
            Assert.EqualTo(IsValid, true);
            Assert.LessThan(_length, _size);

            RawPointer[_length++] = item;
        }

        /// <summary>
        /// removes the item from the list
        /// </summary>
        /// <param name="item">Item.</param>
        public void Remove(in T item)
        {
            Assert.EqualTo(IsValid, true);
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
            Assert.EqualTo(IsValid, true);
            Assert.Range(index, 0, _length);

            var len = --_length;
            for (var i = index; i < len; i++)
                RawPointer[i] = RawPointer[i + 1];
        }


        /// <summary>
        /// removes the item at the given index from the list but does NOT maintain list order
        /// </summary>
        /// <param name="index">Index.</param>
        public void RemoveAtWithSwap(int index)
        {
            Assert.EqualTo(IsValid, true);
            Assert.Range(index, 0, _length);

            var len = _length--;
            RawPointer[index] = RawPointer[len];
        }

        /// <summary>
        /// checks to see if item is in the FastList
        /// </summary>
        /// <param name="item">Item.</param>
        public bool Contains(in T item)
        {
            Assert.EqualTo(IsValid, true);
            var comp = EqualityComparer<T>.Default;
            var len = _length;
            for (var i = 0; i < len; ++i)
            {
                if (comp.Equals(RawPointer[i], item))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// adds all items from array
        /// </summary>
        /// <param name="array">Array.</param>
        public void AddRange(IEnumerable<T> array)
        {
            Assert.EqualTo(IsValid, true);
            foreach (var item in array)
                Add(item);
        }

        /// <summary>
        /// sorts all items in the buffer up to length
        /// </summary>
        public void Sort(Comparison<T> comparison)
        {
            Assert.EqualTo(IsValid, true);
            var span = AsSpan();
            span.Sort(comparison);
        }

        /// <summary>
        /// sorts all items in the buffer up to length
        /// </summary>
        public void Sort(IComparer<T> comparer)
        {
            Assert.EqualTo(IsValid, true);
            var span = AsSpan();
            span.Sort(comparer);
        }

        public static implicit operator NativeSlice<T>(NativeFixedList<T> arr) => arr.Slice();

        public NativeSlice<T> Slice() => Slice(0, Length);

        public NativeSlice<T> Slice(int start) => Slice(start, Length - start);

        public NativeSlice<T> Slice(int start, int length)
        {
            Assert.EqualTo(IsValid, true);
            Assert.GreatherThanEqualTo(start, 0);
            Assert.GreatherThanEqualTo(length, 0);
            Assert.LessThanEqualTo(start + length, Length);
            return new NativeSlice<T>(&RawPointer[start], length);
        }

        public Span<T> AsSpan()
        {
            Assert.EqualTo(IsValid, true);
            return new Span<T>(RawPointer, Length);
        }

        public override string ToString()
        {
            return Slice().ToString();
        }
    }
}
