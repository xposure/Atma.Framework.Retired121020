using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Atma.Entities
{
    public static class DIEntityExtensions
    {
        public static void AddFromNamespace(this ComponentList componentList, Assembly assembly, string name)
        {
            foreach (var it in assembly.GetExportedTypes().Where(t => t.IsValueType && t.Namespace == name))
                componentList.AddComponent(it);
        }

        public static void AddFromAssembly(this SystemManager systemManager, Assembly assembly)
        {

        }
    }
}
