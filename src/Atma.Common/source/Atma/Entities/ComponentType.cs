namespace Atma.Entities
{
    using Atma.Common;

    using System;

    public struct ComponentType : IEquatable<ComponentType>, IComparable<ComponentType>
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

    public static class ComponentType<T>
        where T : unmanaged
    {
        public readonly static ComponentType Type;

        static ComponentType()
        {
            var unmanagedType = UnmanagedType<T>.Type;
            Type = new ComponentType(unmanagedType.ID, unmanagedType.Size);
        }
    }

}
