namespace Atma.Memory
{
    using System;
    using System.Collections.Generic;

    [System.Diagnostics.DebuggerStepThrough]
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
                Handle = allocator.Take<T>(length);
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
            Assert.EqualTo(Handle.IsValid, true);
            Span.Sort(comparison);
        }

        /// <summary>
        /// sorts all items in the buffer up to length
        /// </summary>
        public void Sort(IComparer<T> comparer)
        {
            Assert.EqualTo(Handle.IsValid, true);
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

        public Span<T> Slice() => Slice(0, Length);

        public Span<T> Slice(int start) => Slice(start, Length - start);

        public Span<T> Slice(int start, int length)
        {
            if (length == 0)
                return Span<T>.Empty;

            Assert.EqualTo(Handle.IsValid, true);
            Assert.Range(start, 0, Length);
            Assert.Range(start + length - 1, 0, Length);

            return new Span<T>(RawPointer + start, length);
        }

        public Span<T> Span
        {
            get
            {
                Assert.EqualTo(Handle.IsValid, true);
                return new Span<T>(RawPointer, Length);
            }
        }

        public static implicit operator Span<T>(NativeArray<T> it) => it.Span;

        public override string ToString()
        {
            return Slice().ToString();
        }
    }
}
