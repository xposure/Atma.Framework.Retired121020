namespace Atma
{
    using System;
    using System.Runtime.InteropServices;

    public static class Size
    {
        public static int Of(Type t) => Marshal.SizeOf(t);
    }

    public static class SizeOf<T>
        where T : unmanaged
    {
        public static readonly int Size;


        static SizeOf()
        {
            Size = Marshal.SizeOf(typeof(T));
        }

    }
}