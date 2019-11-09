namespace Atma.Entities
{
    using System;
    using System.Linq;

    public class EntitySpec
    {
        public readonly int ID;
        public readonly ComponentType[] ComponentTypes;
        //public readonly ComponentTypeHelper[] componentTypeHelpers;
        public readonly int EntitySize;


        internal EntitySpec(int id, params ComponentType[] componentTypes)
        {
            ID = id;
            ComponentTypes = componentTypes;
            EntitySize = ComponentTypes.Sum(x => x.Size);
        }

        public EntitySpec(params ComponentType[] componentTypes)
        {
            ID = ComponentType.CalculateId(componentTypes);
            ComponentTypes = componentTypes;
            EntitySize = ComponentTypes.Sum(x => x.Size);
        }

        public bool HasAll(EntitySpec other)
        {
            return ComponentType.HasAll(ComponentTypes, other.ComponentTypes);
        }

        public bool HasAll(Span<ComponentType> componentTypes)
        {
            return ComponentType.HasAny(ComponentTypes, componentTypes);
        }

        public bool HasAny(EntitySpec other)
        {
            return ComponentType.HasAny(ComponentTypes, other.ComponentTypes);
        }

        public bool Has(in ComponentType type)
        {
            var id = type.ID;
            for (var i = 0; i < ComponentTypes.Length; i++)
                if (ComponentTypes[i].ID == id)
                    return true;

            return false;
        }

        public int FindMatches(EntitySpec other, Span<ComponentType> matches)
        {
            return ComponentType.FindMatches(ComponentTypes, other.ComponentTypes, matches);
        }
    }
}