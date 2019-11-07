namespace Atma.Memory
{
    using System;
    using System.Runtime.InteropServices;
    using Shouldly;

    public unsafe class UnsafeTests
    {
        private ref struct Alloc
        {
            private readonly IntPtr _heapPtr;
            public readonly IntPtr AlignedPointer;
            public readonly int Size;

            public Alloc(int size)
            {
                if (!Unsafe.IsAligned16(size))
                    size = Unsafe.Align16(size + 15); // some room for alignment
                Size = size;
                _heapPtr = Marshal.AllocHGlobal(size);
                AlignedPointer = Unsafe.Align16(_heapPtr);
            }

            public void Dispose() //is auto called with ref struct
            {
                Marshal.FreeHGlobal(_heapPtr);
            }
        }

        public void Align16()
        {
            for (var i = 1; i < 16; i++)
            {
                Unsafe.Align16(i * 16).ShouldBe(i * 16);
                for (var k = 1; k < 16; k++)
                {
                    Unsafe.Align16(i * 16 + k).ShouldBe((i + 1) * 16);
                }
            }
        }

        public void IsAligned()
        {
            for (var i = 0; i < 1024; i++)
                Unsafe.IsAligned16((void*)new IntPtr(i)).ShouldBe((i % 16) == 0);
        }

        public void ShouldClear()
        {
            using var alloc = new Alloc(128);
            alloc.Size.ShouldBe(128);
            var ptr = (int*)alloc.AlignedPointer;
            for (var i = 0; i < alloc.Size / 4; i++)
                ptr[i] = 0x5a5a5a5a;

            Unsafe.ClearAlign16((void*)alloc.AlignedPointer, alloc.Size);
            for (var i = 0; i < alloc.Size / 4; i++)
                ptr[i].ShouldBe(0);

            Unsafe.ClearAlign16((void*)alloc.AlignedPointer, alloc.Size, 0x5aa5aa55);
            for (var i = 0; i < alloc.Size / 4; i++)
                ptr[i].ShouldBe(0x5aa5aa55);
        }
    }
}
