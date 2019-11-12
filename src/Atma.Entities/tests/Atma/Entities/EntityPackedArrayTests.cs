namespace Atma.Entities
{
    using System;
    using Atma.Common;
    using Atma.Memory;
    using Shouldly;
    using Xunit;

    public class EntityGroupArrayTests
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

        [Fact]
        public void CanCreateEntityGroup()
        {
            //arrange
            using var memory = new DynamicAllocator();
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            using var entityGroup = new EntityPackedArray(memory, specification);

            //act
            var positions = entityGroup.GetComponentSpan<Position>();
            var velocities = entityGroup.GetComponentSpan<Velocity>();

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
        public void ShouldThrowOnWrongType()
        {
            //arrange
            using var memory = new DynamicAllocator();
            var specification = new EntitySpec(
                ComponentType<Position>.Type
            );

            using var entityGroup = new EntityPackedArray(memory, specification);

            //act
            //assert
            Should.Throw<Exception>(() =>
            {
                var velocities = entityGroup.GetComponentSpan<Velocity>();
            });
        }


        //test is probably pointless, its testing Span more than anything else
        [Fact]
        public void ShouldReadAndWriteEntity()
        {
            //arrange
            using var memory = new DynamicAllocator();
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            using var entityGroup = new EntityPackedArray(memory, specification);

            //act
            var positions = entityGroup.GetComponentSpan<Position>();
            var velocities = entityGroup.GetComponentSpan<Velocity>();
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
            var positions1 = entityGroup.GetComponentSpan<Position>();
            var velocities1 = entityGroup.GetComponentSpan<Velocity>();
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
        public void ShouldMoveEntity()
        {
            //arrange
            using var memory = new DynamicAllocator();
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            using var entityGroup = new EntityPackedArray(memory, specification);

            //act
            var positions = entityGroup.GetComponentSpan<Position>();
            var velocities = entityGroup.GetComponentSpan<Velocity>();
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
            var positions1 = entityGroup.GetComponentSpan<Position>();
            var velocities1 = entityGroup.GetComponentSpan<Velocity>();
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