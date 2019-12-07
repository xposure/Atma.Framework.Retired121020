namespace Atma.Entities
{
    using System;
    using System.Linq;



    public readonly struct EntitySpec : IEquatable<EntitySpec>
    {
        public readonly int ID;
        public readonly ComponentType[] ComponentTypes;
        public readonly int EntitySize;

        internal readonly IEntitySpecGroup[] Grouping;

        public readonly T GetGroupData<T>()
            where T : IEntitySpecGroup
        {
            if (Grouping != null)
                for (var i = 0; i < Grouping.Length; i++)
                    if (Grouping[i] is T group)
                        return group;

            return default;
        }

        internal EntitySpec(int id, Span<ComponentType> componentTypes)
        {
            ID = id;
            ComponentTypes = componentTypes.ToArray();
            EntitySize = ComponentTypes.Sum(x => x.Size);
            Grouping = null;
        }

        internal EntitySpec(Span<ComponentType> componentTypes)
        {
            ID = ComponentType.CalculateId(componentTypes);
            ComponentTypes = componentTypes.ToArray();
            EntitySize = ComponentTypes.Sum(x => x.Size);
            Grouping = null;
        }

        internal EntitySpec(IEntitySpecGroup[] groups, Span<ComponentType> componentTypes)
        {
            ID = ComponentType.CalculateId(componentTypes, groups);
            ComponentTypes = componentTypes.ToArray();
            EntitySize = ComponentTypes.Sum(x => x.Size);
            Grouping = groups;
        }

        public EntitySpec(params ComponentType[] componentTypes)
        {
            ComponentTypes = componentTypes;
            ID = ComponentType.CalculateId(ComponentTypes);
            EntitySize = ComponentTypes.Sum(x => x.Size);
            Grouping = null;
        }

        public EntitySpec(IEntitySpecGroup[] grouping, params ComponentType[] componentTypes)
        {
            ComponentTypes = componentTypes;
            ID = ComponentType.CalculateId(ComponentTypes, grouping);
            EntitySize = ComponentTypes.Sum(x => x.Size);
            Grouping = grouping;
        }

        public bool Has<T>(out int index)
            where T : IEntitySpecGroup
        {
            if (Grouping != null)
                for (index = 0; index < Grouping.Length; index++)
                    if (Grouping[index] is T g)
                        return true;

            index = -1;
            return false;
        }

        public bool Has<T>(T group)
            where T : IEntitySpecGroup
        {
            var groupHash = group.GetHashCode();
            if (Grouping != null)
                for (var i = 0; i < Grouping.Length; i++)
                    if (Grouping[i] is T g)
                        return g.GetHashCode() == groupHash;

            return false;
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

        public int GetComponentIndex<T>() where T : unmanaged => GetComponentIndex(ComponentType<T>.Type.ID);
        public int GetComponentIndex(in ComponentType type) => GetComponentIndex(type.ID);

        public bool Equals(EntitySpec other) => this.ID == other.ID;

        public static implicit operator Span<ComponentType>(EntitySpec it) => it.ComponentTypes;

        public static EntitySpec Create<T0>(params IEntitySpecGroup[] groups)
            where T0 : unmanaged
            => new EntitySpec(groups, ComponentType<T0>.Type);

        public static EntitySpec Create<T0, T1>(params IEntitySpecGroup[] groups)
            where T0 : unmanaged
            where T1 : unmanaged
            => new EntitySpec(groups, ComponentType<T0>.Type, ComponentType<T1>.Type);

        public static EntitySpec Create<T0, T1, T2>(params IEntitySpecGroup[] groups)
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
            => new EntitySpec(groups, ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type);

        public static EntitySpec Create<T0, T1, T2, T3>(params IEntitySpecGroup[] groups)
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            => new EntitySpec(groups, ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type);

        public static EntitySpec Create<T0, T1, T2, T3, T4>(params IEntitySpecGroup[] groups)
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            => new EntitySpec(groups, ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type);

        public static EntitySpec Create<T0, T1, T2, T3, T4, T5>(params IEntitySpecGroup[] groups)
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
            => new EntitySpec(groups, ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type);

        public static EntitySpec Create<T0, T1, T2, T3, T4, T5, T6>(params IEntitySpecGroup[] groups)
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
            where T6 : unmanaged
            => new EntitySpec(groups, ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type);

    }
}