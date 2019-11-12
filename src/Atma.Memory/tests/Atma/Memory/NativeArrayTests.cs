namespace Atma.Memory
{
    using Xunit;
    using Shouldly;
    using Microsoft.Extensions.Logging;
    using Xunit.Abstractions;
    using Divergic.Logging.Xunit;

    public class NativeArrayTests
    {
        public struct Entity
        {
            public uint ID;
            public uint Key;
        }

        private readonly ILoggerFactory _logFactory;

        public NativeArrayTests(ITestOutputHelper output)
        {
            _logFactory = LogFactory.Create(output);
        }

        [Fact]
        public void EntityPoolNewMemoryFailure()
        {

            using var heap = new HeapAllocator(_logFactory);
            using var arr = new NativeArray<Entity>(heap, 4096);
            using var arr1 = new NativeArray<Entity>(heap, 4096);
            using var arr2 = new NativeArray<Entity>(heap, 4096);
            using var arr3 = new NativeArray<Entity>(heap, 4096);

        }

    }
}