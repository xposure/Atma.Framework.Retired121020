namespace Atma
{
    using Atma.Memory;
    using System;
    using System.Buffers;
    using System.Runtime.InteropServices;

    public static class MemoryExtensions
    {
        public static AllocationHandleOld Take<T>(this HeapMemory heap, int length)
            where T : unmanaged
        {
            return heap.Take(SizeOf<T>.Size * length);
        }
    }
}