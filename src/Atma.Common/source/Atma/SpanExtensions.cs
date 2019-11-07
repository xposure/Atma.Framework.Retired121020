using System;
using System.Collections.Generic;
using System.Text;

namespace Atma
{
    public static class SpanExtensions
    {
        public static void Sort<T>(this Span<T> span)
            where T : struct, IComparable<T>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                for (var j = 0; j < span.Length - 1; ++j)
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

        public static void Sort<T, TComparer>(this Span<T> span, TComparer comparer)
           where T : struct
           where TComparer : IComparer<T>
        {
            for (var i = 0; i < span.Length; ++i)
            {
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

        public static void Sort<T>(this Span<T> span, Comparison<T> comparison)
           where T : struct
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

        //public static void Sort<TKey, TValue>(this Span<TKey> keys, Span<TValue> items);

        //public static void Sort<TKey, TValue, TComparer>(this Span<TKey> keys,
        //   Span<TValue> items, TComparer comparer)
        //   where TComparer : IComparer<TKey>;

        //public static void Sort<TKey, TValue>(this Span<TKey> keys,
        //   Span<TValue> items, System.Comparison<TKey> comparison);
    }
}
