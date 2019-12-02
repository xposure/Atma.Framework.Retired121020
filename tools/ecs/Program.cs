using System;
using System.Linq;
using System.IO;
using GlobExpressions;
using McMaster.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace ecs
{
    [Command(Description = "Entity Component System generator.")]
    public class Generator
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Generator>(args);

        [Argument(0, Description = "Manifest file to process.")]
        [Required]
        public string manifest { get; }

        // [Option(Description = "Search pattern, defaults to **/*.json")]
        // public string search { get; } = "**/*.json";

        [Option(Description = "Force rebuild of files.")]
        public bool Force { get; } = false;

        [Option(Description = "Redirect output to console.")]
        public bool console { get; } = false;

        private TextWriter output { get; set; }

        private int _indent = 0;

        private void WriteLine(IEnumerable<string> msgs)
        {
            foreach (var it in msgs)
                WriteLine(it);
        }
        private void WriteLine(string msg)
        {
            output.Write(new string(' ', _indent * 2));
            output.WriteLine(msg);
        }

        private int OnExecute()
        {
            // var project = new Project();
            // project.groups = new List<ProjectGroup>();
            // project.groups.Add(new ProjectGroup() { name = "Render", })


            if (!File.Exists(manifest))
            {
                Console.WriteLine($"Could not find manifest file [{manifest}].");
                return -1;
            }

            using (var fs = File.OpenRead(manifest))
            {
                var dir = Path.GetDirectoryName(manifest);

                var m = Utf8Json.JsonSerializer.Deserialize<Manifest>(fs);
                foreach (var g in m.groups)
                {
                    if (Path.IsPathRooted(g.path) || g.path.Contains(".."))
                    {
                        Console.WriteLine("Path must be relative and a subdirectory.");
                        return -1;
                    }

                    Console.WriteLine(g.name);

                    var workingDir = Path.Combine(dir, g.path);
                    foreach (var file in Glob.Files(workingDir, "*.json", GlobOptions.CaseInsensitive))
                    {
                        if (ProcessFile(m, g, Path.Combine(workingDir, file)) != 0)
                            return -1;
                        Console.WriteLine(" " + file);
                    }


                }
            }

            // foreach (var file in Glob.Files(workingDir, search, GlobOptions.CaseInsensitive))
            // {
            //     if (ProcessFile(file) != 0)
            //         return -1;
            //     //WriteLine(file);
            // }

            return 0;
        }

        protected int ProcessFile(Manifest m, ProjectGroup group, string file)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            while (name.Contains('.'))
                name = Path.GetFileNameWithoutExtension(name);

            var dir = Path.GetDirectoryName(file);
            var dstFile = Path.Combine(dir, name + ".ecs.cs");

            var srcInfo = new FileInfo(file);
            var dstInfo = new FileInfo(dstFile);

            if (!Force && srcInfo.LastWriteTimeUtc == dstInfo.LastWriteTimeUtc)
            {
                Console.WriteLine($"Skipping   [{file}], not modified.");
                return 0;
            }

            Console.WriteLine($"Generating [{file}].");

            output = Console.Out;

            var ns = group.name;
            if (!string.IsNullOrEmpty(m.@namespace))
                ns = m.@namespace + "." + group.name;

            ECSFile ecsFile = null;
            using (var sr = File.OpenRead(file))
                ecsFile = Utf8Json.JsonSerializer.Deserialize<ECSFile>(sr);

            if (string.IsNullOrEmpty(ecsFile.name))
                ecsFile.name = name;

            if (!console)
                output = File.CreateText(dstFile);

            try
            {
                WriteLine($"namespace {ns}{{");
                _indent++;
                WriteLine($"using System;");
                WriteLine($"using Atma.Common;");
                WriteLine($"using Atma.Memory;");
                WriteLine($"using Atma.Entities;");

                WriteLine($"public ref partial struct {ecsFile.name}{{");
                _indent++;
                for (var i = 0; i < ecsFile.variables?.Count; i++)
                {
                    var v = ecsFile.variables[i];
                    WriteLine($"public {v.type} {v.name};");
                }

                for (var i = 0; i < ecsFile.buffers?.Count; i++)
                {
                    var b = ecsFile.buffers[i];
                    WriteLine($"public EntityCommandBuffer {b.name};");
                }


                WriteLine("public ReadOnlySpan<EntityRef> entities;");
                for (var i = 0; i < ecsFile.components?.Count; i++)
                {

                    var c = ecsFile.components[i];
                    var type = $"ReadOnlySpan";
                    if (c.write)
                        type = $"Span";

                    WriteLine($"public {type}<{c.type}> {c.name};");
                }

                WriteLine($"public static void Process(SystemManager sm, EntityManager em) {{");
                _indent++;
                WriteLine($"var system = new {ecsFile.name}();");
                for (var i = 0; i < ecsFile.variables?.Count; i++)
                {
                    var v = ecsFile.variables[i];
                    WriteLine($"system.{v.name} = sm.Variable<{v.name}>(\"{v.name}\");");
                }

                for (var i = 0; i < ecsFile.buffers?.Count; i++)
                {
                    var b = ecsFile.buffers[i];
                    if (b.size > 0)
                        WriteLine($"system.{b.name} = sm.CreateCommandBuffer({b.size});");
                    else
                        WriteLine($"system.{b.name} = sm.CreateCommandBuffer();");
                }

                //WriteComponents(ecsFile);

                for (var i = 0; i < ecsFile.buffers?.Count; i++)
                {
                    var b = ecsFile.buffers[i];
                    if (!b.flush)
                        WriteLine($"system.{b.name}.Execute(em);");
                }
                _indent--;
                WriteLine($"}}");
                WriteLine("partial void Execute(int length);");
                _indent--;
                WriteLine($"}}");

                WriteLine($"public sealed class {ecsFile.name}Processor : ISystem {{");
                _indent++;
                WriteLine($"public string Group => \"{ns}\";");
                WriteLine("private List<Dependency> _dependencies = null;");
                WriteLine("public IEnumerable<Dependency> Dependencies {");
                _indent++;
                WriteLine("if(_dependencies == null) {");
                _indent++;
                WriteLine("_dependencies = new List<Dependency>();");
                for (var i = 0; i < ecsFile.components?.Count; i++)
                {
                    var c = ecsFile.components[i];
                    var type = $"Read";
                    if (c.write)
                        type = $"Write";

                    WriteLine($"_dependencies.Add(new {type}Dependency<{c.type}>());");
                }
                for (var i = 0; i < ecsFile.before?.Count; i++)
                    WriteLine($"_dependencies.Add(new BeforeDependency>(\"{ecsFile.before[i]}\"));");
                for (var i = 0; i < ecsFile.after?.Count; i++)
                    WriteLine($"_dependencies.Add(new AfterDependency>(\"{ecsFile.after[i]}\"));");
                // for (var i = 0; i < group.before?.Count; i++)
                //     WriteLine($"_dependencies.Add(new BeforeDependency>(\"{group.before[i]}\"));");
                // for (var i = 0; i < group.after?.Count; i++)
                //     WriteLine($"_dependencies.Add(new AfterDependency>(\"{group.after[i]}\"));");

                _indent--;
                WriteLine("}");
                WriteLine("return _dependencies;");
                _indent--;
                WriteLine("}");
                WriteLine($"public void Tick(SystemManager systemManager, EntityManager entityManager) => {ecsFile.name}.Process(systemManager, entityManager);");
                _indent--;
                WriteLine($"}}");
                _indent--;
                WriteLine($"}}");

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
                    output.Dispose();
            }
        }

        protected void WriteComponents(ECSFile ecsFile)
        {
            var genericCount = ecsFile.components?.Count ?? 0;
            var generics = genericCount.Range().Select(i => $"T{i}").ToArray();
            var spanGenerics = genericCount.Range().Select(i => $"Span<T{i}> t{i}");
            var where = genericCount.Range().Select(i => $"where T{i}: unmanaged").ToArray();
            var componentType = genericCount.Range().Select(i => $"ComponentType<T{i}>.Type").ToArray();
            var componentIndicies = genericCount.Range().Select(i => $"var c{i} = array.Specification.GetComponentIndex(componentTypes[{i}]);").ToArray();
            var componentArrays = genericCount.Range().Select(i => $"var t{i} = chunk.GetComponentData<T{i}>(c{i}, componentTypes[{i}]);").ToArray();
            var viewArgs = genericCount.Range().Select(i => $"t{i}");

            //WriteLine($"public delegate void ForEachChunk<{generics.Join()}>(int length, ReadOnlySpan<EntityRef> entities, {spanGenerics.Join()}){where.Join(" ")};");
            //WriteLine($"public unsafe static void ForChunk<{generics.Join()}>(this EntityManager em, ForEachChunk<{generics.Join()}> view) ");
            //WriteLine($"  {where.Join(" ")}");
            //WriteLine($"{{");
            //WriteLine($"Span<ComponentType> componentTypes = stackalloc ComponentType[] {{ {componentType.Join()} }};");
            WriteLine($"Span<ComponentType> componentTypes = stackalloc ComponentType[] {{");
            _indent++;
            for (var i = 0; i < ecsFile.components?.Count; i++)
            {
                var c = ecsFile.components[i];

                WriteLine($"ComponentType<{c.type}>.Type" + (i < ecsFile.components.Count - 1 ? "," : ""));
            }
            _indent--;
            WriteLine("}};");
            WriteLine($"var entityArrays = em.EntityArrays;");
            WriteLine($"for (var i = 0; i < entityArrays.Count; i++) {{");
            _indent++;
            WriteLine($"var array = entityArrays[i];");
            WriteLine($"if (array.Specification.HasAll(componentTypes)) {{");
            _indent++;

            WriteLine(componentIndicies);
            WriteLine($"for (var k = 0; k < array.AllChunks.Count; k++) {{");
            _indent++;
            WriteLine($"var chunk = array.AllChunks[k];");

            WriteLine($"system.entities = chunk.Entities;");
            for (var i = 0; i < ecsFile.components?.Count; i++)
            {
                var c = ecsFile.components[i];

                WriteLine($"system.{c.name} = chunk.GetComponentData<{c.type}>(c{i}, componentTypes[{i}]);");
            }
            WriteLine($"system.Execute(chunk.Count);");
            for (var i = 0; i < ecsFile.buffers?.Count; i++)
            {
                var b = ecsFile.buffers[i];
                if (b.flush)
                    WriteLine($"system.{b.name}.Execute(em);");
            }
            _indent--;
            WriteLine($"}}");
            _indent--;
            WriteLine($"}}");
            _indent--;
            WriteLine($"}}");
            //WriteLine($"}}");
        }
    }

    public static class Extensions
    {
        public static IEnumerable<int> Range(this int it)
        {
            for (var i = 0; i < it; i++)
                yield return i;
        }

        public static string Join(this IEnumerable<string> it, string join = ",")
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (var t in it)
            {
                if (!first)
                    sb.Append(join);

                first = false;

                sb.Append(t);
            }
            return sb.ToString();
        }
        //public static void Expand<T>(this T it,  )
    }
}
