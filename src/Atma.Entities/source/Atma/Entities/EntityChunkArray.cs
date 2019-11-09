namespace Atma.Entities
{
    using Atma.Common;
    using Atma.Memory;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using static Atma.Debug;

    public sealed class EntityChunkArray : UnmanagedDispose//, IEntityArray //IEquatable<EntityArchetype>
    {
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

        private int _entityCount = 0;
        private List<EntityChunk> _chunks = new List<EntityChunk>();


        public EntityChunkArray(EntitySpec specification)
        {
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

            var chunk = new EntityChunk(Specification);
            _chunks.Add(chunk);
            return chunk;
        }

        public int Delete(int chunkIndex, int index)
        {
            Assert(chunkIndex >= 0 && chunkIndex < _chunks.Count);

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

        // public static void MoveTo(uint entity, EntityChunkArray src, int srcChunkIndex, int srcIndex, EntityChunkArray dst, out int dstChunkIndex, out int dstIndex)
        // {
        //     dstIndex = dst.Create(entity, out dstChunkIndex);
        //     var srcChunk = src._chunks[srcChunkIndex];
        //     var dstChunk = dst._chunks[dstChunkIndex];

        //     EntityChunk.MoveTo(srcChunk, srcIndex, dstChunk, dstIndex);

        //     src.Delete(srcChunkIndex, srcIndex);
        // }


    }

    // public static void MoveEntity(EntityPool pool, int entity, ArchetypeChunk chunkSrc, int indexSrc, EntityArchetype archetype, EntityArchetype newArchetype)
    // {
    //     var i0 = 0;
    //     var i1 = 0;

    //     var dstEntity = newArchetype.CreateEntityInternal(entity);
    //     var chunkDst = newArchetype.Chunks[dstEntity.ChunkIndex];
    //     var indexDst = dstEntity.Index;

    //     while (i0 < archetype.ComponentTypes.Length && i1 < newArchetype.ComponentTypes.Length)
    //     {
    //         var aType = archetype.ComponentTypes[i0];
    //         var bType = newArchetype.ComponentTypes[i1];
    //         if (aType.ID > bType.ID) i1++;
    //         else if (bType.ID > aType.ID) i0++;
    //         else
    //         {
    //             using var arrSrc = chunkSrc.GetReadComponentArray(i0);
    //             using var arrDst = chunkDst.GetWriteComponentArray(i1);

    //             arrSrc.Array.CopyTo(indexSrc, arrDst.Array, indexDst);

    //             i0++;
    //             i1++;
    //         }
    //     }

    //     chunkSrc.DeleteIndex(pool, indexSrc, true);
    //     archetype._entityCount--; //need to do house keeping
    //     pool[entity] = new Entity(entity, newArchetype.Index, chunkDst.Index, indexDst);
    // }

}
