namespace Atma.Entities
{
    using Atma.Common;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class ComponentTypeInfo
    {
        public readonly ComponentType ComponentType;
        public readonly Type Type;

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
            var validType = _unmanagedHelper.GetInfo(type, out var unmanagedType);
            Contract.EqualTo(validType, true);

            var componentType = new ComponentType(unmanagedType.ID, unmanagedType.Size);
            _components.Add(componentType.ID, componentType);
            _componentTypes.Add(componentType.ID, new ComponentTypeInfo(type, componentType));
            return componentType;
        }

        public bool TryAddComponent(Type type) => TryAddComponent(type, out var _);
        public bool TryAddComponent(Type type, out ComponentType componentType)
        {
            if (!_unmanagedHelper.GetInfo(type, out var unmanagedType))
            {
                componentType = default;
                return false;
            }

            componentType = new ComponentType(unmanagedType.ID, unmanagedType.Size);
            _components.Add(componentType.ID, componentType);
            _componentTypes.Add(componentType.ID, new ComponentTypeInfo(type, componentType));
            return true;
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
        public ComponentType Get(Type type)
        {
            if (!Find(type, out var componentType))
                throw new Exception($"{type.Name} not found.");
            return componentType;
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
}