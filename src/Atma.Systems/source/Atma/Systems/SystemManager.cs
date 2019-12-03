namespace Atma.Systems
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Atma.Common;
    using Atma.Entities;
    using Microsoft.Extensions.Logging;

    public sealed class SystemGroup : ISystem
    {
        private DirectedGraph<ISystem> _depGraph = new DirectedGraph<ISystem>();
        private List<ISystem> _systems = new List<ISystem>();
        private List<Dependency> _dependencies = new List<Dependency>();

        public IEnumerable<Dependency> Dependencies => _dependencies;

        public int Priority { get; }
        public string Name { get; }
        //public string Group { get; }
        public SystemGroup(string name, int priority)
        {
            Name = name;
            //Group = group;
        }

        public T Add<T>(T system)
            where T : ISystem
        {
            _systems.Add(system);
            _depGraph.AddNode(system);
            return system;
        }

        public void Tick(SystemManager systemManager, EntityManager entityManager)
        {
            foreach (var it in _depGraph.PostOrder())
                it.Tick(systemManager, entityManager);
        }

        public void Init()
        {
            if (_systems.Count > 0)
            {
                for (var i = 0; i < _systems.Count; i++)
                    _systems[i].Init();

                var readComponents = new HashSet<ComponentType>();
                var writeComponents = new HashSet<ComponentType>();

                for (var i = 0; i < _systems.Count; i++)
                {
                    var a = _systems[i];

                    for (var j = 0; j < _systems.Count; j++)
                    {
                        if (i == j) continue;
                        var b = _systems[j];

                        if (a.Priority < b.Priority)
                            _depGraph.AddEdge(a, b);
                        else if (a.Priority > b.Priority)
                            _depGraph.AddEdge(b, a);
                        else
                        {
                            foreach (var dep in a.Dependencies)
                                dep.Resolve(_depGraph, a, b);
                        }
                    }

                    foreach (var dep in a.Dependencies)
                    {
                        if (dep is ReadDependency read)
                            readComponents.Add(read.ComponentType);
                        else if (dep is WriteDependency write)
                            writeComponents.Add(write.ComponentType);
                    }
                }


                foreach (var read in readComponents)
                    if (!writeComponents.Contains(read))
                        _dependencies.Add(new ReadDependency(read));

                foreach (var write in writeComponents)
                    _dependencies.Add(new WriteDependency(write));

                if (!_depGraph.Validate())
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Cyclic nodes detected");
                    foreach (var cyclic in _depGraph.CyclicNodes)
                    {
                        var recursion = string.Join("->", _depGraph.PostOrder(cyclic).Select(it => it.Name));
                        sb.AppendLine(recursion);
                    }

                    throw new Exception(sb.ToString());
                }
            }
        }
        public override string ToString() => $"Name: {Name}, Count: {_systems.Count}";
    }

    public sealed class SystemManager
    {
        private ILoggerFactory _logFactory;
        private ILogger _logger;

        private SystemGroup _systems = new SystemGroup("Root", 0);

        public SystemManager(ILoggerFactory logFactory)
        {
            _logFactory = logFactory;
            _logger = logFactory.CreateLogger<SystemManager>();
        }

        public T Add<T>(T system)
            where T : ISystem
        {
            _systems.Add(system);
            return system;
        }

        public void Init()
        {
            _systems.Init();
        }

        public void Tick(EntityManager entityManager)
        {
            _systems.Tick(this, entityManager);
        }

    }
}