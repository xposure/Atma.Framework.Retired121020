using System;
using System.Linq;

namespace ExtensionGenerator
{
    public class ForEach : Command
    {
        public override string Name => "ForEach";

        public override string Description => "Generates the ForEach extension methods.";

        protected override int OnRun()
        {
            Console.WriteLine("namespace Atma{");
            Console.WriteLine("using System;");
            Console.WriteLine("using Atma.Entities;");
            Console.WriteLine("public static class ForEachExtensions{");
            for (var i = 1; i <= 10; i++)
            {
                WriteFunction(i);
            }
            Console.WriteLine("}");
            Console.WriteLine("}");

            return 0;
        }

        protected void WriteFunction(int genericCount)
        {
            var generics = genericCount.Range().Select(i => $"T{i}").ToArray();
            var spanGenerics = genericCount.Range().Select(i => $"ref T{i} t{i}");
            var where = genericCount.Range().Select(i => $"where T{i}: unmanaged").ToArray();
            var componentType = genericCount.Range().Select(i => $"ComponentType<T{i}>.Type").ToArray();
            var componentIndicies = genericCount.Range().Select(i => $"      var c{i} = array.Specification.GetComponentIndex(componentTypes[{i}]);").ToArray();
            var componentArrays = genericCount.Range().Select(i => $"        var t{i} = chunk.PackedArray.GetComponentSpan<T{i}>(c{i}, componentTypes[{i}]);").ToArray();
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
            Console.WriteLine($"          view(entities[j],{viewArgs.Join()}); ");
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