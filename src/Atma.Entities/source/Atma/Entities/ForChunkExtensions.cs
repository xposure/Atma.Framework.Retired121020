using System;
using Atma.Entities;
using Atma.Memory;
public static class ForChunkExtensions
{
    public delegate void ForEachChunk<T0>(int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0) where T0 : unmanaged;
    public delegate void ForEachChunkGroup<G0, T0>(int length, G0 g0, ReadOnlySpan<EntityRef> entities, Span<T0> t0) where T0 : unmanaged;
    public unsafe static void ForChunk<T0>(this EntityChunkList chunkList, ForEachChunk<T0> view)
      where T0 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type };
        chunkList.ForChunk(componentTypes, view);
    }
    internal static void ForChunk<T0>(this EntityChunkList chunkList, Span<ComponentType> componentTypes, ForEachChunk<T0> view)
      where T0 : unmanaged
    {
        var c0 = chunkList.Specification.GetComponentIndex(componentTypes[0]);
        for (var k = 0; k < chunkList.ChunkCount; k++)
        {
            var chunk = chunkList[k];
            var length = chunk.Count;
            var entities = chunk.Entities;
            var t0 = chunk.GetComponentData<T0>(c0, componentTypes[0]);
            view(length, entities, t0);
        }
    }
    public static void ForChunk<T0>(this EntityManager em, ForEachChunk<T0> view)
      where T0 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, view);
    }
    // public static void ForChunkGroup<G0, T0>(this EntityManager em, ForEachChunkGroup<G0, T0> view)
    //   where G0 : IEntitySpecGroup
    //   where T0 : unmanaged
    // {
    //     Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type };
    //     var arrays = em.EntityArrays.FindSmallest(componentTypes);
    //     if (arrays != null)
    //         foreach (var array in arrays)
    //             if (array.Specification.HasAll(componentTypes))
    //                 array.ForChunkGroup<G0>( componentTypes, view);
    // }
    public delegate void ForEachChunk<T0, T1>(int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1) where T0 : unmanaged where T1 : unmanaged;
    public unsafe static void ForChunk<T0, T1>(this EntityChunkList chunkList, ForEachChunk<T0, T1> view)
      where T0 : unmanaged where T1 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type };
        chunkList.ForChunk(componentTypes, view);
    }
    internal static void ForChunk<T0, T1>(this EntityChunkList chunkList, Span<ComponentType> componentTypes, ForEachChunk<T0, T1> view)
      where T0 : unmanaged where T1 : unmanaged
    {
        var c0 = chunkList.Specification.GetComponentIndex(componentTypes[0]);
        var c1 = chunkList.Specification.GetComponentIndex(componentTypes[1]);
        for (var k = 0; k < chunkList.ChunkCount; k++)
        {
            var chunk = chunkList[k];
            var length = chunk.Count;
            var entities = chunk.Entities;
            var t0 = chunk.GetComponentData<T0>(c0, componentTypes[0]);
            var t1 = chunk.GetComponentData<T1>(c1, componentTypes[1]);
            view(length, entities, t0, t1);
        }
    }
    public static void ForChunk<T0, T1>(this EntityManager em, ForEachChunk<T0, T1> view)
      where T0 : unmanaged where T1 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, view);
    }
    public delegate void ForEachChunk<T0, T1, T2>(int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged;
    public unsafe static void ForChunk<T0, T1, T2>(this EntityChunkList chunkList, ForEachChunk<T0, T1, T2> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type };
        chunkList.ForChunk(componentTypes, view);
    }
    internal static void ForChunk<T0, T1, T2>(this EntityChunkList chunkList, Span<ComponentType> componentTypes, ForEachChunk<T0, T1, T2> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged
    {
        var c0 = chunkList.Specification.GetComponentIndex(componentTypes[0]);
        var c1 = chunkList.Specification.GetComponentIndex(componentTypes[1]);
        var c2 = chunkList.Specification.GetComponentIndex(componentTypes[2]);
        for (var k = 0; k < chunkList.ChunkCount; k++)
        {
            var chunk = chunkList[k];
            var length = chunk.Count;
            var entities = chunk.Entities;
            var t0 = chunk.GetComponentData<T0>(c0, componentTypes[0]);
            var t1 = chunk.GetComponentData<T1>(c1, componentTypes[1]);
            var t2 = chunk.GetComponentData<T2>(c2, componentTypes[2]);
            view(length, entities, t0, t1, t2);
        }
    }
    public static void ForChunk<T0, T1, T2>(this EntityManager em, ForEachChunk<T0, T1, T2> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, view);
    }
    public delegate void ForEachChunk<T0, T1, T2, T3>(int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged;
    public unsafe static void ForChunk<T0, T1, T2, T3>(this EntityChunkList chunkList, ForEachChunk<T0, T1, T2, T3> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type };
        chunkList.ForChunk(componentTypes, view);
    }
    internal static void ForChunk<T0, T1, T2, T3>(this EntityChunkList chunkList, Span<ComponentType> componentTypes, ForEachChunk<T0, T1, T2, T3> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
    {
        var c0 = chunkList.Specification.GetComponentIndex(componentTypes[0]);
        var c1 = chunkList.Specification.GetComponentIndex(componentTypes[1]);
        var c2 = chunkList.Specification.GetComponentIndex(componentTypes[2]);
        var c3 = chunkList.Specification.GetComponentIndex(componentTypes[3]);
        for (var k = 0; k < chunkList.ChunkCount; k++)
        {
            var chunk = chunkList[k];
            var length = chunk.Count;
            var entities = chunk.Entities;
            var t0 = chunk.GetComponentData<T0>(c0, componentTypes[0]);
            var t1 = chunk.GetComponentData<T1>(c1, componentTypes[1]);
            var t2 = chunk.GetComponentData<T2>(c2, componentTypes[2]);
            var t3 = chunk.GetComponentData<T3>(c3, componentTypes[3]);
            view(length, entities, t0, t1, t2, t3);
        }
    }
    public static void ForChunk<T0, T1, T2, T3>(this EntityManager em, ForEachChunk<T0, T1, T2, T3> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, view);
    }
    public delegate void ForEachChunk<T0, T1, T2, T3, T4>(int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3, Span<T4> t4) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged;
    public unsafe static void ForChunk<T0, T1, T2, T3, T4>(this EntityChunkList chunkList, ForEachChunk<T0, T1, T2, T3, T4> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type };
        chunkList.ForChunk(componentTypes, view);
    }
    internal static void ForChunk<T0, T1, T2, T3, T4>(this EntityChunkList chunkList, Span<ComponentType> componentTypes, ForEachChunk<T0, T1, T2, T3, T4> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged
    {
        var c0 = chunkList.Specification.GetComponentIndex(componentTypes[0]);
        var c1 = chunkList.Specification.GetComponentIndex(componentTypes[1]);
        var c2 = chunkList.Specification.GetComponentIndex(componentTypes[2]);
        var c3 = chunkList.Specification.GetComponentIndex(componentTypes[3]);
        var c4 = chunkList.Specification.GetComponentIndex(componentTypes[4]);
        for (var k = 0; k < chunkList.ChunkCount; k++)
        {
            var chunk = chunkList[k];
            var length = chunk.Count;
            var entities = chunk.Entities;
            var t0 = chunk.GetComponentData<T0>(c0, componentTypes[0]);
            var t1 = chunk.GetComponentData<T1>(c1, componentTypes[1]);
            var t2 = chunk.GetComponentData<T2>(c2, componentTypes[2]);
            var t3 = chunk.GetComponentData<T3>(c3, componentTypes[3]);
            var t4 = chunk.GetComponentData<T4>(c4, componentTypes[4]);
            view(length, entities, t0, t1, t2, t3, t4);
        }
    }
    public static void ForChunk<T0, T1, T2, T3, T4>(this EntityManager em, ForEachChunk<T0, T1, T2, T3, T4> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, view);
    }
    public delegate void ForEachChunk<T0, T1, T2, T3, T4, T5>(int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3, Span<T4> t4, Span<T5> t5) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged;
    public unsafe static void ForChunk<T0, T1, T2, T3, T4, T5>(this EntityChunkList chunkList, ForEachChunk<T0, T1, T2, T3, T4, T5> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type };
        chunkList.ForChunk(componentTypes, view);
    }
    internal static void ForChunk<T0, T1, T2, T3, T4, T5>(this EntityChunkList chunkList, Span<ComponentType> componentTypes, ForEachChunk<T0, T1, T2, T3, T4, T5> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged
    {
        var c0 = chunkList.Specification.GetComponentIndex(componentTypes[0]);
        var c1 = chunkList.Specification.GetComponentIndex(componentTypes[1]);
        var c2 = chunkList.Specification.GetComponentIndex(componentTypes[2]);
        var c3 = chunkList.Specification.GetComponentIndex(componentTypes[3]);
        var c4 = chunkList.Specification.GetComponentIndex(componentTypes[4]);
        var c5 = chunkList.Specification.GetComponentIndex(componentTypes[5]);
        for (var k = 0; k < chunkList.ChunkCount; k++)
        {
            var chunk = chunkList[k];
            var length = chunk.Count;
            var entities = chunk.Entities;
            var t0 = chunk.GetComponentData<T0>(c0, componentTypes[0]);
            var t1 = chunk.GetComponentData<T1>(c1, componentTypes[1]);
            var t2 = chunk.GetComponentData<T2>(c2, componentTypes[2]);
            var t3 = chunk.GetComponentData<T3>(c3, componentTypes[3]);
            var t4 = chunk.GetComponentData<T4>(c4, componentTypes[4]);
            var t5 = chunk.GetComponentData<T5>(c5, componentTypes[5]);
            view(length, entities, t0, t1, t2, t3, t4, t5);
        }
    }
    public static void ForChunk<T0, T1, T2, T3, T4, T5>(this EntityManager em, ForEachChunk<T0, T1, T2, T3, T4, T5> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, view);
    }
    public delegate void ForEachChunk<T0, T1, T2, T3, T4, T5, T6>(int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3, Span<T4> t4, Span<T5> t5, Span<T6> t6) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged;
    public unsafe static void ForChunk<T0, T1, T2, T3, T4, T5, T6>(this EntityChunkList chunkList, ForEachChunk<T0, T1, T2, T3, T4, T5, T6> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type };
        chunkList.ForChunk(componentTypes, view);
    }
    internal static void ForChunk<T0, T1, T2, T3, T4, T5, T6>(this EntityChunkList chunkList, Span<ComponentType> componentTypes, ForEachChunk<T0, T1, T2, T3, T4, T5, T6> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged
    {
        var c0 = chunkList.Specification.GetComponentIndex(componentTypes[0]);
        var c1 = chunkList.Specification.GetComponentIndex(componentTypes[1]);
        var c2 = chunkList.Specification.GetComponentIndex(componentTypes[2]);
        var c3 = chunkList.Specification.GetComponentIndex(componentTypes[3]);
        var c4 = chunkList.Specification.GetComponentIndex(componentTypes[4]);
        var c5 = chunkList.Specification.GetComponentIndex(componentTypes[5]);
        var c6 = chunkList.Specification.GetComponentIndex(componentTypes[6]);
        for (var k = 0; k < chunkList.ChunkCount; k++)
        {
            var chunk = chunkList[k];
            var length = chunk.Count;
            var entities = chunk.Entities;
            var t0 = chunk.GetComponentData<T0>(c0, componentTypes[0]);
            var t1 = chunk.GetComponentData<T1>(c1, componentTypes[1]);
            var t2 = chunk.GetComponentData<T2>(c2, componentTypes[2]);
            var t3 = chunk.GetComponentData<T3>(c3, componentTypes[3]);
            var t4 = chunk.GetComponentData<T4>(c4, componentTypes[4]);
            var t5 = chunk.GetComponentData<T5>(c5, componentTypes[5]);
            var t6 = chunk.GetComponentData<T6>(c6, componentTypes[6]);
            view(length, entities, t0, t1, t2, t3, t4, t5, t6);
        }
    }
    public static void ForChunk<T0, T1, T2, T3, T4, T5, T6>(this EntityManager em, ForEachChunk<T0, T1, T2, T3, T4, T5, T6> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, view);
    }
    public delegate void ForEachChunk<T0, T1, T2, T3, T4, T5, T6, T7>(int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3, Span<T4> t4, Span<T5> t5, Span<T6> t6, Span<T7> t7) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged;
    public unsafe static void ForChunk<T0, T1, T2, T3, T4, T5, T6, T7>(this EntityChunkList chunkList, ForEachChunk<T0, T1, T2, T3, T4, T5, T6, T7> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type };
        chunkList.ForChunk(componentTypes, view);
    }
    internal static void ForChunk<T0, T1, T2, T3, T4, T5, T6, T7>(this EntityChunkList chunkList, Span<ComponentType> componentTypes, ForEachChunk<T0, T1, T2, T3, T4, T5, T6, T7> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged
    {
        var c0 = chunkList.Specification.GetComponentIndex(componentTypes[0]);
        var c1 = chunkList.Specification.GetComponentIndex(componentTypes[1]);
        var c2 = chunkList.Specification.GetComponentIndex(componentTypes[2]);
        var c3 = chunkList.Specification.GetComponentIndex(componentTypes[3]);
        var c4 = chunkList.Specification.GetComponentIndex(componentTypes[4]);
        var c5 = chunkList.Specification.GetComponentIndex(componentTypes[5]);
        var c6 = chunkList.Specification.GetComponentIndex(componentTypes[6]);
        var c7 = chunkList.Specification.GetComponentIndex(componentTypes[7]);
        for (var k = 0; k < chunkList.ChunkCount; k++)
        {
            var chunk = chunkList[k];
            var length = chunk.Count;
            var entities = chunk.Entities;
            var t0 = chunk.GetComponentData<T0>(c0, componentTypes[0]);
            var t1 = chunk.GetComponentData<T1>(c1, componentTypes[1]);
            var t2 = chunk.GetComponentData<T2>(c2, componentTypes[2]);
            var t3 = chunk.GetComponentData<T3>(c3, componentTypes[3]);
            var t4 = chunk.GetComponentData<T4>(c4, componentTypes[4]);
            var t5 = chunk.GetComponentData<T5>(c5, componentTypes[5]);
            var t6 = chunk.GetComponentData<T6>(c6, componentTypes[6]);
            var t7 = chunk.GetComponentData<T7>(c7, componentTypes[7]);
            view(length, entities, t0, t1, t2, t3, t4, t5, t6, t7);
        }
    }
    public static void ForChunk<T0, T1, T2, T3, T4, T5, T6, T7>(this EntityManager em, ForEachChunk<T0, T1, T2, T3, T4, T5, T6, T7> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, view);
    }
    public delegate void ForEachChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8>(int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3, Span<T4> t4, Span<T5> t5, Span<T6> t6, Span<T7> t7, Span<T8> t8) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged;
    public unsafe static void ForChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this EntityChunkList chunkList, ForEachChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type };
        chunkList.ForChunk(componentTypes, view);
    }
    internal static void ForChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this EntityChunkList chunkList, Span<ComponentType> componentTypes, ForEachChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged
    {
        var c0 = chunkList.Specification.GetComponentIndex(componentTypes[0]);
        var c1 = chunkList.Specification.GetComponentIndex(componentTypes[1]);
        var c2 = chunkList.Specification.GetComponentIndex(componentTypes[2]);
        var c3 = chunkList.Specification.GetComponentIndex(componentTypes[3]);
        var c4 = chunkList.Specification.GetComponentIndex(componentTypes[4]);
        var c5 = chunkList.Specification.GetComponentIndex(componentTypes[5]);
        var c6 = chunkList.Specification.GetComponentIndex(componentTypes[6]);
        var c7 = chunkList.Specification.GetComponentIndex(componentTypes[7]);
        var c8 = chunkList.Specification.GetComponentIndex(componentTypes[8]);
        for (var k = 0; k < chunkList.ChunkCount; k++)
        {
            var chunk = chunkList[k];
            var length = chunk.Count;
            var entities = chunk.Entities;
            var t0 = chunk.GetComponentData<T0>(c0, componentTypes[0]);
            var t1 = chunk.GetComponentData<T1>(c1, componentTypes[1]);
            var t2 = chunk.GetComponentData<T2>(c2, componentTypes[2]);
            var t3 = chunk.GetComponentData<T3>(c3, componentTypes[3]);
            var t4 = chunk.GetComponentData<T4>(c4, componentTypes[4]);
            var t5 = chunk.GetComponentData<T5>(c5, componentTypes[5]);
            var t6 = chunk.GetComponentData<T6>(c6, componentTypes[6]);
            var t7 = chunk.GetComponentData<T7>(c7, componentTypes[7]);
            var t8 = chunk.GetComponentData<T8>(c8, componentTypes[8]);
            view(length, entities, t0, t1, t2, t3, t4, t5, t6, t7, t8);
        }
    }
    public static void ForChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this EntityManager em, ForEachChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, view);
    }
    public delegate void ForEachChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3, Span<T4> t4, Span<T5> t5, Span<T6> t6, Span<T7> t7, Span<T8> t8, Span<T9> t9) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged;
    public unsafe static void ForChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this EntityChunkList chunkList, ForEachChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type, ComponentType<T9>.Type };
        chunkList.ForChunk(componentTypes, view);
    }
    internal static void ForChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this EntityChunkList chunkList, Span<ComponentType> componentTypes, ForEachChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged
    {
        var c0 = chunkList.Specification.GetComponentIndex(componentTypes[0]);
        var c1 = chunkList.Specification.GetComponentIndex(componentTypes[1]);
        var c2 = chunkList.Specification.GetComponentIndex(componentTypes[2]);
        var c3 = chunkList.Specification.GetComponentIndex(componentTypes[3]);
        var c4 = chunkList.Specification.GetComponentIndex(componentTypes[4]);
        var c5 = chunkList.Specification.GetComponentIndex(componentTypes[5]);
        var c6 = chunkList.Specification.GetComponentIndex(componentTypes[6]);
        var c7 = chunkList.Specification.GetComponentIndex(componentTypes[7]);
        var c8 = chunkList.Specification.GetComponentIndex(componentTypes[8]);
        var c9 = chunkList.Specification.GetComponentIndex(componentTypes[9]);
        for (var k = 0; k < chunkList.ChunkCount; k++)
        {
            var chunk = chunkList[k];
            var length = chunk.Count;
            var entities = chunk.Entities;
            var t0 = chunk.GetComponentData<T0>(c0, componentTypes[0]);
            var t1 = chunk.GetComponentData<T1>(c1, componentTypes[1]);
            var t2 = chunk.GetComponentData<T2>(c2, componentTypes[2]);
            var t3 = chunk.GetComponentData<T3>(c3, componentTypes[3]);
            var t4 = chunk.GetComponentData<T4>(c4, componentTypes[4]);
            var t5 = chunk.GetComponentData<T5>(c5, componentTypes[5]);
            var t6 = chunk.GetComponentData<T6>(c6, componentTypes[6]);
            var t7 = chunk.GetComponentData<T7>(c7, componentTypes[7]);
            var t8 = chunk.GetComponentData<T8>(c8, componentTypes[8]);
            var t9 = chunk.GetComponentData<T9>(c9, componentTypes[9]);
            view(length, entities, t0, t1, t2, t3, t4, t5, t6, t7, t8, t9);
        }
    }
    public static void ForChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this EntityManager em, ForEachChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type, ComponentType<T9>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, view);
    }
}
