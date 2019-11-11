namespace Atma.Memory
{
    using System;
    using Shouldly;
    using Xunit;

    public class DynamicMemoryTests
    {
        [Fact]
        public void ShoudAllocate()
        {
            //arrange
            var allocator = new DynamicAllocator();

            //act
            var handle0 = allocator.Take(1024);
            var handle1 = allocator.Take(1024);

            //assert
            handle0.Address.ShouldNotBe(IntPtr.Zero);
            handle0.IsValid.ShouldBe(true);

            handle1.Address.ShouldNotBe(IntPtr.Zero);
            handle1.IsValid.ShouldBe(true);

            allocator.Size.ShouldBe(2048u);
        }

        [Fact]
        public void ShouldFree()
        {
            //arrange
            var allocator = new DynamicAllocator();

            //act
            var handle0 = allocator.Take(1024);
            var handle1 = allocator.Take(1024);

            allocator.Free(ref handle0);

            //assert
            handle0.Address.ShouldBe(IntPtr.Zero);
            handle0.IsValid.ShouldBe(false);

            handle1.Address.ShouldNotBe(IntPtr.Zero);
            handle1.IsValid.ShouldBe(true);

            allocator.Size.ShouldBe(1024u);
        }
    }
}