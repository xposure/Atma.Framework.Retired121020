namespace Atma.Systems
{
    using System;
    using System.Collections.Generic;
    using Atma.Common;
    using Atma.Entities;
    using Microsoft.Extensions.Logging;

    public sealed class SystemGroup : ISystem
    {
        private DirectedGraph<ISystem> _depGraph = new DirectedGraph<ISystem>();
        private List<ISystem> _systems = new List<ISystem>();
        private Dictionary<string, Dependency> _dependencies = new Dictionary<string, Dependency>();
        public IEnumerable<Dependency> Dependencies => throw new System.NotImplementedException();

        public int Priority { get; }
        public string Name { get; }
        //public string Group { get; }
        public SystemGroup(string name, int priority)
        {
            Name = name;
            //Group = group;
        }

        public void Add(ISystem system)
        {
            _systems.Add(system);
            _depGraph.AddNode(system);
        }

        public void Tick(SystemManager systemManager, EntityManager entityManager)
        {
            foreach (var it in _depGraph.ReversePostOrder())
                it.Tick(systemManager, entityManager);
        }

        public void Init()
        {
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

            }

            _depGraph.Validate(true);
        }
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