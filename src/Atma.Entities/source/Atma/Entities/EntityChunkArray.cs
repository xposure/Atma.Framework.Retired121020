namespace Atma.Entities
{
    using Atma.Memory;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;

    public sealed class EntityChunkArray : UnmanagedDispose//, IEntityArray //IEquatable<EntityArchetype>
    {
        private ILogger _logger;
        private ILoggerFactory _logFactory;
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

        public EntityChunkArray(ILoggerFactory logFactory, IAllocator allocator, EntitySpec specification)
        {
            _logFactory = logFactory;
            _logger = logFactory.CreateLogger<EntityChunkArray>();
            _allocator = allocator;
            Specification = specification;
        }

        public int Create(uint entity, out int chunkIndex)
        {
            _entityCount++;
            var chunk = GetOrCreateFreeChunk(out chunkIndex);
            return chunk.Create(entity);
        }


        public void Create(NativeSlice<uint> entity)
        {
            var i = 0;
            while (i < entity.Length)
            {
                var remaining = entity.Length - i;
                var chunk = GetOrCreateFreeChunk(out var chunkIndex);
                var count = chunk.Free > remaining ? remaining : chunk.Free;
                chunk.Create(entity.Slice(i, count));
                i += count;
                _entityCount += count;
            }
        }

        internal unsafe NativeSlice<Entity> Copy(ComponentType* componentType, ref void* src, in NativeSlice<Entity> entities, bool oneToMany)
        {
            Assert.GreatherThan(entities.Length, 0);

            var componentIndex = Specification.GetComponentIndex(componentType->ID);
            var chunkIndex = entities[0].ChunkIndex;
            var chunk = _chunks[chunkIndex];

            ref var e = ref entities[0];
            var index = e.Index;
            var length = 1;
            for (; length < entities.Length; length++)
            {
                ref var e1 = ref entities[length];
                if (e.SpecIndex != e1.SpecIndex || e1.ChunkIndex != e.ChunkIndex || e1.Index != ++index)
                    break;
            }

            chunk.PackedArray.Copy(componentIndex, ref src, e.Index, length, oneToMany);
            return entities.Slice(length);
        }


        private EntityChunk GetOrCreateFreeChunk(out int chunkIndex)
        {
            for (chunkIndex = 0; chunkIndex < _chunks.Count; chunkIndex++)
                if (_chunks[chunkIndex].Free > 0)
                    return _chunks[chunkIndex];

            var chunk = new EntityChunk(_logFactory, _allocator, Specification);
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
