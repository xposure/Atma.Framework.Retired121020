namespace Atma.Common
{
    using System;
    using System.Threading;

    public readonly struct UnmanagedType
    {
        internal static int UniqueID;

        public readonly int ID;
        public readonly int Size;

        internal UnmanagedType(Type type, int size)
        {
            ID = type.GetHashCode();// type.FullName.StableHashCode();
            Size = size;
        }
        internal UnmanagedType(int id, int size)
        {
            ID = id;// type.GetHashCode();// type.FullName.StableHashCode();
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
            var type = Interlocked.Increment(ref UnmanagedType.UniqueID);
            Type = new UnmanagedType(typeof(T), size);
        }
    }
}
