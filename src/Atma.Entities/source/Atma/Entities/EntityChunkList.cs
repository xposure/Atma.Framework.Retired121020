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
        public readonly int SpecIndex;

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

        public EntityChunkList(ILoggerFactory logFactory, IAllocator allocator, EntitySpec specification, int specIndex)
        {
            _logFactory = logFactory;
            _logger = logFactory.CreateLogger<EntityChunkList>();
            _allocator = allocator;
            Specification = specification;
            SpecIndex = specIndex;
        }

        internal void Create(EntityRef entity, out int chunkIndex)
        {
            Span<EntityRef> entities = stackalloc[] { entity };
            var chunk = GetOrCreateFreeChunk(out chunkIndex);
            chunk.Create(entities);
            _entityCount++;
        }


        internal void Create(Span<EntityRef> entity)
        {
            var i = 0;
            while (i < entity.Length)
            {
                var chunk = GetOrCreateFreeChunk(out var chunkIndex);
                var startIndex = chunk.Count;
                var created = chunk.Create(entity.Slice(i));

                _entityCount += created;
                i += created;
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


        //TODO: Remove the chunk index out param
        private EntityChunk GetOrCreateFreeChunk(out int chunkIndex)
        {
            for (chunkIndex = 0; chunkIndex < _chunks.Count; chunkIndex++)
                if (_chunks[chunkIndex].Free > 0)
                    return _chunks[chunkIndex];

            var chunk = new EntityChunk(_logFactory, _allocator, Specification, SpecIndex, chunkIndex);
            _chunks.Add(chunk);
            return chunk;
        }

        public void Delete(EntityRef entity)
        {
            Assert.Range(entity.ChunkIndex, 0, _chunks.Count);

            _entityCount--;

            var chunk = _chunks[entity.ChunkIndex];
            chunk.Delete(entity);
        }

        internal void Delete(int chunkIndex, Span<EntityRef> indicies)
        {
            Assert.Range(chunkIndex, 0, _chunks.Count);

            var chunk = _chunks[chunkIndex];
            chunk.Delete(indicies);

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
