using Shouldly;
using Xunit;

namespace Atma.Memory
{
    public unsafe class DynamicBufferTests
    {
        public struct Data
        {
            public float x;
        }

        public struct Data1
        {
            public int x;
            public int y;
        }

        public struct Data2
        {
            public int z;
            public byte b;
        }

        [Fact]
        public void AllocatesDynamicTypes()
        {
            using var buffer = new NativeBuffer(Allocator.Temp);
            var d0 = buffer.Add(new Data() { x = 10 });
            buffer.Length.ShouldBe(4);

            var d1 = buffer.Add(new Data1() { x = 22, y = 33 });
            buffer.Length.ShouldBe(12);

            var d2 = buffer.Add(new Data2() { z = 44, b = 2 });
            buffer.Length.ShouldBe(20);
        }
    }
}
