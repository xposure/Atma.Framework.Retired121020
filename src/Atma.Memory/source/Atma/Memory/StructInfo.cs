namespace Atma.Memory
{
    using System;
    using System.Reflection;
    using Atma.Common;

    public class StructInfo
    {

    }

    public readonly struct StructField
    {
        public readonly int Index;
        public readonly int Offset;
        public readonly int Size;
        public readonly UnmanagedType UnmanagedType;
        public readonly bool IsWritable;

    }

    public class StructInfo<T>
    {
        public static readonly StructInfo info;

        static StructInfo()
        {

        }
    }
}