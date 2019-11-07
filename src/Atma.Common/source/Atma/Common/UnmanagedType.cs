namespace Atma.Common
{
    using System;

    internal struct UnmanagedType
    {
        public readonly int ID;
        //public Type Type { get; }
        public readonly int Size;

        internal UnmanagedType(Type type, int size)
        {
            //type.
            ID = type.GetHashCode();// type.FullName.StableHashCode();
            //Type = type;
            Size = size;
        }
    }
}
