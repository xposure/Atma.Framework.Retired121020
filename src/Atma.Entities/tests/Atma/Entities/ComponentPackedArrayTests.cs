namespace Atma.Entities
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Atma.Common;
    using Atma.Memory;
    using Divergic.Logging.Xunit;
    using Microsoft.Extensions.Logging;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public class ComponentPackedArrayTests
    {
        [System.Diagnostics.DebuggerStepThrough]
        private struct Position : IEquatable<Position>
        {
            public int X;
            public int Y;

            public Position(int v)
            {
                X = v;
                Y = v;
            }

            public Position(int x, int y)
            {
                X = x;
                Y = y;
            }

            public bool Equals([AllowNull] Position other) => X == other.X && Y == other.Y;

            public override string ToString() => $"{{ X: {X}, Y: {Y} }}";
        }


        [System.Diagnostics.DebuggerStepThrough]
        private struct Velocity : IEquatable<Velocity>
        {
            public int VX;
            public int VY;

            public Velocity(int v)
            {
                VX = v;
                VY = v;
            }

            public Velocity(int vx, int vy)
            {
                VX = vx;
                VY = vy;
            }


            public bool Equals([AllowNull] Velocity other) => VX == other.VX && VY == other.VY;

            public override string ToString() => $"{{ VX: {VX}, VY: {VY} }}";
        }

        private readonly ILoggerFactory _logFactory;

        public ComponentPackedArrayTests(ITestOutputHelper output)
        {
            _logFactory = LogFactory.Create(output);
        }


        [Fact]
        public void CanCreatePackedArray()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            using var entityGroup = new ComponentPackedArray(_logFactory, memory, specification);

            //act
            var positions = entityGroup.GetComponentData<Position>();
            var velocities = entityGroup.GetComponentData<Velocity>();

            //assert
            for (var i = 0; i < entityGroup.Length; i++)
            {
                ref var p = ref positions[i];
                ref var v = ref velocities[i];
                p.X.ShouldBe(0);
                p.Y.ShouldBe(0);
                v.VX.ShouldBe(0);
                v.VY.ShouldBe(0);
            }
        }

        [Fact]
        public void ShouldReset()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            using var packedArray = new ComponentPackedArray(_logFactory, memory, specification);

            //act
            var positions = packedArray.GetComponentData<Position>();
            var velocities = packedArray.GetComponentData<Velocity>();
            positions[0] = new Position(1);
            velocities[0] = new Velocity(2);
            positions[1] = new Position(3);
            velocities[1] = new Velocity(4);

            packedArray.Reset(0);
            packedArray.Reset<Position>(1);

            //assert
            positions[0].ShouldBe(new Position(0));
            velocities[0].ShouldBe(new Velocity(0));

            positions[1].ShouldBe(new Position(0));
            velocities[1].ShouldBe(new Velocity(4));
        }

        [Fact]
        public void ShouldThrowOnWrongType()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specification = new EntitySpec(ComponentType<Position>.Type);

            using var packedArray = new ComponentPackedArray(_logFactory, memory, specification);

            //act

            //assert
            Should.Throw<Exception>(() =>
            {
                var velocities = packedArray.GetComponentData<Velocity>();
            });
        }

        public void ShouldGetComponentIndex()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specification = new EntitySpec(ComponentType<Position>.Type, ComponentType<Velocity>.Type);

            using var packedArray = new ComponentPackedArray(_logFactory, memory, specification);

            //act
            var p0 = packedArray.GetComponentIndex<Position>();
            var v0 = packedArray.GetComponentIndex<Velocity>();

            //assert
            p0.ShouldBe(0);
            v0.ShouldBe(1);
        }

        //test is probably pointless, its testing Span more than anything else
        [Fact]
        public void ShouldReadAndWriteEntity()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            using var entityGroup = new ComponentPackedArray(_logFactory, memory, specification);

            //act
            var positions = entityGroup.GetComponentData<Position>();
            var velocities = entityGroup.GetComponentData<Velocity>();
            {
                for (var i = 0; i < entityGroup.Length; i++)
                {
                    ref var p = ref positions[i];
                    ref var v = ref velocities[i];
                    p.X = i;
                    p.Y = i + 1;
                    v.VX = i + 2;
                    v.VY = i + 3;
                }
            }

            //assert
            var positions1 = entityGroup.GetComponentData<Position>();
            var velocities1 = entityGroup.GetComponentData<Velocity>();
            {
                for (var i = 0; i < entityGroup.Length; i++)
                {
                    ref var p = ref positions1[i];
                    ref var v = ref velocities1[i];
                    p.X.ShouldBe(i);
                    p.Y.ShouldBe(i + 1);
                    v.VX.ShouldBe(i + 2);
                    v.VY.ShouldBe(i + 3);
                }
            }
        }


        [Fact]
        public unsafe void ShouldCopyPtrArrayToComponent()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            using var packedArray = new ComponentPackedArray(_logFactory, memory, specification);
            var componentIndex = packedArray.GetComponentIndex<Position>();
            var span = packedArray.GetComponentData<Position>();

            var ptr = stackalloc[] { new Position(100, 100), new Position(200, 200), new Position(400, 100), new Position(100, 400) };
            var src = (void*)ptr;

            //act
            packedArray.Copy(componentIndex, ref src, 0, 4, true);

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

        [Fact]
        public unsafe void ShouldCopyPtrToComponent()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            using var packedArray = new ComponentPackedArray(_logFactory, memory, specification);
            var componentIndex = packedArray.GetComponentIndex<Position>();
            var span = packedArray.GetComponentData<Position>();

            var ptr = stackalloc[] { new Position(100, 100) };
            var src = (void*)ptr;

            //act
            packedArray.Copy(componentIndex, ref src, 0, 4, false);

            //assert
            span[0].X.ShouldBe(100);
            span[0].Y.ShouldBe(100);
            span[1].X.ShouldBe(100);
            span[1].Y.ShouldBe(100);
            span[2].X.ShouldBe(100);
            span[2].Y.ShouldBe(100);
            span[3].X.ShouldBe(100);
            span[3].Y.ShouldBe(100);
        }

        [Fact]
        public void ShouldMoveEntity()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            using var entityGroup = new ComponentPackedArray(_logFactory, memory, specification);

            //act
            var positions = entityGroup.GetComponentData<Position>();
            var velocities = entityGroup.GetComponentData<Velocity>();
            {
                ref var p = ref positions[1];
                ref var v = ref velocities[1];
                p.X = 10;
                p.Y = 20;
                v.VX = 30;
                v.VY = 40;
            }

            entityGroup.Move(1, 0);

            //assert
            var positions1 = entityGroup.GetComponentData<Position>();
            var velocities1 = entityGroup.GetComponentData<Velocity>();
            {
                ref var p = ref positions1[0];
                ref var v = ref velocities1[0];
                p.X.ShouldBe(10);
                p.Y.ShouldBe(20);
                v.VX.ShouldBe(30);
                v.VY.ShouldBe(40);
            }
        }
    }
}