namespace Atma.Systems
{
    using Atma.Common;

    public abstract class Dependency
    {
        public void Resolve(DirectedGraph<ISystem> graph, ISystem a, ISystem b)
        {
            OnResolve(graph, a, b);
        }
        protected abstract void OnResolve(DirectedGraph<ISystem> graph, ISystem a, ISystem b);
    }

    public sealed class ReadDependency<T> : Dependency
        where T : unmanaged
    {
        protected override void OnResolve(DirectedGraph<ISystem> graph, ISystem a, ISystem b)
        {
            foreach (var it in b.Dependencies)
                if (it is WriteDependency<T>)
                    graph.AddEdge(a, b);
        }
    }
    public sealed class WriteDependency<T> : Dependency
        where T : unmanaged
    {
        protected override void OnResolve(DirectedGraph<ISystem> graph, ISystem a, ISystem b)
        {

        }
    }

    public sealed class BeforeDependency : Dependency
    {
        public string Name { get; }
        public BeforeDependency(string name)
        {
            Name = name;
        }
        protected override void OnResolve(DirectedGraph<ISystem> graph, ISystem a, ISystem b)
        {
            if (string.Compare(Name, b.Name, true) == 0)
                graph.AddEdge(b, a);
        }
    }


    public sealed class AfterDependency : Dependency
    {
        public string Name { get; }
        public AfterDependency(string name)
        {
            Name = name;
        }
        protected override void OnResolve(DirectedGraph<ISystem> graph, ISystem a, ISystem b)
        {
            if (string.Compare(Name, b.Name, true) == 0)
                graph.AddEdge(a, b);
        }
    }
}