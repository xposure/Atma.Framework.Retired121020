namespace Atma.Memory
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using static Atma.Debug;

    public unsafe ref struct NativeSlice<T>
        where T : unmanaged
    {
        private readonly T* _rawAddress;
        public readonly int Length;

        public T* RawPointer => _rawAddress;
        public T* EndPointer => _rawAddress + Length;

        internal NativeSlice(in AllocationHandle handle, int start, int length)
        {
            _rawAddress = (T*)handle.Address + start;
            Length = length;
        }

        public NativeSlice(void* rawAddress, int length)
        {
            _rawAddress = (T*)rawAddress;
            Length = length;
        }

        /// <summary>
        /// provided for ease of access though it is recommended to just access the buffer directly.
        /// </summary>
        /// <param name="index">Index.</param>
        public ref T this[int index]
        {
            get
            {
                Assert(index >= 0 && index < Length);
                return ref RawPointer[index];
            }
        }

        /// <summary>
        /// checks to see if item is in the FastList
        /// </summary>
        /// <param name="item">Item.</param>
        public bool Contains(in T item)
        {
            var comp = EqualityComparer<T>.Default;
            for (var i = 0; i < Length; ++i)
            {
                if (comp.Equals(RawPointer[i], item))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// sorts all items in the buffer up to length
        /// </summary>
        public void Sort(Comparison<T> comparison)
        {
            var span = AsSpan();
            span.Sort(comparison);
        }

        /// <summary>
        /// sorts all items in the buffer up to length
        /// </summary>
        public void Sort(IComparer<T> comparer)
        {
            var span = AsSpan();
            span.Sort(comparer);
        }

        public Span<T> AsSpan() => new Span<T>(RawPointer, Length);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append('[');
            for (var i = 0; i < Length; i++)
            {
                sb.Append(this[i]);
                if (i < Length - 1)
                    sb.Append(',');
            }
            sb.Append(']');
            return sb.ToString();
        }
    }
}
