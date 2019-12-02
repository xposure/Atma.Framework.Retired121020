using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ecs
{
    public class Project
    {
        public string @namespace { get; set; }
        public List<ProjectGroup> groups { get; set; }

        public int Process(Generator gen)
        {
            for (var i = 0; i < groups?.Count; i++)
            {
                var group = groups[i];
                if (group.Process(gen) != 0)
                    return -1;
            }

            return 0;
        }
    }

    public class ProjectGroup
    {
        internal Project project;
        internal ProjectGroup parent;

        public string name { get; set; }
        public string path { get; set; }

        public int Process(Generator gen)
        {
            Console.WriteLine(GetNamespace());
            return 0;
        }

        protected string GetNamespace()
        {
            if (parent != null)
                return $"{parent.name}.{name}";

            if (!string.IsNullOrEmpty(project.@namespace))
                return $"{project.@namespace}.{name}";

            return name;
        }
    }

    public class Manifest
    {
        public string @namespace { get; set; }
        public List<ProjectGroup> groups { get; set; }

    }

    public class Component
    {
        public string type { get; set; }
        public string name { get; set; }
        public bool write { get; set; } = false;
    }

    public class Variable
    {
        public string type { get; set; }
        public string name { get; set; }
    }

    public class Buffer
    {
        public string name { get; set; }
        public bool flush { get; set; }
        public int size { get; set; }
    }

    public class ECSFile
    {
        public string name { get; set; }
        public List<Component> components { get; set; }
        public List<Variable> variables { get; set; }
        public List<Buffer> buffers { get; set; }
        public List<string> before { get; set; }
        public List<string> after { get; set; }

    }
}