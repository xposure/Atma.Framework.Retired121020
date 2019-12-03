namespace Atma.Systems
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Atma.Common;
    using Atma.Entities;

    // public abstract class Dependency : IEquatable<Dependency>
    // {
    //     public bool Equals(Dependency other) => OnEqual(other);

    //     public abstract int Priority { get; }

    //     protected abstract bool OnEqual(Dependency other);

    //     public void Resolve(DirectedGraph<ISystem> graph, ISystem a, ISystem b)
    //     {
    //         OnResolve(graph, a, b);
    //     }
    //     protected abstract void OnResolve(DirectedGraph<ISystem> graph, ISystem a, ISystem b);
    // }


    // public class ComponentDependency : Dependency
    // {
    //     private ComponentType _componentType;
    //     public ComponentType ComponentType => _componentType;

    //     public readonly bool Writable;

    //     public ComponentDependency(in ComponentType componentType, bool writable)
    //     {
    //         _componentType = componentType;
    //         Writable = writable;
    //     }

    //     public override int Priority => 1000;

    //     protected override bool OnEqual(Dependency other)
    //     {
    //         if (other is null || !(other is ComponentDependency it))
    //             return false;

    //         return _componentType.ID == it._componentType.ID && Writable == it.Writable;
    //     }

    //     protected override void OnResolve(DirectedGraph<ISystem> graph, ISystem a, ISystem b)
    //     {
    //         foreach (var it in b.Dependencies)
    //         {
    //             if (it is ComponentDependency dep && dep.ComponentType.ID == _componentType.ID)
    //             {
    //                 //TODO: if both of these are writable, we are going to have cyclic issues
    //                 if (Writable != dep.Writable)
    //                 {
    //                     if (Writable)
    //                         graph.AddEdge(a, b);
    //                     else
    //                         graph.AddEdge(b, a);
    //                 }
    //             }
    //         }
    //     }
    // }

    // public class ComponentDependency<T> : ComponentDependency
    //         where T : unmanaged
    // {
    //     public ComponentDependency(bool writable) : base(ComponentType<T>.Type, writable) { }
    // }
    // public class ReadDependency<T> : ComponentDependency<T>
    //         where T : unmanaged
    // {
    //     public ReadDependency() : base(false) { }
    // }

    // public class WriteDependency<T> : ComponentDependency<T>
    //     where T : unmanaged
    // {
    //     public WriteDependency() : base(true) { }
    // }

    // public class BeforeDependency : Dependency
    // {
    //     public string Name { get; }

    //     public override int Priority => 2000;

    //     public BeforeDependency(string name)
    //     {
    //         Name = name;
    //     }

    //     protected override bool OnEqual(Dependency other)
    //     {
    //         if (other is null || !(other is BeforeDependency it))
    //             return false;

    //         return Name == it.Name;
    //     }

    //     protected override void OnResolve(DirectedGraph<ISystem> graph, ISystem a, ISystem b)
    //     {
    //         if (string.Compare(Name, b.Name, true) == 0)
    //             graph.AddEdge(a, b);
    //     }
    // }


    // public class AfterDependency : Dependency
    // {
    //     public string Name { get; }
    //     public override int Priority => 1000;
    //     public AfterDependency(string name)
    //     {
    //         Name = name;
    //     }

    //     protected override bool OnEqual(Dependency other)
    //     {
    //         if (other is null || !(other is AfterDependency it))
    //             return false;

    //         return Name == it.Name;
    //     }

    //     protected override void OnResolve(DirectedGraph<ISystem> graph, ISystem a, ISystem b)
    //     {
    //         if (string.Compare(Name, b.Name, true) == 0)
    //             graph.AddEdge(b, a);
    //     }
    // }


    public class DependencyListConfig
    {
        private DependencyList _list;

        internal DependencyListConfig(DependencyList list)
        {
            _list = list;
        }

        public DependencyListConfig Read<T>()
            where T : unmanaged
        {
            var componentType = ComponentType<T>.Type;
            if (!_list._allComponents.Add(componentType))
                throw new Exception("Duplicate type");
            _list._readComponents.Add(componentType);
            return this;
        }

        public DependencyListConfig Write<T>()
            where T : unmanaged
        {
            var componentType = ComponentType<T>.Type;
            if (!_list._allComponents.Add(componentType))
                throw new Exception("Duplicate type");

            _list._writeComponents.Add(componentType);
            return this;
        }

        public DependencyListConfig Before(string name)
        {
            _list._before.Add(name);
            return this;
        }

        public DependencyListConfig After(string name)
        {
            _list._after.Add(name);
            return this;
        }
    }

    public sealed class DependencyList
    {
        public readonly string Name;
        public readonly int Priority;

        internal HashSet<ComponentType> _readComponents = new HashSet<ComponentType>();
        internal HashSet<ComponentType> _writeComponents = new HashSet<ComponentType>();
        internal HashSet<ComponentType> _allComponents = new HashSet<ComponentType>();

        internal HashSet<string> _before = new HashSet<string>();
        internal HashSet<string> _after = new HashSet<string>();

        public DependencyList(string name, int priority, Action<DependencyListConfig> config)
        {
            Name = name;
            Priority = priority;
            config?.Invoke(new DependencyListConfig(this));
        }

        private DependencyList(string name, int priority)
        {
            Name = name;
            Priority = priority;
        }

        public IEnumerable<ComponentType> Components
        {
            get
            {
                foreach (var read in _readComponents)
                    yield return read;

                foreach (var write in _writeComponents)
                    yield return write;
            }
        }

        internal bool IsBefore(DependencyList other)
        {
            foreach (var it in _before)
                if (string.Compare(it, other.Name, true) == 0)
                    return true;
            foreach (var it in other._after)
                if (string.Compare(it, Name, true) == 0)
                    return true;
            return false;
        }

        internal bool IsAfter(DependencyList other)
        {
            foreach (var it in _after)
                if (string.Compare(it, other.Name, true) == 0)
                    return true;
            foreach (var it in other._before)
                if (string.Compare(it, Name, true) == 0)
                    return true;
            return false;
        }

        internal bool HasWriteComponent(in ComponentType type)
        {
            foreach (var it in _writeComponents)
                if (it.ID == type.ID)
                    return true;
            return false;
        }
        internal bool HasReadComponent(in ComponentType type)
        {
            foreach (var it in _readComponents)
                if (it.ID == type.ID)
                    return true;
            return false;
        }

        internal bool HasComponent(in ComponentType type, out bool writable)
        {
            if (HasWriteComponent(type))
            {
                writable = true;
                return true;
            }
            writable = false;
            if (HasReadComponent(type))
                return true;

            return false;
        }

        internal bool HasComponent(in ComponentType type) => HasWriteComponent(type) || HasReadComponent(type);

        public bool IsDependentOn(DependencyList other)
        {
            if (IsBefore(other)) return false;
            else if (other.IsBefore(this)) return true;

            if (other.Priority < Priority) return true; //we depend on them first
            else if (other.Priority > Priority) return false; //they are dependent on us

            foreach (var read in _readComponents)
                if (other._writeComponents.Contains(read))
                    return true;

            return false;
        }

        public static DependencyList MergeComponents(string name, int priority, IEnumerable<DependencyList> lists)
        {
            var list = new DependencyList(name, priority);

            foreach (var it in lists)
                foreach (var write in it._writeComponents)
                    if (list._writeComponents.Add(write))
                        list._allComponents.Add(write);

            foreach (var it in lists)
                foreach (var read in it._readComponents)
                    if (!list._writeComponents.Contains(read))
                        if (list._readComponents.Add(read))
                            list._allComponents.Add(read);

            return list;
        }
    }

    public enum ComponentState
    {
        Missing,
        Read,
        Write
    }

    public class ComponentMatrix
    {
        private ISystem[] _systems;
        private ComponentType[] _components;

        public readonly int Rows;
        public readonly int Columns;

        public ReadOnlySpan<ISystem> Systems => _systems;
        public ReadOnlySpan<ComponentType> Components => _components;

        private ComponentState[] _matrix;

        public ReadOnlySpan<ComponentState> GetRow(int row)
        {
            return new ReadOnlySpan<ComponentState>(_matrix, (row * _components.Length), _components.Length);
        }

        public ReadOnlySpan<ComponentState> GetRow(ISystem system)
        {
            for (var i = 0; i < _systems.Length; i++)
                if (_systems[i] == system)
                    return GetRow(i);

            throw new ArgumentException(nameof(system));
        }

        public ComponentState this[ISystem system, in ComponentType componentType]
        {
            get
            {
                var componentIndex = -1;
                for (var i = 0; i < _components.Length; i++)
                {
                    if (_components[i].ID == componentType.ID)
                    {
                        componentIndex = i;
                        break;
                    }
                }

                if (componentIndex == -1)
                    return ComponentState.Missing;

                var systemIndex = -1;
                for (var i = 0; i < _systems.Length; i++)
                {
                    if (_systems[i] == system)
                    {
                        systemIndex = i;
                        break;
                    }
                }

                if (systemIndex == -1)
                    return ComponentState.Missing;

                return _matrix[(systemIndex * _components.Length) + componentIndex];
            }
        }

        public ComponentMatrix(IEnumerable<ISystem> systems)
        {
            _systems = systems.ToArray();

            _components = _systems
                            .SelectMany(it => it.Dependencies.Components)
                            .GroupBy(it => it)
                            .Where(it => it.Count() > 1)
                            .Select(it => it.Key)
                            .ToArray();

            _matrix = new ComponentState[_systems.Length * _components.Length];

            for (var i = 0; i < _systems.Length; i++)
            {
                for (var k = 0; k < _components.Length; k++)
                {
                    var system = _systems[i];
                    var component = _components[k];
                    var idx = (i * _components.Length) + k;

                    if (system.Dependencies.HasComponent(component, out var writable))
                    {
                        if (writable)
                            _matrix[idx] = ComponentState.Write;
                        else
                            _matrix[idx] = ComponentState.Read;
                    }
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            var systemNameLength = _systems.Max(it => it.Name.Length);
            var componentNameLength = _components.Max(it => ComponentType.LookUp(it).Name.Length);

            for (var row = -1; row < _systems.Length; row++)
            {
                if (row == -1)
                {
                    sb.Append(' ', systemNameLength + 2);
                    for (var col = 0; col < _components.Length; col++)
                    {
                        var type = ComponentType.LookUp(_components[col]);
                        sb.Append(type.Name);
                        sb.Append(' ', componentNameLength - type.Name.Length + 1);
                    }
                }
                else
                {
                    for (var col = -1; col < _components.Length; col++)
                    {
                        if (col == -1)
                        {
                            sb.Append(_systems[row].Name);
                            sb.Append(' ', systemNameLength - _systems[row].Name.Length + 2);
                        }
                        else
                        {
                            var idx = (row * _components.Length) + col;
                            var state = _matrix[idx];

                            var wrote = 0;
                            if (state == ComponentState.Read)
                            {
                                sb.Append("Read");
                                wrote = 4;
                            }
                            else if (state == ComponentState.Write)
                            {
                                sb.Append("Write");
                                wrote = 5;
                            }
                            sb.Append(' ', componentNameLength - wrote + 1);
                        }
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

}