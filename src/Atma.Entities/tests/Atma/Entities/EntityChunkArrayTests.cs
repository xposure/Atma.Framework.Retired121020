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

        [Fact]
        public unsafe void ShoudlCopyPtrData()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specifcation = new EntitySpec(
                ComponentType<Position>.Type
            );

            using var chunkArray = new EntityChunkArray(_logFactory, memory, specifcation);
            var entity = 1u;
            var specIndex = 0;
            var index = chunkArray.Create(entity, out var chunkIndex);
            var componentType = stackalloc[] { ComponentType<Position>.Type };
            var componentIndex = chunkArray.Specification.GetComponentIndex(*componentType);
            var span = chunkArray.AllChunks[chunkIndex].PackedArray.GetComponentSpan<Position>();

            var ptr = stackalloc[] { new Position(100, 100), new Position(200, 200), new Position(400, 100), new Position(100, 400) };
            var src = (void*)ptr;

            using var entities = new NativeArray<Entity>(memory, 4);
            entities[0] = new Entity(entity, specIndex, chunkIndex, index);

            //act
            var remaining = chunkArray.Copy(componentType, ref src, entities.Slice());

            //assert
            remaining.Length.ShouldBe(3);
            span[0].X.ShouldBe(100);
            span[0].Y.ShouldBe(100);
        }

        [Fact]
        public unsafe void ShouldCopyMoreThanOne()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specifcation = new EntitySpec(
                ComponentType<Position>.Type
            );

            using var chunkArray = new EntityChunkArray(_logFactory, memory, specifcation);
            var entity0 = 1u;
            var entity1 = 2u;
            var entity2 = 3u;
            var entity3 = 4u;
            var specIndex = 0;
            var index0 = chunkArray.Create(entity0, out var chunkIndex);
            var index1 = chunkArray.Create(entity1, out var _);
            var index2 = chunkArray.Create(entity2, out var _);
            var index3 = chunkArray.Create(entity3, out var _);

            var componentType = stackalloc[] { ComponentType<Position>.Type };
            var componentIndex = chunkArray.Specification.GetComponentIndex(*componentType);
            var span = chunkArray.AllChunks[chunkIndex].PackedArray.GetComponentSpan<Position>();

            var ptr = stackalloc[] { new Position(100, 100), new Position(200, 200), new Position(400, 100), new Position(100, 400) };
            var src = (void*)ptr;

            using var entities = new NativeArray<Entity>(memory, 4);
            entities[0] = new Entity(entity0, specIndex, chunkIndex, index0);
            entities[1] = new Entity(entity1, specIndex, chunkIndex, index1);

            //swapping 2 and 3 to see if we can resume correctly
            entities[2] = new Entity(entity3, specIndex, chunkIndex, index3);
            entities[3] = new Entity(entity2, specIndex, chunkIndex, index2);

            //act
            var slice = entities.Slice();
            var attempts = 4;//make sure we don't get in a inifinite loop
            while (slice.Length > 0 && attempts-- > 0)
            {
                slice = chunkArray.Copy(componentType, ref src, slice);
            }

            //assert
            slice.Length.ShouldBe(0);
            attempts.ShouldBe(1);
            span[0].X.ShouldBe(100);
            span[0].Y.ShouldBe(100);
            span[1].X.ShouldBe(200);
            span[1].Y.ShouldBe(200);
            span[3].X.ShouldBe(400);
            span[3].Y.ShouldBe(100);
            span[2].X.ShouldBe(100);
            span[2].Y.ShouldBe(400);
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