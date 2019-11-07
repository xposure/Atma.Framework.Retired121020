namespace Atma.Memory
{
    using System;
    using System.Collections.Generic;

    using static Atma.Debug;

    public unsafe struct NativeArray<T> : IDisposable
        where T : unmanaged
    {
        private int _length;
        private AllocationHandle _handle;

        public int Length => _length;

        public int ElementSize => SizeOf<T>.Size;

        public bool IsCreated => _length > 0;

        public T* RawPointer => (T*)_handle.Address;
        public T* EndPointer => (T*)_handle.Address + _length;

        public IntPtr RawIntPtr => new IntPtr(_handle.Address);
        public IntPtr EndIntPtr => new IntPtr((T*)_handle.Address + _length);

        public NativeArray(Allocator allocator, int length = 8)
        {
            var sizeOfElement = SizeOf<T>.Size;

            if (length > 0)
            {
                _handle = MemoryManager.Take(allocator, sizeOfElement * length);
                _length = length;
            }
            else
            {
                _handle = default;
                _length = 0;
            }

        }

        /// <summary>
        /// provided for ease of access though it is recommended to just access the buffer directly.
        /// </summary>
        /// <param name="index">Index.</param>
        public ref T this[int index]
        {
            get
            {
                Assert(_handle.IsValid);
                Assert(index >= 0 && index < _length);
                return ref RawPointer[index];
            }
        }

        internal AllocationHandle TakeOwnership()
        {
            Assert(IsCreated);

            var handle = _handle.Clone();

            _length = 0;
            _handle = default;

            return handle;
        }

        /// <summary>
        /// clears the list and nulls out all items in the buffer
        /// </summary>
        public void Clear()
        {
            Assert(_handle.IsValid);
            Unsafe.ClearAlign16(RawPointer, ElementSize * _length);
            _length = 0;
        }

        /// <summary>
        /// sorts all items in the buffer up to length
        /// </summary>
        public void Sort(Comparison<T> comparison)
        {
            Assert(_handle.IsValid);
            Span.Sort(comparison);
        }

        /// <summary>
        /// sorts all items in the buffer up to length
        /// </summary>
        public void Sort(IComparer<T> comparer)
        {
            Assert(_handle.IsValid);
            Span.Sort(comparer);
        }

        public void Dispose()
        {
            //Assert(_handle.IsValid);
            if (_handle.IsValid)
                MemoryManager.Free(ref _handle);
            _length = 0;
        }

        public void CopyTo(ref NativeArray<T> other)
        {
            var len = Math.Min(this.Length, other.Length);
            var src = this.RawPointer;
            var dst = other.RawPointer;

            for (var i = 0; i < len; i++)
                *dst++ = *src++;
        }

        public static implicit operator NativeSlice<T>(NativeArray<T> arr) => arr.Slice();
        public NativeSlice<T> Slice() => Slice(0, _length);

        public NativeSlice<T> Slice(int start) => Slice(start, _length - start);

        public NativeSlice<T> Slice(int start, int length)
        {
            Assert(_handle.IsValid);
            Assert(start >= 0);
            Assert(length > 0);
            Assert(start + length <= _length);
            return new NativeSlice<T>(_handle, start, length);
        }

        public Span<T> Span
        {
            get
            {
                Assert(_handle.IsValid);
                return new Span<T>(RawPointer, _length);
            }
        }

        public override string ToString()
        {
            return Slice().ToString();
        }
    }
}
