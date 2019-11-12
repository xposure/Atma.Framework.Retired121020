namespace Atma.Entities
{
    using System;
    using Atma.Memory;
    using Divergic.Logging.Xunit;
    using Microsoft.Extensions.Logging;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public class EntityChunkArrayTests
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

        public EntityChunkArrayTests(ITestOutputHelper output)
        {
            _logFactory = LogFactory.Create(output);
        }
        [Fact]
        public void ShouldCreateChunkArray()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specifcation = new EntitySpec(
                ComponentType<Position>.Type
            );

            using var chunkArray = new EntityChunkArray(_logFactory, memory, specifcation);

            //act
            var index0 = chunkArray.Create(1, out var chunkIndex);
            var index1 = chunkArray.Create(2, out chunkIndex);

            //assert
            index0.ShouldBe(0);
            index1.ShouldBe(1);
            chunkIndex.ShouldBe(0);
        }


        [Fact]
        public void ShouldDeleteAndMove()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specifcation = new EntitySpec(
                ComponentType<Position>.Type
            );

            using var chunkArray = new EntityChunkArray(_logFactory, memory, specifcation);

            //act
            var index0 = chunkArray.Create(1, out var chunkIndex0);
            var index1 = chunkArray.Create(2, out var chunkIndex1);
            var span = chunkArray.AllChunks[chunkIndex0].PackedArray.GetComponentSpan<Position>();
            span[index1] = new Position(10, 20);
            chunkArray.Delete(chunkIndex0, index0);

            //assert
            span[index0].X.ShouldBe(10);
            span[index0].Y.ShouldBe(20);

        }



        [Fact]
        public void ShouldExpand()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specifcation = new EntitySpec(
                ComponentType<Position>.Type
            );

            using var chunkArray = new EntityChunkArray(_logFactory, memory, specifcation);

            //act
            for (var i = 0; i < Entity.ENTITY_MAX + 1; i++)
                chunkArray.Create(1, out var chunkIndex);

            //assert
            chunkArray.Capacity.ShouldBe(Entity.ENTITY_MAX * 2);
            chunkArray.ChunkCount.ShouldBe(2);
            chunkArray.EntityCount.ShouldBe(Entity.ENTITY_MAX + 1);
            chunkArray.Free.ShouldBe(Entity.ENTITY_MAX - 1);
        }

        // public void ShouldMoveToAnotherArray()
        // {
        //     //arrange
        //     var specifcation = new EntitySpec(
        //         ComponentType<Position>.Type
        //     );

        //     var srcArray = new EntityChunkArray(_logFactory, specifcation);
        //     var dstArray = new EntityChunkArray(_logFactory, specifcation);

        //     //act
        //     var srcIndex = srcArray.Create(1, out var chunkIndex);
        //     Console.WriteLine(chunkIndex);
        //     var srcSpan = srcArray.AllChunks[chunkIndex].PackedArray.GetComponentSpan<Position>();
        //     srcSpan[srcIndex] = new Position(10, 20);

        //     var tempIndex = dstArray.Create(2, out var tempChunkIndex); //pushing the datain dst to check that it inserted correctly
        //     EntityChunkArray.MoveTo(1, srcArray, chunkIndex, srcIndex, dstArray, out var dstChunkIndex, out var dstIndex);

        //     //assert
        //     chunkIndex.ShouldBe(0);
        //     srcIndex.ShouldBe(0);
        //     srcArray.EntityCount.ShouldBe(0);

        //     dstChunkIndex.ShouldBe(0);
        //     dstIndex.ShouldBe(1);
        //     dstArray.EntityCount.ShouldBe(2);
        //     var dstSpan = dstArray.AllChunks[dstChunkIndex].PackedArray.GetComponentSpan<Position>();
        //     dstSpan[dstIndex].X.ShouldBe(10);
        //     dstSpan[dstIndex].Y.ShouldBe(20);

        // }
    }
}