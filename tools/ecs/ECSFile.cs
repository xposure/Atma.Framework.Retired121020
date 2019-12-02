using System.Collections.Generic;

namespace ecs
{

    public class Component
    {
        public string type { get; set; }
        public string name { get; set; }
        public bool write { get; set; } = false;
    }

    public class ECSFile
    {
        public string name { get; set; }

        public List<Component> components { get; set; }


    }
}