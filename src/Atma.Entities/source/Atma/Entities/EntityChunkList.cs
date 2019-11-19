namespace Atma.Entities
{
    using Atma.Memory;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;

    public readonly struct CreatedEntity : IEquatable<CreatedEntity>
    {
        public readonly int ChunkIndex;
        public readonly int Index;

        public CreatedEntity(int chunkIndex, int index)
        {
            ChunkIndex = chunkIndex;
            Index = index;
        }

        public bool Equals(CreatedEntity other) => ChunkIndex == other.ChunkIndex && Index == other.Index;

        public override string ToString() => $"{{ ChunkIndex: {ChunkIndex}, Index: {Index} }}";
    }

    public sealed class EntityChunkList : UnmanagedDispose//, IEntityArray //IEquatable<EntityArchetype>
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

        public EntityChunkList(ILoggerFactory logFactory, IAllocator allocator, EntitySpec specification)
        {
            _logFactory = logFactory;
            _logger = logFactory.CreateLogger<EntityChunkList>();
            _allocator = allocator;
            Specification = specification;
        }

        public int Create(uint entity, out int chunkIndex)
        {
            Span<uint> entities = stackalloc[] { entity };
            Span<CreatedEntity> createdEntities = stackalloc CreatedEntity[1];

            Create(entities, createdEntities);
            ref var createdEntity = ref createdEntities[0];
            chunkIndex = createdEntity.ChunkIndex;
            return createdEntity.Index;
        }


        internal void Create(Span<uint> entity, Span<CreatedEntity> createdEntities)
        {
            var i = 0;
            while (i < entity.Length)
            {
                var chunk = GetOrCreateFreeChunk(out var chunkIndex);
                var startIndex = chunk.Count;
                var created = chunk.Create(entity.Slice(i));

                for (var j = 0; j < created; ++j)
                    createdEntities[i++] = new CreatedEntity(chunkIndex, startIndex + j);

                _entityCount += created;
            }
        }

        internal unsafe void Copy(int specIndex, ComponentType* componentType, ref void* src, Span<EntityRef> entities, bool incrementSource)
        {
            while (entities.Length > 0)
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
                    Assert.EqualTo(e1.SpecIndex, specIndex);

                    if (e1.ChunkIndex != e.ChunkIndex || e1.Index != ++index)
                        break;
                }

                chunk.PackedArray.Copy(componentIndex, ref src, e.Index, length, incrementSource);
                entities = entities.Slice(length);
            }
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

        public MovedEntity Delete(int chunkIndex, int index)
        {
            Assert.Range(chunkIndex, 0, _chunks.Count);

            _entityCount--;

            var chunk = _chunks[chunkIndex];
            return chunk.Delete(index);
        }

        internal void Delete(int chunkIndex, Span<int> indicies, ref SpanList<MovedEntity> movedEntities)
        {
            Assert.Range(chunkIndex, 0, _chunks.Count);

            var chunk = _chunks[chunkIndex];
            chunk.Delete(indicies, ref movedEntities);

            _entityCount -= indicies.Length;
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
