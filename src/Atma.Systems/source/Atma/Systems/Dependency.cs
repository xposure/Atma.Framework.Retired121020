namespace Atma.Systems
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Atma.Common;
    using Atma.Entities;

    public abstract class Dependency : IEquatable<Dependency>
    {
        public bool Equals(Dependency other) => OnEqual(other);

        protected abstract bool OnEqual(Dependency other);

        public void Resolve(DirectedGraph<ISystem> graph, ISystem a, ISystem b)
        {
            OnResolve(graph, a, b);
        }
        protected abstract void OnResolve(DirectedGraph<ISystem> graph, ISystem a, ISystem b);
    }

    public class ComponentDependency<T> : ComponentDependency
            where T : unmanaged
    {
        public ComponentDependency(bool writable) : base(ComponentType<T>.Type, writable) { }
    }

    public class ComponentDependency : Dependency
    {
        private ComponentType _componentType;
        public ComponentType ComponentType => _componentType;

        public readonly bool Writable;

        public ComponentDependency(in ComponentType componentType, bool writable)
        {
            _componentType = componentType;
            Writable = writable;
        }

        protected override bool OnEqual(Dependency other)
        {
            if (other is null || !(other is ComponentDependency it))
                return false;

            return _componentType.ID == it._componentType.ID && Writable == it.Writable;
        }

        protected override void OnResolve(DirectedGraph<ISystem> graph, ISystem a, ISystem b)
        {
            foreach (var it in b.Dependencies)
            {
                if (it is ComponentDependency dep && dep.ComponentType.ID == _componentType.ID)
                {
                    //TODO: if both of these are writable, we are going to have cyclic issues
                    if (Writable != dep.Writable)
                    {
                        if (Writable)
                            graph.AddEdge(a, b);
                        else
                            graph.AddEdge(b, a);
                    }
                }
            }
        }
    }

    public class ReadDependency<T> : ComponentDependency<T>
            where T : unmanaged
    {
        public ReadDependency() : base(false) { }
    }

    public class WriteDependency<T> : ComponentDependency<T>
        where T : unmanaged
    {
        public WriteDependency() : base(true) { }
    }

    public class BeforeDependency : Dependency
    {
        public string Name { get; }
        public BeforeDependency(string name)
        {
            Name = name;
        }

        protected override bool OnEqual(Dependency other)
        {
            if (other is null || !(other is BeforeDependency it))
                return false;

            return Name == it.Name;
        }

        protected override void OnResolve(DirectedGraph<ISystem> graph, ISystem a, ISystem b)
        {
            if (string.Compare(Name, b.Name, true) == 0)
                graph.AddEdge(a, b);
        }
    }


    public class AfterDependency : Dependency
    {
        public string Name { get; }
        public AfterDependency(string name)
        {
            Name = name;
        }

        protected override bool OnEqual(Dependency other)
        {
            if (other is null || !(other is AfterDependency it))
                return false;

            return Name == it.Name;
        }

        protected override void OnResolve(DirectedGraph<ISystem> graph, ISystem a, ISystem b)
        {
            if (string.Compare(Name, b.Name, true) == 0)
                graph.AddEdge(b, a);
        }
    }

    public enum DependencyState
    {
        Missing,
        Read,
        Write
    }

    public class DependencyMatrix
    {
        private ISystem[] _systems;
        private ComponentType[] _components;

        public readonly int Rows;
        public readonly int Columns;

        public ReadOnlySpan<ISystem> Systems => _systems;
        public ReadOnlySpan<ComponentType> Components => _components;

        private DependencyState[] _matrix;

        public ReadOnlySpan<DependencyState> GetRow(int row)
        {
            return new ReadOnlySpan<DependencyState>(_matrix, (row * _components.Length), _components.Length);
        }

        public ReadOnlySpan<DependencyState> GetRow(ISystem system)
        {
            for (var i = 0; i < _systems.Length; i++)
                if (_systems[i] == system)
                    return GetRow(i);

            throw new ArgumentException(nameof(system));
        }

        public DependencyState this[ISystem system, in ComponentType componentType]
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
                    return DependencyState.Missing;

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
                    return DependencyState.Missing;

                return _matrix[(systemIndex * _components.Length) + componentIndex];
            }
        }

        public DependencyMatrix(IEnumerable<ISystem> systems)
        {
            _systems = systems.ToArray();

            _components = _systems
                            .SelectMany(it => it.Dependencies.OfType<ComponentDependency>())
                            .GroupBy(it => it.ComponentType)
                            .Where(it => it.Count() > 1)
                            .Select(it => it.Key)
                            .ToArray();

            _matrix = new DependencyState[_systems.Length * _components.Length];

            for (var i = 0; i < _systems.Length; i++)
            {
                for (var k = 0; k < _components.Length; k++)
                {
                    var system = _systems[i];
                    var component = _components[k];
                    var idx = (i * _components.Length) + k;

                    var dep =
                        system
                            .Dependencies
                            .OfType<ComponentDependency>()
                            .FirstOrDefault(it => it.ComponentType.ID == component.ID);

                    if (dep != null)
                    {
                        if (dep.Writable)
                            _matrix[idx] = DependencyState.Write;
                        else
                            _matrix[idx] = DependencyState.Read;
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
                            if (state == DependencyState.Read)
                            {
                                sb.Append("Read");
                                wrote = 4;
                            }
                            else if (state == DependencyState.Write)
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