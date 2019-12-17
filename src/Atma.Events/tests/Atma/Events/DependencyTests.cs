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

    public unsafe class EmptyTest
    {
        public struct Position
        {
            public int x, y;
        }

        public struct Velocity
        {
            public int x, y;
        }
        private readonly ILoggerFactory _logFactory;

        public EmptyTest(ITestOutputHelper output)
        {
            _logFactory = LogFactory.Create(output);
        }


        [Fact]
        public void TestFact()
        {

        }
    }
}