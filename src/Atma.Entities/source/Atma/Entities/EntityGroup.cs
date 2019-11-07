namespace Atma.Entities
{
    using System;
    using static Atma.Debug;

    public interface IEntityChunk : IDisposable
    {
        int Count { get; }
        int Free { get; }

        EntitySpecification Specification { get; }

        int Create();
        void Delete(int index);
    }

    public class EntityChunk : UnmanagedDispose, IEntityChunk
    {
        private int _entityCount = 0;
        private EntityGroupArray _groupArray;

        public int Count => _entityCount;
        public int Free => Entity.ENTITY_MAX - _entityCount;
        public EntitySpecification Specification { get; }

        public EntityChunk(EntitySpecification specifcation)
        {
            Specification = specifcation;
            _groupArray = new EntityGroupArray(specifcation);
        }

        public int Create()
        {
            Assert(Free > 0);

            return _entityCount++;
            //throw new System.NotImplementedException();
        }

        public void Delete(int index)
        {
            Assert(index >= 0 && index < _entityCount);
            if (index < _entityCount - 1)
            {

            }

            _entityCount--;
        }

        protected override void OnManagedDispose()
        {
            _groupArray = null;
        }

        protected override void OnUnmanagedDispose()
        {
            _groupArray.Dispose();
        }
    }
}