namespace Atma.Memory
{
    using System;
    using System.Collections.Generic;

    [System.Diagnostics.DebuggerStepThrough]
    public unsafe ref struct SpanList<T>
     where T : unmanaged
    {
        private readonly Span<T> _span;
        private int _length;
        public int Length => _length;
        public int Free => MaxLength - _length;
        public int MaxLength => _span.Length;

        public int ElementSize => SizeOf<T>.Size;

        public SpanList(Span<T> data, int length = 0)
        {
            _span = data;
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
                Assert.Range(index, 0, _length);
                return ref _span[index];
            }
        }

        /// <summary>
        /// clears the list and nulls out all items in the buffer
        /// </summary>
        public void Clear()
        {
            for (var i = 0; i < _length; i++)
                _span[i] = default;

            _length = 0;
        }

        /// <summary>
        /// works just like clear except it does not null our all the items in the buffer. Useful when dealing with structs.
        /// </summary>
        public void Reset()
        {
            _length = 0;
        }

        /// <summary>
        /// adds the item to the list
        /// </summary>
        public void Add(in T item)
        {
            Assert.LessThan(_length, _span.Length);
            _span[_length++] = item;
        }

        /// <summary>
        /// removes the item from the list
        /// </summary>
        /// <param name="item">Item.</param>
        public void Remove(in T item)
        {
            var comp = EqualityComparer<T>.Default;
            var len = Length;
            for (var i = 0; i < len; ++i)
            {
                if (comp.Equals(_span[i], item))
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
            Assert.Range(index, 0, _length);

            var len = --_length;
            for (var i = index; i < len; i++)
                _span[i] = _span[i + 1];
        }


        /// <summary>
        /// removes the item at the given index from the list but does NOT maintain list order
        /// </summary>
        /// <param name="index">Index.</param>
        public void RemoveAtWithSwap(int index)
        {
            Assert.Range(index, 0, _length);

            var len = _length--;
            _span[index] = _span[len];
        }

        /// <summary>
        /// checks to see if item is in the FastList
        /// </summary>
        /// <param name="item">Item.</param>
        public bool Contains(in T item)
        {
            var comp = EqualityComparer<T>.Default;
            var len = _length;
            for (var i = 0; i < len; ++i)
            {
                if (comp.Equals(_span[i], item))
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
            foreach (var item in array)
                Add(item);
        }

        /// <summary>
        /// sorts all items in the buffer up to length
        /// </summary>
        public void Sort(Comparison<T> comparison) => Slice().Sort(comparison);

        /// <summary>
        /// sorts all items in the buffer up to length
        /// </summary>
        public void Sort(IComparer<T> comparer) => Slice().Sort(comparer);

        public static implicit operator Span<T>(SpanList<T> it) => it._span;

        public static implicit operator SpanList<T>(Span<T> it) => new SpanList<T>(it);

        public Span<T> Slice() => Slice(0, Length);

        public Span<T> Slice(int start) => Slice(start, Length - start);

        public Span<T> Slice(int start, int length) => _span.Slice(start, length);

        public override string ToString() => $"{{ Length: {_length}, Span: {_span.ToString()} }}";
    }
}
