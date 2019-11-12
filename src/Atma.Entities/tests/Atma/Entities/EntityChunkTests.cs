namespace Atma.Entities
{
    using System;
    using Atma.Memory;
    using Shouldly;
    using Xunit;

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

        [Fact]
        public void ShouldCreateEntity()
        {
            //arrange
            using var memory = new DynamicAllocator();
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            using var entityChunk = new EntityChunk(memory, specification);

            //act
            var free = entityChunk.Free;
            var index = entityChunk.Create(1);

            //assert
            index.ShouldBe(0);
            entityChunk.GetEntity(index).ShouldBe(1u);
            entityChunk.Count.ShouldBe(1);
            entityChunk.Free.ShouldBe(free - 1);
        }

        [Fact]
        public void ShouldDeleteEntity()
        {
            //arrange
            using var memory = new DynamicAllocator();
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            using var entityChunk = new EntityChunk(memory, specification);

            //act
            var free = entityChunk.Free;
            var index0 = entityChunk.Create(1);
            var index1 = entityChunk.Create(2);
            entityChunk.Delete(index0);

            //assert
            entityChunk.Count.ShouldBe(1);
            entityChunk.Free.ShouldBe(free - 1);
            entityChunk.GetEntity(0).ShouldBe(2u);
        }

        [Fact]
        public void ShouldCopyToOtherChunk()
        {
            //arrange
            using var memory = new DynamicAllocator();
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            using var chunk0 = new EntityChunk(memory, specification);
            using var chunk1 = new EntityChunk(memory, specification);


            //act
            var free = chunk0.Free;
            var index = chunk0.Create(1);

            //assert
            index.ShouldBe(0);
            chunk0.Count.ShouldBe(1);
            chunk0.Free.ShouldBe(free - 1);
        }
    }
}