namespace Atma.Entities
{
    using System.Linq;

    public class EntitySpec
    {
        public readonly int ID;
        public readonly ComponentType[] ComponentTypes;
        public readonly int EntitySize;

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

        public bool HasAny(EntitySpec other)
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