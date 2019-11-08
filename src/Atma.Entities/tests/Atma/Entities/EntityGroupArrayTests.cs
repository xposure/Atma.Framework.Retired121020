namespace Atma.Entities
{
    using System;
    using Atma.Common;
    using Shouldly;

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

        public void CanCreateEntityGroup()
        {
            //arrange
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            var entityGroup = new EntityGroupArray(specification);

            //act
            using var lockPositions = entityGroup.ReadComponent<Position>(out var positions);
            using var lockVelocities = entityGroup.ReadComponent<Velocity>(out var velocities);

            //assert
            for (var i = 0; i < entityGroup.Length; i++)
            {
                ref readonly var p = ref positions[i];
                ref readonly var v = ref velocities[i];
                p.X.ShouldBe(0);
                p.Y.ShouldBe(0);
                v.VX.ShouldBe(0);
                v.VY.ShouldBe(0);
            }
        }

        public void ShouldThrowOnWrongType()
        {
            //arrange
            var specification = new EntitySpec(
                ComponentType<Position>.Type
            );

            var entityGroup = new EntityGroupArray(specification);

            //act
            //assert
            Should.Throw<Exception>(() =>
            {
                using var lockVelocities = entityGroup.ReadComponent<Velocity>(out var velocities);
            });
        }


        //test is probably pointless, its testing Span more than anything else
        public void ShouldReadAndWriteEntity()
        {
            //arrange
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            var entityGroup = new EntityGroupArray(specification);

            //act
            using (var lockPositions = entityGroup.WriteComponent<Position>(out var positions))
            using (var lockVelocities = entityGroup.WriteComponent<Velocity>(out var velocities))
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
            using (var lockPositions = entityGroup.ReadComponent<Position>(out var positions1))
            using (var lockVelocities = entityGroup.ReadComponent<Velocity>(out var velocities1))
            {
                for (var i = 0; i < entityGroup.Length; i++)
                {
                    ref readonly var p = ref positions1[i];
                    ref readonly var v = ref velocities1[i];
                    p.X.ShouldBe(i);
                    p.Y.ShouldBe(i + 1);
                    v.VX.ShouldBe(i + 2);
                    v.VY.ShouldBe(i + 3);
                }
            }
        }

        public void ShouldMoveEntity()
        {
            //arrange
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            var entityGroup = new EntityGroupArray(specification);

            //act
            using (var lockPositions = entityGroup.WriteComponent<Position>(out var positions))
            using (var lockVelocities = entityGroup.WriteComponent<Velocity>(out var velocities))
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
            using (var lockPositions = entityGroup.ReadComponent<Position>(out var positions1))
            using (var lockVelocities = entityGroup.ReadComponent<Velocity>(out var velocities1))
            {
                ref readonly var p = ref positions1[0];
                ref readonly var v = ref velocities1[0];
                p.X.ShouldBe(10);
                p.Y.ShouldBe(20);
                v.VX.ShouldBe(30);
                v.VY.ShouldBe(40);
            }
        }
    }
}