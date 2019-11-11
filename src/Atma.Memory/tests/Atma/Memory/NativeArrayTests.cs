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

            using var memory = new DynamicAllocator();
            using var heap = new HeapAllocator(memory);
            using var arr = new NativeArray<Entity>(heap, 4096);

        }

    }
}