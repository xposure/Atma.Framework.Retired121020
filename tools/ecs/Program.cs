using System;
using System.Linq;
using System.IO;
using GlobExpressions;
using McMaster.Extensions.CommandLineUtils;

namespace ecs
{
    [Command(Description = "Entity Component System generator.")]
    class Program
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Argument(0, Description = "Working directory, defaults to current directory.")]
        public string workingDir { get; } = Environment.CurrentDirectory;

        [Option(Description = "Search pattern, defaults to **/*.json")]
        public string search { get; } = "**/*.json";

        [Option(Description = "Force rebuild of files.")]
        public bool Force { get; } = false;

        [Option(Description = "Redirect output to console.")]
        public bool console { get; } = false;

        private int OnExecute()
        {
            if (!Directory.Exists(workingDir))
            {
                Console.WriteLine($"Could not find directory [{workingDir}].");
                return -1;
            }

            foreach (var file in Glob.Files(workingDir, search, GlobOptions.CaseInsensitive))
            {
                if (ProcessFile(file) != 0)
                    return -1;
                //Console.WriteLine(file);
            }

            return 0;
        }

        protected int ProcessFile(string file)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            while (name.Contains('.'))
                name = Path.GetFileNameWithoutExtension(name);

            var dir = Path.GetDirectoryName(file);
            var output = Path.Combine(dir, name + ".ecs.cs");

            var srcInfo = new FileInfo(file);
            var dstInfo = new FileInfo(output);

            if (!Force && srcInfo.LastWriteTimeUtc == dstInfo.LastWriteTimeUtc)
            {
                Console.WriteLine($"Skipping   [{file}], not modified.");
                return 0;
            }

            Console.WriteLine($"Generating [{file}].");

            TextWriter outFile = Console.Out;

            ECSFile ecsFile = null;
            using (var sr = File.OpenRead(file))
                ecsFile = Utf8Json.JsonSerializer.Deserialize<ECSFile>(sr);

            Console.WriteLine(ecsFile.name);

            if (string.IsNullOrEmpty(ecsFile.name))
                ecsFile.name = name;

            if (!console)
                outFile = File.CreateText(output);

            try
            {
                //Console.WriteLine("namespace Atma{");
                outFile.WriteLine($"using System;");
                outFile.WriteLine($"using Atma.Common;");
                outFile.WriteLine($"using Atma.Memory;");
                outFile.WriteLine($"using Atma.Entities;");

                outFile.WriteLine($"public ref struct {ecsFile.name}{{");

                for (var i = 0; i < ecsFile.components.Count; i++)
                {

                    var c = ecsFile.components[i];
                    var type = $"ReadOnlySpan";
                    if (c.write)
                        type = $"Span";

                    outFile.WriteLine($"  public {type}<{c.type}> {c.name};");

                    //WriteFunction(i);
                }
                outFile.WriteLine($"}}");
                //outFile.WriteLine($"}}");

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return -1;
            }
            finally
            {
                if (!console)
                    outFile.Dispose();
            }
        }

        // protected void WriteFunction(int genericCount)
        // {
        //     var generics = genericCount.Range().Select(i => $"T{i}").ToArray();
        //     var spanGenerics = genericCount.Range().Select(i => $"Span<T{i}> t{i}");
        //     var where = genericCount.Range().Select(i => $"where T{i}: unmanaged").ToArray();
        //     var componentType = genericCount.Range().Select(i => $"ComponentType<T{i}>.Type").ToArray();
        //     var componentIndicies = genericCount.Range().Select(i => $"      var c{i} = array.Specification.GetComponentIndex(componentTypes[{i}]);").ToArray();
        //     var componentArrays = genericCount.Range().Select(i => $"        var t{i} = chunk.GetComponentData<T{i}>(c{i}, componentTypes[{i}]);").ToArray();
        //     var viewArgs = genericCount.Range().Select(i => $"t{i}");

        //     Console.WriteLine($"public delegate void ForEachChunk<{generics.Join()}>(int length, ReadOnlySpan<EntityRef> entities, {spanGenerics.Join()}){where.Join(" ")};");
        //     Console.WriteLine($"public unsafe static void ForChunk<{generics.Join()}>(this EntityManager em, ForEachChunk<{generics.Join()}> view) ");
        //     Console.WriteLine($"  {where.Join(" ")}");
        //     Console.WriteLine($"{{");
        //     Console.WriteLine($"  Span<ComponentType> componentTypes = stackalloc ComponentType[] {{ {componentType.Join()} }};");
        //     Console.WriteLine($"  var entityArrays = em.EntityArrays;");
        //     Console.WriteLine($"  for (var i = 0; i < entityArrays.Count; i++) {{");
        //     Console.WriteLine($"    var array = entityArrays[i];");
        //     Console.WriteLine($"    if (array.Specification.HasAll(componentTypes)) {{");
        //     Console.WriteLine($"{componentIndicies.Join("\n")}");
        //     Console.WriteLine($"      for (var k = 0; k < array.AllChunks.Count; k++) {{");
        //     Console.WriteLine($"        var chunk = array.AllChunks[k];");
        //     Console.WriteLine($"        var length = chunk.Count;");
        //     Console.WriteLine($"        var entities = chunk.Entities;");
        //     Console.WriteLine($"{componentArrays.Join("\n")}");
        //     Console.WriteLine($"        view(length,entities,{viewArgs.Join()}); ");
        //     Console.WriteLine($"      }}");
        //     Console.WriteLine($"    }}");
        //     Console.WriteLine($"  }}");
        //     Console.WriteLine($"}}");
        // }
    }
}
