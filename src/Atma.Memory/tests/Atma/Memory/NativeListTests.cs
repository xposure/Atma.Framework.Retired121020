namespace Atma.Memory
{
    using System;
    using Divergic.Logging.Xunit;
    using Microsoft.Extensions.Logging;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public class NativeListTests
    {
        private struct Data
        {
            public int x;
            public int y;
            public float b;
        }

        private readonly ILoggerFactory _logFactory;

        public NativeListTests(ITestOutputHelper output)
        {
            _logFactory = LogFactory.Create(output);
        }

        [Fact]
        public void ShouldAdd()
        {
            using var m = new DynamicAllocator(_logFactory);
            using var x = new NativeList<Data>(m);

            x.Add(new Data() { x = 10, y = 11, b = 12 });
            x.Length.ShouldBe(1);
            x.Add(new Data() { x = 20, y = 21, b = 22 });
            x.Length.ShouldBe(2);
            x.Add(new Data() { x = 30, y = 31, b = 32 });
            x.Length.ShouldBe(3);

            x[0].x.ShouldBe(10);
            x[0].y.ShouldBe(11);
            x[0].b.ShouldBe(12);

            x[1].x.ShouldBe(20);
            x[1].y.ShouldBe(21);
            x[1].b.ShouldBe(22);

            x[2].x.ShouldBe(30);
            x[2].y.ShouldBe(31);
            x[2].b.ShouldBe(32);

            x.RemoveAt(1);
            x.Length.ShouldBe(2);

            x[0].x.ShouldBe(10);
            x[0].y.ShouldBe(11);
            x[0].b.ShouldBe(12);

            x[1].x.ShouldBe(30);
            x[1].y.ShouldBe(31);
            x[1].b.ShouldBe(32);

        }

        [Fact]
        public void ShouldResize()
        {
            using var m = new DynamicAllocator(_logFactory);
            using var x = new NativeList<Data>(m);

            var maxLength = x.MaxLength;
            for (var i = 0; i < 1024; i++)
            {
                var shouldGrow = x.Length == x.MaxLength;
                x.Add(new Data() { x = 10 + i, y = 11 + i, b = 0x5a });
                x.Length.ShouldBe(i + 1);
                if (shouldGrow)
                {
                    x.MaxLength.ShouldNotBe(maxLength);
                    maxLength = x.MaxLength;
                }
                else
                {
                    x.MaxLength.ShouldBe(maxLength);
                }
            }

            for (var i = 0; i < x.Length; i++)
            {
                x[i].x.ShouldBe(10 + i);
                x[i].y.ShouldBe(11 + i);
                x[i].b.ShouldBe(0x5a);
            }

        }

        [Fact]
        public unsafe void ShouldSlice()
        {
            using var memory = new DynamicAllocator(_logFactory);
            using var list = new NativeList<Data>(memory);

            Span<Data> data = stackalloc[] {
                new Data() { b = 1, x = 2, y = 3 },
                new Data() { b = 2, x = 1, y = 3 },
                new Data() { b = 3, x = 2, y = 1 },
                new Data() { b = 2, x = 3, y = 1 },
                new Data() { b = 1, x = 3, y = 2 },
                new Data() { b = 2, x = 1, y = 2 }
            };

            for (var i = 0; i < data.Length; i++)
                list.Add(data[i]);


            var slice = list.Slice();
            slice.Length.ShouldBe(6);
            for (var i = 0; i < data.Length; i++)
            {
                slice[i].x.ShouldBe(data[i].x);
                slice[i].b.ShouldBe(data[i].b);
                slice[i].y.ShouldBe(data[i].y);
            }

            slice = list.Slice(2);
            slice.Length.ShouldBe(4);
            for (var i = 2; i < data.Length; i++)
            {
                slice[i - 2].x.ShouldBe(data[i].x);
                slice[i - 2].b.ShouldBe(data[i].b);
                slice[i - 2].y.ShouldBe(data[i].y);
            }

            slice = list.Slice(2, 2);
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
