namespace Atma.Entities
{
    using System;
    using Atma.Memory;
    using Divergic.Logging.Xunit;
    using Microsoft.Extensions.Logging;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public class ComponentArrayTests
    {
        private struct Position
        {
            public int X;
            public int Y;

            public Position(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        private struct Velocity
        {
            public int VX;
            public int VY;

            public Velocity(int vx, int vy)
            {
                VX = vx;
                VY = vy;
            }
        }

        private readonly ILoggerFactory _logFactory;

        public ComponentArrayTests(ITestOutputHelper output)
        {
            _logFactory = LogFactory.Create(output);
        }

        [Fact]
        public void ShouldGetIndex()
        {
            //arrange
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator = new DynamicAllocator(_logFactory);
            using var data = new ComponentDataArray(_logFactory, allocator, componentType, 32);

            //act
            var span = data.AsSpan<Position>();
            ref var p = ref span[0];

            //assert
            p.X.ShouldBe(0);
            p.Y.ShouldBe(0);
        }

        [Fact]
        public void ShouldSetIndex()
        {
            //arrange
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator = new DynamicAllocator(_logFactory);
            using var data = new ComponentDataArray(_logFactory, allocator, componentType, 32);

            //act
            var span = data.AsSpan<Position>();
            ref var p0 = ref span[0];
            p0.X = 10;
            p0.Y = 20;

            ref var p1 = ref span[0];

            //assert
            p0.X.ShouldBe(10);
            p0.Y.ShouldBe(20);
            p0.X.ShouldBe(p1.X);
            p0.Y.ShouldBe(p1.Y);
        }

        [Fact]
        public void ShouldCopyToAnotherArray()
        {
            //arrange
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator0 = new DynamicAllocator(_logFactory);
            using var data0 = new ComponentDataArray(_logFactory, allocator0, componentType, 32);
            using IAllocator allocator1 = new DynamicAllocator(_logFactory);
            using var data1 = new ComponentDataArray(_logFactory, allocator0, componentType, 32);

            //act
            var span0 = data0.AsSpan<Position>();
            ref var p0 = ref span0[0];
            p0.X = 10;
            p0.Y = 20;

            ComponentDataArray.CopyTo(data0, 0, data1, 1);

            //assert
            var span1 = data1.AsSpan<Position>();
            ref var p1 = ref span1[1];
            p1.X.ShouldBe(10);
            p1.Y.ShouldBe(20);
        }

        [Fact]
        public void ShouldReset()
        {
            //arrange
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator = new DynamicAllocator(_logFactory);
            using var data = new ComponentDataArray(_logFactory, allocator, componentType, 32);

            //act
            var span = data.AsSpan<Position>();
            {
                ref var p0 = ref span[0];
                p0.X = 10;
                p0.Y = 20;
            }

            data.Reset(0);

            //assert
            var readspan = data.AsSpan<Position>();
            {
                ref var p0 = ref readspan[0];
                p0.X.ShouldBe(0);
                p0.Y.ShouldBe(0);
                p0.X.ShouldBe(0);
                p0.Y.ShouldBe(0);
            }
        }

        [Fact]
        public void ShouldThrowOnWrongType()
        {
            //arrange
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator = new DynamicAllocator(_logFactory);
            using var data = new ComponentDataArray(_logFactory, allocator, componentType, 32);

            //act

            //assert
            Should.Throw<Exception>(() =>
            {
                var span = data.AsSpan<Velocity>();
            });
        }

        [Fact]
        public void ShouldMoveData()
        {
            //arrange
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator = new DynamicAllocator(_logFactory);
            using var data = new ComponentDataArray(_logFactory, allocator, componentType, 32);

            //act
            var span = data.AsSpan<Position>();
            {
                ref var p0 = ref span[1];
                p0.X = 10;
                p0.Y = 20;
            }

            data.Move(1, 0);

            var readspan = data.AsSpan<Position>();
            ref var p1 = ref readspan[0];

            //assert
            p1.X.ShouldBe(10);
            p1.Y.ShouldBe(20);
            p1.X.ShouldBe(p1.X);
            p1.Y.ShouldBe(p1.Y);
        }


        [Fact]
        public unsafe void ShouldCopyVoidPtr()
        {
            //arrange
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator = new DynamicAllocator(_logFactory);
            using var data = new ComponentDataArray(_logFactory, allocator, componentType, 32);

            var span = data.AsSpan<Position>();
            var ptr = stackalloc[] { new Position(100, 100), new Position(200, 200), new Position(400, 100), new Position(100, 400) };
            var src = (void*)ptr;

            //act
            data.Copy(ref src, 0, 4, false);

            //assert
            span[0].X.ShouldBe(100);
            span[0].Y.ShouldBe(100);
            span[1].X.ShouldBe(200);
            span[1].Y.ShouldBe(200);
            span[2].X.ShouldBe(400);
            span[2].Y.ShouldBe(100);
            span[3].X.ShouldBe(100);
            span[3].Y.ShouldBe(400);
        }
    }
}