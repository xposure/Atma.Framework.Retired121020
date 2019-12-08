using System;
using System.Linq;

namespace ExtensionGenerator
{
    public class SpecCreate : Command
    {
        public override string Name => "SpecCreate";

        public override string Description => "Generates the Create generics methods.";

        protected override int OnRun()
        {
            Console.WriteLine("namespace Atma.Entities{");
            Console.WriteLine("using System;");
            Console.WriteLine("using Atma;");
            Console.WriteLine("using Atma.Entities;");
            Console.WriteLine("using Atma.Memory;");
            Console.WriteLine("public readonly partial struct EntitySpec{");
            for (var i = 1; i <= 20; i++)
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
            var spanGenerics = genericCount.Range().Select(i => $"Span<T{i}> t{i}");
            var where = genericCount.Range().Select(i => $"where T{i}: unmanaged").ToArray();
            var spanArgs = genericCount.Range().Select(i => $"ref T{i} t{i}");
            var viewArgs = genericCount.Range().Select(i => $"ref t{i}[i]");
            var componentType = genericCount.Range().Select(i => $"ComponentType<T{i}>.Type").ToArray();
            var componentArrays = genericCount.Range().Select(i => $"  var t{i} = chunk.GetComponentData<T{i}>();").ToArray();
            /*
               public static EntitySpec Create<T0>(params IEntitySpecGroup[] groups)
                        where T0 : unmanaged
                        => new EntitySpec(groups, ComponentType<T0>.Type);
            */

            Console.WriteLine($"public static EntitySpec Create<{generics.Join()}>(params IEntitySpecGroup[] groups)");
            Console.WriteLine($"  {where.Join(" ")}");
            Console.WriteLine($"  => new EntitySpec(groups, new [] {{ {componentType.Join()}}} );");
        }
    }
}