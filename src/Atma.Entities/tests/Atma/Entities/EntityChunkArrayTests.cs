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

        public void ShouldCreateChunkArray()
        {
            //arrange
            var specifcation = new EntitySpec(
                ComponentType<Position>.Type
            );

            var chunkArray = new EntityChunkArray(specifcation);

            //act
            var index0 = chunkArray.Create(out var chunkIndex);
            var index1 = chunkArray.Create(out chunkIndex);

            //assert
            index0.ShouldBe(0);
            index1.ShouldBe(1);
            chunkIndex.ShouldBe(0);
        }


        public void ShouldDeleteAndMove()
        {
            //arrange
            var specifcation = new EntitySpec(
                ComponentType<Position>.Type
            );

            var chunkArray = new EntityChunkArray(specifcation);

            //act
            var index0 = chunkArray.Create(out var chunkIndex0);
            var index1 = chunkArray.Create(out var chunkIndex1);
            var span = chunkArray.AllChunks[chunkIndex0].PackedArray.GetComponentSpan<Position>();
            span[index1] = new Position(10, 20);
            chunkArray.Delete(chunkIndex0, index0);

            //assert
            span[index0].X.ShouldBe(10);
            span[index0].Y.ShouldBe(20);

        }



        public void ShouldExpand()
        {
            //arrange
            var specifcation = new EntitySpec(
                ComponentType<Position>.Type
            );

            var chunkArray = new EntityChunkArray(specifcation);

            //act
            for (var i = 0; i < Entity.ENTITY_MAX + 1; i++)
                chunkArray.Create(out var chunkIndex);

            //assert
            chunkArray.Capacity.ShouldBe(Entity.ENTITY_MAX * 2);
            chunkArray.ChunkCount.ShouldBe(2);
            chunkArray.EntityCount.ShouldBe(Entity.ENTITY_MAX + 1);
            chunkArray.Free.ShouldBe(Entity.ENTITY_MAX - 1);
        }

        public void ShouldMoveToAnotherArray()
        {
            //arrange
            var specifcation = new EntitySpec(
                ComponentType<Position>.Type
            );

            var srcArray = new EntityChunkArray(specifcation);
            var dstArray = new EntityChunkArray(specifcation);

            //act
            var srcIndex = srcArray.Create(out var chunkIndex);
            Console.WriteLine(chunkIndex);
            var srcSpan = srcArray.AllChunks[chunkIndex].PackedArray.GetComponentSpan<Position>();
            srcSpan[srcIndex] = new Position(10, 20);

            var tempIndex = dstArray.Create(out var tempChunkIndex); //pushing the datain dst to check that it inserted correctly
            EntityChunkArray.MoveTo(srcArray, chunkIndex, srcIndex, dstArray, out var dstChunkIndex, out var dstIndex);

            //assert
            chunkIndex.ShouldBe(0);
            srcIndex.ShouldBe(0);
            srcArray.EntityCount.ShouldBe(0);

            dstChunkIndex.ShouldBe(0);
            dstIndex.ShouldBe(1);
            dstArray.EntityCount.ShouldBe(2);
            var dstSpan = dstArray.AllChunks[dstChunkIndex].PackedArray.GetComponentSpan<Position>();
            dstSpan[dstIndex].X.ShouldBe(10);
            dstSpan[dstIndex].Y.ShouldBe(20);

        }
    }
}