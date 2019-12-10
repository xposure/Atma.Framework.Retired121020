namespace Atma.Systems
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Atma.Common;
    using Atma.Entities;

    [AttributeUsage(AttributeTargets.Struct)]
    public class Any : Attribute
    {
        private Type[] _types;

        public Type[] Types => _types;

        public Any(params Type[] types)
        {
            _types = types;
        }
    }

    [AttributeUsage(AttributeTargets.Struct)]
    public class Has : Attribute
    {
        private Type[] _types;

        public Type[] Types => _types;

        public Has(params Type[] types)
        {
            _types = types;
        }
    }

    [AttributeUsage(AttributeTargets.Struct)]
    public class Ignore : Attribute
    {
        private Type[] _types;

        public Type[] Types => _types;

        public Ignore(params Type[] types)
        {
            _types = types;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnly : Attribute
    {

    }

    public struct EntityField
    {
        public ComponentType ComponentType;
        public int Offset;
        public int Size;
    }

    public abstract class EntityView
    {
        private HashSet<ComponentType> _hasComponents = new HashSet<ComponentType>();
        private HashSet<ComponentType> _anyComponents = new HashSet<ComponentType>();
        private HashSet<ComponentType> _ignoreComponents = new HashSet<ComponentType>();
        private HashSet<ComponentType> _allComponents = new HashSet<ComponentType>();
        private HashSet<ComponentType> _readComponents = new HashSet<ComponentType>();
        private HashSet<ComponentType> _writeComponents = new HashSet<ComponentType>();
        protected readonly EntitySpec _spec, _anySpec, _ignoreSpec;

        protected readonly Type _type;
        protected readonly FieldInfo[] _fields;
        protected readonly EntityField[] _entityFields;
        private ComponentList _components = new ComponentList();
        private DependencyList _dependencies;
        public DependencyList Dependencies => _dependencies;

        public bool IsValid { get; private set; }

        protected EntityView(Type type)
        {
            _type = type;

            IsValid = _components.TryAddComponent(type);
            _fields = _type.GetFields().Where(x => x.FieldType.IsPointer).ToArray();
            _entityFields = new EntityField[_fields.Length];

            if (IsValid)
            {
                FindIgnoreComponents();
                _ignoreSpec = new EntitySpec(_ignoreComponents.ToArray());
            }

            if (IsValid)
            {
                FindAnyComponents();
                _anySpec = new EntitySpec(_anyComponents.ToArray());
            }

            if (IsValid) FindHasComponents();
            if (IsValid)
            {
                var components = FindPointerComponents();
                foreach (var it in _hasComponents)
                    components.Add(it);
                if (components != null)
                    _spec = new EntitySpec(components.ToArray());
            }

            _dependencies = new DependencyList(string.Empty, 0, config =>
            {
                foreach (var it in _readComponents)
                    config.Read(it);
                foreach (var it in _writeComponents)
                    config.Write(it);
            });
        }

        private List<ComponentType> FindPointerComponents()
        {
            var componentTypes = new List<ComponentType>();

            for (var i = 0; i < _fields.Length; i++)
            {
                var fieldType = _fields[i].FieldType;
                if (!fieldType.IsPointer)
                {
                    //continue;
                    IsValid = false;
                    return null;
                }

                var elementType = fieldType.GetElementType();
                if (!_components.TryAddComponent(elementType, out var componentType))
                {
                    IsValid = false;
                    return null;
                }

                if (!_allComponents.Add(componentType))
                {
                    IsValid = false;
                    return null;
                }

                if (_fields[i].GetCustomAttribute<ReadOnly>() != null)
                    _readComponents.Add(componentType);
                else
                    _writeComponents.Add(componentType);

                componentTypes.Add(componentType);
                _entityFields[i] = new EntityField()
                {
                    ComponentType = componentType,
                    Offset = (int)Marshal.OffsetOf(_type, _fields[i].Name),
                    Size = componentType.Size
                };
            }

            if (componentTypes.Count == 0)
                IsValid = false;



            return componentTypes;
        }

        private void FindIgnoreComponents()
        {
            var ignore = _type.GetCustomAttribute<Ignore>();
            if (ignore != null)
            {
                for (var i = 0; i < ignore.Types.Length; i++)
                {
                    if (!_components.IsValid(_fields[i].DeclaringType))
                    {
                        IsValid = false;
                        return;
                    }

                    var componentType = _components.AddComponent(_fields[i].DeclaringType);
                    if (!_allComponents.Add(componentType))
                    {
                        IsValid = false;
                        return;
                    }

                    _ignoreComponents.Add(componentType);
                }
            }
        }

        private void FindHasComponents()
        {
            var has = _type.GetCustomAttribute<Has>();
            if (has != null)
            {
                for (var i = 0; i < has.Types.Length; i++)
                {
                    if (!_components.IsValid(_fields[i].DeclaringType))
                    {
                        IsValid = false;
                        return;
                    }

                    var componentType = _components.AddComponent(_fields[i].DeclaringType);
                    if (!_allComponents.Add(componentType))
                    {
                        IsValid = false;
                        return;
                    }

                    _hasComponents.Add(componentType);
                }
            }
        }

        private void FindAnyComponents()
        {
            var any = _type.GetCustomAttribute<Any>();
            if (any != null)
            {
                for (var i = 0; i < any.Types.Length; i++)
                {
                    if (!_components.IsValid(_fields[i].DeclaringType))
                    {
                        IsValid = false;
                        return;
                    }

                    var componentType = _components.AddComponent(_fields[i].DeclaringType);
                    if (!_allComponents.Add(componentType))
                    {
                        IsValid = false;
                        return;
                    }

                    _anyComponents.Add(componentType);
                }
            }
        }

        protected abstract void Execute(EntityManager entityManager, EntityChunkList array);

        public unsafe void Execute(EntityManager entityManager)
        {
            if (!IsValid)
                throw new Exception("EntityView was not valid. Make sure you are only working with pointers of components.");
            var smallest = entityManager.EntityArrays.FindSmallest(_spec);
            if (smallest != null)
            {
                for (var i = 0; i < smallest.Count; i++)
                {
                    var array = smallest[i];
                    if (array.Specification.HasAll(_spec.ComponentTypes) &&
                        (_anyComponents.Count == 0 || array.Specification.HasAny(_anySpec.ComponentTypes)) &&
                        (_ignoreComponents.Count == 0 || array.Specification.HasNone(_ignoreSpec.ComponentTypes)))
                        Execute(entityManager, array);
                }
            }
        }
    }

    public abstract class System<T> : EntityView
        where T : unmanaged
    {
        public System() : base(typeof(T))
        {

        }

        protected unsafe override void Execute(EntityManager entityManager, EntityChunkList array)
        {
            var actor = stackalloc[] { new T() };
            var actorPtr = (byte*)actor;

            var length = _entityFields.Length;
            Span<EntityField> entityFields = stackalloc EntityField[length];
            for (var i = 0; i < length; i++)
                entityFields[i] = _entityFields[i];

            var ptrs = stackalloc byte**[length];
            for (var i = 0; i < length; i++)
                ptrs[i] = (byte**)&actorPtr[entityFields[i].Offset];

            var indices = stackalloc int[length];
            for (var i = 0; i < length; i++)
                indices[i] = array.Specification.GetComponentIndex(entityFields[i].ComponentType);

            for (var i = 0; i < array.AllChunks.Count; i++)
            {
                var chunk = array.AllChunks[i];
                for (var j = 0; j < length; j++)
                    (*ptrs[j]) = (byte*)chunk.PackedArray[indices[j]].Memory;

                for (var j = 0; j < chunk.Count; j++)
                {
                    //actor->Execute();
                    Execute(*actor);

                    for (var k = 0; k < length; k++)
                        (*ptrs[k]) += entityFields[k].Size;
                }
            }
        }

        protected unsafe abstract void Execute(in T t);
    }
}