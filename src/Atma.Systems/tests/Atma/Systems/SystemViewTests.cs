namespace Atma.Systems
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Atma.Entities;
    using Atma.Memory;
    using Divergic.Logging.Xunit;
    using Microsoft.Extensions.Logging;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public class SystemViewTests
    {
        private readonly ILoggerFactory _logFactory;

        public SystemViewTests(ITestOutputHelper output)
        {
            _logFactory = LogFactory.Create(output);
        }

        public struct Position
        {
            public int x, y;
        }

        public struct Velocity
        {
            public int x, y;
        }

        // public unsafe struct Actor
        // {
        //     public Position* position;
        //     public Velocity* velocity;
        // }

        // public sealed unsafe class ActorSystem : System<Actor>
        // {
        //     protected override void Execute(in Actor t)
        //     {
        //         t.position->x += t.velocity->x;
        //         t.position->y += t.velocity->y;
        //     }
        // }

        public class ActorSystem : SystemProducer
        {
            public ActorSystem() : base("Test")
            {
            }

            private void Execute(ref Position position, in Velocity velocity)
            {
                position.x += velocity.x;
                position.y += velocity.y;
            }

            [Priority(-1)]
            private void Execute(ref Position position)
            {
                position.x += 0;
                position.y += 0;
            }
        }

        [Fact]
        public void ShouldExecuteSystemMethods()
        {
            var memory = new DynamicAllocator(_logFactory);
            var em = new EntityManager(_logFactory, memory);
            var sm = new SystemManager(_logFactory, em, memory);
            sm.AddStage("Default");

            using var sys = new ActorSystem();
            sm.Register(sys);

            sm.Init();

            var spec = EntitySpec.Create<Position, Velocity>();
            var e0 = em.Create(spec);
            em.Replace(e0, new Position() { x = 10, y = 11 });
            em.Replace(e0, new Velocity() { x = 1, y = 1 });

            var e1 = em.Create(spec);
            em.Replace(e1, new Position() { x = 12, y = 13 });
            em.Replace(e1, new Velocity() { x = 2, y = 2 });

            sm.Tick();

            em.Get<Position>(e0).x.ShouldBe(11);
            em.Get<Position>(e0).y.ShouldBe(12);
            em.Get<Position>(e1).x.ShouldBe(14);
            em.Get<Position>(e1).y.ShouldBe(15);


        }
    }
}