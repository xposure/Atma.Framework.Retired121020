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
        private EntityPackedArray _packedArray;

        public int Count => _entityCount;
        public int Free => Entity.ENTITY_MAX - _entityCount;
        public EntitySpec Specification { get; }
        public EntityPackedArray PackedArray => _packedArray;

        public EntityChunk(EntitySpec specifcation)
        {
            Specification = specifcation;
            _packedArray = new EntityPackedArray(specifcation);
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
                _packedArray.Move(_entityCount - 1, index);

            _entityCount--;
        }

        protected override void OnManagedDispose()
        {
            _packedArray = null;
        }

        protected override void OnUnmanagedDispose()
        {
            _packedArray.Dispose();
        }

        public static void MoveTo(EntityChunk srcChunk, int srcIndex, EntityChunk dstChunk, int dstIndex)
        {
            EntityPackedArray.CopyTo(srcChunk._packedArray, srcIndex, dstChunk._packedArray, dstIndex);
        }
    }
}