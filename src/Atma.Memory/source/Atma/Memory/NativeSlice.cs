// namespace Atma.Memory
// {
//     using System;
//     using System.Collections.Generic;
//     using System.Text;

//     [System.Diagnostics.DebuggerStepThrough]
//     public unsafe readonly ref struct Span<T>
//             where T : unmanaged
//     {
//         private readonly T* _rawAddress;
//         public readonly int Length;

//         public T* RawPointer => _rawAddress;
//         public T* EndPointer => _rawAddress + Length;

//         public static Span<T> Empty => new Span<T>(null, 0);
//         public bool IsEmpty => Length == 0;

//         internal Span(in AllocationHandle handle, int start, int length)
//         {
//             _rawAddress = (T*)handle.Address + start;
//             Length = length;
//         }

//         public Span(void* rawAddress, int length)
//         {
//             _rawAddress = (T*)rawAddress;
//             Length = length;
//         }

//         /// <summary>
//         /// provided for ease of access though it is recommended to just access the buffer directly.
//         /// </summary>
//         /// <param name="index">Index.</param>
//         public ref T this[int index]
//         {
//             get
//             {
//                 Assert.Range(index, 0, Length);
//                 return ref RawPointer[index];
//             }
//         }

//         /// <summary>
//         /// checks to see if item is in the FastList
//         /// </summary>
//         /// <param name="item">Item.</param>
//         public bool Contains(in T item)
//         {
//             var comp = EqualityComparer<T>.Default;
//             for (var i = 0; i < Length; ++i)
//             {
//                 if (comp.Equals(RawPointer[i], item))
//                     return true;
//             }

//             return false;
//         }

//         /// <summary>
//         /// sorts all items in the buffer up to length
//         /// </summary>
//         public void Sort(Comparison<T> comparison)
//         {
//             var span = AsSpan();
//             span.Sort(comparison);
//         }

//         /// <summary>
//         /// sorts all items in the buffer up to length
//         /// </summary>
//         public void Sort(IComparer<T> comparer)
//         {
//             var span = AsSpan();
//             span.Sort(comparer);
//         }

//         public System.Span<T> AsSpan() => new System.Span<T>(RawPointer, Length);

//         public Span<T> Slice() => Slice(0, Length);

//         public Span<T> Slice(int start) => Slice(start, Length - start);

//         public static implicit operator ReadOnlySpan<T>(Span<T> it) => new ReadOnlySpan<T>(it._rawAddress, it.Length);

//         public Span<T> Slice(int start, int length)
//         {
//             if (length == 0)
//                 return Empty;

//             Assert.Range(start, 0, Length);
//             Assert.Range(start + length - 1, start, Length);
//             var addr = (T*)_rawAddress;
//             addr += start;
//             return new Span<T>(addr, length);
//         }

//         public override string ToString()
//         {
//             var sb = new StringBuilder();
//             sb.Append('[');
//             for (var i = 0; i < Length; i++)
//             {
//                 sb.Append(this[i]);
//                 if (i < Length - 1)
//                     sb.Append(',');
//             }
//             sb.Append(']');
//             return sb.ToString();
//         }
//     }

//     [System.Diagnostics.DebuggerStepThrough]
//     public unsafe readonly ref struct ReadOnlySpan<T>
//             where T : unmanaged
//     {
//         private readonly T* _rawAddress;
//         public readonly int Length;

//         public T* RawPointer => _rawAddress;
//         public T* EndPointer => _rawAddress + Length;

//         public static ReadOnlySpan<T> Empty => new ReadOnlySpan<T>(null, 0);
//         public bool IsEmpty => Length == 0;

//         internal ReadOnlySpan(in AllocationHandle handle, int start, int length)
//         {
//             _rawAddress = (T*)handle.Address + start;
//             Length = length;
//         }

//         public ReadOnlySpan(void* rawAddress, int length)
//         {
//             _rawAddress = (T*)rawAddress;
//             Length = length;
//         }

//         /// <summary>
//         /// provided for ease of access though it is recommended to just access the buffer directly.
//         /// </summary>
//         /// <param name="index">Index.</param>
//         public readonly ref T this[int index]
//         {
//             get
//             {
//                 Assert.Range(index, 0, Length);
//                 return ref RawPointer[index];
//             }
//         }

