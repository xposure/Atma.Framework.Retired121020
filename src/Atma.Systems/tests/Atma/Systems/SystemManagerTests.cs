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


        [Stages(1)]
        public class SystemTest : SystemProducer
        {
            public float dt = 0;
            public int Executed0 = 0, Executed1 = 0, Executed2 = 0;

            public void Execute(ref Position position)
            {
                Executed0++;
            }

            [Stages(2)]
            public void Execute(ref Velocity velocity)
            {
                Executed1++;
            }

            [Stages(4)]
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

            var s0 = sm.AddStage(1, "Default (Stage 1)");
            var s1 = sm.AddStage(2, "Stage 2");
            var s2 = sm.AddStage(4, "Stage 3");
            var s3 = sm.AddStage(8, "Stage 4");

            var sys = new SystemTest();
            sys.dt = 0.16f;
            sys.Register(sm);

            sm.Init();

            sm.Tick(1);
            sm.Tick(2);
            sm.Tick(4);
            sm.Tick(8);

            sys.Executed0.ShouldBe(1);
            sys.Executed1.ShouldBe(1);
            sys.Executed2.ShouldBe(1);

        }

    }
}