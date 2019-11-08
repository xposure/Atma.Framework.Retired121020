namespace Atma.Entities
{
    using System;
    using Shouldly;

    public class EntityChunkTests
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

        public void ShouldCreateEntity()
        {
            //arrange
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            var entityChunk = new EntityChunk(specification);

            //act
            var free = entityChunk.Free;
            var index = entityChunk.Create();

            //assert
            index.ShouldBe(0);
            entityChunk.Count.ShouldBe(1);
            entityChunk.Free.ShouldBe(free - 1);
        }

        public void ShouldDeleteEntity()
        {
            //arrange
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            var entityChunk = new EntityChunk(specification);

            //act
            var free = entityChunk.Free;
            var index = entityChunk.Create();
            entityChunk.Delete(index);

            //assert
            entityChunk.Count.ShouldBe(0);
            entityChunk.Free.ShouldBe(free);
        }
    }
}