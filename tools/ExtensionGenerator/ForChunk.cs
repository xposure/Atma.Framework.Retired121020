using System;
using System.Linq;

namespace ExtensionGenerator
{
    public class ForChunk : Command
    {
        public override string Name => "ForChunk";

        public override string Description => "Generates the ForChunk extension methods.";

        protected override int OnRun()
        {
            Console.WriteLine("using System;");
            Console.WriteLine("using Atma.Entities;");
            Console.WriteLine("using Atma.Memory;");
            Console.WriteLine("public static class ForChunkExtensions{");
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
            var componentType = genericCount.Range().Select(i => $"ComponentType<T{i}>.Type").ToArray();
            var componentIndicies = genericCount.Range().Select(i => $"      var c{i} = chunkList.Specification.GetComponentIndex(componentTypes[{i}]);").ToArray();
            var componentArrays = genericCount.Range().Select(i => $"        var t{i} = chunk.GetComponentData<T{i}>(c{i}, componentTypes[{i}]);").ToArray();
            var viewArgs = genericCount.Range().Select(i => $"t{i}");

            Console.WriteLine($"public delegate void ForEachChunk<{generics.Join()}>(int length, ReadOnlySpan<EntityRef> entities, {spanGenerics.Join()}){where.Join(" ")};");
            Console.WriteLine($"public unsafe static void ForChunk<{generics.Join()}>(this EntityChunkList chunkList, ForEachChunk<{generics.Join()}> view) ");
            Console.WriteLine($"  {where.Join(" ")}");
            Console.WriteLine($"{{");
            Console.WriteLine($"  Span<ComponentType> componentTypes = stackalloc ComponentType[] {{ {componentType.Join()} }};");
            Console.WriteLine($"  chunkList.ForChunk(componentTypes, view);");
            Console.WriteLine($"}}");
            Console.WriteLine($"internal static void ForChunk<{generics.Join()}>(this EntityChunkList chunkList, Span<ComponentType> componentTypes, ForEachChunk<{generics.Join()}> view) ");
            Console.WriteLine($"  {where.Join(" ")}");
            Console.WriteLine($"{{");
            Console.WriteLine($"{componentIndicies.Join("\n")}");
            Console.WriteLine($"      for (var k = 0; k < chunkList.ChunkCount; k++) {{");
            Console.WriteLine($"        var chunk = chunkList[k];");
            Console.WriteLine($"        var length = chunk.Count;");
            Console.WriteLine($"        var entities = chunk.Entities;");
            Console.WriteLine($"{componentArrays.Join("\n")}");
            Console.WriteLine($"        view(length,entities,{viewArgs.Join()}); ");
            Console.WriteLine($"      }}");
            Console.WriteLine($"}}");
            Console.WriteLine($"public static void ForChunk<{generics.Join()}>(this EntityManager em, ForEachChunk<{generics.Join()}> view) ");
            Console.WriteLine($"  {where.Join(" ")}");
            Console.WriteLine($"{{");
            Console.WriteLine($"  Span<ComponentType> componentTypes = stackalloc ComponentType[] {{ {componentType.Join()} }};");
            Console.WriteLine($"  var arrays = em.EntityArrays.FindSmallest(componentTypes);");
            Console.WriteLine($"  if(arrays != null)");
            Console.WriteLine($"    foreach (var array in arrays)");
            Console.WriteLine($"      if (array.Specification.HasAll(componentTypes))");
            Console.WriteLine($"        array.ForChunk(componentTypes, view);");
            Console.WriteLine($"}}");

            // Console.WriteLine($"public delegate void ForEachChunk<{generics.Join()}>(int length, ReadOnlySpan<EntityRef> entities, {spanGenerics.Join()}){where.Join(" ")};");
            // Console.WriteLine($"public unsafe static void ForChunk<{generics.Join()}>(this EntityManager em, ForEachChunk<{generics.Join()}> view) ");
            // Console.WriteLine($"  {where.Join(" ")}");
            // Console.WriteLine($"{{");
            // Console.WriteLine($"  Span<ComponentType> componentTypes = stackalloc ComponentType[] {{ {componentType.Join()} }};");
            // Console.WriteLine($"  var entityArrays = em.EntityArrays;");
            // Console.WriteLine($"  for (var i = 0; i < entityArrays.Count; i++) {{");
            // Console.WriteLine($"    var array = entityArrays[i];");
            // Console.WriteLine($"    if (array.Specification.HasAll(componentTypes)) {{");
            // Console.WriteLine($"{componentIndicies.Join("\n")}");
            // Console.WriteLine($"      for (var k = 0; k < array.AllChunks.Count; k++) {{");
            // Console.WriteLine($"        var chunk = array.AllChunks[k];");
            // Console.WriteLine($"        var length = chunk.Count;");
            // Console.WriteLine($"        var entities = chunk.Entities;");
            // Console.WriteLine($"{componentArrays.Join("\n")}");
            // Console.WriteLine($"        view(length,entities,{viewArgs.Join()}); ");
            // Console.WriteLine($"      }}");
            // Console.WriteLine($"    }}");
            // Console.WriteLine($"  }}");
            // Console.WriteLine($"}}");
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