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

        public bool HasAll(EntitySpecification other)
        {
            return ComponentType.HasAll(ComponentTypes, other.ComponentTypes);
        }

        public bool HasAny(EntitySpecification other)
        {
            return ComponentType.HasAll(ComponentTypes, other.ComponentTypes);
        }

        public bool Has(in ComponentType type)
        {
            var id = type.ID;
            for (var i = 0; i < ComponentTypes.Length; i++)
                if (ComponentTypes[i].ID == id)
                    return true;

            return false;
        }
    }
}