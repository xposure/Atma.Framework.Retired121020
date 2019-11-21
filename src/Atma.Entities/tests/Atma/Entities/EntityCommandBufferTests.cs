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

    public unsafe class EntityCommandBufferTests
    {
        private readonly ILoggerFactory _logFactory;

        public EntityCommandBufferTests(ITestOutputHelper output)
        {
            _logFactory = LogFactory.Create(output);
        }

        [Fact]
        public void ShouldCreateEntity()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Position>.Type, ComponentType<Velocity>.Type);

            buffer.Create(spec);

            buffer.Execute(em);

            em.EntityCount.ShouldBe(1);
            em.EntityArrays[0].EntityCount.ShouldBe(1);
        }


        [Fact]
        public void ShouldCreateManyEntity()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory, 8);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Position>.Type, ComponentType<Velocity>.Type);

            buffer.Create(spec, 16384);

            buffer.Execute(em);

            em.EntityCount.ShouldBe(16384);
            em.EntityArrays[0].EntityCount.ShouldBe(16384);
        }

        [Fact]
        public void ShouldDeleteEntity()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Position>.Type, ComponentType<Velocity>.Type);

            var id = em.Create(spec);

            buffer.Delete(id);

            buffer.Execute(em);

            em.EntityCount.ShouldBe(0);
            em.EntityArrays[0].EntityCount.ShouldBe(0);
        }

        [Fact]
        public void ShouldDeleteManyEntities()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Position>.Type, ComponentType<Velocity>.Type);

            const int samples = 32;

            var ids = new uint[samples];
            em.Create(spec, ids);

            var idsToDelete = new[] { ids[0], ids[11], ids[22], ids[23], ids[3], ids[2], ids[15], ids[17], ids[29], ids[21] };

            buffer.Delete(idsToDelete);
            buffer.Execute(em);

            em.EntityCount.ShouldBe(samples - idsToDelete.Length);
            em.EntityArrays[0].EntityCount.ShouldBe(samples - idsToDelete.Length);
            var entities = em.EntityArrays[0].AllChunks[0].Entities.ToArray();

            var remainingIds = entities.Select(x => x.ID).ToArray();
            remainingIds.Any(x => idsToDelete.Contains(x)).ShouldBe(false);

            var otherIds = ids.Where(x => !idsToDelete.Contains(x)).ToArray();
            remainingIds.All(x => otherIds.Contains(x)).ShouldBe(true);
        }

        [Fact]
        public void ShouldDeleteEntityOnLastComponentOne()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Position>.Type, ComponentType<Velocity>.Type);

            const int samples = 32;

            var ids = new uint[samples];
            em.Create(spec, ids);

            buffer.Remove(ids, spec.ComponentTypes);
            buffer.Execute(em);

            em.EntityCount.ShouldBe(0);
            em.EntityArrays.Count.ShouldBe(1);
            em.EntityArrays[0].EntityCount.ShouldBe(0);
        }

        [Fact]
        public void ShouldReplaceOne()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Position>.Type, ComponentType<Velocity>.Type);

            var id = em.Create(spec);

            buffer.Replace(id, new Position(10, 10));

            buffer.Execute(em);

            em.EntityCount.ShouldBe(1);
            em.EntityArrays[0].EntityCount.ShouldBe(1);
            em.Get<Position>(id).ShouldBe(new Position(10, 10));
        }

        [Fact]
        public void ShouldReplaceManyWithOne()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Position>.Type, ComponentType<Velocity>.Type);

            var position = new Position(10, 10);
            const int samples = 32;

            var ids = new uint[samples];
            em.Create(spec, ids);

            buffer.Replace(ids, position);
            buffer.Execute(em);

            em.EntityCount.ShouldBe(32);
            em.EntityArrays[0].EntityCount.ShouldBe(32);
            var positions = ids.Select(id => em.Get<Position>(id)).ToArray();

            positions.ShouldAllBe(x => x == position);
        }

        [Fact]
        public void ShouldReplaceManyWithMany()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Position>.Type, ComponentType<Velocity>.Type);

            const int samples = 32;
            var positions = Enumerable.Range(0, samples).Select(x => new Position(x, 10)).ToArray();

            var ids = new uint[samples];
            em.Create(spec, ids);

            buffer.Replace(ids, positions.AsSpan());
            buffer.Execute(em);

            em.EntityCount.ShouldBe(32);
            em.EntityArrays[0].EntityCount.ShouldBe(32);

            var positionResults = ids.Select(id => em.Get<Position>(id)).ToArray();
            positionResults.ShouldBe(positions);
        }

        // [Fact]
        // public void ShouldThrowReplaceManyWithMismatchLength()
        // {

        // }

        // [Fact]
        // public void ShouldThrowReplaceOneWithOutComponent()
        // {

        // }

        // [Fact]
        // public void ShouldThrowReplaceOneWithMany()
        // {

        // }


        [Fact]
        public void ShouldAssignOne()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Velocity>.Type);

            var id = em.Create(spec);

            buffer.Assign(id, new Position(10, 10));

            buffer.Execute(em);

            em.EntityCount.ShouldBe(1);
            em.EntityArrays.Count.ShouldBe(2);
            em.EntityArrays[0].EntityCount.ShouldBe(0);
            em.EntityArrays[1].EntityCount.ShouldBe(1);
            em.Get<Position>(id).ShouldBe(new Position(10, 10));
        }

        [Fact]
        public void ShouldAssignManyWithOne()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Velocity>.Type);

            var position = new Position(10, 10);
            const int samples = 32;

            var ids = new uint[samples];
            em.Create(spec, ids);

            buffer.Assign(ids, position);
            buffer.Execute(em);

            em.EntityCount.ShouldBe(32);
            em.EntityArrays.Count.ShouldBe(2);
            em.EntityArrays[0].EntityCount.ShouldBe(0);
            em.EntityArrays[1].EntityCount.ShouldBe(32);
            var positions = ids.Select(id => em.Get<Position>(id)).ToArray();

            positions.ShouldAllBe(x => x == position);
        }

        [Fact]
        public void ShouldAssignManyWithMany()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Velocity>.Type);

            const int samples = 32;

            var ids = new uint[samples];
            em.Create(spec, ids);

            var idsToChange = new[] { ids[0], ids[11], ids[22], ids[23], ids[3], ids[2], ids[15], ids[17], ids[29], ids[21] };
            var remainingIds = ids.Where(x => !idsToChange.Contains(x)).ToArray();
            var positions = idsToChange.Select(x => new Position((int)x, 10)).ToArray();

            buffer.Assign(idsToChange, positions.AsSpan());
            buffer.Execute(em);

            em.EntityCount.ShouldBe(samples);
            em.EntityArrays.Count.ShouldBe(2);
            em.EntityArrays[0].EntityCount.ShouldBe(samples - idsToChange.Length);
            em.EntityArrays[1].EntityCount.ShouldBe(idsToChange.Length);

            var entities0 = em.EntityArrays[0].AllChunks[0].Entities.ToArray();
            entities0.Any(x => idsToChange.Contains(x.ID)).ShouldBe(false);
            entities0.All(x => remainingIds.Contains(x.ID)).ShouldBe(true);

            var entities1 = em.EntityArrays[1].AllChunks[0].Entities.ToArray();
            entities1.Any(x => remainingIds.Contains(x.ID)).ShouldBe(false);
            entities1.All(x => idsToChange.Contains(x.ID)).ShouldBe(true);

            var positionResults = idsToChange.Select(id => em.Get<Position>(id)).ToArray();
            positionResults.ShouldBe(positions);
        }


        // [Fact]
        // public void ShouldThrowAssignManyWithMismatchLength()
        // {

        // }

        // [Fact]
        // public void ShouldThrowAssignOneWithComponent()
        // {

        // }

        // [Fact]
        // public void ShouldThrowAssignOneWithMany()
        // {

        // }


        [Fact]
        public void ShouldUpdateOne()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Velocity>.Type);

            var id = em.Create(spec);

            buffer.Update(id, new Position(10, 10));

            buffer.Execute(em);

            em.EntityCount.ShouldBe(1);
            em.EntityArrays.Count.ShouldBe(2);
            em.EntityArrays[0].EntityCount.ShouldBe(0);
            em.EntityArrays[1].EntityCount.ShouldBe(1);
            em.Get<Position>(id).ShouldBe(new Position(10, 10));
        }

        [Fact]
        public void ShouldUpdateManyWithOne()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Velocity>.Type);

            var position = new Position(10, 10);

            const int samples = 32;

            var ids = new uint[samples];
            em.Create(spec, ids);

            var idsToChange = new[] { ids[0], ids[11], ids[22], ids[23], ids[3], ids[2], ids[15], ids[17], ids[29], ids[21] };
            var remainingIds = ids.Where(x => !idsToChange.Contains(x)).ToArray();

            buffer.Update(idsToChange, position);
            buffer.Execute(em);

            em.EntityCount.ShouldBe(samples);
            em.EntityArrays.Count.ShouldBe(2);
            em.EntityArrays[0].EntityCount.ShouldBe(samples - idsToChange.Length);
            em.EntityArrays[1].EntityCount.ShouldBe(idsToChange.Length);

            var entities0 = em.EntityArrays[0].AllChunks[0].Entities.ToArray();
            entities0.Any(x => idsToChange.Contains(x.ID)).ShouldBe(false);
            entities0.All(x => remainingIds.Contains(x.ID)).ShouldBe(true);

            var entities1 = em.EntityArrays[1].AllChunks[0].Entities.ToArray();
            entities1.Any(x => remainingIds.Contains(x.ID)).ShouldBe(false);
            entities1.All(x => idsToChange.Contains(x.ID)).ShouldBe(true);

            var positionResults = idsToChange.Select(id => em.Get<Position>(id)).ToArray();
            positionResults.ShouldAllBe(x => x == position);
        }

        [Fact]
        public void ShouldUpdateManyWithMany()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Velocity>.Type);

            const int samples = 32;

            var ids = new uint[samples];
            em.Create(spec, ids);

            var idsToChange = new[] { ids[0], ids[11], ids[22], ids[23], ids[3], ids[2], ids[15], ids[17], ids[29], ids[21] };
            var remainingIds = ids.Where(x => !idsToChange.Contains(x)).ToArray();
            var positions = idsToChange.Select(x => new Position((int)x, 10)).ToArray();

            buffer.Update(idsToChange, positions.AsSpan());
            buffer.Execute(em);

            em.EntityCount.ShouldBe(samples);
            em.EntityArrays.Count.ShouldBe(2);
            em.EntityArrays[0].EntityCount.ShouldBe(samples - idsToChange.Length);
            em.EntityArrays[1].EntityCount.ShouldBe(idsToChange.Length);

            var entities0 = em.EntityArrays[0].AllChunks[0].Entities.ToArray();
            entities0.Any(x => idsToChange.Contains(x.ID)).ShouldBe(false);
            entities0.All(x => remainingIds.Contains(x.ID)).ShouldBe(true);

            var entities1 = em.EntityArrays[1].AllChunks[0].Entities.ToArray();
            entities1.Any(x => remainingIds.Contains(x.ID)).ShouldBe(false);
            entities1.All(x => idsToChange.Contains(x.ID)).ShouldBe(true);

            var positionResults = idsToChange.Select(id => em.Get<Position>(id)).ToArray();
            positionResults.ShouldBe(positions);
        }

        [Fact]
        public void ShouldRemoveOne()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Velocity>.Type, ComponentType<Position>.Type);

            var id = em.Create(spec);

            buffer.Remove<Position>(id);

            buffer.Execute(em);

            em.EntityCount.ShouldBe(1);
            em.EntityArrays.Count.ShouldBe(2);
            em.EntityArrays[0].EntityCount.ShouldBe(0);
            em.EntityArrays[1].EntityCount.ShouldBe(1);
            em.Has<Position>(id).ShouldBe(false);
        }

        [Fact]
        public void ShouldRemoveMany()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Velocity>.Type, ComponentType<Position>.Type);

            const int samples = 32;

            var ids = new uint[samples];
            em.Create(spec, ids);

            var idsToChange = new[] { ids[0], ids[11], ids[22], ids[23], ids[3], ids[2], ids[15], ids[17], ids[29], ids[21] };
            var remainingIds = ids.Where(x => !idsToChange.Contains(x)).ToArray();

            buffer.Remove<Position>(idsToChange);
            buffer.Execute(em);

            em.EntityCount.ShouldBe(samples);
            em.EntityArrays.Count.ShouldBe(2);
            em.EntityArrays[0].EntityCount.ShouldBe(samples - idsToChange.Length);
            em.EntityArrays[1].EntityCount.ShouldBe(idsToChange.Length);

            var entities0 = em.EntityArrays[0].AllChunks[0].Entities.ToArray();
            entities0.Any(x => idsToChange.Contains(x.ID)).ShouldBe(false);
            entities0.All(x => remainingIds.Contains(x.ID)).ShouldBe(true);

            var entities1 = em.EntityArrays[1].AllChunks[0].Entities.ToArray();
            entities1.Any(x => remainingIds.Contains(x.ID)).ShouldBe(false);
            entities1.All(x => idsToChange.Contains(x.ID)).ShouldBe(true);

            var positionResults = idsToChange.Select(id => em.Has<Position>(id)).ToArray();
            positionResults.ShouldAllBe(x => x == false);
        }

        private struct Filler
        {
            public float z, w;
        }

        [Fact]
        public void ShouldRemoveManyWithMany()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Velocity>.Type, ComponentType<Position>.Type, ComponentType<Filler>.Type);

            const int samples = 32;

            var ids = new uint[samples];
            em.Create(spec, ids);

            var idsToChange = new[] { ids[0], ids[11], ids[22], ids[23], ids[3], ids[2], ids[15], ids[17], ids[29], ids[21] };
            var remainingIds = ids.Where(x => !idsToChange.Contains(x)).ToArray();

            var typesToRemove = new[] { spec.ComponentTypes[1], spec.ComponentTypes[2] };
            buffer.Remove(idsToChange, typesToRemove);
            buffer.Execute(em);

            em.EntityCount.ShouldBe(samples);
            em.EntityArrays.Count.ShouldBe(2);
            em.EntityArrays[0].EntityCount.ShouldBe(samples - idsToChange.Length);
            em.EntityArrays[1].EntityCount.ShouldBe(idsToChange.Length);

            var entities0 = em.EntityArrays[0].AllChunks[0].Entities.ToArray();
            entities0.Any(x => idsToChange.Contains(x.ID)).ShouldBe(false);
            entities0.All(x => remainingIds.Contains(x.ID)).ShouldBe(true);

            var entities1 = em.EntityArrays[1].AllChunks[0].Entities.ToArray();
            entities1.Any(x => remainingIds.Contains(x.ID)).ShouldBe(false);
            entities1.All(x => idsToChange.Contains(x.ID)).ShouldBe(true);

            var positionResults = idsToChange.Select(id => em.Has<Position>(id)).ToArray();
            positionResults.ShouldAllBe(x => x == false);

            var fillerResults = idsToChange.Select(id => em.Has<Filler>(id)).ToArray();
            fillerResults.ShouldAllBe(x => x == false);

            var velocityResults = idsToChange.Select(id => em.Has<Velocity>(id)).ToArray();
            velocityResults.ShouldAllBe(x => x == true);

        }
    }
}