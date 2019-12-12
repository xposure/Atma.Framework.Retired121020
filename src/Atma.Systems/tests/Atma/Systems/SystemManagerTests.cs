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

    public class SystemManagerTests
    {
        private readonly ILoggerFactory _logFactory;

        public SystemManagerTests(ITestOutputHelper output)
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


        [Stages("Default")]
        public class SystemTest : SystemProducer
        {
            public float dt = 0;
            public int Executed0 = 0, Executed1 = 0, Executed2 = 0;

            public void Execute(ref Position position)
            {
                Executed0++;
            }

            [Stages("Stage 2")]
            public void Execute(ref Velocity velocity)
            {
                Executed1++;
            }

            [Stages("Stage 3")]
            public void Execute(ref Position position, ref Velocity velocity)
            {
                Executed2++;
            }

        }

        [Fact]
        public void Test()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var em = new EntityManager(_logFactory, memory);
            using var sm = new SystemManager(_logFactory, em, memory);

            var spec = EntitySpec.Create<Position, Velocity>();
            var entity = em.Create(spec);

            sm.AddStage("Default");
            sm.AddStage("Stage 2");
            sm.AddStage("Stage 3");
            sm.AddStage("Stage 4");

            var sys = new SystemTest();
            sys.dt = 0.16f;
            sys.Register(sm);

            sm.Init();

            sm.Tick("Default", "Stage 2", "Stage 3", "Stage 4");

            sys.Executed0.ShouldBe(1);
            sys.Executed1.ShouldBe(1);
            sys.Executed2.ShouldBe(1);

        }

    }
}