using System;
using Atma;
using Atma.Entities;
using Atma.Memory;
public static class ForEntityExtensions
{
    public delegate void ForEachEntity<T0>(uint entity, ref T0 t0) where T0 : unmanaged;
    public unsafe static void ForEntity<T0>(this EntityManager em, ForEachEntity<T0> view)
      where T0 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
            {
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0) =>
                    {
                        for (var i = 0; i < length; i++)
                            view(entities[i].ID, ref t0[i]);
                    });
            }
    }
    public unsafe static void ForEntity<T0>(this EntityChunkList chunkList, ForEachEntity<T0> view)
      where T0 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type };
        Assert.EqualTo(chunkList.Specification.HasAll(componentTypes), true);
        chunkList.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0) =>
        {
            for (var i = 0; i < length; i++)
                view(entities[i].ID, ref t0[i]);
        });
    }
    public unsafe static void ForEntity<T0>(this EntityChunk chunk, ForEachEntity<T0> view)
      where T0 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type };
        Assert.EqualTo(chunk.Specification.HasAll(componentTypes), true);
        var length = chunk.Count;
        var entities = chunk.Entities;
        var t0 = chunk.GetComponentData<T0>();
        for (var i = 0; i < length; i++)
            view(entities[i].ID, ref t0[i]);
    }
    public delegate void ForEachEntity<T0, T1>(uint entity, ref T0 t0, ref T1 t1) where T0 : unmanaged where T1 : unmanaged;
    public unsafe static void ForEntity<T0, T1>(this EntityManager em, ForEachEntity<T0, T1> view)
      where T0 : unmanaged where T1 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
            {
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1) =>
                    {
                        for (var i = 0; i < length; i++)
                            view(entities[i].ID, ref t0[i], ref t1[i]);
                    });
            }
    }
    public unsafe static void ForEntity<T0, T1>(this EntityChunkList chunkList, ForEachEntity<T0, T1> view)
      where T0 : unmanaged where T1 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type };
        Assert.EqualTo(chunkList.Specification.HasAll(componentTypes), true);
        chunkList.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1) =>
        {
            for (var i = 0; i < length; i++)
                view(entities[i].ID, ref t0[i], ref t1[i]);
        });
    }
    public unsafe static void ForEntity<T0, T1>(this EntityChunk chunk, ForEachEntity<T0, T1> view)
      where T0 : unmanaged where T1 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type };
        Assert.EqualTo(chunk.Specification.HasAll(componentTypes), true);
        var length = chunk.Count;
        var entities = chunk.Entities;
        var t0 = chunk.GetComponentData<T0>();
        var t1 = chunk.GetComponentData<T1>();
        for (var i = 0; i < length; i++)
            view(entities[i].ID, ref t0[i], ref t1[i]);
    }
    public delegate void ForEachEntity<T0, T1, T2>(uint entity, ref T0 t0, ref T1 t1, ref T2 t2) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged;
    public unsafe static void ForEntity<T0, T1, T2>(this EntityManager em, ForEachEntity<T0, T1, T2> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
            {
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2) =>
                    {
                        for (var i = 0; i < length; i++)
                            view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i]);
                    });
            }
    }
    public unsafe static void ForEntity<T0, T1, T2>(this EntityChunkList chunkList, ForEachEntity<T0, T1, T2> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type };
        Assert.EqualTo(chunkList.Specification.HasAll(componentTypes), true);
        chunkList.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2) =>
        {
            for (var i = 0; i < length; i++)
                view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i]);
        });
    }
    public unsafe static void ForEntity<T0, T1, T2>(this EntityChunk chunk, ForEachEntity<T0, T1, T2> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type };
        Assert.EqualTo(chunk.Specification.HasAll(componentTypes), true);
        var length = chunk.Count;
        var entities = chunk.Entities;
        var t0 = chunk.GetComponentData<T0>();
        var t1 = chunk.GetComponentData<T1>();
        var t2 = chunk.GetComponentData<T2>();
        for (var i = 0; i < length; i++)
            view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i]);
    }
    public delegate void ForEachEntity<T0, T1, T2, T3>(uint entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged;
    public unsafe static void ForEntity<T0, T1, T2, T3>(this EntityManager em, ForEachEntity<T0, T1, T2, T3> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
            {
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3) =>
                    {
                        for (var i = 0; i < length; i++)
                            view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i]);
                    });
            }
    }
    public unsafe static void ForEntity<T0, T1, T2, T3>(this EntityChunkList chunkList, ForEachEntity<T0, T1, T2, T3> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type };
        Assert.EqualTo(chunkList.Specification.HasAll(componentTypes), true);
        chunkList.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3) =>
        {
            for (var i = 0; i < length; i++)
                view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i]);
        });
    }
    public unsafe static void ForEntity<T0, T1, T2, T3>(this EntityChunk chunk, ForEachEntity<T0, T1, T2, T3> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type };
        Assert.EqualTo(chunk.Specification.HasAll(componentTypes), true);
        var length = chunk.Count;
        var entities = chunk.Entities;
        var t0 = chunk.GetComponentData<T0>();
        var t1 = chunk.GetComponentData<T1>();
        var t2 = chunk.GetComponentData<T2>();
        var t3 = chunk.GetComponentData<T3>();
        for (var i = 0; i < length; i++)
            view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i]);
    }
    public delegate void ForEachEntity<T0, T1, T2, T3, T4>(uint entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged;
    public unsafe static void ForEntity<T0, T1, T2, T3, T4>(this EntityManager em, ForEachEntity<T0, T1, T2, T3, T4> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
            {
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3, Span<T4> t4) =>
                    {
                        for (var i = 0; i < length; i++)
                            view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i]);
                    });
            }
    }
    public unsafe static void ForEntity<T0, T1, T2, T3, T4>(this EntityChunkList chunkList, ForEachEntity<T0, T1, T2, T3, T4> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type };
        Assert.EqualTo(chunkList.Specification.HasAll(componentTypes), true);
        chunkList.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3, Span<T4> t4) =>
        {
            for (var i = 0; i < length; i++)
                view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i]);
        });
    }
    public unsafe static void ForEntity<T0, T1, T2, T3, T4>(this EntityChunk chunk, ForEachEntity<T0, T1, T2, T3, T4> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type };
        Assert.EqualTo(chunk.Specification.HasAll(componentTypes), true);
        var length = chunk.Count;
        var entities = chunk.Entities;
        var t0 = chunk.GetComponentData<T0>();
        var t1 = chunk.GetComponentData<T1>();
        var t2 = chunk.GetComponentData<T2>();
        var t3 = chunk.GetComponentData<T3>();
        var t4 = chunk.GetComponentData<T4>();
        for (var i = 0; i < length; i++)
            view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i]);
    }
    public delegate void ForEachEntity<T0, T1, T2, T3, T4, T5>(uint entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged;
    public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5>(this EntityManager em, ForEachEntity<T0, T1, T2, T3, T4, T5> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
            {
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3, Span<T4> t4, Span<T5> t5) =>
                    {
                        for (var i = 0; i < length; i++)
                            view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i]);
                    });
            }
    }
    public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5>(this EntityChunkList chunkList, ForEachEntity<T0, T1, T2, T3, T4, T5> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type };
        Assert.EqualTo(chunkList.Specification.HasAll(componentTypes), true);
        chunkList.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3, Span<T4> t4, Span<T5> t5) =>
        {
            for (var i = 0; i < length; i++)
                view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i]);
        });
    }
    public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5>(this EntityChunk chunk, ForEachEntity<T0, T1, T2, T3, T4, T5> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type };
        Assert.EqualTo(chunk.Specification.HasAll(componentTypes), true);
        var length = chunk.Count;
        var entities = chunk.Entities;
        var t0 = chunk.GetComponentData<T0>();
        var t1 = chunk.GetComponentData<T1>();
        var t2 = chunk.GetComponentData<T2>();
        var t3 = chunk.GetComponentData<T3>();
        var t4 = chunk.GetComponentData<T4>();
        var t5 = chunk.GetComponentData<T5>();
        for (var i = 0; i < length; i++)
            view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i]);
    }
    public delegate void ForEachEntity<T0, T1, T2, T3, T4, T5, T6>(uint entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged;
    public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5, T6>(this EntityManager em, ForEachEntity<T0, T1, T2, T3, T4, T5, T6> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
            {
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3, Span<T4> t4, Span<T5> t5, Span<T6> t6) =>
                    {
                        for (var i = 0; i < length; i++)
                            view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i]);
                    });
            }
    }
    public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5, T6>(this EntityChunkList chunkList, ForEachEntity<T0, T1, T2, T3, T4, T5, T6> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type };
        Assert.EqualTo(chunkList.Specification.HasAll(componentTypes), true);
        chunkList.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3, Span<T4> t4, Span<T5> t5, Span<T6> t6) =>
        {
            for (var i = 0; i < length; i++)
                view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i]);
        });
    }
    public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5, T6>(this EntityChunk chunk, ForEachEntity<T0, T1, T2, T3, T4, T5, T6> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type };
        Assert.EqualTo(chunk.Specification.HasAll(componentTypes), true);
        var length = chunk.Count;
        var entities = chunk.Entities;
        var t0 = chunk.GetComponentData<T0>();
        var t1 = chunk.GetComponentData<T1>();
        var t2 = chunk.GetComponentData<T2>();
        var t3 = chunk.GetComponentData<T3>();
        var t4 = chunk.GetComponentData<T4>();
        var t5 = chunk.GetComponentData<T5>();
        var t6 = chunk.GetComponentData<T6>();
        for (var i = 0; i < length; i++)
            view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i]);
    }
    public delegate void ForEachEntity<T0, T1, T2, T3, T4, T5, T6, T7>(uint entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged;
    public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5, T6, T7>(this EntityManager em, ForEachEntity<T0, T1, T2, T3, T4, T5, T6, T7> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
            {
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3, Span<T4> t4, Span<T5> t5, Span<T6> t6, Span<T7> t7) =>
                    {
                        for (var i = 0; i < length; i++)
                            view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i], ref t7[i]);
                    });
            }
    }
    public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5, T6, T7>(this EntityChunkList chunkList, ForEachEntity<T0, T1, T2, T3, T4, T5, T6, T7> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type };
        Assert.EqualTo(chunkList.Specification.HasAll(componentTypes), true);
        chunkList.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3, Span<T4> t4, Span<T5> t5, Span<T6> t6, Span<T7> t7) =>
        {
            for (var i = 0; i < length; i++)
                view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i], ref t7[i]);
        });
    }
    public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5, T6, T7>(this EntityChunk chunk, ForEachEntity<T0, T1, T2, T3, T4, T5, T6, T7> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type };
        Assert.EqualTo(chunk.Specification.HasAll(componentTypes), true);
        var length = chunk.Count;
        var entities = chunk.Entities;
        var t0 = chunk.GetComponentData<T0>();
        var t1 = chunk.GetComponentData<T1>();
        var t2 = chunk.GetComponentData<T2>();
        var t3 = chunk.GetComponentData<T3>();
        var t4 = chunk.GetComponentData<T4>();
        var t5 = chunk.GetComponentData<T5>();
        var t6 = chunk.GetComponentData<T6>();
        var t7 = chunk.GetComponentData<T7>();
        for (var i = 0; i < length; i++)
            view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i], ref t7[i]);
    }
    public delegate void ForEachEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8>(uint entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged;
    public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this EntityManager em, ForEachEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
            {
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3, Span<T4> t4, Span<T5> t5, Span<T6> t6, Span<T7> t7, Span<T8> t8) =>
                    {
                        for (var i = 0; i < length; i++)
                            view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i], ref t7[i], ref t8[i]);
                    });
            }
    }
    public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this EntityChunkList chunkList, ForEachEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type };
        Assert.EqualTo(chunkList.Specification.HasAll(componentTypes), true);
        chunkList.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3, Span<T4> t4, Span<T5> t5, Span<T6> t6, Span<T7> t7, Span<T8> t8) =>
        {
            for (var i = 0; i < length; i++)
                view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i], ref t7[i], ref t8[i]);
        });
    }
    public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this EntityChunk chunk, ForEachEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type };
        Assert.EqualTo(chunk.Specification.HasAll(componentTypes), true);
        var length = chunk.Count;
        var entities = chunk.Entities;
        var t0 = chunk.GetComponentData<T0>();
        var t1 = chunk.GetComponentData<T1>();
        var t2 = chunk.GetComponentData<T2>();
        var t3 = chunk.GetComponentData<T3>();
        var t4 = chunk.GetComponentData<T4>();
        var t5 = chunk.GetComponentData<T5>();
        var t6 = chunk.GetComponentData<T6>();
        var t7 = chunk.GetComponentData<T7>();
        var t8 = chunk.GetComponentData<T8>();
        for (var i = 0; i < length; i++)
            view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i], ref t7[i], ref t8[i]);
    }
    public delegate void ForEachEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(uint entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged;
    public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this EntityManager em, ForEachEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type, ComponentType<T9>.Type };
        var arrays = em.EntityArrays.FindSmallest(componentTypes);
        if (arrays != null)
            foreach (var array in arrays)
            {
                if (array.Specification.HasAll(componentTypes))
                    array.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3, Span<T4> t4, Span<T5> t5, Span<T6> t6, Span<T7> t7, Span<T8> t8, Span<T9> t9) =>
                    {
                        for (var i = 0; i < length; i++)
                            view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i], ref t7[i], ref t8[i], ref t9[i]);
                    });
            }
    }
    public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this EntityChunkList chunkList, ForEachEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type, ComponentType<T9>.Type };
        Assert.EqualTo(chunkList.Specification.HasAll(componentTypes), true);
        chunkList.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, Span<T0> t0, Span<T1> t1, Span<T2> t2, Span<T3> t3, Span<T4> t4, Span<T5> t5, Span<T6> t6, Span<T7> t7, Span<T8> t8, Span<T9> t9) =>
        {
            for (var i = 0; i < length; i++)
                view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i], ref t7[i], ref t8[i], ref t9[i]);
        });
    }
    public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this EntityChunk chunk, ForEachEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> view)
      where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged
    {
        Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type, ComponentType<T9>.Type };
        Assert.EqualTo(chunk.Specification.HasAll(componentTypes), true);
        var length = chunk.Count;
        var entities = chunk.Entities;
        var t0 = chunk.GetComponentData<T0>();
        var t1 = chunk.GetComponentData<T1>();
        var t2 = chunk.GetComponentData<T2>();
        var t3 = chunk.GetComponentData<T3>();
        var t4 = chunk.GetComponentData<T4>();
        var t5 = chunk.GetComponentData<T5>();
        var t6 = chunk.GetComponentData<T6>();
        var t7 = chunk.GetComponentData<T7>();
        var t8 = chunk.GetComponentData<T8>();
        var t9 = chunk.GetComponentData<T9>();
        for (var i = 0; i < length; i++)
            view(entities[i].ID, ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i], ref t7[i], ref t8[i], ref t9[i]);
    }
}
