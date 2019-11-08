namespace Atma.Entities
{
    using System;
    using Shouldly;

    public class EntityArchetypeTests
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

        public void ShouldCreateArchetype()
        {
            //arrange
            var specifcation = new EntitySpecification(
                ComponentType<Position>.Type
            );

            var archetype = new EntityArchetype2(specifcation);

            //act
            var index0 = archetype.Create(out var chunkIndex);
            var index1 = archetype.Create(out chunkIndex);


            //assert
            index0.ShouldBe(0);
            index1.ShouldBe(1);
        }

        public void ShouldExpand()
        {
            //arrange
            var specifcation = new EntitySpecification(
                ComponentType<Position>.Type
            );

            var archetype = new EntityArchetype2(specifcation);

            //act
            for (var i = 0; i < Entity.ENTITY_MAX + 1; i++)
                archetype.Create(out var chunkIndex);

            //assert
            archetype.Capacity.ShouldBe(Entity.ENTITY_MAX * 2);
            archetype.ChunkCount.ShouldBe(2);
            archetype.EntityCount.ShouldBe(Entity.ENTITY_MAX + 1);
            archetype.Free.ShouldBe(Entity.ENTITY_MAX - 1);
        }
    }
}