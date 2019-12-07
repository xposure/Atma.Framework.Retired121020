using System;
using System.Linq;

namespace ExtensionGenerator
{
    public class ForEach : Command
    {
        public override string Name => "ForEntity";

        public override string Description => "Generates the ForEntity extension methods.";

        protected override int OnRun()
        {
            Console.WriteLine("using System;");
            Console.WriteLine("using Atma;");
            Console.WriteLine("using Atma.Entities;");
            Console.WriteLine("using Atma.Memory;");
            Console.WriteLine("public static class ForEntityExtensions{");
            for (var i = 1; i <= 10; i++)
            {
                WriteFunction(i);
            }
            Console.WriteLine("}");

            return 0;
        }

        protected void WriteFunction(int genericCount)
        {
            var generics = genericCount.Range().Select(i => $"T{i}").ToArray();
            var spanGenerics = genericCount.Range().Select(i => $"Span<T{i}> t{i}");
            var where = genericCount.Range().Select(i => $"where T{i}: unmanaged").ToArray();
            var spanArgs = genericCount.Range().Select(i => $"ref T{i} t{i}");
            var viewArgs = genericCount.Range().Select(i => $"ref t{i}[i]");
            var componentType = genericCount.Range().Select(i => $"ComponentType<T{i}>.Type").ToArray();
            var componentArrays = genericCount.Range().Select(i => $"  var t{i} = chunk.GetComponentData<T{i}>();").ToArray();

            Console.WriteLine($"public delegate void ForEachEntity<{generics.Join()}>(uint entity, {spanArgs.Join()}){where.Join(" ")};");
            Console.WriteLine($"public unsafe static void ForEntity<{generics.Join()}>(this EntityManager em, ForEachEntity<{generics.Join()}> view) ");
            Console.WriteLine($"  {where.Join(" ")}");
            Console.WriteLine($"{{");
            Console.WriteLine($"  Span<ComponentType> componentTypes = stackalloc ComponentType[] {{ {componentType.Join()} }};");
            Console.WriteLine($"  var arrays = em.EntityArrays.FindSmallest(componentTypes);");
            Console.WriteLine($"  if(arrays != null)");
            Console.WriteLine($"    foreach (var array in arrays) {{");
            Console.WriteLine($"      if(array.Specification.HasAll(componentTypes))");
            Console.WriteLine($"        array.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, {spanGenerics.Join()}) => {{");
            Console.WriteLine($"          for (var i = 0; i < length; i++)");
            Console.WriteLine($"            view(entities[i].ID, {viewArgs.Join()});");
            Console.WriteLine($"        }});");
            Console.WriteLine($"  }}");
            Console.WriteLine($"}}");
            Console.WriteLine($"public unsafe static void ForEntity<{generics.Join()}>(this EntityChunkList chunkList, ForEachEntity<{generics.Join()}> view) ");
            Console.WriteLine($"  {where.Join(" ")}");
            Console.WriteLine($"{{");
            Console.WriteLine($"  Span<ComponentType> componentTypes = stackalloc ComponentType[] {{ {componentType.Join()} }};");
            Console.WriteLine($"  Assert.EqualTo(chunkList.Specification.HasAll(componentTypes), true);");
            Console.WriteLine($"  chunkList.ForChunk(componentTypes, (int length, ReadOnlySpan<EntityRef> entities, {spanGenerics.Join()}) => {{");
            Console.WriteLine($"    for (var i = 0; i < length; i++)");
            Console.WriteLine($"      view(entities[i].ID, {viewArgs.Join()});");
            Console.WriteLine($"  }});");
            Console.WriteLine($"}}");
            Console.WriteLine($"public unsafe static void ForEntity<{generics.Join()}>(this EntityChunk chunk, ForEachEntity<{generics.Join()}> view) ");
            Console.WriteLine($"  {where.Join(" ")}");
            Console.WriteLine($"{{");
            Console.WriteLine($"  Span<ComponentType> componentTypes = stackalloc ComponentType[] {{ {componentType.Join()} }};");
            Console.WriteLine($"  Assert.EqualTo(chunk.Specification.HasAll(componentTypes), true);");
            Console.WriteLine($"  var length = chunk.Count;");
            Console.WriteLine($"  var entities = chunk.Entities;");
            Console.WriteLine($"{componentArrays.Join("\n")}");
            Console.WriteLine($"    for (var i = 0; i < length; i++)");
            Console.WriteLine($"      view(entities[i].ID, {viewArgs.Join()});");
            Console.WriteLine($"}}");


            /*

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


            */


            // Console.WriteLine($"public delegate void ForEachEntity<{generics.Join()}>(uint entity, {spanArgs.Join()}){where.Join(" ")};");
            // Console.WriteLine($"public unsafe static void ForEntity<{generics.Join()}>(this EntityManager em, ForEachEntity<{generics.Join()}> view) ");
            // Console.WriteLine($"  {where.Join(" ")}");
            // Console.WriteLine($"{{");
            // Console.WriteLine($"  em.ForChunk((int length, ReadOnlySpan<EntityRef> entities, {spanGenerics.Join()}) => {{");
            // Console.WriteLine($"    for (var i = 0; i < length; i++)");
            // Console.WriteLine($"      view(entities[i].ID, {viewArgs.Join()});");
            // Console.WriteLine($"  }});");
            // Console.WriteLine($"}}");



            // public unsafe static void ForChunkEntity<T0, T1>(this EntityManager em, ForEachExtensions.ForEachEntity<T0, T1> view)
            //   where T0 : unmanaged where T1 : unmanaged
            // {
            //     em.ForChunk((int length, ReadOnlySpan<uint> entities, Span<T0> t0s, Span<T1> t1s) =>
            //     {
            //         for (var i = 0; i < length; i++)
            //             view(entities[i], ref t0s[i], ref t1s[i]);
            //     });
            // }            
        }

