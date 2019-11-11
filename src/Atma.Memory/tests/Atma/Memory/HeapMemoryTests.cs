namespace Atma.Memory
{
    using Shouldly;
    using System;
    using System.Runtime.InteropServices;
    using Xunit;
    using static Atma.Debug;

    public unsafe class HeapMemoryTests
    {
        [Fact]
        public void ShouldAllocAligned()
        {
            using (var alloc = new HeapMemory(256))
                alloc.Size.ShouldBe(256);

            using (var alloc = new HeapMemory(257))
                alloc.Size.ShouldBe(256 + 16);
        }

        [Fact]
        public void ShouldClear()
        {
            using (var alloc = new HeapMemory(256))
            {
                alloc.Clear(0x5aa55aaa);
                var begin = (int*)alloc.AlignedPointer;
                var end = (int*)alloc.EndPointer;
                while (begin < end)
                    (*begin++).ShouldBe(0x5aa55aaa);
            }
        }

        [Fact]
        public void HeapShouldSplit()
        {
            //var bytes = stackalloc byte[512];
            var size = 512;
            var damnit = Marshal.AllocHGlobal(size);
            try
            {
                var data = (byte*)damnit;
                var heapPtr = (HeapAllocation*)data;
                HeapAllocation.Initialize(heapPtr, size);

                PointerExtensions.ShouldBe(heapPtr->Header.Previous, null);
                PointerExtensions.ShouldBe(heapPtr->Header.Next, null);
                heapPtr->Header.Flags.ShouldBe((HeapAllocationFlags)0);
                heapPtr->Header.Size.ShouldBe(size - HeapAllocation.HEADERSIZE);
                HeapAllocation.Count(heapPtr).ShouldBe(1);

                HeapAllocation.Split(heapPtr, 64);
                HeapAllocation.Count(heapPtr).ShouldBe(2);

                var nextHeapPtr = heapPtr->Header.Next;

                PointerExtensions.ShouldBe(heapPtr->Header.Previous, null);
                PointerExtensions.ShouldNotBe(heapPtr->Header.Next, null);
                PointerExtensions.ShouldBe(heapPtr->Header.Next, nextHeapPtr);
                nextHeapPtr->Header.Flags.ShouldBe((HeapAllocationFlags)0);
                nextHeapPtr->Header.Size.ShouldBe(size - HeapAllocation.HEADERSIZE * 2 - 64);

                HeapAllocation.Split(nextHeapPtr, 64);
                HeapAllocation.Count(heapPtr).ShouldBe(3);
                var nextNextHeapPtr = nextHeapPtr->Header.Next;

                PointerExtensions.ShouldBe(nextHeapPtr->Header.Previous, heapPtr);
                PointerExtensions.ShouldNotBe(nextHeapPtr->Header.Next, null);
                PointerExtensions.ShouldBe(nextHeapPtr->Header.Next, nextNextHeapPtr);
                nextNextHeapPtr->Header.Flags.ShouldBe((HeapAllocationFlags)0);
                nextNextHeapPtr->Header.Size.ShouldBe(size - HeapAllocation.HEADERSIZE * 3 - 128);

                //should take remaining
                HeapAllocation.Split(nextNextHeapPtr, 256);
                HeapAllocation.Count(heapPtr).ShouldBe(3);

                PointerExtensions.ShouldBe(nextHeapPtr->Header.Previous, heapPtr);
                PointerExtensions.ShouldNotBe(nextHeapPtr->Header.Next, null);
                PointerExtensions.ShouldBe(nextHeapPtr->Header.Next, nextNextHeapPtr);
                nextNextHeapPtr->Header.Flags.ShouldBe(HeapAllocationFlags.Used);
                nextNextHeapPtr->Header.Size.ShouldBe(288);

            }
            finally
            {
                Marshal.FreeHGlobal(damnit);
            }
        }

        [Fact]
        public void HeapShouldFindFree()
        {
            //var bytes = stackalloc byte[512];
            var size = 512;
            var damnit = Marshal.AllocHGlobal(size);
            try
            {
                var data = (byte*)damnit;
                var heapPtr = (HeapAllocation*)data;
                HeapAllocation.Initialize(heapPtr, size);

                var free0 = HeapAllocation.FindUnused(heapPtr);
                Assert(free0 != null);
                HeapAllocation.Split(free0, 32);
                var free1 = HeapAllocation.FindUnused(heapPtr);
                HeapAllocation.Split(free1, 32);

                Assert(free0 != free1);

                HeapAllocation.Count(heapPtr).ShouldBe(3);
            }
            finally
            {
                Marshal.FreeHGlobal(damnit);
            }
        }

        [Fact]
        public void HeapShouldFree()
        {
            //var bytes = stackalloc byte[512];
            var size = 512;
            var loops = size / (HeapAllocation.HEADERSIZE * 2);
            var damnit = Marshal.AllocHGlobal(size);
            try
            {
                var data = (byte*)damnit;
                var heapPtr = (HeapAllocation*)data;
                HeapAllocation.Initialize(heapPtr, size);

                var allocations = new HeapAllocation*[loops];

                for (var i = 0; i < loops; i++)
                {
                    var free = HeapAllocation.FindUnused(heapPtr);
                    HeapAllocation.Split(free, HeapAllocation.HEADERSIZE);
                    allocations[i] = free;
                }

                //verify all pointers
                HeapAllocation.Count(heapPtr).ShouldBe(8);
                HeapAllocation.CountReverse(allocations[loops - 1]).ShouldBe(8);

                //we should have nothing left free
                Assert(HeapAllocation.FindUnused(heapPtr) == null);

                for (var i = 1; i < loops; i += 2)
                    HeapAllocation.Free(allocations[i]);

                //verify all pointers
                HeapAllocation.Count(heapPtr).ShouldBe(8);
                HeapAllocation.CountReverse(allocations[loops - 1]).ShouldBe(8);

                for (var i = 0; i < loops; i += 2)
                {
                    HeapAllocation.Free(allocations[i]);
                    //verify all pointers
                    HeapAllocation.Count(heapPtr).ShouldBe(loops - i - 1);
                    HeapAllocation.CountReverse(allocations[loops - 1]).ShouldBe(loops - i - 1);
                }

                HeapAllocation.Count(heapPtr).ShouldBe(1);
                HeapAllocation.CountReverse(allocations[loops - 1]).ShouldBe(1);

                PointerExtensions.ShouldBe(heapPtr->Header.Previous, null);
                PointerExtensions.ShouldBe(heapPtr->Header.Next, null);
                heapPtr->Header.Flags.ShouldBe((HeapAllocationFlags)0);
                heapPtr->Header.Size.ShouldBe(size - HeapAllocation.HEADERSIZE);
                HeapAllocation.Count(heapPtr).ShouldBe(1);
            }
            finally
            {
                Marshal.FreeHGlobal(damnit);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct Data
        {
            public int x;
            public float y;
            public byte t;
        }

        // public void ShouldAllocateHeapAndFree()
        // {
        //     var allocSize = 4096;
        //     var size = SizeOf<Data>.Size;
        //     using (var heap = new HeapMemory(allocSize))
        //     {
        //         heap.UsedSize.ShouldBe(0);
        //         heap.FreeSize.ShouldBe(allocSize);

        //         var handle = heap.Take<Data>(10);
        //         var ptr = (byte*)handle.Address;
        //         heap.UsedSize.ShouldBe(Unsafe.Align16(size * 10) + HeapAllocation.HEADERSIZE);

        //         var handle1 =heap.Take<Data>(10);
        //         var ptr1 = (byte*)handle1.Address;
        //         heap.UsedSize.ShouldBe((Unsafe.Align16(size * 10 )+ HeapAllocation.HEADERSIZE) * 2);
        //         PointerExtensions.ShouldNotBe(ptr, null);
        //         PointerExtensions.ShouldNotBe(ptr1, null);
        //         PointerExtensions.ShouldNotBe(ptr, ptr1);

        //         heap.Free(ref handle);
        //         var newhandle = heap.Take<Data>(10);
        //         var newptr = (byte*)newhandle.Address;
        //         PointerExtensions.ShouldBe(newptr, ptr);

        //         heap.Free(ref handle1);
        //         var newhandle1 = heap.Take<Data>(10);
        //         var newptr1 = (byte*)newhandle1.Address;
        //         PointerExtensions.ShouldBe(newptr1, ptr1);

        //     }
        // }

        // public void StackAllocatorShouldThrow()
        // {
        //     var allocSize = 4096;
        //     var size = SizeOf<Data>.Size;
        //     using (var heap = new HeapMemory(allocSize, allocator: Allocator.Stack))
        //     {
        //         heap.UsedSize.ShouldBe(0);
        //         heap.FreeSize.ShouldBe(allocSize);

        //         var handle = heap.Take<Data>(10);
        //         var ptr = (byte*)handle.Address;
        //         heap.UsedSize.ShouldBe(Unsafe.Align16(size * 10) + HeapAllocation.HEADERSIZE);

        //         var handle1 = heap.Take<Data>(10);
        //         var ptr1 = (byte*)handle1.Address;
        //         heap.UsedSize.ShouldBe((Unsafe.Align16(size * 10) + HeapAllocation.HEADERSIZE) * 2);

        //         heap.Free(ref handle1);
        //         heap.Free(ref handle);
        //     }

        //     using (var heap = new HeapMemory(allocSize, allocator: Allocator.Stack))
        //     {
        //         heap.UsedSize.ShouldBe(0);
        //         heap.FreeSize.ShouldBe(allocSize);

        //         var handle = heap.Take<Data>(10);
        //         var ptr = (byte*)handle.Address;
        //         heap.UsedSize.ShouldBe(Unsafe.Align16(size * 10) + HeapAllocation.HEADERSIZE);

        //         var handle1 = heap.Take<Data>(10);
        //         var ptr1 = (byte*)handle1.Address;
        //         heap.UsedSize.ShouldBe((Unsafe.Align16(size * 10) + HeapAllocation.HEADERSIZE) * 2);

        //         Should.Throw<Exception>(() => heap.Free(ref handle));

        //     }

        // }

    }

    public unsafe static class PointerExtensions
    {
        public static void ShouldBe<T>(T* source, T* value)
            where T : unmanaged
        {
            if (source != value)
                throw new System.Exception($"Pointers mismatched");
        }

        public static void ShouldNotBe<T>(T* source, T* value)
           where T : unmanaged
        {
            if (source == value)
                throw new System.Exception($"Pointers mismatched");
        }
    }
}
