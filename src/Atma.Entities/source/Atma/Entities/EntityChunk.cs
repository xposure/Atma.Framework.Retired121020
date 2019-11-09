namespace Atma.Entities
{
    using System;
    using static Atma.Debug;

    public sealed class EntityChunk : UnmanagedDispose//, IEntityChunk
    {
        private int _entityCount = 0;

        private uint[] _entities;
        private EntityPackedArray _packedArray;

        public int Count => _entityCount;
        public int Free => Entity.ENTITY_MAX - _entityCount;
        public EntitySpec Specification { get; }
        public EntityPackedArray PackedArray => _packedArray;

        public EntityChunk(EntitySpec specifcation)
        {
            Specification = specifcation;
            _packedArray = new EntityPackedArray(specifcation);
            _entities = new uint[_packedArray.Length];
        }

        public uint GetEntity(int index)
        {
            Assert(index >= 0 && index < _entityCount);
            return _entities[index];
        }

        public int Create(uint entity)
        {
            Assert(Free > 0);

            var index = _entityCount++;
            _entities[index] = entity;
            return index;
        }

        public int Delete(int index)
        {
            Assert(index >= 0 && index < _entityCount);
            _entityCount--;

            var movedIndex = -1;
            if (index < _entityCount) //removing the last element, no need to patch
            {
                movedIndex = index;
                _packedArray.Move(_entityCount, index);
                _entities[index] = _entities[_entityCount];
            }

            _entities[_entityCount] = 0;
            return movedIndex;
        }

        protected override void OnManagedDispose()
        {
            _entities = null;
            _packedArray = null;
        }

        protected override void OnUnmanagedDispose()
        {
            _packedArray.Dispose();
        }

        // public static void MoveTo(EntityChunk srcChunk, int srcIndex, EntityChunk dstChunk, int dstIndex)
        // {
        //     dstChunk._entities[dstIndex] = srcChunk._entities[srcIndex];
        //     EntityPackedArray.CopyTo(srcChunk._packedArray, srcIndex, dstChunk._packedArray, dstIndex);
        // }
    }
}