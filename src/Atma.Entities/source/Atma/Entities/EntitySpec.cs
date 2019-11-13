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

        internal EntitySpec(int id, Span<ComponentType> componentTypes)
        {
            ID = id;
            ComponentTypes = componentTypes.ToArray();
            EntitySize = ComponentTypes.Sum(x => x.Size);
        }

        public EntitySpec(params ComponentType[] componentTypes)
        {
            ComponentTypes = componentTypes;
            ID = ComponentType.CalculateId(ComponentTypes);
            EntitySize = ComponentTypes.Sum(x => x.Size);
        }

        public bool HasAll(EntitySpec other) => HasAll(other.ComponentTypes);

        public bool HasAll(Span<ComponentType> componentTypes) => ComponentType.HasAll(ComponentTypes, componentTypes);

        public bool HasAny(EntitySpec other) => HasAny(other.ComponentTypes);

        public bool HasAny(Span<ComponentType> componentTypes) => ComponentType.HasAny(ComponentTypes, componentTypes);

        public bool Has(in ComponentType type)
        {
            var id = type.ID;
            for (var i = 0; i < ComponentTypes.Length; i++)
                if (ComponentTypes[i].ID == id)
                    return true;

            return false;
        }

        public int FindMatches(EntitySpec other, Span<ComponentType> results)
        {
            return ComponentType.FindMatches(ComponentTypes, other.ComponentTypes, results);
        }

        public int GetComponentIndex(in ComponentType type)
        {
            var id = type.ID;
            for (var i = 0; i < ComponentTypes.Length; i++)
                if (ComponentTypes[i].ID == id)
                    return i;
            return -1;
        }

        public static EntitySpec Create<T0>()
            where T0 : unmanaged
            => new EntitySpec(ComponentType<T0>.Type);

        public static EntitySpec Create<T0, T1>()
            where T0 : unmanaged
            where T1 : unmanaged
            => new EntitySpec(ComponentType<T0>.Type, ComponentType<T1>.Type);

        public static EntitySpec Create<T0, T1, T2>()
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
            => new EntitySpec(ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type);

    }
}