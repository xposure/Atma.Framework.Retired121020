namespace Atma.Systems
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Atma.Common;
    using Atma.Entities;
    using Microsoft.Extensions.Logging;

    public class SystemGroup : ISystem
    {
        private bool _inited = false;
        protected readonly Type _type;

        private DirectedGraph<ISystem> _depGraph = new DirectedGraph<ISystem>();
        private List<ISystem> _systems = new List<ISystem>();
        private DependencyList _dependencies;

        public DependencyList Dependencies => _dependencies;

        public int Priority { get; }
        public string Name { get; }
        public bool Disabled { get; set; }

        protected SystemGroup()
        {
            _type = this.GetType();

            var name = _type.GetCustomAttribute<NameAttribute>();
            Name = name?.Name ?? _type.Name;

            var priority = _type.GetCustomAttribute<PriorityAttribute>();
            Priority = priority?.Priority ?? 0;
        }

        internal SystemGroup(string name)
        {
            _type = this.GetType();

            Name = name;

            var priority = _type.GetCustomAttribute<PriorityAttribute>();
            Priority = priority?.Priority ?? 0;
        }

        internal SystemGroup(string name, int priority)
        {
            _type = this.GetType();

            Name = name;
            Priority = priority;
        }

        public T Add<T>(T system)
            where T : ISystem
        {
            if (_inited)
                throw new Exception("Can not modify the group after init.");

            _systems.Add(system);
            return system;
        }

        public void Tick(SystemManager systemManager, EntityManager entityManager)
        {
            foreach (var it in _depGraph.PostOrder())
                if (!it.Disabled)
                    it.Tick(systemManager, entityManager);
        }

        public virtual void Init()
        {
            if (_inited)
                return;

            _inited = true;

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
                    //try
                    //{
                    var depMatirx = new ComponentMatrix(_depGraph.PostOrder(_depGraph.CyclicNode));
                    sb.AppendLine(depMatirx.ToString());
                    sb.AppendLine();
                    //}
                    //catch (Exception ex) { }

                    sb.AppendLine(_depGraph.ToString());

                    throw new Exception(sb.ToString());
                }
            }
        }

        public override string ToString() => $"Name: {Name}, Count: {_systems.Count}";
    }
}