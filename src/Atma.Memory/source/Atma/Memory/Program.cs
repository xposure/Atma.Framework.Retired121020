// using System;
// using System.Buffers;
// using System.Runtime.InteropServices;
// using System.Security.Cryptography;
// using BenchmarkDotNet.Attributes;
// using BenchmarkDotNet.Running;

// //https://github.com/Leopotam/ecs
// //https://forum.unity.com/threads/ecs-memory-layout.532028/
// //https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/Documentation~/resources.md
// //https://www.youtube.com/watch?v=JxI3Eu5DPwE

// class Program
// {
//     public struct EntityArcheType
//     {
//         private int _hashCode;



//     }

//     public class EntityArcheTypeManager
//     {



//     }

//     public struct Entity
//     {

//     }

//     public class EntityManager
//     {

//     }

//     public class MemoryAllocator
//     {
//         private Memory<byte> data;
//         public int offset = 0;



//         public MemoryAllocator(int size)
//         {
//             data = new byte[size];
//         }

//         public Memory<T> Take<T>(int elements)
//             where T : unmanaged
//         {
//             var startOffset = offset;
//             var requestSize = SizeOf<T>.Size * elements;
//             if (offset + requestSize >= data.Length)
//                 throw new OutOfMemoryException(); //TODO: Need to add a custom exception

//             offset += requestSize;

//             return Utils.Cast<byte, T>(data, startOffset, requestSize);
//         }

//         public void DumpToConsole()
//         {
//             var span = data.Span;
//             for (var l = 0; l < data.Length; l += 16)
//             {
//                 var pos = l;
//                 for (var i = 0; i < 16; i++)
//                 {
//                     if (i > 0)
//                     {
//                         if ((i % 4) == 0) Console.Write(" ");
//                         if ((i % 8) == 0) Console.Write(" ");
//                     }

//                     if (pos >= data.Length)
//                         Console.Write("  ");
//                     else
//                         Console.Write("{0:X2}", span[pos]);

//                     Console.Write(" ");
//                     pos++;
//                 }

//                 Console.Write("  ");
//                 pos = l;
//                 for (var i = 0; i < 16; i++)
//                 {
//                     if (pos >= data.Length)
//                         Console.Write(" ");
//                     else
//                     {
//                         var ch = (char)span[pos];
//                         if (Char.IsLetterOrDigit(ch) || Char.IsPunctuation(ch) || char.IsSeparator(ch))
//                             Console.Write(ch);
//                         else
//                             Console.Write(".");
//                     }
//                     pos++;
//                 }

//                 Console.WriteLine();
//             }
//         }
//     }

//     static void Main()
//     {
//         Memory<byte> bytes = new byte[1024];

//         Memory<ushort> typed = Utils.Cast<byte, ushort>(bytes);
//         Console.WriteLine(typed.Length); // 512
//         var p = new Processor();
//         typed.Span[0] = 0x5432;
//         //p.Do(typed.Span);

//         // note CPU endianness matters re the layout
//         Console.WriteLine(bytes.Span[0]); // 50 = 0x32
//         ref var x = ref typed.Span[0];
//         x = 10;
//         Console.WriteLine(bytes.Span[0]); // 50 = 0x32
//         Console.WriteLine(bytes.Span[1]); // 84 = 0x54

//         //  Console.WriteLine(summary.ToString());

//         var memtest = new MemoryAllocator(128);
//         for (var i = 0; i < 2; i++)
//         {
//             var tt = memtest.Take<ushort>(16);

//             tt.Span[0] = 0xff;
//             tt.Span[2] = 0xff;
//             tt.Span[1] = 0xaa;
//             tt.Span[3] = 0x55;
//             tt.Span[4] = 0xaa55;
//             tt.Span[5] = 0x5a5a;
//         }

//         Console.WriteLine(memtest.offset);
//         memtest.DumpToConsole();

//         var summary = BenchmarkRunner.Run<Processor>();

//     }
// }

// public class Processor
// {

//     [StructLayout(LayoutKind.Sequential)]
//     private struct Position
//     {
//         public float x, y;
//     }

//     [StructLayout(LayoutKind.Sequential)]
//     private struct Speed
//     {
//         public float x, y;
//     }

//     [StructLayout(LayoutKind.Sequential)]
//     private struct InterleavedWrap
//     {
//         public Position position;
//         public Speed speed;
//     }

