namespace Atma.Memory
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    [System.Diagnostics.DebuggerStepThrough]
    public unsafe readonly ref struct NativeSlice<T>
            where T : unmanaged
    {
        private readonly T* _rawAddress;
        public readonly int Length;

        public T* RawPointer => _rawAddress;
        public T* EndPointer => _rawAddress + Length;

        public static NativeSlice<T> Empty => new NativeSlice<T>(null, 0);
        public bool IsEmpty => Length == 0;

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
                Assert.Range(index, 0, Length);
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

        public NativeSlice<T> Slice() => Slice(0, Length);

        public NativeSlice<T> Slice(int start) => Slice(start, Length - start);

        public static implicit operator NativeReadOnlySlice<T>(NativeSlice<T> it) => new NativeReadOnlySlice<T>(it._rawAddress, it.Length);

        public NativeSlice<T> Slice(int start, int length)
        {
            Assert.Range(start, 0, Length);
            Assert.Range(start + length - 1, start, Length);
            var addr = (T*)_rawAddress;
            addr += start;
            return new NativeSlice<T>(addr, length);
        }

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

    [System.Diagnostics.DebuggerStepThrough]
    public unsafe readonly ref struct NativeReadOnlySlice<T>
            where T : unmanaged
    {
        private readonly T* _rawAddress;
        public readonly int Length;

        public T* RawPointer => _rawAddress;
        public T* EndPointer => _rawAddress + Length;

        public static NativeReadOnlySlice<T> Empty => new NativeReadOnlySlice<T>(null, 0);
        public bool IsEmpty => Length == 0;

        internal NativeReadOnlySlice(in AllocationHandle handle, int start, int length)
        {
            _rawAddress = (T*)handle.Address + start;
            Length = length;
        }

        public NativeReadOnlySlice(void* rawAddress, int length)
        {
            _rawAddress = (T*)rawAddress;
            Length = length;
        }

        /// <summary>
        /// provided for ease of access though it is recommended to just access the buffer directly.
        /// </summary>
        /// <param name="index">Index.</param>
        public readonly ref T this[int index]
        {
            get
            {
                Assert.Range(index, 0, Length);
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


        public NativeReadOnlySlice<T> Slice() => Slice(0, Length);

        public NativeReadOnlySlice<T> Slice(int start) => Slice(start, Length - start);

        public NativeReadOnlySlice<T> Slice(int start, int length)
        {
            Assert.GreatherThanEqualTo(start, 0);
            Assert.GreatherThanEqualTo(length, 0);
            Assert.LessThanEqualTo(start + length, Length);
            var addr = (T*)_rawAddress;
            addr += start;
            return new NativeReadOnlySlice<T>(addr, Length - start);
        }

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

    public static class NativeSliceExtensions
    {
        public static T[] ToArray<T>(this NativeSlice<T> it)
            where T : unmanaged
        {
            var t = new T[it.Length];
            for (var i = 0; i < it.Length; i++)
                t[i] = it[i];
            return t;
        }

        public static T[] ToArray<T>(this NativeReadOnlySlice<T> it)
            where T : unmanaged
        {
            var t = new T[it.Length];
            for (var i = 0; i < it.Length; i++)
                t[i] = it[i];
            return t;
        }

        public static void InsertionSort(this NativeSlice<int> span)
        {
            for (var i = 0; i < span.Length - 1; i++)
            {
                for (var j = i + 1; j > 0; j--)
                {
                    if (span[j - 1] > span[j])
                    {
                        var temp = span[j - 1];
                        span[j - 1] = span[j];
                        span[j] = temp;
                    }
                }
            }
        }

        public static void InsertionSort<T>(this NativeSlice<T> span)
            where T : unmanaged
        {
            for (var i = 0; i < span.Length - 1; i++)
            {
                for (var j = i + 1; j > 0; j--)
                {
                    if (span[j - 1].GetHashCode() > span[j].GetHashCode())
                    {
                        var temp = span[j - 1];
                        span[j - 1] = span[j];
                        span[j] = temp;
                    }
                }
            }
        }

        public static void Sort<T>(this NativeSlice<T> span)
            where T : unmanaged, IComparable<T>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                for (var j = 0; j < span.Length - 1; ++j)
                //TODO: this code doesn't appear correct to me, j shouldn't start at 0?
                {
                    if (span[j].CompareTo(span[j + 1]) > 0)
                    {
                        var temp = span[j];
                        span[j] = span[j + 1];
                        span[j + 1] = temp;
                    }
                }
            }
        }

        public static void Sort<T, TComparer>(this NativeSlice<T> span, TComparer comparer)
           where T : unmanaged
           where TComparer : IComparer<T>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                //TODO: this code doesn't appear correct to me, j shouldn't start at 0?
                for (var j = 0; j < span.Length - 1; ++j)
                {
                    if (comparer.Compare(span[j], span[j + 1]) > 0)
                    {
                        var temp = span[j];
                        span[j] = span[j + 1];
                        span[j + 1] = temp;
                    }
                }
            }
        }

        public static void Sort<T>(this NativeSlice<T> span, Comparison<T> comparison)
           where T : unmanaged
        {
            for (var i = 0; i < span.Length; ++i)
            {
                for (var j = 0; j < span.Length - 1; ++j)
                {
                    if (comparison(span[j], span[j + 1]) > 0)
                    {
                        var temp = span[j];
                        span[j] = span[j + 1];
                        span[j + 1] = temp;
                    }
                }
            }
        }
    }
}
