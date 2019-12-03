namespace Atma.Systems
{
    using System;
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

    public class ReadDependency<T> : ReadDependency
        where T : unmanaged
    {
        public ReadDependency() : base(ComponentType<T>.Type) { }
    }

    public class ReadDependency : Dependency
    {
        private ComponentType _componentType;
        public ComponentType ComponentType => _componentType;

        public ReadDependency(in ComponentType componentType)
        {
            _componentType = componentType;
        }

        protected override bool OnEqual(Dependency other)
        {
            if (other is null || !(other is ReadDependency it))
                return false;

            return _componentType.ID == it._componentType.ID;
        }

        protected override void OnResolve(DirectedGraph<ISystem> graph, ISystem a, ISystem b)
        {

        }
    }
    public class WriteDependency : Dependency
    {
        private ComponentType _componentType;

        public ComponentType ComponentType => _componentType;

        public WriteDependency(in ComponentType componentType)
        {
            _componentType = componentType;
        }

        protected override bool OnEqual(Dependency other)
        {
            if (other is null || !(other is WriteDependency it))
                return false;

            return _componentType.ID == it._componentType.ID;
        }

        protected override void OnResolve(DirectedGraph<ISystem> graph, ISystem a, ISystem b)
        {
            foreach (var it in b.Dependencies)
                if (it is ReadDependency read && read.ComponentType.ID == _componentType.ID)
                    graph.AddEdge(a, b);
        }
    }

    public class WriteDependency<T> : WriteDependency
        where T : unmanaged
    {
        public WriteDependency() : base(ComponentType<T>.Type) { }
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
}