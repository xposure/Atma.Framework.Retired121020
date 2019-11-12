namespace Atma
{
    using System;
    using Atma.Entities;

    public static class EntityManagerExtensions
    {
        public delegate void ForEachView<T0>(uint entity, ref T0 t0) where T0 : unmanaged;

        public unsafe static void ForEach<T0>(this EntityManager em, ForEachView<T0> view)
              where T0 : unmanaged
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] {
                ComponentType<T0>.Type
            };

            var entityArrays = em.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var t0i = -1;
                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        if (t0i == -1) t0i = chunk.PackedArray.GetComponentIndex(componentTypes[0]);
                        var entities = chunk.Entities;
                        var t0 = chunk.PackedArray.GetComponentSpan<T0>(t0i, componentTypes[0]);

                        for (var j = 0; j < length; j++)
                        {
                            view(entities[j], ref t0[j]);
                        }
                    }
                }
            }
        }
        public delegate void ForEachViewNoEntity<T0>(ref T0 t0) where T0 : unmanaged;

        public unsafe static void ForEach<T0>(this EntityManager em, ForEachViewNoEntity<T0> view)
              where T0 : unmanaged
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] {
                ComponentType<T0>.Type
            };

            var entityArrays = em.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var t0i = -1;
                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        if (t0i == -1) t0i = chunk.PackedArray.GetComponentIndex(componentTypes[0]);
                        var t0 = chunk.PackedArray.GetComponentSpan<T0>(t0i, componentTypes[0]);

                        for (var j = 0; j < length; j++)
                        {
                            view(ref t0[j]);
                        }
                    }
                }
            }
        }


        public delegate void ForEachView<T0, T1>(uint entity, ref T0 t0, ref T1 t1) where T0 : unmanaged where T1 : unmanaged;

        public unsafe static void ForEach<T0, T1>(this EntityManager em, ForEachView<T0, T1> view)
              where T0 : unmanaged
              where T1 : unmanaged
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] {
                ComponentType<T0>.Type,
                ComponentType<T1>.Type
            };

            var entityArrays = em.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var t0i = -1;
                    var t1i = -1;

                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        if (t0i == -1) t0i = chunk.PackedArray.GetComponentIndex(componentTypes[0]);
                        if (t1i == -1) t1i = chunk.PackedArray.GetComponentIndex(componentTypes[1]);

                        var entities = chunk.Entities;
                        var t0 = chunk.PackedArray.GetComponentSpan<T0>(t0i, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentSpan<T1>(t1i, componentTypes[1]);
                        for (var j = 0; j < length; j++)
                        {
                            view(entities[j], ref t0[j], ref t1[j]);
                        }
                    }
                }
            }
        }

        public delegate void ForEachChunk<T0, T1>(int length, ReadOnlySpan<uint> entities, Span<T0> t0, Span<T1> t1) where T0 : unmanaged where T1 : unmanaged;
        public unsafe static void ForChunk<T0, T1>(this EntityManager em, ForEachChunk<T0, T1> view)
             where T0 : unmanaged
             where T1 : unmanaged
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] {
                ComponentType<T0>.Type,
                ComponentType<T1>.Type
            };

            var entityArrays = em.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var t0i = -1;
                    var t1i = -1;

                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        if (t0i == -1) t0i = chunk.PackedArray.GetComponentIndex(componentTypes[0]);
                        if (t1i == -1) t1i = chunk.PackedArray.GetComponentIndex(componentTypes[1]);

                        var entities = chunk.Entities.AsSpan();
                        var t0 = chunk.PackedArray.GetComponentSpan<T0>(t0i, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentSpan<T1>(t1i, componentTypes[1]);
                        view(length, entities, t0, t1);
                    }
                }
            }
        }

        public delegate void ForEachViewNoEntity<T0, T1>(ref T0 t0, ref T1 t1) where T0 : unmanaged where T1 : unmanaged;
        public unsafe static void ForEach<T0, T1>(this EntityManager em, ForEachViewNoEntity<T0, T1> view)
           where T0 : unmanaged
           where T1 : unmanaged
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] {
                ComponentType<T0>.Type,
                ComponentType<T1>.Type
            };

            var entityArrays = em.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var t0i = -1;
                    var t1i = -1;

                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        if (t0i == -1) t0i = chunk.PackedArray.GetComponentIndex(componentTypes[0]);
                        if (t1i == -1) t1i = chunk.PackedArray.GetComponentIndex(componentTypes[1]);

                        var t0 = chunk.PackedArray.GetComponentSpan<T0>(t0i, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentSpan<T1>(t1i, componentTypes[1]);
                        for (var j = 0; j < length; j++)
                        {
                            view(ref t0[j], ref t1[j]);
                        }
                    }
                }
            }
        }
    }
}