namespace Atma.Memory
{
    using Divergic.Logging.Xunit;
    using Microsoft.Extensions.Logging;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public class PageObjectPoolTests
    {
        private readonly ILoggerFactory _logFactory;

        public PageObjectPoolTests(ITestOutputHelper output)
        {
            _logFactory = LogFactory.Create(output);
        }


        [Fact]
        public void ShouldReturnSameIdButDifferentVersion()
        {
            using var memory = new DynamicAllocator(_logFactory);
            using var pool = new PagedObjectPool<int>(memory);
            pool.Take();
            var id = pool.Take();
            pool.Return(id);

            var newid = pool.Take();
            newid.ShouldNotBe(id);
            (newid & 0xffffff).ShouldBe(id & 0xffffff);
        }
    }
}