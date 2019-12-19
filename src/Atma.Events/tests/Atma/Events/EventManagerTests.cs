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

    public unsafe class EventManagerTests
    {
        private readonly ILoggerFactory _logFactory;

        public EventManagerTests(ITestOutputHelper output)
        {
            _logFactory = LogFactory.Create(output);
        }

        [Fact]
        public void ShouldObserveAndExecute()
        {
            using var e = new EventManager();
            var i = 0f;

            e.Subscribe("Tick", (float t) => i += t);

            e.Fire("Tick", 123f);

            i.ShouldBe(123f);
            e.Fire("asdf", 321f);

            i.ShouldBe(123f);

            e.Fire("Tick", 123); //int does not implicitly cast, should we think about that?
            i.ShouldBe(123f);



        }
    }
}