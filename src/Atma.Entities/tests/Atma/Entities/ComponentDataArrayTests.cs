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
        }

        private struct Velocity
        {
            public int VX;
            public int VY;
        }

        public void ShouldGetIndex()
        {
            //arrange
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator = new StackAllocator(1024, clearToZero: true);
            using var data = new ComponentDataArray2(allocator, componentType, 32);

            //act
            using var writeLock = data.AsSpan<Position>(out var span);
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
            using var writeLock = data.AsSpan<Position>(out var span);
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
                using var writeLock = data.AsSpan<Velocity>(out var span);
            });
        }

        public void ShouldMoveData()
        {
            //arrange
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator = new StackAllocator(1024, clearToZero: true);
            using var data = new ComponentDataArray2(allocator, componentType, 32);

            //act
            using (var writeLock = data.AsSpan<Position>(out var span))
            {
                ref var p0 = ref span[1];
                p0.X = 10;
                p0.Y = 20;
            }

            data.Move(1, 0);

            using var readLock = data.AsReadOnlySpan<Position>(out var readspan);
            ref readonly var p1 = ref readspan[0];

            //assert
            p1.X.ShouldBe(10);
            p1.Y.ShouldBe(20);
            p1.X.ShouldBe(p1.X);
            p1.Y.ShouldBe(p1.Y);
        }

        public void ShouldThrowsOnMultipleWriteLocks()
        {
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator = new StackAllocator(1024, clearToZero: true);
            using var data = new ComponentDataArray2(allocator, componentType, 32);

            //act
            using var writeLock0 = data.AsSpan<Position>(out var span1);

            //assert
            Should.Throw<Exception>(() => data.AsSpan<Position>(out var span2));
        }

        public void ShouldThrowOnThreadRecusionLock()
        {
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator = new StackAllocator(1024, clearToZero: true);
            using var data = new ComponentDataArray2(allocator, componentType, 32);

            //act
            using var readLock0 = data.AsReadOnlySpan<Position>(out var span0);

            //assert
            Should.Throw<Exception>(() => data.AsReadOnlySpan<Position>(out var span1));
        }

        public void ShouldAllowMultipleThreadRead()
        {
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator = new StackAllocator(1024, clearToZero: true);
            using var data = new ComponentDataArray2(allocator, componentType, 32);

            //act
            using var readLock0 = data.AsReadOnlySpan<Position>(out var span0);

            //assert
            var didThrow = false;
            var task = new Thread(() =>
            {
                try
                {
                    using var readLock1 = data.AsReadOnlySpan<Position>(out var span1);
                }
                catch
                {
                    didThrow = true;
                }
            });

            task.Start();
            task.Join();

            didThrow.ShouldBe(false);
        }

        public void ShouldThrowOnWriteWhileOnRead()
        {
            var componentType = ComponentType<Position>.Type;
            using IAllocator allocator = new StackAllocator(1024, clearToZero: true);
            using var data = new ComponentDataArray2(allocator, componentType, 32);

            //act
            using var readLock0 = data.AsReadOnlySpan<Position>(out var span0);

            //assert
            var didThrow = false;
            var task = new Thread(() =>
            {
                try
                {
                    using var readLock1 = data.AsSpan<Position>(out var span1);
                }
                catch
                {
                    didThrow = true;
                }
            });

            task.Start();
            task.Join();

            didThrow.ShouldBe(true);
        }
    }
}