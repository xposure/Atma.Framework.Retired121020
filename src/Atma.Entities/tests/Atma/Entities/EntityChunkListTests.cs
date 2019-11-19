namespace Atma.Entities
{
    using System;
    using System.Linq;
    using Atma.Memory;
    using Divergic.Logging.Xunit;
    using Microsoft.Extensions.Logging;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public class EntityChunkListTests
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

        public EntityChunkListTests(ITestOutputHelper output)
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

            using var chunkArray = new EntityChunkList(_logFactory, memory, specifcation, 0);
            using var entityPool = new EntityPool(_logFactory, memory);

            var id0 = entityPool.TakeRef();
            var id1 = entityPool.TakeRef();

            //act
            chunkArray.Create(id0, out var chunkIndex);
            chunkArray.Create(id1, out chunkIndex);

            //assert
            chunkArray.EntityCount.ShouldBe(2);
            id0.Index.ShouldBe(0);
            id1.Index.ShouldBe(1);
            chunkIndex.ShouldBe(0);
        }

        [Fact]
        public unsafe void ShouldCreateManyEntities()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specifcation = new EntitySpec(ComponentType<Position>.Type);

            using var chunkArray = new EntityChunkList(_logFactory, memory, specifcation, 0);
            using var entityPool = new EntityPool(_logFactory, memory);

            var entities = new EntityRef[Entity.ENTITY_MAX * 2];
            entityPool.Take(entities);

            //act
            chunkArray.Create(entities);

            // //assert
            // var created = createdEntities.ToArray();
            // var first = created.Take(Entity.ENTITY_MAX).ToArray();
            // var firstSet = Enumerable.Range(0, Entity.ENTITY_MAX).Select(x => new CreatedEntity(0, x)).ToArray();
            // first.ShouldBe(firstSet);

            // var second = created.Skip(Entity.ENTITY_MAX).Take(Entity.ENTITY_MAX).ToArray();
            // var secondSet = Enumerable.Range(0, Entity.ENTITY_MAX).Select(x => new CreatedEntity(1, x)).ToArray();
            // second.ShouldBe(secondSet);

            chunkArray.ChunkCount.ShouldBe(2);
            chunkArray.EntityCount.ShouldBe(entities.Length);
        }


        [Fact]
        public void ShouldDeleteAndMove()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specifcation = new EntitySpec(
                ComponentType<Position>.Type
            );

            using var chunkArray = new EntityChunkList(_logFactory, memory, specifcation, 0);
            using var entityPool = new EntityPool(_logFactory, memory);

            var id0 = entityPool.TakeRef();
            var id1 = entityPool.TakeRef();

            //act
            chunkArray.Create(id0, out var chunkIndex0);
            chunkArray.Create(id1, out var chunkIndex1);
            var span = chunkArray.AllChunks[chunkIndex0].PackedArray.GetComponentData<Position>();
            span[id1.Index] = new Position(10, 20);
            chunkArray.Delete(id0, entityPool);

            //assert
            span[id0.Index].X.ShouldBe(10);
            span[id0.Index].Y.ShouldBe(20);

        }



        // [Fact]
        // public void ShouldExpand()
        // {
        //     //arrange
        //     using var memory = new DynamicAllocator(_logFactory);
        //     var specifcation = new EntitySpec(
        //         ComponentType<Position>.Type
        //     );

        //     using var chunkArray = new EntityChunkList(_logFactory, memory, specifcation);

        //     //act
        //     for (var i = 0; i < Entity.ENTITY_MAX + 1; i++)
        //         chunkArray.Create(1, out var chunkIndex);

        //     //assert
        //     chunkArray.Capacity.ShouldBe(Entity.ENTITY_MAX * 2);
        //     chunkArray.ChunkCount.ShouldBe(2);
        //     chunkArray.EntityCount.ShouldBe(Entity.ENTITY_MAX + 1);
        //     chunkArray.Free.ShouldBe(Entity.ENTITY_MAX - 1);
        // }

        [Fact]
        public unsafe void ShoudlCopyPtrData()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specifcation = new EntitySpec(
                ComponentType<Position>.Type
            );

            var specIndex = 0;
            using var chunkArray = new EntityChunkList(_logFactory, memory, specifcation, specIndex);
            using var entityPool = new EntityPool(_logFactory, memory);

            var entity = entityPool.TakeRef(); ;
            chunkArray.Create(entity, out var chunkIndex);
            var componentType = stackalloc[] { ComponentType<Position>.Type };
            var componentIndex = chunkArray.Specification.GetComponentIndex(*componentType);
            var span = chunkArray.AllChunks[chunkIndex].PackedArray.GetComponentData<Position>();

            //act
            var ptr = stackalloc[] { new Position(100, 100), new Position(200, 200), new Position(400, 100), new Position(100, 400) };
            var src = (void*)ptr;

            Span<EntityRef> entityRefs = stackalloc[] { entity };
            chunkArray.Copy(specIndex, componentType, ref src, entityRefs, false);

            //assert
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

            var specIndex = 0;
            using var chunkArray = new EntityChunkList(_logFactory, memory, specifcation, specIndex);
            using var entityPool = new EntityPool(_logFactory, memory);
            var entity0 = entityPool.TakeRef();
            var entity1 = entityPool.TakeRef();
            var entity2 = entityPool.TakeRef();
            var entity3 = entityPool.TakeRef();
            chunkArray.Create(entity0, out var chunkIndex);
            chunkArray.Create(entity1, out var _);
            chunkArray.Create(entity2, out var _);
            chunkArray.Create(entity3, out var _);

            var componentType = stackalloc[] { ComponentType<Position>.Type };
            var componentIndex = chunkArray.Specification.GetComponentIndex(*componentType);
            var span = chunkArray.AllChunks[chunkIndex].PackedArray.GetComponentData<Position>();

            var ptr = stackalloc[] { new Position(100, 100), new Position(200, 200), new Position(400, 100), new Position(100, 400) };
            var src = (void*)ptr;

            Span<EntityRef> entityRefs = stackalloc[] {
                entity0,
                entity1,
                //swapping 2 and 3 to see if we can resume correctly
                entity3,
                entity2,
            };
            //act
            chunkArray.Copy(specIndex, componentType, ref src, entityRefs, true);

            //assert
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