namespace Atma.Entities
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Atma.Memory;
    using Shouldly;

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

        public void ShouldGetIndex()
        {
            //arrange
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator = new StackAllocator(1024, clearToZero: true);
            using var data = new ComponentDataArray2(allocator, componentType, 32);

            //act
            var span = data.AsSpan<Position>();
            ref var p = ref span[0];

            //assert
            p.X.ShouldBe(0);
            p.Y.ShouldBe(0);
        }

        public void ShouldSetIndex()
        {
            //arrange
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator = new StackAllocator(1024, clearToZero: true);
            using var data = new ComponentDataArray2(allocator, componentType, 32);

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

        public void ShouldCopyToAnotherArray()
        {
            //arrange
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator0 = new StackAllocator(1024, clearToZero: true);
            using var data0 = new ComponentDataArray2(allocator0, componentType, 32);
            using IAllocator allocator1 = new StackAllocator(1024, clearToZero: true);
            using var data1 = new ComponentDataArray2(allocator0, componentType, 32);

            //act
            var span0 = data0.AsSpan<Position>();
            ref var p0 = ref span0[0];
            p0.X = 10;
            p0.Y = 20;

            ComponentDataArray2.CopyTo(data0, 0, data1, 1);

            //assert
            var span1 = data1.AsSpan<Position>();
            ref var p1 = ref span1[1];
            p1.X.ShouldBe(10);
            p1.Y.ShouldBe(20);
        }

        public void ShouldReset()
        {
            //arrange
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator = new StackAllocator(1024, clearToZero: true);
            using var data = new ComponentDataArray2(allocator, componentType, 32);

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

        public void ShouldThrowOnWrongType()
        {
            //arrange
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator = new StackAllocator(1024, clearToZero: true);
            using var data = new ComponentDataArray2(allocator, componentType, 32);

            //act

            //assert
            Should.Throw<Exception>(() =>
            {
                var span = data.AsSpan<Velocity>();
            });
        }

        public void ShouldMoveData()
        {
            //arrange
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator = new StackAllocator(1024, clearToZero: true);
            using var data = new ComponentDataArray2(allocator, componentType, 32);

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
    }
}