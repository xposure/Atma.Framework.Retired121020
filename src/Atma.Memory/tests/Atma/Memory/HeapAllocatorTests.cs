namespace Atma.Memory
{
    using System;
    using Xunit;
    using Xunit.Abstractions;
    using Shouldly;
    using static Atma.Debug;
    using Microsoft.Extensions.Logging;
    using Divergic.Logging.Xunit;

    public class HeapAllocatorTests
    {
        private readonly ILoggerFactory _logFactory;

        public HeapAllocatorTests(ITestOutputHelper output)
        {
            _logFactory = LogFactory.Create(output);
        }

        [Fact]
        public unsafe void HeapAllocationShouldAllocate()
        {
            //arrange
            var blocks = 256;
            var size = blocks * HeapAllocation.HeapSize;
            var memory = stackalloc HeapAllocation[blocks];
            var span = new System.Span<HeapAllocation>(memory, blocks);
            *memory = new HeapAllocation(size);
            //memory->Blocks = (uint)blocks - 1; //offset the first heap block


            //act
            HeapAllocation.Split(memory, 1);

            //assert
            span[0].Flags.ShouldBe(1u);
            span[0].Blocks.ShouldBe(1u);
            span[0].SizeInBytes.ShouldBe(span[0].Blocks * HeapAllocation.HeapSize);
            HeapAllocation.CountFreeBlocks(memory).ShouldBe((uint)blocks - 3u);
            HeapAllocation.CountUsedBlocks(memory, out var allocations).ShouldBe((uint)3u);
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
            var size = blocks * HeapAllocation.HeapSize;
            var memory = stackalloc HeapAllocation[blocks];
            var span = new System.Span<HeapAllocation>(memory, blocks);
            *memory = new HeapAllocation(size);

            //act
            HeapAllocation.Split(memory, 1);
            HeapAllocation.Free(memory);

            //assert
            span[0].Blocks.ShouldBe((uint)blocks - 1);
            span[0].SizeInBytes.ShouldBe((uint)(blocks - 1) * HeapAllocation.HeapSize);
            HeapAllocation.CountFreeBlocks(memory).ShouldBe((uint)blocks - 1u);
            HeapAllocation.CountUsedBlocks(memory, out var allocations).ShouldBe((uint)1u);
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
        public unsafe void HeapAllocationShouldFillGap()
        {
            //arrange
            var blocks = 256;
            var size = blocks * HeapAllocation.HeapSize;
            var memory = stackalloc HeapAllocation[blocks];
            var span = new System.Span<HeapAllocation>(memory, blocks);
            *memory = new HeapAllocation(size);

            //act
            HeapAllocation.Split(memory, 1); //255
            HeapAllocation.Split(&memory[2], 1); //252
            HeapAllocation.Split(&memory[4], 1); //249

            HeapAllocation.Free(&memory[2]);

            //assert
            span[0].Blocks.ShouldBe(1u);
            span[2].Blocks.ShouldBe(1u);
            span[4].Blocks.ShouldBe(1u);
            span[6].Blocks.ShouldBe((uint)blocks - 7);

            span[0].SizeInBytes.ShouldBe((uint)HeapAllocation.HeapSize);
            span[2].SizeInBytes.ShouldBe((uint)HeapAllocation.HeapSize);
            span[4].SizeInBytes.ShouldBe((uint)HeapAllocation.HeapSize);
            span[6].SizeInBytes.ShouldBe((uint)(blocks - 7) * HeapAllocation.HeapSize);

            HeapAllocation.CountFreeBlocks(memory).ShouldBe((uint)blocks - 6u);
            HeapAllocation.CountUsedBlocks(memory, out var allocations).ShouldBe((uint)6u);
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
            HeapAllocation.Free(&memory[0]);

            //assert2
            span[0].Blocks.ShouldBe(3u);
            span[2].Blocks.ShouldBe(0u);
            span[4].Blocks.ShouldBe(1u);
            span[6].Blocks.ShouldBe((uint)blocks - 7);

            span[0].SizeInBytes.ShouldBe((uint)HeapAllocation.HeapSize * 3);
            span[2].SizeInBytes.ShouldBe(0u);
            span[4].SizeInBytes.ShouldBe((uint)HeapAllocation.HeapSize);
            span[6].SizeInBytes.ShouldBe((uint)(blocks - 7) * HeapAllocation.HeapSize);

            HeapAllocation.CountFreeBlocks(memory).ShouldBe((uint)blocks - 4u);
            HeapAllocation.CountUsedBlocks(memory, out allocations).ShouldBe((uint)4u);
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
            HeapAllocation.Free(&memory[4]);

            //assert3
            span[0].Blocks.ShouldBe((uint)(blocks - 1));
            span[2].Blocks.ShouldBe(0u);
            span[4].Blocks.ShouldBe(0u);
            span[6].Blocks.ShouldBe(0u);

            span[0].SizeInBytes.ShouldBe((uint)(HeapAllocation.HeapSize * (blocks - 1)));
            span[2].SizeInBytes.ShouldBe(0u);
            span[4].SizeInBytes.ShouldBe(0u);
            span[6].SizeInBytes.ShouldBe(0u);

            HeapAllocation.CountFreeBlocks(memory).ShouldBe((uint)(blocks - 1u));
            HeapAllocation.CountUsedBlocks(memory, out allocations).ShouldBe(1u);
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
            using var memory = new HeapAllocator(_logFactory);

            //act
            var handle = memory.Take(1024);

            //assert
            handle.Address.ShouldNotBe(IntPtr.Zero);
        }
    }
}