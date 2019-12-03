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
        private ILogger _logger;

        private DirectedGraph<ISystem> _depGraph = new DirectedGraph<ISystem>();
        private List<ISystem> _systems = new List<ISystem>();
        private DependencyList _dependencies;

        public DependencyList Dependencies => _dependencies;

        public int Priority { get; }
        public string Name { get; }

        public bool Disabled { get; set; }
        //public string Group { get; }
        public SystemGroup(ILoggerFactory logFactory, string name, int priority)
        {
            Name = name;
            _logger = logFactory.CreateLogger($"SystemGroup::{name}[{priority}]");
            Priority = priority;
            //Group = group;
        }

        public T Add<T>(T system)
            where T : ISystem
        {
            _systems.Add(system);
            return system;
        }

        public void Tick(SystemManager systemManager, EntityManager entityManager)
        {
            foreach (var it in _depGraph.PostOrder())
                if (!it.Disabled)
                    it.Tick(systemManager, entityManager);
        }

        public void Init()
        {
            _depGraph.Clear();
            if (_systems.Count > 0)
            {
                for (var i = 0; i < _systems.Count; i++)
                {
                    _depGraph.AddNode(_systems[i]);
                    _systems[i].Init();
                }

                _dependencies = DependencyList.MergeComponents(Name, Priority, _systems.Select(it => it.Dependencies));

                for (var i = 0; i < _systems.Count; i++)
                {
                    var a = _systems[i].Dependencies;
                    for (var j = 0; j < _systems.Count; j++)
                    {
                        if (i == j) continue;
                        var b = _systems[j].Dependencies;
                        if (a.IsDependentOn(b))
                            _depGraph.AddEdge(_systems[i], _systems[j]);
                    }
                }

                if (!_depGraph.Validate())
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Cyclic nodes detected");

                    sb.AppendLine();
                    var depMatirx = new ComponentMatrix(_depGraph.PostOrder(_depGraph.CyclicNode));
                    sb.AppendLine(depMatirx.ToString());
                    sb.AppendLine();

                    sb.AppendLine(_depGraph.ToString());

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

        private EntityManager _entityManager;

        private SystemGroup _systems;

        public SystemManager(ILoggerFactory logFactory, EntityManager em)
        {
            _logFactory = logFactory;
            _logger = logFactory.CreateLogger<SystemManager>();
            _entityManager = em;
            _systems = new SystemGroup(logFactory, "Root", 0);
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

        public void Tick()
        {
            if (!_systems.Disabled)
                _systems.Tick(this, _entityManager);
        }
    }


}