namespace Atma.Common
{
    using System;

    public struct UnmanagedType
    {
        public readonly int ID;
        public readonly int Size;

        internal UnmanagedType(Type type, int size)
        {
            ID = type.GetHashCode();// type.FullName.StableHashCode();
            Size = size;
        }
    }

    public unsafe static class UnmanagedType<T>
        where T : unmanaged
    {
        public static readonly UnmanagedType Type;

        static UnmanagedType()
        {
            var size = sizeof(T);
            var type = typeof(T);
            Type = new UnmanagedType(type, size);
        }
    }
}
