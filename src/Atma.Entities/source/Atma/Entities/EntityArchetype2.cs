namespace Atma.Entities
{
    using Atma.Common;
    using Atma.Memory;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using static Atma.Debug;

    public interface IEntityArchetype2
    {
        int ID { get; }
        bool IsValid { get; }
        int ArchetypeIndex { get; }
        int EntityCount { get; }
        int Capacity { get; }
        int Free { get; }
        int ChunkCount { get; }
        IReadOnlyList<EntityChunk> AllChunks { get; }
        IEnumerable<EntityChunk> ActiveChunks { get; }

        int Create();
        void Create(NativeSlice<int> indices);
        //internal void CreateEntity(EntityPool entityPool, in NativeSlice<int> entities)
        void Delete(int index);

        //bool HasEntity(int entity);

        bool HasComponent(ComponentType componentType);
        bool HasComponent<T>() where T : unmanaged;
        //internal void DestroyEntity(EntityPool pool, int entity, ArchetypeChunk chunk, int index)
    }

    public sealed class EntityArchetype2 //: IEquatable<EntityArchetype>
    {

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
}
