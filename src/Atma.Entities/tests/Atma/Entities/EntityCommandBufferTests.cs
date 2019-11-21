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


        //         [Fact]
        //         public void ShouldCreateEntity()
        //         {
        //             //arrange
        //             using var memory = new HeapAllocator(_logFactory);
        //             using var buffer = new EntityCommandBuffer(memory);
        //             using var em = new EntityManager(_logFactory, memory);

        //             Span<ComponentType> componentTypes = stackalloc[] { ComponentType<Position>.Type, ComponentType<Velocity>.Type };

        //             //act
        //             buffer.CreateEntity(componentTypes);
        //             buffer.Execute(em);

        //             //assert
        //             em.EntityCount.ShouldBe(1);
        //         }

        //         [Fact]
        //         public void ShouldRemoveEntity()
        //         {
        //             //arrange
        //             using var memory = new HeapAllocator(_logFactory);
        //             using var buffer = new EntityCommandBuffer(memory);
        //             using var em = new EntityManager(_logFactory, memory);

        //             Span<ComponentType> componentTypes = stackalloc[] { ComponentType<Position>.Type, ComponentType<Velocity>.Type };
        //             var entity = em.Create(componentTypes);

        //             //act
        //             buffer.RemoveEntity(entity);
        //             buffer.Execute(em);

        //             //assert
        //             em.EntityCount.ShouldBe(0);
        //         }

        //         [Fact]
        //         public void ShouldAssignComponent()
        //         {
        //             //arrange
        //             using var memory = new HeapAllocator(_logFactory);
        //             using var buffer = new EntityCommandBuffer(memory);
        //             using var em = new EntityManager(_logFactory, memory);

        //             Span<ComponentType> componentTypes = stackalloc[] { ComponentType<Position>.Type };

        //             //act
        //             buffer.CreateEntity(componentTypes);
        //             buffer.AssignComponent(new Velocity(100, 200));
        //             buffer.Execute(em);

        //             var entity = em.Create(componentTypes);
        //             buffer.AssignComponent(entity, new Velocity(200, 100));
        //             buffer.Execute(em);

        //             //assert
        //             var v = em.Get<Velocity>(1);
        //             v.VX.ShouldBe(100);
        //             v.VY.ShouldBe(200);

        //             v = em.Get<Velocity>(entity);
        //             v.VX.ShouldBe(200);
        //             v.VY.ShouldBe(100);
        //         }

        //         [Fact]
        //         public void ShouldReplaceComponent()
        //         {
        //             //arrange
        //             using var memory = new HeapAllocator(_logFactory);
        //             using var buffer = new EntityCommandBuffer(memory);
        //             using var em = new EntityManager(_logFactory, memory);

        //             Span<ComponentType> componentTypes = stackalloc[] { ComponentType<Position>.Type };

        //             //act
        //             buffer.CreateEntity(componentTypes);
        //             buffer.ReplaceComponent(new Position(100, 200));
        //             buffer.Execute(em);

        //             var entity = em.Create(componentTypes);
        //             buffer.ReplaceComponent(entity, new Position(200, 100));
        //             buffer.Execute(em);

        //             //assert
        //             var p = em.Get<Position>(1);
        //             p.X.ShouldBe(100);
        //             p.Y.ShouldBe(200);

        //             p = em.Get<Position>(entity);
        //             p.X.ShouldBe(200);
        //             p.Y.ShouldBe(100);
        //         }

        //         [Fact]
        //         public void ShouldUpdateComponent()
        //         {
        //             //arrange
        //             using var memory = new HeapAllocator(_logFactory);
        //             using var buffer = new EntityCommandBuffer(memory);
        //             using var em = new EntityManager(_logFactory, memory);

        //             Span<ComponentType> componentTypes = stackalloc[] { ComponentType<Velocity>.Type };

        //             //act
        //             buffer.CreateEntity(componentTypes);
        //             buffer.UpdateComponent(new Position(100, 200));
        //             buffer.Execute(em);

        //             var entity = em.Create(componentTypes);
        //             buffer.UpdateComponent(entity, new Position(200, 100));
        //             buffer.Execute(em);

        //             //assert
        //             var p = em.Get<Position>(1);
        //             p.X.ShouldBe(100);
        //             p.Y.ShouldBe(200);

        //             p = em.Get<Position>(entity);
        //             p.X.ShouldBe(200);
        //             p.Y.ShouldBe(100);
        //         }

        //         [Fact]
        //         public void ShouldRemoveComponent()
        //         {
        //             //arrange
        //             using var memory = new HeapAllocator(_logFactory);
        //             using var buffer = new EntityCommandBuffer(memory);
        //             using var em = new EntityManager(_logFactory, memory);

        //             Span<ComponentType> componentTypes = stackalloc[] { ComponentType<Position>.Type, ComponentType<Velocity>.Type };
        //             var entity = em.Create(componentTypes);

        //             //act
        //             buffer.RemoveComponent<Position>(entity);
        //             buffer.Execute(em);

        //             //assert
        //             em.EntityCount.ShouldBe(1);
        //             em.Has<Position>(1).ShouldBe(false);
        //         }

        //         [Fact]
        //         public void ShouldRemoveEntityOnLastComponent()
        //         {
        //             //arrange
        //             using var memory = new HeapAllocator(_logFactory);
        //             using var buffer = new EntityCommandBuffer(memory);
        //             using var em = new EntityManager(_logFactory, memory);

        //             Span<ComponentType> componentTypes = stackalloc[] { ComponentType<Position>.Type };
        //             var entity = em.Create(componentTypes);

        //             //act
        //             buffer.RemoveComponent<Position>(entity);
        //             buffer.Execute(em);

        //             //assert
        //             em.EntityCount.ShouldBe(0);
        //         }
    }
}