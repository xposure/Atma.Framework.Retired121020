namespace Atma
{
    using System;
    using Atma.Entities;
    public static class ForEachExtensions
    {
        public delegate void ForEachEntity<T0>(uint entity, ref T0 t0) where T0 : unmanaged;
        public unsafe static void ForEntity<T0>(this EntityManager em, ForEachEntity<T0> view)
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
                        var entities = chunk.Entities.AsSpan();
                        var t0 = chunk.PackedArray.GetComponentSpan<T0>(c0, componentTypes[0]);
                        for (var j = 0; j < length; j++)
                            view(entities[j], ref t0[j]);
                    }
                }
            }
        }
        public delegate void ForEachEntity<T0, T1>(uint entity, ref T0 t0, ref T1 t1) where T0 : unmanaged where T1 : unmanaged;
        public unsafe static void ForEntity<T0, T1>(this EntityManager em, ForEachEntity<T0, T1> view)
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
                        var entities = chunk.Entities.AsSpan();
                        var t0 = chunk.PackedArray.GetComponentSpan<T0>(c0, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentSpan<T1>(c1, componentTypes[1]);
                        for (var j = 0; j < length; j++)
                            view(entities[j], ref t0[j], ref t1[j]);
                    }
                }
            }
        }
        public delegate void ForEachEntity<T0, T1, T2>(uint entity, ref T0 t0, ref T1 t1, ref T2 t2) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged;
        public unsafe static void ForEntity<T0, T1, T2>(this EntityManager em, ForEachEntity<T0, T1, T2> view)
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
                        var entities = chunk.Entities.AsSpan();
                        var t0 = chunk.PackedArray.GetComponentSpan<T0>(c0, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentSpan<T1>(c1, componentTypes[1]);
                        var t2 = chunk.PackedArray.GetComponentSpan<T2>(c2, componentTypes[2]);
                        for (var j = 0; j < length; j++)
                            view(entities[j], ref t0[j], ref t1[j], ref t2[j]);
                    }
                }
            }
        }
        public delegate void ForEachEntity<T0, T1, T2, T3>(uint entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged;
        public unsafe static void ForEntity<T0, T1, T2, T3>(this EntityManager em, ForEachEntity<T0, T1, T2, T3> view)
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
                        var entities = chunk.Entities.AsSpan();
                        var t0 = chunk.PackedArray.GetComponentSpan<T0>(c0, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentSpan<T1>(c1, componentTypes[1]);
                        var t2 = chunk.PackedArray.GetComponentSpan<T2>(c2, componentTypes[2]);
                        var t3 = chunk.PackedArray.GetComponentSpan<T3>(c3, componentTypes[3]);
                        for (var j = 0; j < length; j++)
                            view(entities[j], ref t0[j], ref t1[j], ref t2[j], ref t3[j]);
                    }
                }
            }
        }
        public delegate void ForEachEntity<T0, T1, T2, T3, T4>(uint entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged;
        public unsafe static void ForEntity<T0, T1, T2, T3, T4>(this EntityManager em, ForEachEntity<T0, T1, T2, T3, T4> view)
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
                        var entities = chunk.Entities.AsSpan();
                        var t0 = chunk.PackedArray.GetComponentSpan<T0>(c0, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentSpan<T1>(c1, componentTypes[1]);
                        var t2 = chunk.PackedArray.GetComponentSpan<T2>(c2, componentTypes[2]);
                        var t3 = chunk.PackedArray.GetComponentSpan<T3>(c3, componentTypes[3]);
                        var t4 = chunk.PackedArray.GetComponentSpan<T4>(c4, componentTypes[4]);
                        for (var j = 0; j < length; j++)
                            view(entities[j], ref t0[j], ref t1[j], ref t2[j], ref t3[j], ref t4[j]);
                    }
                }
            }
        }
        public delegate void ForEachEntity<T0, T1, T2, T3, T4, T5>(uint entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged;
        public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5>(this EntityManager em, ForEachEntity<T0, T1, T2, T3, T4, T5> view)
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
                        var entities = chunk.Entities.AsSpan();
                        var t0 = chunk.PackedArray.GetComponentSpan<T0>(c0, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentSpan<T1>(c1, componentTypes[1]);
                        var t2 = chunk.PackedArray.GetComponentSpan<T2>(c2, componentTypes[2]);
                        var t3 = chunk.PackedArray.GetComponentSpan<T3>(c3, componentTypes[3]);
                        var t4 = chunk.PackedArray.GetComponentSpan<T4>(c4, componentTypes[4]);
                        var t5 = chunk.PackedArray.GetComponentSpan<T5>(c5, componentTypes[5]);
                        for (var j = 0; j < length; j++)
                            view(entities[j], ref t0[j], ref t1[j], ref t2[j], ref t3[j], ref t4[j], ref t5[j]);
                    }
                }
            }
        }
        public delegate void ForEachEntity<T0, T1, T2, T3, T4, T5, T6>(uint entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged;
        public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5, T6>(this EntityManager em, ForEachEntity<T0, T1, T2, T3, T4, T5, T6> view)
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
                        var entities = chunk.Entities.AsSpan();
                        var t0 = chunk.PackedArray.GetComponentSpan<T0>(c0, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentSpan<T1>(c1, componentTypes[1]);
                        var t2 = chunk.PackedArray.GetComponentSpan<T2>(c2, componentTypes[2]);
                        var t3 = chunk.PackedArray.GetComponentSpan<T3>(c3, componentTypes[3]);
                        var t4 = chunk.PackedArray.GetComponentSpan<T4>(c4, componentTypes[4]);
                        var t5 = chunk.PackedArray.GetComponentSpan<T5>(c5, componentTypes[5]);
                        var t6 = chunk.PackedArray.GetComponentSpan<T6>(c6, componentTypes[6]);
                        for (var j = 0; j < length; j++)
                            view(entities[j], ref t0[j], ref t1[j], ref t2[j], ref t3[j], ref t4[j], ref t5[j], ref t6[j]);
                    }
                }
            }
        }
        public delegate void ForEachEntity<T0, T1, T2, T3, T4, T5, T6, T7>(uint entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged;
        public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5, T6, T7>(this EntityManager em, ForEachEntity<T0, T1, T2, T3, T4, T5, T6, T7> view)
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
                        var entities = chunk.Entities.AsSpan();
                        var t0 = chunk.PackedArray.GetComponentSpan<T0>(c0, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentSpan<T1>(c1, componentTypes[1]);
                        var t2 = chunk.PackedArray.GetComponentSpan<T2>(c2, componentTypes[2]);
                        var t3 = chunk.PackedArray.GetComponentSpan<T3>(c3, componentTypes[3]);
                        var t4 = chunk.PackedArray.GetComponentSpan<T4>(c4, componentTypes[4]);
                        var t5 = chunk.PackedArray.GetComponentSpan<T5>(c5, componentTypes[5]);
                        var t6 = chunk.PackedArray.GetComponentSpan<T6>(c6, componentTypes[6]);
                        var t7 = chunk.PackedArray.GetComponentSpan<T7>(c7, componentTypes[7]);
                        for (var j = 0; j < length; j++)
                            view(entities[j], ref t0[j], ref t1[j], ref t2[j], ref t3[j], ref t4[j], ref t5[j], ref t6[j], ref t7[j]);
                    }
                }
            }
        }
        public delegate void ForEachEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8>(uint entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged;
        public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this EntityManager em, ForEachEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8> view)
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
                        var entities = chunk.Entities.AsSpan();
                        var t0 = chunk.PackedArray.GetComponentSpan<T0>(c0, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentSpan<T1>(c1, componentTypes[1]);
                        var t2 = chunk.PackedArray.GetComponentSpan<T2>(c2, componentTypes[2]);
                        var t3 = chunk.PackedArray.GetComponentSpan<T3>(c3, componentTypes[3]);
                        var t4 = chunk.PackedArray.GetComponentSpan<T4>(c4, componentTypes[4]);
                        var t5 = chunk.PackedArray.GetComponentSpan<T5>(c5, componentTypes[5]);
                        var t6 = chunk.PackedArray.GetComponentSpan<T6>(c6, componentTypes[6]);
                        var t7 = chunk.PackedArray.GetComponentSpan<T7>(c7, componentTypes[7]);
                        var t8 = chunk.PackedArray.GetComponentSpan<T8>(c8, componentTypes[8]);
                        for (var j = 0; j < length; j++)
                            view(entities[j], ref t0[j], ref t1[j], ref t2[j], ref t3[j], ref t4[j], ref t5[j], ref t6[j], ref t7[j], ref t8[j]);
                    }
                }
            }
        }
        public delegate void ForEachEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(uint entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged;
        public unsafe static void ForEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this EntityManager em, ForEachEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> view)
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
                        var entities = chunk.Entities.AsSpan();
                        var t0 = chunk.PackedArray.GetComponentSpan<T0>(c0, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentSpan<T1>(c1, componentTypes[1]);
                        var t2 = chunk.PackedArray.GetComponentSpan<T2>(c2, componentTypes[2]);
                        var t3 = chunk.PackedArray.GetComponentSpan<T3>(c3, componentTypes[3]);
                        var t4 = chunk.PackedArray.GetComponentSpan<T4>(c4, componentTypes[4]);
                        var t5 = chunk.PackedArray.GetComponentSpan<T5>(c5, componentTypes[5]);
                        var t6 = chunk.PackedArray.GetComponentSpan<T6>(c6, componentTypes[6]);
                        var t7 = chunk.PackedArray.GetComponentSpan<T7>(c7, componentTypes[7]);
                        var t8 = chunk.PackedArray.GetComponentSpan<T8>(c8, componentTypes[8]);
                        var t9 = chunk.PackedArray.GetComponentSpan<T9>(c9, componentTypes[9]);
                        for (var j = 0; j < length; j++)
                            view(entities[j], ref t0[j], ref t1[j], ref t2[j], ref t3[j], ref t4[j], ref t5[j], ref t6[j], ref t7[j], ref t8[j], ref t9[j]);
                    }
                }
            }
        }
    }
}
