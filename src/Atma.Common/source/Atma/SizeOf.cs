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
            try
            {
                Size = Marshal.SizeOf<T>();
                //Size = Marshal.SizeOf(typeof(T));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(typeof(T));
                Size = -1;
            }
        }

    }
}