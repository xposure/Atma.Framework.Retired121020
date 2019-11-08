namespace Atma.Entities
{
    using Atma.Common;
    using Atma.Memory;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using static Atma.Debug;

    public interface IEntityArray
    {
        int EntityCount { get; }
        int Capacity { get; }
        int Free { get; }
        int ChunkCount { get; }
        IReadOnlyList<EntityChunk> AllChunks { get; }
        IEnumerable<EntityChunk> ActiveChunks { get; }

        EntitySpec Specification { get; }

        int Create(out int chunkIndex);
        void Delete(int chunkIndex, int index);

        //bool HasComponent(in ComponentType componentType);
    }

    public sealed class EntityArray : UnmanagedDispose, IEntityArray //IEquatable<EntityArchetype>
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


        public EntityArray(EntitySpec specification)
        {
            Specification = specification;
        }

        public int Create(out int chunkIndex)
        {
            _entityCount++;
            var chunk = GetOrCreateFreeChunk(out chunkIndex);
            return chunk.Create();
        }

        private EntityChunk GetOrCreateFreeChunk(out int chunkIndex)
        {
            for (chunkIndex = 0; chunkIndex < _chunks.Count; chunkIndex++)
                if (_chunks[chunkIndex].Free > 0)
                    return _chunks[chunkIndex];

            chunkIndex++;
            var chunk = new EntityChunk(Specification);
            _chunks.Add(chunk);
            return chunk;
        }

        public void Delete(int chunkIndex, int index)
        {
            Assert(chunkIndex >= 0 && chunkIndex < _chunks.Count);

            var chunk = _chunks[chunkIndex];
            chunk.Delete(index);
            _entityCount--;
        }

        //public bool HasComponent(in ComponentType componentType) => Specification.Has(componentType);
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
