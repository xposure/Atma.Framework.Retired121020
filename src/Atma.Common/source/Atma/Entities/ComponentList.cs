namespace Atma.Entities
{
    using Atma.Common;
    using static Atma.Debug;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    //TODO: Think this is getting removed
    public class ComponentTypeInfo
    {
        public readonly ComponentType ComponentType;
        public readonly Type Type;
        public Func<IComponentDataArray> CreateArray { get; set; }

        public ComponentTypeInfo(Type type, in ComponentType componentType)
        {
            Type = type;
            ComponentType = componentType;
        }
    }


    public sealed class ComponentList : IEnumerable<ComponentType>
    {
        private readonly LookupList<ComponentType> _components = new LookupList<ComponentType>();
        private readonly LookupList<ComponentTypeInfo> _componentTypes = new LookupList<ComponentTypeInfo>();
        private readonly UnmanagedHelper _unmanagedHelper = new UnmanagedHelper();

        public ComponentList()
        {
            AddComponent<Entity>();
        }

        public ComponentType AddComponent<T>() where T : unmanaged => AddComponent(typeof(T));

        public ComponentType AddComponent(Type type)
        {
            if (_unmanagedHelper.GetInfo(type, out var unmanagedType))
            {
                var componentType = new ComponentType(unmanagedType.ID, unmanagedType.Size);
                _components.Add(componentType.ID, componentType);
                _componentTypes.Add(componentType.ID, new ComponentTypeInfo(type, componentType));
                return componentType;
            }

            Assert(false);
            return default;
        }

        public void AddFromNamespace(Assembly assembly, string name)
        {
            foreach (var it in assembly.GetTypes().Where(t => t.IsValueType && t.Namespace == name))
                AddComponent(it);
        }

        public bool IsValid<T>() => IsValid(typeof(T));

        public bool IsValid(Type type)
        {
            var id = type.GetHashCode();
            foreach (var it in _components.AllObjects)
                if (it.ID == id)
                    return true;

            return false;
        }

        public ComponentTypeInfo GetTypeInfo(in ComponentType componentType)
        {
            if (!_componentTypes.TryGetValue(componentType.ID, out var type))
                return null;

            return type;
        }

        public ComponentType Get<T>()
        {
            if (Find<T>(out var componenType))
                return componenType;

            Assert(false);
            return default;
        }

        public bool Find<T>(out ComponentType componentType) => Find(typeof(T), out componentType);

        public bool Find(Type type, out ComponentType componentType) => Find(type.GetHashCode(), out componentType);
        public bool Find(int id, out ComponentType componentType)
        {
            foreach (var it in _components.AllObjects)
            {
                if (it.ID == id)
                {
                    componentType = it;
                    return true;
                }
            }

            componentType = default;
            return false;
        }

        public IEnumerator<ComponentType> GetEnumerator() => _components.AllObjects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _components.AllObjects.GetEnumerator();
    }

    internal class UnmanagedHelper
    {
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

            if (!_cacheTypes.TryGetValue(t, out var ut))
            {
                var size = Size.Of(t);
                unmanagedType = new UnmanagedType(t, size);
                _cacheTypes.Add(t, ut);
            }

            return true;
        }
    }
}
