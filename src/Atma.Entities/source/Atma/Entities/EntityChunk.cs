namespace Atma.Entities
{
    using System;
    using static Atma.Debug;

    public interface IEntityChunk : IDisposable
    {
        int Count { get; }
        int Free { get; }

        EntitySpec Specification { get; }

        int Create();
        void Delete(int index);
    }

    public class EntityChunk : UnmanagedDispose, IEntityChunk
    {
        private int _entityCount = 0;
        private EntityPackedArray _groupArray;

        public int Count => _entityCount;
        public int Free => Entity.ENTITY_MAX - _entityCount;
        public EntitySpec Specification { get; }

        public EntityChunk(EntitySpec specifcation)
        {
            Specification = specifcation;
            _groupArray = new EntityPackedArray(specifcation);
        }

        public int Create()
        {
            Assert(Free > 0);

            return _entityCount++;
        }

        public void Delete(int index)
        {
            Assert(index >= 0 && index < _entityCount);
            if (index < _entityCount - 1)
                _groupArray.Move(_entityCount - 1, index);

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

        public static void MoveTo(EntityChunk srcChunk, int srcIndex, EntityChunk dstChunk, int dstIndex)
        {
            EntityPackedArray.CopyTo(srcChunk._groupArray, srcIndex, dstChunk._groupArray, dstIndex);
        }
    }
}