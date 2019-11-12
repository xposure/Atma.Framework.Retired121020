namespace Atma.Memory
{
    using System;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;
    using Divergic.Logging.Xunit;
    using Microsoft.Extensions.Logging;

    public class DynamicMemoryTests
    {
        private readonly ILoggerFactory _logFactory;

        public DynamicMemoryTests(ITestOutputHelper output)
        {
            _logFactory = LogFactory.Create(output);
        }


        [Fact]
        public void ShoudAllocate()
        {
            //arrange
            using var allocator = new DynamicAllocator(_logFactory);

            //act
            using var handle0 = allocator.TakeScoped(1024);
            using var handle1 = allocator.TakeScoped(1024);

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
            using var allocator = new DynamicAllocator(_logFactory);

            //act
            using var handle0 = allocator.TakeScoped(1024);
            using var handle1 = allocator.TakeScoped(1024);

            handle0.Free();

            //assert
            handle0.Address.ShouldBe(IntPtr.Zero);
            handle0.IsValid.ShouldBe(false);

            handle1.Address.ShouldNotBe(IntPtr.Zero);
            handle1.IsValid.ShouldBe(true);

            allocator.Size.ShouldBe(1024u);
        }
    }
}