namespace Atma.Entities
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Atma.Memory;
    using Divergic.Logging.Xunit;
    using Microsoft.Extensions.Logging;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public class EntityChunkTests
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

        public EntityChunkTests(ITestOutputHelper output)
        {
            _logFactory = LogFactory.Create(output);
        }

        [Fact]
        public void ShouldCreateOneEntity()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            using var entityChunk = new EntityChunk(_logFactory, memory, specification);

            //act
            var free = entityChunk.Free;
            var index = entityChunk.Create(1);

            //assert
            index.ShouldBe(0);
            entityChunk.Get(index).ShouldBe(1u);
            entityChunk.Count.ShouldBe(1);
            entityChunk.Free.ShouldBe(free - 1);
        }

        public unsafe void ShouldCreateManyEntities()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specification = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            using var entityChunk = new EntityChunk(_logFactory, memory, specification);

            var stackIds = stackalloc uint[entityChunk.Free + 1];
            var ids = new Span<uint>(stackIds, entityChunk.Free + 1);
            for (var i = 0; i < ids.Length; i++)
                ids[i] = (uint)i + 1u;


            //act
            var created = entityChunk.Create(ids);

            //assert
            entityChunk.Get(0).ShouldBe(1u);
            entityChunk.Get(created - 1).ShouldBe(ids[created - 1]);
            entityChunk.Count.ShouldBe(created);
            entityChunk.Free.ShouldBe(0);
            created.ShouldBe(Entity.ENTITY_MAX);
        }

        [Fact]
        public void ShouldDeleteOneEntity()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specification = new EntitySpec(ComponentType<Position>.Type);

            using var entityChunk = new EntityChunk(_logFactory, memory, specification);
            var span = entityChunk.PackedArray.GetComponentData<Position>(0);

            //act
            span[0] = new Position(1);
            span[1] = new Position(2);

            var free = entityChunk.Free;
            var index0 = entityChunk.Create(1);
            var index1 = entityChunk.Create(2);
            entityChunk.Delete(index0);

            //assert
            entityChunk.Count.ShouldBe(1);
            entityChunk.Free.ShouldBe(free - 1);
            entityChunk.Get(0).ShouldBe(2u);
            span[0].ShouldBe(new Position(2));
        }

        [Fact]
        public unsafe void ShouldDeleteManyEntities()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            var specification = new EntitySpec(ComponentType<Position>.Type);

            using var entityChunk = new EntityChunk(_logFactory, memory, specification);

            var stackIds = stackalloc uint[entityChunk.Free + 1];
            var ids = new Span<uint>(stackIds, entityChunk.Free + 1);
            for (var i = 0; i < ids.Length; i++)
                ids[i] = (uint)i + 1u;

            var created = entityChunk.Create(ids);

            var span = entityChunk.PackedArray.GetComponentData<Position>();
            for (var i = 0; i < span.Length; i++)
                span[i] = new Position(i + 1);

            //act
            var stackDeleteIndicies = stackalloc uint[128];
            var deleteIndicies = new Span<int>(stackDeleteIndicies, 128);
            for (var i = 0; i < deleteIndicies.Length; i++)
                deleteIndicies[i] = i + 128;

            SpanList<MovedEntity> moveIds = stackalloc MovedEntity[deleteIndicies.Length];
            entityChunk.Delete(deleteIndicies, ref moveIds);

            //assert
            var first = Enumerable.Range(0, 128).Select(x => (uint)x + 1).ToArray();
            var second = Enumerable.Range(0, 128).Select(x => (uint)(Entity.ENTITY_MAX - x)).ToArray();
            var third = Enumerable.Range(256, Entity.ENTITY_MAX - 256 - 128).Select(x => (uint)x + 1).ToArray();
            var fourth = Enumerable.Range(0, 128).Select(x => 0).Select(x => (uint)x).ToArray();

            var firstSet = entityChunk.Entities.Slice(0, 128).ToArray();
            var secondSet = entityChunk.Entities.Slice(128, 128).ToArray();
            var thirdSet = entityChunk.Entities.Slice(256, Entity.ENTITY_MAX - 256 - 128).ToArray();
            var fourthSet = entityChunk.Entities.Slice(Entity.ENTITY_MAX - 128, 128).ToArray();

            firstSet.ShouldBe(first);
            secondSet.ShouldBe(second);
            thirdSet.ShouldBe(third);
            fourthSet.ShouldBe(fourth);

            var firstPosition = Enumerable.Range(0, 128).Select(x => new Position(x + 1)).ToArray();
            var secondPosition = Enumerable.Range(0, 128).Select(x => new Position(Entity.ENTITY_MAX - x)).ToArray();
            var thirdPosition = Enumerable.Range(256, Entity.ENTITY_MAX - 256 - 128).Select(x => new Position(x + 1)).ToArray();

            var firstSetPosition = span.Slice(0, 128).ToArray();
            var secondSetPosition = span.Slice(128, 128).ToArray();
            var thirdSetPosition = span.Slice(256, Entity.ENTITY_MAX - 256 - 128).ToArray();

            firstSetPosition.ShouldBe(firstPosition);
            secondSetPosition.ShouldBe(secondPosition);
            thirdSetPosition.ShouldBe(thirdPosition);

#if DEBUG
            //we only reset the data in debug mode for performance reasons
            var fourthPosition = Enumerable.Range(0, 128).Select(x => new Position(0)).ToArray();
            var fourthSetPosition = span.Slice(Entity.ENTITY_MAX - 128, 128).ToArray();
            fourthSetPosition.ShouldBe(fourthPosition);
#endif

            entityChunk.Count.ShouldBe(Entity.ENTITY_MAX - 128);
            entityChunk.Free.ShouldBe(128);
        }

    }
}