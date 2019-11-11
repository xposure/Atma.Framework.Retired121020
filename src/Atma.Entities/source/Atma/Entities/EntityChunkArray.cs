namespace Atma.Entities
{
    using Atma.Memory;

    using System.Collections.Generic;

    public sealed class EntityChunkArray : UnmanagedDispose//, IEntityArray //IEquatable<EntityArchetype>
    {
        private IAllocator _allocator;
        private List<EntityChunk> _chunks = new List<EntityChunk>();
        private int _entityCount = 0;

        public int EntityCount => _entityCount;
        public int Capacity => _chunks.Count * Entity.ENTITY_MAX;
        public int Free => Capacity - _entityCount;
        public int ChunkCount => _chunks.Count;

        public IReadOnlyList<EntityChunk> AllChunks => _chunks;
        public IEnumerable<EntityChunk> ActiveChunks
        {
            get
            {
                for (var i = 0; i < _chunks.Count; i++)
                    if (_chunks[i].Count > 0)
                        yield return _chunks[i];
            }
        }
        public EntitySpec Specification { get; }

        public EntityChunkArray(IAllocator allocator, EntitySpec specification)
        {
            _allocator = allocator;
            Specification = specification;
        }

        public int Create(uint entity, out int chunkIndex)
        {
            _entityCount++;
            var chunk = GetOrCreateFreeChunk(out chunkIndex);
            return chunk.Create(entity);
        }

        private EntityChunk GetOrCreateFreeChunk(out int chunkIndex)
        {
            for (chunkIndex = 0; chunkIndex < _chunks.Count; chunkIndex++)
                if (_chunks[chunkIndex].Free > 0)
                    return _chunks[chunkIndex];

            var chunk = new EntityChunk(_allocator, Specification);
            _chunks.Add(chunk);
            return chunk;
        }

        public int Delete(int chunkIndex, int index)
        {
            Assert.Range(chunkIndex, 0, _chunks.Count);

            _entityCount--;

            var chunk = _chunks[chunkIndex];
            return chunk.Delete(index);
        }

        protected override void OnUnmanagedDispose()
        {
            for (int i = _chunks.Count - 1; i >= 0; i--)
            {
                _chunks[i].Dispose();
                _chunks[i] = null;
            }

            _chunks.Clear();
            _chunks = null;
        }
    }
}