//         /// <summary>
//         /// checks to see if item is in the FastList
//         /// </summary>
//         /// <param name="item">Item.</param>
//         public bool Contains(in T item)
//         {
//             var comp = EqualityComparer<T>.Default;
//             for (var i = 0; i < Length; ++i)
//             {
//                 if (comp.Equals(RawPointer[i], item))
//                     return true;
//             }

//             return false;
//         }


//         public ReadOnlySpan<T> Slice() => Slice(0, Length);

//         public ReadOnlySpan<T> Slice(int start) => Slice(start, Length - start);

//         public ReadOnlySpan<T> Slice(int start, int length)
//         {
//             Assert.Range(start, 0, Length);
//             Assert.Range(start + length - 1, start, Length);
//             var addr = (T*)_rawAddress;
//             addr += start;
//             return new ReadOnlySpan<T>(addr, length);
//         }


//         public override string ToString()
//         {
//             var sb = new StringBuilder();
//             sb.Append('[');
//             for (var i = 0; i < Length; i++)
//             {
//                 sb.Append(this[i]);
//                 if (i < Length - 1)
//                     sb.Append(',');
//             }
//             sb.Append(']');
//             return sb.ToString();
//         }
//     }

//     public static class NativeSliceExtensions
//     {
//         public static T[] ToArray<T>(this Span<T> it)
//             where T : unmanaged
//         {
//             var t = new T[it.Length];
//             for (var i = 0; i < it.Length; i++)
//                 t[i] = it[i];
//             return t;
//         }

//         public static T[] ToArray<T>(this ReadOnlySpan<T> it)
//             where T : unmanaged
//         {
//             var t = new T[it.Length];
//             for (var i = 0; i < it.Length; i++)
//                 t[i] = it[i];
//             return t;
//         }

//         public static void InsertionSort(this Span<int> span)
//         {
//             for (var i = 0; i < span.Length - 1; i++)
//             {
//                 for (var j = i + 1; j > 0; j--)
//                 {
//                     if (span[j - 1] > span[j])
//                     {
//                         var temp = span[j - 1];
//                         span[j - 1] = span[j];
//                         span[j] = temp;
//                     }
//                 }
//             }
//         }

//         public static void InsertionSort<T>(this Span<T> span)
//             where T : unmanaged
//         {
//             for (var i = 0; i < span.Length - 1; i++)
//             {
//                 for (var j = i + 1; j > 0; j--)
//                 {
//                     if (span[j - 1].GetHashCode() > span[j].GetHashCode())
//                     {
//                         var temp = span[j - 1];
//                         span[j - 1] = span[j];
//                         span[j] = temp;
//                     }
//                 }
//             }
//         }

//         public static void Sort<T>(this Span<T> span)
//             where T : unmanaged, IComparable<T>
//         {
//             for (var i = 0; i < span.Length; ++i)
//             {
//                 for (var j = 0; j < span.Length - 1; ++j)
//                 //TODO: this code doesn't appear correct to me, j shouldn't start at 0?
//                 {
//                     if (span[j].CompareTo(span[j + 1]) > 0)
//                     {
//                         var temp = span[j];
//                         span[j] = span[j + 1];
//                         span[j + 1] = temp;
//                     }
//                 }
//             }
//         }

//         public static void Sort<T, TComparer>(this Span<T> span, TComparer comparer)
//            where T : unmanaged
//            where TComparer : IComparer<T>
//         {
//             for (var i = 0; i < span.Length; ++i)
//             {
//                 //TODO: this code doesn't appear correct to me, j shouldn't start at 0?
//                 for (var j = 0; j < span.Length - 1; ++j)
//                 {
//                     if (comparer.Compare(span[j], span[j + 1]) > 0)
//                     {
//                         var temp = span[j];
//                         span[j] = span[j + 1];
//                         span[j + 1] = temp;
//                     }
//                 }
//             }
//         }

//         public static void Sort<T>(this Span<T> span, Comparison<T> comparison)
//            where T : unmanaged
//         {
//             for (var i = 0; i < span.Length; ++i)
//             {
//                 for (var j = 0; j < span.Length - 1; ++j)
//                 {
//                     if (comparison(span[j], span[j + 1]) > 0)
//                     {
//                         var temp = span[j];
//                         span[j] = span[j + 1];
//                         span[j + 1] = temp;
//                     }
//                 }
//             }
//         }
//     }
// }
