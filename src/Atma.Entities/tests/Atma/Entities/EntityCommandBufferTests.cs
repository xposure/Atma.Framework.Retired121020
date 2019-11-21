namespace Atma.Entities
{
    using System;
    using Atma.Memory;
    using Divergic.Logging.Xunit;
    using Microsoft.Extensions.Logging;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public unsafe class EntityCommandBufferTests
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

            buffer.CreateEntity(spec);

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

            buffer.CreateEntity(spec, 16384);

            buffer.Execute(em);

            em.EntityCount.ShouldBe(16384);
            em.EntityArrays[0].EntityCount.ShouldBe(16384);
        }

        [Fact]
        public void ShouldRemoveEntity()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Position>.Type, ComponentType<Velocity>.Type);

            var id = em.Create(spec);

            buffer.RemoveEntity(id);

            buffer.Execute(em);

            em.EntityCount.ShouldBe(0);
            em.EntityArrays[0].EntityCount.ShouldBe(0);
        }

        [Fact]
        public void ShouldRemoveEntityOnLastComponentOne()
        {

        }

        [Fact]
        public void ShouldRemoveEntityOnLastComponentMany()
        {

        }

        [Fact]
        public void ShouldRemoveEntityOnLastComponentPartial()
        {

        }

        [Fact]
        public void ShouldReplaceOne()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Position>.Type, ComponentType<Velocity>.Type);

            var id = em.Create(spec);

            buffer.ReplaceComponent(id, new Position(10, 10));

            buffer.Execute(em);

            em.EntityCount.ShouldBe(1);
            em.EntityArrays[0].EntityCount.ShouldBe(1);
            em.Get<Position>(id).ShouldBe(new Position(10, 10));
        }

        [Fact]
        public void ShouldReplaceManyWithOne()
        {

        }

        [Fact]
        public void SouldReplaceManyWithMany()
        {

        }

        [Fact]
        public void ShouldThrowReplaceManyWithMismatchLength()
        {

        }

        [Fact]
        public void ShouldThrowReplaceOneWithOutComponent()
        {

        }

        [Fact]
        public void ShouldThrowReplaceOneWithMany()
        {

        }


        [Fact]
        public void ShouldAssignOne()
        {

        }

        [Fact]
        public void ShouldAssignManyWithOne()
        {

        }

        [Fact]
        public void SouldAssignManyWithMany()
        {

        }

        [Fact]
        public void ShouldThrowAssignManyWithMismatchLength()
        {

        }

        [Fact]
        public void ShouldThrowAssignOneWithComponent()
        {

        }

        [Fact]
        public void ShouldThrowAssignOneWithMany()
        {

        }

    }
}