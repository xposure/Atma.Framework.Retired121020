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
                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        var t0 = chunk.PackedArray.GetComponentSpan<T0>();
                        for (var j = 0; j < length; j++)
                        {
                            var entity = chunk.GetEntity(j);
                            view(entity, ref t0[j]);
                        }
                    }
                }
            }
        }
    }
}