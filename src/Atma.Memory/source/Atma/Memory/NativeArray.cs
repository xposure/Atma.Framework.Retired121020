namespace Atma.Memory
{
    using System;
    using System.Collections.Generic;

    using static Atma.Debug;

    public unsafe struct NativeArray<T> : IDisposable
        where T : unmanaged
    {
        internal AllocationHandle Handle;
        internal IAllocator Allocator;

        public readonly int Length;

        public int ElementSize => SizeOf<T>.Size;
        public bool IsCreated => Length > 0;

        public T* RawPointer => (T*)Handle.Address;
        public T* EndPointer => (T*)Handle.Address + Length;

        public IntPtr RawIntPtr => Handle.Address;
        public IntPtr EndIntPtr => new IntPtr((T*)Handle.Address + Length);

        public NativeArray(IAllocator allocator, int length)
        {
            Allocator = allocator;
            var sizeOfElement = SizeOf<T>.Size;

            if (length > 0)
            {
                Handle = allocator.Take(sizeOfElement * length);
                Length = length;
            }
            else
            {
                Handle = default;
                Length = 0;
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
                Assert(Handle.IsValid);
                Assert(index >= 0 && index < Length);
                return ref RawPointer[index];
            }
        }

        /// <summary>
        /// clears the list and nulls out all items in the buffer
        /// </summary>
        public void Clear()
        {
            Assert(Handle.IsValid);
            var length = Length;

            var ptr = (T*)RawIntPtr;
            while (length-- > 0)
                *ptr = default;

            //Unsafe.ClearAlign16(RawPointer, ElementSize * Length);
        }

        /// <summary>
        /// sorts all items in the buffer up to length
        /// </summary>
        public void Sort(Comparison<T> comparison)
        {
            Assert(Handle.IsValid);
            Span.Sort(comparison);
        }

        /// <summary>
        /// sorts all items in the buffer up to length
        /// </summary>
        public void Sort(IComparer<T> comparer)
        {
            Assert(Handle.IsValid);
            Span.Sort(comparer);
        }

        public void Dispose()
        {
            //Assert(_handle.IsValid);
            if (Handle.IsValid)
                Allocator.Free(ref Handle);
        }

        public void CopyTo(ref NativeArray<T> other)
        {
            var len = Math.Min(this.Length, other.Length);
            var src = this.RawPointer;
            var dst = other.RawPointer;

            while (len-- > 0)
                //for (var i = 0; i < len; i++)
                *dst++ = *src++;
        }

        public static implicit operator NativeSlice<T>(NativeArray<T> arr) => arr.Slice();
        public NativeSlice<T> Slice() => Slice(0, Length);

        public NativeSlice<T> Slice(int start) => Slice(start, Length - start);

        public NativeSlice<T> Slice(int start, int length)
        {
            Assert(Handle.IsValid);
            Assert(start >= 0);
            Assert(length > 0);
            Assert(start + length <= Length);
            return new NativeSlice<T>(Handle, start, length);
        }

        public Span<T> Span
        {
            get
            {
                Assert(Handle.IsValid);
                return new Span<T>(RawPointer, Length);
            }
        }

        public override string ToString()
        {
            return Slice().ToString();
        }
    }
}
