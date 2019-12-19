namespace Atma.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Atma.Entities;
    using Divergic.Logging.Xunit;
    using Microsoft.Extensions.Logging;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public unsafe class EventAutoWireTests
    {
        private readonly ILoggerFactory _logFactory;

        public EventAutoWireTests(ITestOutputHelper output)
        {
            _logFactory = LogFactory.Create(output);
        }

        public class DummyThing : UnmanagedDispose
        {

            public int TickValue = 0;
            public int UpdateValue = 0;

            [Event]
            public void Tick() => TickValue++;

            [Event("Tick")]
            public void Test() => TickValue++;

            [Event("Tick")]
            public void Test(int t) => TickValue += t;


            [Event]
            public void Update() => UpdateValue++;
        }



        [Fact]
        public void ShouldObserveAndExecute()
        {
            using var e = new EventManager();
            using var a = new AutoEventManager(e);

            using var dummy1 = new DummyThing();
            using var dummy2 = new DummyThing();

            a.Subscribe(dummy1);

            e.Fire("Tick");
            dummy1.TickValue.ShouldBe(2);
            dummy1.UpdateValue.ShouldBe(0);
            dummy2.TickValue.ShouldBe(0);
            dummy2.UpdateValue.ShouldBe(0);

            a.Subscribe(dummy2);
            e.Fire("Tick");
            dummy1.TickValue.ShouldBe(4);
            dummy1.UpdateValue.ShouldBe(0);
            dummy2.TickValue.ShouldBe(2);
            dummy2.UpdateValue.ShouldBe(0);

            e.Fire("Update");
            dummy1.TickValue.ShouldBe(4);
            dummy1.UpdateValue.ShouldBe(1);
            dummy2.TickValue.ShouldBe(2);
            dummy2.UpdateValue.ShouldBe(1);

        }
    }
}