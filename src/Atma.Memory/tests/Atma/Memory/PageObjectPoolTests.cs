namespace Atma.Memory
{
    using Shouldly;
    using Xunit;

    public class PageObjectPoolTests
    {
        [Fact]
        public void ShouldReturnSameIdButDifferentVersion()
        {
            using var memory = new DynamicAllocator();
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