        protected void WriteFunctionOld(int genericCount)
        {
            var generics = genericCount.Range().Select(i => $"T{i}").ToArray();
            var spanGenerics = genericCount.Range().Select(i => $"ref T{i} t{i}");
            var where = genericCount.Range().Select(i => $"where T{i}: unmanaged").ToArray();
            var componentType = genericCount.Range().Select(i => $"ComponentType<T{i}>.Type").ToArray();
            var componentIndicies = genericCount.Range().Select(i => $"      var c{i} = array.Specification.GetComponentIndex(componentTypes[{i}]);").ToArray();
            var componentArrays = genericCount.Range().Select(i => $"        var t{i} = chunk.GetComponentSpan<T{i}>(c{i}, componentTypes[{i}]);").ToArray();
            var viewArgs = genericCount.Range().Select(i => $"ref t{i}[j]");

            Console.WriteLine($"public delegate void ForEachEntity<{generics.Join()}>(uint entity, {spanGenerics.Join()}){where.Join(" ")};");
            Console.WriteLine($"public unsafe static void ForEntity<{generics.Join()}>(this EntityManager em, ForEachEntity<{generics.Join()}> view) ");
            Console.WriteLine($"  {where.Join(" ")}");
            Console.WriteLine($"{{");
            Console.WriteLine($"  Span<ComponentType> componentTypes = stackalloc ComponentType[] {{ {componentType.Join()} }};");
            Console.WriteLine($"  var entityArrays = em.EntityArrays;");
            Console.WriteLine($"  for (var i = 0; i < entityArrays.Count; i++) {{");
            Console.WriteLine($"    var array = entityArrays[i];");
            Console.WriteLine($"    if (array.Specification.HasAll(componentTypes)) {{");
            Console.WriteLine($"{componentIndicies.Join("\n")}");
            Console.WriteLine($"      for (var k = 0; k < array.AllChunks.Count; k++) {{");
            Console.WriteLine($"        var chunk = array.AllChunks[k];");
            Console.WriteLine($"        var length = chunk.Count;");
            Console.WriteLine($"        var entities = chunk.Entities.AsSpan();");
            Console.WriteLine($"{componentArrays.Join("\n")}");
            Console.WriteLine($"        for (var j = 0; j < length; j++)");
            Console.WriteLine($"          view(entities[j].ID,{viewArgs.Join()}); ");
            Console.WriteLine($"      }}");
            Console.WriteLine($"    }}");
            Console.WriteLine($"  }}");
            Console.WriteLine($"}}");
        }
    }


    //             for (var k = 0; k < array.AllChunks.Count; k++)
    //             {
    //                 var chunk = array.AllChunks[k];
    //                 var length = chunk.Count;

    //                 var entities = chunk.Entities.AsSpan();
    //                 var t0 = chunk.PackedArray.GetComponentSpan<T0>(t0i, componentTypes[0]);
    //                 var t1 = chunk.PackedArray.GetComponentSpan<T1>(t1i, componentTypes[1]);
    //                 view(length,entities,t0, t1);
    //             }
    //         }
    //     }
    // }
}