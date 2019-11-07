namespace Atma.Entities
{
    using Atma.Common;

    using System;


    public unsafe struct ComponentType : IEquatable<ComponentType>, IComparable<ComponentType>
    {


        public readonly int ID;
        public readonly int Size;

        internal ComponentType(int id, int size)
        {
            ID = id;
            Size = size;
        }

        public override int GetHashCode()
        {
            return ID;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ComponentType o)) return false;
            return ID == o.ID;
        }

        public bool Equals(ComponentType other)
        {
            if (other == null) return false;
            return this.ID == other.ID;
        }

        public int CompareTo(ComponentType other)
        {
            if (ID > other.ID) return 1;
            else if (ID < other.ID) return -1;
            return 0;
        }

        public static bool operator ==(ComponentType a, ComponentType b)
        {
            //if (a is null)
            //    return b is null;
            //else if (b is null)
            //    return false;

            return a.ID == b.ID;
        }

        public static bool operator !=(ComponentType a, ComponentType b) => !(a == b);

        public override string ToString()
        {
            return StructHelper.ToString(ref this);
        }
    }

    public unsafe delegate void Copy(void* src, void* dst);
    public class ComponentTypeHelper
    {
        private static ConcurrentLookupList<ComponentTypeHelper> _helpers = new ConcurrentLookupList<ComponentTypeHelper>();

        public static ComponentTypeHelper Get(in ComponentType componentType)
        {
            if (!_helpers.TryGetValue(componentType.ID, out var componentHelper))
                return null;

            return componentHelper;
        }

        public readonly ComponentType ComponentType;
        public readonly Copy Copy;
        internal ComponentTypeHelper(ComponentType componentType, Copy copy)
        {
            ComponentType = componentType;
            Copy = copy;
            _helpers.Add(componentType.ID, this);
        }
    }


    public unsafe static class ComponentType<T>
        where T : unmanaged
    {
        public readonly static ComponentType Type;
        public readonly static ComponentTypeHelper Helper;

        static ComponentType()
        {
            var unmanagedType = UnmanagedType<T>.Type;
            Type = new ComponentType(unmanagedType.ID, unmanagedType.Size);

            Copy copy = (void* src, void* dst) => *(T*)dst = *(T*)src;
            Helper = new ComponentTypeHelper(Type, copy);
        }
    }

}
