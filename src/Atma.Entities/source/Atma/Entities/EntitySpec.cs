namespace Atma.Entities
{
    using System;
    using System.Linq;

    public readonly struct EntitySpec : IEquatable<EntitySpec>
    {
        public readonly int ID;
        public readonly ComponentType[] ComponentTypes;
        public readonly int EntitySize;

        internal EntitySpec(int id, Span<ComponentType> componentTypes)
        {
            ID = id;
            ComponentTypes = componentTypes.ToArray();
            EntitySize = ComponentTypes.Sum(x => x.Size);
        }


        internal EntitySpec(Span<ComponentType> componentTypes)
        {
            ID = ComponentType.CalculateId(componentTypes);
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

        public bool HasNone(Span<ComponentType> componentTypes) => !ComponentType.HasAny(ComponentTypes, componentTypes);
        public bool HasNone(EntitySpec other) => !HasAny(other.ComponentTypes);

        public bool Has(in ComponentType type) => Has(type.ID);

        internal bool Has(int id)
        {
            for (var i = 0; i < ComponentTypes.Length; i++)
                if (ComponentTypes[i].ID == id)
                    return true;

            return false;
        }

        public int FindMatches(EntitySpec other, Span<ComponentType> results)
        {
            return ComponentType.FindMatches(ComponentTypes, other.ComponentTypes, results);
        }

        internal int GetComponentIndex(int componentId)
        {
            for (var i = 0; i < ComponentTypes.Length; i++)
                if (ComponentTypes[i].ID == componentId)
                    return i;
            return -1;
        }

        public int GetComponentIndex(in ComponentType type) => GetComponentIndex(type.ID);

        public bool Equals(EntitySpec other) => this.ID == other.ID;

        public static implicit operator Span<ComponentType>(EntitySpec it) => it.ComponentTypes;

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

        public static EntitySpec Create<T0, T1, T2, T3>()
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            => new EntitySpec(ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type);

    }
}