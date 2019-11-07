namespace PatchPackages
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    class Program
    {
        static async Task<int> ProcessCsProj(string file, string gitVersion)
        {
            string xml = await File.ReadAllTextAsync(file);
            var csproj = XDocument.Parse(xml);
            foreach (var itemgroup in csproj.Root.Elements("ItemGroup"))
            {
                foreach (var pr in itemgroup.Elements("PackageReference"))
                {
                    var include = pr.Attribute("Include");
                    var version = pr.Attribute("Version");

                    if (include != null && version != null)
                    {
                        if (include.Value.StartsWith("Atma.", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Console.WriteLine($"Patching package reference {include.Value} to version {gitVersion}");
                            version.Value = gitVersion;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Could not find include and version attribute on package reference...");
                        Console.WriteLine(pr.ToString());
                    }
                }
            }


            xml = csproj.ToString(SaveOptions.None);
            await File.WriteAllTextAsync(file, xml);

            return 0;
        }

        static async Task<int> ProcessNuSpec(string file, string gitVersion)
        {
            string xml = await File.ReadAllTextAsync(file);
            var csproj = XDocument.Parse(xml);
            var metadata = csproj.Root.Element("metadata");
            var metaVersion = metadata.Element("version");
            Console.WriteLine($"Patching package meta package to version {gitVersion}");

            metaVersion.Value = gitVersion;

            var deps = metadata.Element("dependencies");
            foreach (var dep in deps.Elements("dependency"))
            {
                var id = dep.Attribute("id");
                var version = dep.Attribute("version");

                if (id != null && version != null)
                {
                    if (id.Value.StartsWith("Atma.", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine($"Patching package reference {id.Value} to version {gitVersion}");
                        version.Value = gitVersion;
                    }
                }
                else
                {
                    Console.WriteLine("Could not find include and version attribute on package reference...");
                    Console.WriteLine(dep.ToString());
                }
            }


            xml = csproj.ToString(SaveOptions.None);
            await File.WriteAllTextAsync(file, xml);

            return 0;
        }

        static async Task<int> Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Expected 1 argument as a file path to patch");
                return -1;
            }

            var gitVersion = Environment.GetEnvironmentVariable("GitVersion_NuGetVersion");
            if (string.IsNullOrEmpty(gitVersion))
            {
                Console.WriteLine("Could not find gitversion in enviroment vars.");
                return -2;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine($"Cound not find file {args[0]}");
                return -3;
            }

            if (Path.GetExtension(args[0]) == ".nuspec")
                return await ProcessNuSpec(args[0], gitVersion);
            else
                return await ProcessCsProj(args[0], gitVersion);


        }
    }
}

