namespace Atma.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
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

    public struct UnknownType
    {

    }

    public sealed class UnmanagedHelper
    {
        internal static ConcurrentLookupList<Type> _typeLookup = new ConcurrentLookupList<Type>();

        public static Type LookUp(int id)
        {
            if (!_typeLookup.TryGetValue(id, out var type))
                return typeof(UnknownType);

            return type;
        }

        private Dictionary<Type, bool> _unmanagedCache = new Dictionary<Type, bool>();
        private Dictionary<Type, UnmanagedType> _cacheTypes = new Dictionary<Type, UnmanagedType>();

        public bool IsUnManaged(Type t)
        {
            var result = false;
            if (_unmanagedCache.ContainsKey(t))
                return _unmanagedCache[t];
            else if (t.IsPrimitive || t.IsPointer || t.IsEnum)
                result = true;
            else if (t.IsGenericType || !t.IsValueType)
                result = false;
            else
                result = t.GetFields(BindingFlags.Public |
                   BindingFlags.NonPublic | BindingFlags.Instance)
                    .All(x => IsUnManaged(x.FieldType));
            _unmanagedCache.Add(t, result);
            return result;
        }

        public bool GetInfo(Type t, out UnmanagedType unmanagedType)
        {
            unmanagedType = default;
            if (!IsUnManaged(t))
                return false;

            if (!_cacheTypes.TryGetValue(t, out unmanagedType))
            {
                var size = Size.Of(t);
                unmanagedType = new UnmanagedType(t, size);
                _cacheTypes.Add(t, unmanagedType);
                _typeLookup.TryAdd(unmanagedType.ID, t);
            }

            return true;
        }
    }
}
