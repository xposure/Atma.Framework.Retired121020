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
        public void ShouldCreate()
        {
            //arrange
            using var memory = new HeapAllocator(_logFactory);
            using var buffer = new EntityCommandBuffer(memory);
            using var em = new EntityManager(_logFactory, memory);

            Span<ComponentType> componentTypes = stackalloc[] { ComponentType<Position>.Type, ComponentType<Velocity>.Type };

            //act
            buffer.CreateEntity(componentTypes);
            buffer.Execute(em);

            //assert

            em.EntityCount.ShouldBe(1);
        }
    }
}