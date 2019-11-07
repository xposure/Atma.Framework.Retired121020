using System.Collections.Generic;

namespace Atma.Entities
{
    public sealed class DependencyView : DependencyResolver
    {
        public override void Resolve(ComponentSystemList list)
        {
            foreach (var system in list.Systems)
                foreach (var read in system.ReadComponents)
                    foreach (var write in GetWithWrite(list, system, read))
                        list.AddDependency(system, write);
        }

        private IEnumerable<ComponentSystem> GetWithWrite(ComponentSystemList list, ComponentSystem initiator, ComponentType type)
        {
            foreach (var system in list.Systems)
                if (system != initiator)
                    foreach (var write in system.WriteComponents)
                        if (write == type)
                            yield return system;
        }

    }
}
