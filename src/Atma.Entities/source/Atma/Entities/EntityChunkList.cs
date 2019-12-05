namespace Atma.Entities
{
    using Atma.Memory;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;

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
        public EntitySpec Specification { get; }

        public EntityChunkList(ILoggerFactory logFactory, IAllocator allocator, EntitySpec specification, int specIndex)
        {
            _logFactory = logFactory;
            _logger = logFactory.CreateLogger<EntityChunkList>();
            _allocator = allocator;
            Specification = specification;
            SpecIndex = specIndex;
        }

        public EntityChunk this[int index]
        {
            get
            {
                Assert.Range(index, 0, _chunks.Count);
                return _chunks[index];
            }
        }

        internal void Create(in EntityRef entity)
        {
            Span<EntityRef> entities = stackalloc[] { entity };
            var chunk = GetOrCreateFreeChunk();
            chunk.Create(entities);
            _entityCount++;
        }


        internal void Create(Span<EntityRef> entity)
        {
            var i = 0;
            while (i < entity.Length)
            {
                var chunk = GetOrCreateFreeChunk();
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

        private EntityChunk GetOrCreateFreeChunk()
        {
            var chunkIndex = 0;
            for (; chunkIndex < _chunks.Count; chunkIndex++)
                if (_chunks[chunkIndex].Free > 0)
                    return _chunks[chunkIndex];

            var chunk = new EntityChunk(_logFactory, _allocator, Specification, SpecIndex, chunkIndex);
            _chunks.Add(chunk);
            return chunk;
        }

        public void Delete(in EntityRef entity)
        {
            Assert.Range(entity.ChunkIndex, 0, _chunks.Count);

            _entityCount--;

            var chunk = _chunks[entity.ChunkIndex];
            chunk.Delete(entity);
        }

        internal void Delete(Span<EntityRef> entities)
        {
            if (entities.Length == 0)
                return;
            var chunkIndex = entities[0].ChunkIndex;
            var lastIndex = 0;

            //we have to delete backwards or things will move around on us and we 
            //can pass in temp refs to moving entities + its faster to delete backwards
            //since there is a chance of no swaps
            entities.Sort(EntityRef.KeySortDesc);

            Assert.EqualTo(entities[0].SpecIndex, SpecIndex);
            for (var i = 1; i < entities.Length; i++)
            {
                ref var e = ref entities[i];
                if (e.ChunkIndex != chunkIndex)
                {
                    //flush
                    var amountToRemove = i - lastIndex - 1;
                    if (amountToRemove > 0)
                    {
                        var chunk = _chunks[chunkIndex];
                        chunk.Delete(entities.Slice(lastIndex, amountToRemove));
                    }

                    chunkIndex = e.ChunkIndex;
                    lastIndex = i + 1;
                }

                Assert.EqualTo(e.SpecIndex, SpecIndex);
            }

            if (entities.Length - lastIndex > 0)
            {
                var chunk = _chunks[chunkIndex];
                chunk.Delete(entities.Slice(lastIndex, entities.Length - lastIndex));
            }
            _entityCount -= entities.Length;
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

        internal static void CopyTo(EntityChunkList src, Span<EntityRef> srcEntities, EntityChunkList dst, Span<EntityRef> dstEntities)
        {
            //SharedComponentArrays(srcArray, dstArray, (src, dst) => ComponentDataArray.CopyTo(src, srcIndex, dst, dstIndex));

            Contract.EqualTo(srcEntities.Length, dstEntities.Length);
            if (srcEntities.Length == 0)
                return;

            var count = srcEntities.Length;

            var i0 = 0;
            var i1 = 0;

            Span<ComponentType> a = src.Specification.ComponentTypes;
            Span<ComponentType> b = dst.Specification.ComponentTypes;
            while (i0 < a.Length && i1 < b.Length)
            {
                var aType = a[i0];
                var bType = b[i1];
                if (aType.ID > bType.ID) i1++;
                else if (bType.ID > aType.ID) i0++;
                else
                {
                    var srcComponentIndex = src.Specification.GetComponentIndex(aType);
                    var dstComponentIndex = dst.Specification.GetComponentIndex(bType);

                    //TODO: batch this bitch
                    for (var i = 0; i < count; i++)
                    {
                        ref var srcEntity = ref srcEntities[i];
                        ref var dstEntity = ref dstEntities[i];
                        var srcChunk = src.AllChunks[srcEntity.ChunkIndex];
                        var dstChunk = dst.AllChunks[dstEntity.ChunkIndex];
                        ComponentPackedArray.CopyTo(srcChunk.PackedArray, srcComponentIndex, srcEntity.Index, dstChunk.PackedArray, dstComponentIndex, dstEntity.Index);
                    }

                    i0++;
                    i1++;
                }
            }
        }

    }
}