//     private Memory<byte> interleaved;
//     private Memory<byte> nonInterleaved;

//     [Params(1024, 8192, 16384)]
//     public int N;

//     [GlobalSetup]
//     public void Setup()
//     {
//         var size = (SizeOf<Position>.Size + SizeOf<Speed>.Size) * N;
//         interleaved = new byte[size];
//         nonInterleaved = new byte[size];

//         m = Utils.Cast<byte, InterleavedWrap>(interleaved);
//         position = Utils.Cast<byte, Position>(nonInterleaved, 0, SizeOf<Position>.Size * N);
//         speed = Utils.Cast<byte, Speed>(nonInterleaved, SizeOf<Position>.Size * N, SizeOf<Speed>.Size * N);
//     }

//     private Memory<InterleavedWrap> m;
//     [Benchmark]
//     public void Interleaved()
//     {
//         var m = Utils.Cast<byte, InterleavedWrap>(interleaved);
//         var span = m.Span;
//         for (var i = 0; i < N; ++i)
//         {
//             ref var t = ref span[i];
//             t.speed.x = 20;
//             t.speed.y = 10;
//             t.position.x += t.speed.x;
//             t.position.y += t.speed.y;
//         }
//     }

//     [Benchmark]
//     public void NonInterleaved()
//     {
//         var spanPosition = position.Span;
//         var spanSpeed = speed.Span;
//         for (var i = 0; i < N; ++i)
//         {
//             ref var ps = ref spanPosition[i];
//             ref var ss = ref spanSpeed[i];
//             ss.x = 20;
//             ss.y = 10;
//             ps.x += ss.x;
//             ps.y += ss.y;
//         }
//     }

//     private Memory<Position> position;
//     private Memory<Speed> speed;

//     [Benchmark]
//     public void NonInterleavedCall()
//     {
//         var spanPosition = position.Span;
//         var spanSpeed = speed.Span;
//         for (var i = 0; i < N; ++i)
//         {
//             ref var ps = ref spanPosition[i];
//             ref var ss = ref spanSpeed[i];
//             _NonInterleavedCall(ref ps, ref ss);
//         }
//     }

//     private void _NonInterleavedCall(ref Position p, ref Speed s)
//     {
//         s.x = 20;
//         s.y = 10;
//         p.x += s.x;
//         p.y += s.y;
//     }
// }

// public static class SizeOf<T>
// {
//     public readonly static int Size;
//     static SizeOf()
//     {
//         Size = Marshal.SizeOf(typeof(T));
//     }
// }

// static class Utils
// {

//     public static Memory<TTo> Cast<TFrom, TTo>(Memory<TFrom> from)
//         where TFrom : unmanaged
//         where TTo : unmanaged
//     {
//         // avoid the extra allocation/indirection, at the cost of a gen-0 box
//         if (typeof(TFrom) == typeof(TTo)) return (Memory<TTo>)(object)from;

//         return new CastMemoryManager<TFrom, TTo>(from).Memory;
//     }

//     public static Memory<TTo> Cast<TFrom, TTo>(Memory<TFrom> from, int offset, int size)
//     where TFrom : unmanaged
//     where TTo : unmanaged
//     {
//         // avoid the extra allocation/indirection, at the cost of a gen-0 box
//         if (typeof(TFrom) == typeof(TTo)) return (Memory<TTo>)(object)from;

//         return new CastMemoryManager<TFrom, TTo>(from, offset, size).Memory;
//     }


//     private sealed class CastMemoryManager<TFrom, TTo> : MemoryManager<TTo>
//         where TFrom : unmanaged
//         where TTo : unmanaged
//     {
//         private readonly Memory<TFrom> _from;
//         private readonly int _startOffset;
//         private readonly int _length;

//         public CastMemoryManager(Memory<TFrom> from, int startOffset = -1, int length = -1)
//         {
//             _from = from;
//             _startOffset = startOffset > -1 ? startOffset : 0;
//             _length = length > -1 ? length : from.Length;
//         }

//         public override Span<TTo> GetSpan()
//         {
//             var span = _from.Span.Slice(_startOffset, _length);
//             return MemoryMarshal.Cast<TFrom, TTo>(span);
//         }

//         protected override void Dispose(bool disposing) { }
//         public override MemoryHandle Pin(int elementIndex = 0)
//             => throw new NotSupportedException();
//         public override void Unpin()
//             => throw new NotSupportedException();
//     }
// }