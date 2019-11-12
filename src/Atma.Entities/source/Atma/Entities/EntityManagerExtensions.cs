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
                    var t0t = ComponentType<T0>.Type;
                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        if (t0i == -1) t0i = chunk.PackedArray.GetComponentIndex(t0t);
                        var t0 = chunk.PackedArray.GetComponentSpan<T0>(t0i, t0t);

                        for (var j = 0; j < length; j++)
                        {
                            var entity = chunk.GetEntity(j);
                            view(entity, ref t0[j]);
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
                ComponentType<T0>.Type
            };

            var entityArrays = em.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var t0i = -1;
                    var t1i = -1;
                    var t0t = ComponentType<T0>.Type;
                    var t1t = ComponentType<T1>.Type;

                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        if (t0i == -1) t0i = chunk.PackedArray.GetComponentIndex(t0t);
                        if (t1i == -1) t1i = chunk.PackedArray.GetComponentIndex(t1t);

                        var t0 = chunk.PackedArray.GetComponentSpan<T0>(t0i, t0t);
                        var t1 = chunk.PackedArray.GetComponentSpan<T1>(t1i, t1t);
                        for (var j = 0; j < length; j++)
                        {
                            var entity = chunk.GetEntity(j);
                            view(entity, ref t0[j], ref t1[j]);
                        }
                    }
                }
            }
        }
    }
}