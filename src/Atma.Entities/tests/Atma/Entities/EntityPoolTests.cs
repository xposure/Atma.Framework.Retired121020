namespace Atma.Entities
{
    using Atma.Common;
    using Shouldly;

    public class EntityPoolTests
    {
        public void EntityPool()
        {
            var entityPool = new EntityPool();

            for (var i = 0; i < 10000; i++)
                entityPool.Take().ShouldBe(i);
        }
    }
}