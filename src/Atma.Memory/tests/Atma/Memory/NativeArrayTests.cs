namespace Atma.Memory
{
    using Xunit;
    using Shouldly;
    using Microsoft.Extensions.Logging;
    using Xunit.Abstractions;
    using Divergic.Logging.Xunit;
    using System;

    public class NativeArrayTests
    {
        public struct Entity
        {
            public uint ID;
            public uint Key;
        }

        private struct Data
        {
            public int x;
            public int y;
            public float b;
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

        [Fact]
        public unsafe void ShouldSlice()
        {
            using var memory = new DynamicAllocator(_logFactory);

            System.Span<Data> data = stackalloc[] {
                new Data() { b = 1, x = 2, y = 3 },
                new Data() { b = 2, x = 1, y = 3 },
                new Data() { b = 3, x = 2, y = 1 },
                new Data() { b = 2, x = 3, y = 1 },
                new Data() { b = 1, x = 3, y = 2 },
                new Data() { b = 2, x = 1, y = 2 }
            };

            using var it = new NativeArray<Data>(memory, data.Length);


            for (var i = 0; i < data.Length; i++)
                it[i] = data[i];



            var slice = it.Slice();
            slice.Length.ShouldBe(6);
            for (var i = 0; i < data.Length; i++)
            {
                slice[i].x.ShouldBe(data[i].x);
                slice[i].b.ShouldBe(data[i].b);
                slice[i].y.ShouldBe(data[i].y);
            }

            slice = it.Slice(2);
            slice.Length.ShouldBe(4);
            for (var i = 2; i < data.Length; i++)
            {
                slice[i - 2].x.ShouldBe(data[i].x);
                slice[i - 2].b.ShouldBe(data[i].b);
                slice[i - 2].y.ShouldBe(data[i].y);
            }

            slice = it.Slice(2, 2);
            slice.Length.ShouldBe(2);
            for (var i = 2; i < 4; i++)
            {
                slice[i - 2].x.ShouldBe(data[i].x);
                slice[i - 2].b.ShouldBe(data[i].b);
                slice[i - 2].y.ShouldBe(data[i].y);
            }
        }
    }
}