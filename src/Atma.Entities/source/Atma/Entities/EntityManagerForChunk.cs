namespace Atma
{
    using System;
    using Atma.Entities;
    using Atma.Memory;
    public static class ForChunkExtensions
    {
        public delegate void ForEachChunk<T0>(int length, NativeReadOnlySlice<uint> entities, NativeSlice<T0> t0) where T0 : unmanaged;
        public unsafe static void ForChunk<T0>(this EntityManager em, ForEachChunk<T0> view)
          where T0 : unmanaged
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type };
            var entityArrays = em.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var c0 = array.Specification.GetComponentIndex(componentTypes[0]);
                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        var entities = chunk.Entities;
                        var t0 = chunk.PackedArray.GetComponentData<T0>(c0, componentTypes[0]);
                        view(length, entities, t0);
                    }
                }
            }
        }
        public delegate void ForEachChunk<T0, T1>(int length, NativeReadOnlySlice<uint> entities, NativeSlice<T0> t0, NativeSlice<T1> t1) where T0 : unmanaged where T1 : unmanaged;
        public unsafe static void ForChunk<T0, T1>(this EntityManager em, ForEachChunk<T0, T1> view)
          where T0 : unmanaged where T1 : unmanaged
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type };
            var entityArrays = em.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var c0 = array.Specification.GetComponentIndex(componentTypes[0]);
                    var c1 = array.Specification.GetComponentIndex(componentTypes[1]);
                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        var entities = chunk.Entities;
                        var t0 = chunk.PackedArray.GetComponentData<T0>(c0, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentData<T1>(c1, componentTypes[1]);
                        view(length, entities, t0, t1);
                    }
                }
            }
        }
        public delegate void ForEachChunk<T0, T1, T2>(int length, NativeReadOnlySlice<uint> entities, NativeSlice<T0> t0, NativeSlice<T1> t1, NativeSlice<T2> t2) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged;
        public unsafe static void ForChunk<T0, T1, T2>(this EntityManager em, ForEachChunk<T0, T1, T2> view)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type };
            var entityArrays = em.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var c0 = array.Specification.GetComponentIndex(componentTypes[0]);
                    var c1 = array.Specification.GetComponentIndex(componentTypes[1]);
                    var c2 = array.Specification.GetComponentIndex(componentTypes[2]);
                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        var entities = chunk.Entities;
                        var t0 = chunk.PackedArray.GetComponentData<T0>(c0, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentData<T1>(c1, componentTypes[1]);
                        var t2 = chunk.PackedArray.GetComponentData<T2>(c2, componentTypes[2]);
                        view(length, entities, t0, t1, t2);
                    }
                }
            }
        }
        public delegate void ForEachChunk<T0, T1, T2, T3>(int length, NativeReadOnlySlice<uint> entities, NativeSlice<T0> t0, NativeSlice<T1> t1, NativeSlice<T2> t2, NativeSlice<T3> t3) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged;
        public unsafe static void ForChunk<T0, T1, T2, T3>(this EntityManager em, ForEachChunk<T0, T1, T2, T3> view)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type };
            var entityArrays = em.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var c0 = array.Specification.GetComponentIndex(componentTypes[0]);
                    var c1 = array.Specification.GetComponentIndex(componentTypes[1]);
                    var c2 = array.Specification.GetComponentIndex(componentTypes[2]);
                    var c3 = array.Specification.GetComponentIndex(componentTypes[3]);
                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        var entities = chunk.Entities;
                        var t0 = chunk.PackedArray.GetComponentData<T0>(c0, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentData<T1>(c1, componentTypes[1]);
                        var t2 = chunk.PackedArray.GetComponentData<T2>(c2, componentTypes[2]);
                        var t3 = chunk.PackedArray.GetComponentData<T3>(c3, componentTypes[3]);
                        view(length, entities, t0, t1, t2, t3);
                    }
                }
            }
        }
        public delegate void ForEachChunk<T0, T1, T2, T3, T4>(int length, NativeReadOnlySlice<uint> entities, NativeSlice<T0> t0, NativeSlice<T1> t1, NativeSlice<T2> t2, NativeSlice<T3> t3, NativeSlice<T4> t4) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged;
        public unsafe static void ForChunk<T0, T1, T2, T3, T4>(this EntityManager em, ForEachChunk<T0, T1, T2, T3, T4> view)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type };
            var entityArrays = em.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var c0 = array.Specification.GetComponentIndex(componentTypes[0]);
                    var c1 = array.Specification.GetComponentIndex(componentTypes[1]);
                    var c2 = array.Specification.GetComponentIndex(componentTypes[2]);
                    var c3 = array.Specification.GetComponentIndex(componentTypes[3]);
                    var c4 = array.Specification.GetComponentIndex(componentTypes[4]);
                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        var entities = chunk.Entities;
                        var t0 = chunk.PackedArray.GetComponentData<T0>(c0, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentData<T1>(c1, componentTypes[1]);
                        var t2 = chunk.PackedArray.GetComponentData<T2>(c2, componentTypes[2]);
                        var t3 = chunk.PackedArray.GetComponentData<T3>(c3, componentTypes[3]);
                        var t4 = chunk.PackedArray.GetComponentData<T4>(c4, componentTypes[4]);
                        view(length, entities, t0, t1, t2, t3, t4);
                    }
                }
            }
        }
        public delegate void ForEachChunk<T0, T1, T2, T3, T4, T5>(int length, NativeReadOnlySlice<uint> entities, NativeSlice<T0> t0, NativeSlice<T1> t1, NativeSlice<T2> t2, NativeSlice<T3> t3, NativeSlice<T4> t4, NativeSlice<T5> t5) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged;
        public unsafe static void ForChunk<T0, T1, T2, T3, T4, T5>(this EntityManager em, ForEachChunk<T0, T1, T2, T3, T4, T5> view)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type };
            var entityArrays = em.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var c0 = array.Specification.GetComponentIndex(componentTypes[0]);
                    var c1 = array.Specification.GetComponentIndex(componentTypes[1]);
                    var c2 = array.Specification.GetComponentIndex(componentTypes[2]);
                    var c3 = array.Specification.GetComponentIndex(componentTypes[3]);
                    var c4 = array.Specification.GetComponentIndex(componentTypes[4]);
                    var c5 = array.Specification.GetComponentIndex(componentTypes[5]);
                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        var entities = chunk.Entities;
                        var t0 = chunk.PackedArray.GetComponentData<T0>(c0, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentData<T1>(c1, componentTypes[1]);
                        var t2 = chunk.PackedArray.GetComponentData<T2>(c2, componentTypes[2]);
                        var t3 = chunk.PackedArray.GetComponentData<T3>(c3, componentTypes[3]);
                        var t4 = chunk.PackedArray.GetComponentData<T4>(c4, componentTypes[4]);
                        var t5 = chunk.PackedArray.GetComponentData<T5>(c5, componentTypes[5]);
                        view(length, entities, t0, t1, t2, t3, t4, t5);
                    }
                }
            }
        }
        public delegate void ForEachChunk<T0, T1, T2, T3, T4, T5, T6>(int length, NativeReadOnlySlice<uint> entities, NativeSlice<T0> t0, NativeSlice<T1> t1, NativeSlice<T2> t2, NativeSlice<T3> t3, NativeSlice<T4> t4, NativeSlice<T5> t5, NativeSlice<T6> t6) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged;
        public unsafe static void ForChunk<T0, T1, T2, T3, T4, T5, T6>(this EntityManager em, ForEachChunk<T0, T1, T2, T3, T4, T5, T6> view)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type };
            var entityArrays = em.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var c0 = array.Specification.GetComponentIndex(componentTypes[0]);
                    var c1 = array.Specification.GetComponentIndex(componentTypes[1]);
                    var c2 = array.Specification.GetComponentIndex(componentTypes[2]);
                    var c3 = array.Specification.GetComponentIndex(componentTypes[3]);
                    var c4 = array.Specification.GetComponentIndex(componentTypes[4]);
                    var c5 = array.Specification.GetComponentIndex(componentTypes[5]);
                    var c6 = array.Specification.GetComponentIndex(componentTypes[6]);
                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        var entities = chunk.Entities;
                        var t0 = chunk.PackedArray.GetComponentData<T0>(c0, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentData<T1>(c1, componentTypes[1]);
                        var t2 = chunk.PackedArray.GetComponentData<T2>(c2, componentTypes[2]);
                        var t3 = chunk.PackedArray.GetComponentData<T3>(c3, componentTypes[3]);
                        var t4 = chunk.PackedArray.GetComponentData<T4>(c4, componentTypes[4]);
                        var t5 = chunk.PackedArray.GetComponentData<T5>(c5, componentTypes[5]);
                        var t6 = chunk.PackedArray.GetComponentData<T6>(c6, componentTypes[6]);
                        view(length, entities, t0, t1, t2, t3, t4, t5, t6);
                    }
                }
            }
        }
        public delegate void ForEachChunk<T0, T1, T2, T3, T4, T5, T6, T7>(int length, NativeReadOnlySlice<uint> entities, NativeSlice<T0> t0, NativeSlice<T1> t1, NativeSlice<T2> t2, NativeSlice<T3> t3, NativeSlice<T4> t4, NativeSlice<T5> t5, NativeSlice<T6> t6, NativeSlice<T7> t7) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged;
        public unsafe static void ForChunk<T0, T1, T2, T3, T4, T5, T6, T7>(this EntityManager em, ForEachChunk<T0, T1, T2, T3, T4, T5, T6, T7> view)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type };
            var entityArrays = em.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var c0 = array.Specification.GetComponentIndex(componentTypes[0]);
                    var c1 = array.Specification.GetComponentIndex(componentTypes[1]);
                    var c2 = array.Specification.GetComponentIndex(componentTypes[2]);
                    var c3 = array.Specification.GetComponentIndex(componentTypes[3]);
                    var c4 = array.Specification.GetComponentIndex(componentTypes[4]);
                    var c5 = array.Specification.GetComponentIndex(componentTypes[5]);
                    var c6 = array.Specification.GetComponentIndex(componentTypes[6]);
                    var c7 = array.Specification.GetComponentIndex(componentTypes[7]);
                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        var entities = chunk.Entities;
                        var t0 = chunk.PackedArray.GetComponentData<T0>(c0, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentData<T1>(c1, componentTypes[1]);
                        var t2 = chunk.PackedArray.GetComponentData<T2>(c2, componentTypes[2]);
                        var t3 = chunk.PackedArray.GetComponentData<T3>(c3, componentTypes[3]);
                        var t4 = chunk.PackedArray.GetComponentData<T4>(c4, componentTypes[4]);
                        var t5 = chunk.PackedArray.GetComponentData<T5>(c5, componentTypes[5]);
                        var t6 = chunk.PackedArray.GetComponentData<T6>(c6, componentTypes[6]);
                        var t7 = chunk.PackedArray.GetComponentData<T7>(c7, componentTypes[7]);
                        view(length, entities, t0, t1, t2, t3, t4, t5, t6, t7);
                    }
                }
            }
        }
        public delegate void ForEachChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8>(int length, NativeReadOnlySlice<uint> entities, NativeSlice<T0> t0, NativeSlice<T1> t1, NativeSlice<T2> t2, NativeSlice<T3> t3, NativeSlice<T4> t4, NativeSlice<T5> t5, NativeSlice<T6> t6, NativeSlice<T7> t7, NativeSlice<T8> t8) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged;
        public unsafe static void ForChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this EntityManager em, ForEachChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8> view)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type };
            var entityArrays = em.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var c0 = array.Specification.GetComponentIndex(componentTypes[0]);
                    var c1 = array.Specification.GetComponentIndex(componentTypes[1]);
                    var c2 = array.Specification.GetComponentIndex(componentTypes[2]);
                    var c3 = array.Specification.GetComponentIndex(componentTypes[3]);
                    var c4 = array.Specification.GetComponentIndex(componentTypes[4]);
                    var c5 = array.Specification.GetComponentIndex(componentTypes[5]);
                    var c6 = array.Specification.GetComponentIndex(componentTypes[6]);
                    var c7 = array.Specification.GetComponentIndex(componentTypes[7]);
                    var c8 = array.Specification.GetComponentIndex(componentTypes[8]);
                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        var entities = chunk.Entities;
                        var t0 = chunk.PackedArray.GetComponentData<T0>(c0, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentData<T1>(c1, componentTypes[1]);
                        var t2 = chunk.PackedArray.GetComponentData<T2>(c2, componentTypes[2]);
                        var t3 = chunk.PackedArray.GetComponentData<T3>(c3, componentTypes[3]);
                        var t4 = chunk.PackedArray.GetComponentData<T4>(c4, componentTypes[4]);
                        var t5 = chunk.PackedArray.GetComponentData<T5>(c5, componentTypes[5]);
                        var t6 = chunk.PackedArray.GetComponentData<T6>(c6, componentTypes[6]);
                        var t7 = chunk.PackedArray.GetComponentData<T7>(c7, componentTypes[7]);
                        var t8 = chunk.PackedArray.GetComponentData<T8>(c8, componentTypes[8]);
                        view(length, entities, t0, t1, t2, t3, t4, t5, t6, t7, t8);
                    }
                }
            }
        }
        public delegate void ForEachChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(int length, NativeReadOnlySlice<uint> entities, NativeSlice<T0> t0, NativeSlice<T1> t1, NativeSlice<T2> t2, NativeSlice<T3> t3, NativeSlice<T4> t4, NativeSlice<T5> t5, NativeSlice<T6> t6, NativeSlice<T7> t7, NativeSlice<T8> t8, NativeSlice<T9> t9) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged;
        public unsafe static void ForChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this EntityManager em, ForEachChunk<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> view)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type, ComponentType<T9>.Type };
            var entityArrays = em.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var c0 = array.Specification.GetComponentIndex(componentTypes[0]);
                    var c1 = array.Specification.GetComponentIndex(componentTypes[1]);
                    var c2 = array.Specification.GetComponentIndex(componentTypes[2]);
                    var c3 = array.Specification.GetComponentIndex(componentTypes[3]);
                    var c4 = array.Specification.GetComponentIndex(componentTypes[4]);
                    var c5 = array.Specification.GetComponentIndex(componentTypes[5]);
                    var c6 = array.Specification.GetComponentIndex(componentTypes[6]);
                    var c7 = array.Specification.GetComponentIndex(componentTypes[7]);
                    var c8 = array.Specification.GetComponentIndex(componentTypes[8]);
                    var c9 = array.Specification.GetComponentIndex(componentTypes[9]);
                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        var entities = chunk.Entities;
                        var t0 = chunk.PackedArray.GetComponentData<T0>(c0, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentData<T1>(c1, componentTypes[1]);
                        var t2 = chunk.PackedArray.GetComponentData<T2>(c2, componentTypes[2]);
                        var t3 = chunk.PackedArray.GetComponentData<T3>(c3, componentTypes[3]);
                        var t4 = chunk.PackedArray.GetComponentData<T4>(c4, componentTypes[4]);
                        var t5 = chunk.PackedArray.GetComponentData<T5>(c5, componentTypes[5]);
                        var t6 = chunk.PackedArray.GetComponentData<T6>(c6, componentTypes[6]);
                        var t7 = chunk.PackedArray.GetComponentData<T7>(c7, componentTypes[7]);
                        var t8 = chunk.PackedArray.GetComponentData<T8>(c8, componentTypes[8]);
                        var t9 = chunk.PackedArray.GetComponentData<T9>(c9, componentTypes[9]);
                        view(length, entities, t0, t1, t2, t3, t4, t5, t6, t7, t8, t9);
                    }
                }
            }
        }
    }
}
