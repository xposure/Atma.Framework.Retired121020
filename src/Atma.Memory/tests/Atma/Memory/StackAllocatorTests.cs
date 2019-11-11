namespace Atma.Memory
{
    using System;
    using Shouldly;
    using Xunit;

    public unsafe class StackAllocatorTests
    {
        [Fact]
        public void ShouldAllocateFromFront()
        {
            //arrange
            using IAllocator memory = new DynamicAllocator();
            using IAllocator it = new StackAllocator(memory, 1024);

            //act
            var handle1 = it.Take<int>(1);

            //assert
            handle1.Id.ShouldBe((uint)0);
        }


        [Fact]
        public void GetsSameAddressFront()
        {
            //arrange
            using IAllocator memory = new DynamicAllocator();
            using IAllocator it = new StackAllocator(memory, 1024);

            //act
            var handle1 = it.Take<int>(1);
            var addr1 = handle1.Address;
            it.Free(ref handle1);

            var handle2 = it.Take<int>(1);
            var addr2 = handle2.Address;
            it.Free(ref handle2);

            //assert
            addr2.ShouldBe(addr1);
            handle2.Id.ShouldBe(handle1.Id);
        }

        [Fact]
        public void GetsSameAddressBack()
        {
            //arrange
            using IAllocator memory = new DynamicAllocator();
            using IAllocator it = new StackAllocator(memory, 1024);

            //act
            var handle1 = it.Take<int>(1);
            var addr1 = handle1.Address;
            it.Free(ref handle1);

            var handle2 = it.Take<int>(1);
            var addr2 = handle2.Address;
            it.Free(ref handle2);

            //assert
            addr2.ShouldBe(addr1);
            handle2.Id.ShouldBe(handle1.Id);
        }

        [Fact]
        public void ShouldThrowOnUnorderedFree()
        {
            //arrange
            using IAllocator memory = new DynamicAllocator();
            using IAllocator it = new StackAllocator(memory, 1024);

            //act
            var handle1 = it.Take<int>(1);
            var addr1 = handle1.Address;

            var handle2 = it.Take<int>(1);
            var addr2 = handle2.Address;
            Should.Throw<Exception>(() => it.Free(ref handle1));

            //assert
            addr2.ShouldNotBe(addr1);
            handle2.Id.ShouldNotBe(handle1.Id);
        }
    }
}