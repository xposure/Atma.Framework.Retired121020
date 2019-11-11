namespace Atma.Memory
{
    using System;
    using Xunit;
    using Shouldly;
    using static Atma.Debug;

    public class HeapAllocatorTests
    {
        [Fact]
        public unsafe void HeapAllocationShouldAllocate()
        {
            //arrange
            var blocks = 256;
            var size = blocks * HeapAllocation2.HeapSize;
            var memory = stackalloc HeapAllocation2[blocks];
            var span = new Span<HeapAllocation2>(memory, blocks);
            memory->Blocks = (uint)blocks - 1; //offset the first heap block

            //act
            HeapAllocation2.Split(memory, 1);

            //assert
            span[0].Flags.ShouldBe(1u);
            span[0].Blocks.ShouldBe(1u);
            span[0].SizeInBytes.ShouldBe(span[0].Blocks * HeapAllocation2.HeapSize);
            HeapAllocation2.CountFreeBlocks(memory).ShouldBe((uint)blocks - 3u);
            HeapAllocation2.CountUsedBlocks(memory, out var allocations).ShouldBe((uint)3u);
            allocations.ShouldBe(1);
            span[2].Blocks.ShouldBe((uint)blocks - 3u);

            Assert(span[2].Next == null);
            Assert(span[0].Previous == null);
            Assert(span[0].Next == &memory[2]);
            Assert(span[2].Previous == &memory[0]);
        }

        [Fact]
        public unsafe void HeapAllocationShouldFree()
        {
            //arrange
            var blocks = 256;
            var size = blocks * HeapAllocation2.HeapSize;
            var memory = stackalloc HeapAllocation2[blocks];
            var span = new Span<HeapAllocation2>(memory, blocks);
            memory->Blocks = (uint)blocks - 1; //offset the first heap block

            //act
            HeapAllocation2.Split(memory, 1);
            HeapAllocation2.Free(memory);

            //assert
            span[0].Blocks.ShouldBe((uint)blocks - 1);
            span[0].SizeInBytes.ShouldBe((uint)(blocks - 1) * HeapAllocation2.HeapSize);
            HeapAllocation2.CountFreeBlocks(memory).ShouldBe((uint)blocks - 1u);
            HeapAllocation2.CountUsedBlocks(memory, out var allocations).ShouldBe((uint)1u);
            allocations.ShouldBe(0);
            span[2].Blocks.ShouldBe(0u);
            Assert(span[0].Previous == null);
            Assert(span[0].Next == null);
            Assert(span[2].Next == null);
            Assert(span[2].Previous == null);
            Assert(span[2].Flags == 0);
            //Assert(span[2].Previous == null);
        }

        [Fact]
        public unsafe void HeppAlocationShouldFillGap()
        {
            //arrange
            var blocks = 256;
            var size = blocks * HeapAllocation2.HeapSize;
            var memory = stackalloc HeapAllocation2[blocks];
            var span = new Span<HeapAllocation2>(memory, blocks);
            memory->Blocks = (uint)blocks - 1; //offset the first heap block

            //act
            HeapAllocation2.Split(memory, 1); //255
            HeapAllocation2.Split(&memory[2], 1); //252
            HeapAllocation2.Split(&memory[4], 1); //249

            HeapAllocation2.Free(&memory[2]);

            //assert
            span[0].Blocks.ShouldBe(1u);
            span[2].Blocks.ShouldBe(1u);
            span[4].Blocks.ShouldBe(1u);
            span[6].Blocks.ShouldBe((uint)blocks - 7);

            span[0].SizeInBytes.ShouldBe((uint)HeapAllocation2.HeapSize);
            span[2].SizeInBytes.ShouldBe((uint)HeapAllocation2.HeapSize);
            span[4].SizeInBytes.ShouldBe((uint)HeapAllocation2.HeapSize);
            span[6].SizeInBytes.ShouldBe((uint)(blocks - 7) * HeapAllocation2.HeapSize);

            HeapAllocation2.CountFreeBlocks(memory).ShouldBe((uint)blocks - 6u);
            HeapAllocation2.CountUsedBlocks(memory, out var allocations).ShouldBe((uint)6u);
            allocations.ShouldBe(2);

            Assert(span[0].Previous == null);
            Assert(span[2].Previous == &memory[0]);
            Assert(span[4].Previous == &memory[2]);
            Assert(span[6].Previous == &memory[4]);

            Assert(span[0].Next == &memory[2]);
            Assert(span[2].Next == &memory[4]);
            Assert(span[4].Next == &memory[6]);
            Assert(span[6].Next == null);

            span[0].Flags.ShouldBe(1u);
            span[2].Flags.ShouldBe(0u);
            span[4].Flags.ShouldBe(1u);
            span[6].Flags.ShouldBe(0u);

            //act2
            HeapAllocation2.Free(&memory[0]);

            //assert2
            span[0].Blocks.ShouldBe(3u);
            span[2].Blocks.ShouldBe(0u);
            span[4].Blocks.ShouldBe(1u);
            span[6].Blocks.ShouldBe((uint)blocks - 7);

            span[0].SizeInBytes.ShouldBe((uint)HeapAllocation2.HeapSize * 3);
            span[2].SizeInBytes.ShouldBe(0u);
            span[4].SizeInBytes.ShouldBe((uint)HeapAllocation2.HeapSize);
            span[6].SizeInBytes.ShouldBe((uint)(blocks - 7) * HeapAllocation2.HeapSize);

            HeapAllocation2.CountFreeBlocks(memory).ShouldBe((uint)blocks - 4u);
            HeapAllocation2.CountUsedBlocks(memory, out allocations).ShouldBe((uint)4u);
            allocations.ShouldBe(1);

            Assert(span[0].Previous == null);
            Assert(span[2].Previous == null);
            Assert(span[4].Previous == &memory[0]);
            Assert(span[6].Previous == &memory[4]);

            Assert(span[0].Next == &memory[4]);
            Assert(span[2].Next == null);
            Assert(span[4].Next == &memory[6]);
            Assert(span[6].Next == null);

            span[0].Flags.ShouldBe(0u);
            span[2].Flags.ShouldBe(0u);
            span[4].Flags.ShouldBe(1u);
            span[6].Flags.ShouldBe(0u);


            //act3
            HeapAllocation2.Free(&memory[4]);

            //assert3
            span[0].Blocks.ShouldBe((uint)(blocks - 1));
            span[2].Blocks.ShouldBe(0u);
            span[4].Blocks.ShouldBe(0u);
            span[6].Blocks.ShouldBe(0u);

            span[0].SizeInBytes.ShouldBe((uint)(HeapAllocation2.HeapSize * (blocks - 1)));
            span[2].SizeInBytes.ShouldBe(0u);
            span[4].SizeInBytes.ShouldBe(0u);
            span[6].SizeInBytes.ShouldBe(0u);

            HeapAllocation2.CountFreeBlocks(memory).ShouldBe((uint)(blocks - 1u));
            HeapAllocation2.CountUsedBlocks(memory, out allocations).ShouldBe(1u);
            allocations.ShouldBe(0);

            Assert(span[0].Previous == null);
            Assert(span[2].Previous == null);
            Assert(span[4].Previous == null);
            Assert(span[6].Previous == null);

            Assert(span[0].Next == null);
            Assert(span[2].Next == null);
            Assert(span[4].Next == null);
            Assert(span[6].Next == null);

            span[0].Flags.ShouldBe(0u);
            span[2].Flags.ShouldBe(0u);
            span[4].Flags.ShouldBe(0u);
            span[6].Flags.ShouldBe(0u);

        }

        [Fact]
        public void HeapShouldAllocate()
        {
            //arrange
            using var unmanaged = new DynamicAllocator();
            using var memory = new HeapAllocator(unmanaged);

            //act
            var handle = memory.Take(1024);

            //assert
            handle.Address.ShouldNotBe(IntPtr.Zero);
        }
    }
}