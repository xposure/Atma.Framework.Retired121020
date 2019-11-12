namespace Atma.Memory
{
    using Xunit;
    using Shouldly;

    public class NativeArrayTests
    {
        public struct Entity
        {
            public uint ID;
            public uint Key;
        }

        [Fact]
        public void EntityPoolNewMemoryFailure()
        {

            using var heap = new HeapAllocator();
            using var arr = new NativeArray<Entity>(heap, 4096);

        }

    }
}