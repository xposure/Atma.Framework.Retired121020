namespace Atma.Systems
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Atma.Common;
    using Atma.Entities;

    public class SystemGroup : SystemBase
    {
        private DirectedGraph<ISystem> _depGraph = new DirectedGraph<ISystem>();
        private List<ISystem> _systems = new List<ISystem>();

        protected internal SystemGroup(string name = null, string group = null, int? priority = null, string[] stages = null)
            : base(name, group, priority, stages) { }


        public T Add<T>(T system)
            where T : ISystem
        {
            if (_inited)
                throw new Exception("Can not modify the group after init.");

            if (!string.IsNullOrEmpty(system.Group))
            {
                var groups = system.Group.Split('/');
                return AddInternal(groups, system);
            }

            return AddInternal(Span<string>.Empty, system);
        }

        private T AddInternal<T>(Span<string> group, T system)
            where T : ISystem
        {
            if (group.Length == 0)
            {
                _systems.Add(system);
                return system;
            }

            var systemGroup = GetOrCreateGroup(group[0]);
            return systemGroup.AddInternal(group.Slice(1), system);
        }

        private SystemGroup GetOrCreateGroup(string group)
        {
            for (var i = 0; i < _systems.Count; i++)
                if (_systems[i] is SystemGroup g && string.Compare(g.Name, group, true) == 0)
                    return g;

            return Add(new SystemGroup(group, null, 0));
        }

        protected override void OnTick(SystemManager systemManager, EntityManager entityManager)
        {
            foreach (var it in _depGraph.PostOrder())
                if (!it.Disabled)
                    it.Tick(systemManager, entityManager);
        }

        protected override void OnInit()
        {
            if (_systems.Count > 0)
            {
                for (var i = 0; i < _systems.Count; i++)
                {
                    _depGraph.AddNode(_systems[i]);
                    _systems[i].Init();
                }
            }
        }

        protected override void OnGatherDependencies(DependencyListConfig config)
        {
            config.MergeComponents(_systems.Select(it => it.Dependencies));

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

        public override string ToString() => $"{base.ToString()}, Count: {_systems.Count}";
    }
}