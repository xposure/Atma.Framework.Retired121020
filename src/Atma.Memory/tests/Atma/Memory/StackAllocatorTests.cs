namespace Atma.Memory
{
    using System;
    using Divergic.Logging.Xunit;
    using Microsoft.Extensions.Logging;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public unsafe class StackAllocatorTests
    {
        private readonly ILoggerFactory _logFactory;

        public StackAllocatorTests(ITestOutputHelper output)
        {
            _logFactory = LogFactory.Create(output);
        }
        [Fact]
        public void ShouldAllocateFromFront()
        {
            //arrange
            using IAllocator memory = new DynamicAllocator(_logFactory);
            using IAllocator it = new StackAllocator(memory, 1024);

            //act
            using var handle1 = it.TakeScoped<int>(1, _logFactory);

            //assert
            handle1.Id.ShouldBe((uint)0);

            handle1.Free();
            handle1.Address.ShouldBe(IntPtr.Zero);
            handle1.Id.ShouldBe(0u);
            handle1.Flags.ShouldBe(0u);
        }


        [Fact]
        public void GetsSameAddressFront()
        {
            //arrange
            using IAllocator memory = new DynamicAllocator(_logFactory);
            using IAllocator it = new StackAllocator(memory, 1024);

            //act
            using var handle1 = it.TakeScoped<int>(1, _logFactory);
            var addr1 = handle1.Address;
            handle1.Free();

            using var handle2 = it.TakeScoped<int>(1, _logFactory);
            var addr2 = handle2.Address;
            handle2.Free();

            //assert
            addr2.ShouldBe(addr1);
            handle2.Id.ShouldBe(handle1.Id);
        }

        [Fact]
        public void GetsSameAddressBack()
        {
            //arrange
            using IAllocator memory = new DynamicAllocator(_logFactory);
            using IAllocator it = new StackAllocator(memory, 1024);

            //act
            using var handle1 = it.TakeScoped<int>(1, _logFactory);
            var addr1 = handle1.Address;
            handle1.Free();

            using var handle2 = it.TakeScoped<int>(1, _logFactory);
            var addr2 = handle2.Address;
            handle2.Free();

            //assert
            addr2.ShouldBe(addr1);
            handle2.Id.ShouldBe(handle1.Id);
        }

        [Fact]
        public void ShouldThrowOnUnorderedFree()
        {
            //arrange
            using IAllocator memory = new DynamicAllocator(_logFactory);
            using IAllocator it = new StackAllocator(memory, 1024);

            //act
            using var handle1 = it.TakeScoped<int>(1, _logFactory);
            var addr1 = handle1.Address;

            using var handle2 = it.TakeScoped<int>(1, _logFactory);
            var addr2 = handle2.Address;

            var allocHandle = handle1.Handle;
            Should.Throw<Exception>(() => it.Free(ref allocHandle));

            //assert
            addr2.ShouldNotBe(addr1);
            handle2.Id.ShouldNotBe(handle1.Id);
        }
    }
}