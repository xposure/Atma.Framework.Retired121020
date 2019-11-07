namespace Atma.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static Atma.Debug;

    public class EntitySpecification
    {
        public readonly int ID;
        public readonly ComponentType[] ComponentTypes;
        public readonly int EntitySize;

        public EntitySpecification(params ComponentType[] componentTypes)
        {
            ID = ComponentType.CalculateId(componentTypes);
            ComponentTypes = componentTypes;
            EntitySize = ComponentTypes.Sum(x => x.Size);
        }

        public static bool HasAll(EntitySpecification a, EntitySpecification b)
        {
            return ComponentType.HasAll(a.ComponentTypes, b.ComponentTypes);
        }

        public static bool HasAny(EntitySpecification a, EntitySpecification b)
        {
            return ComponentType.HasAll(a.ComponentTypes, b.ComponentTypes);
        }
    }
}