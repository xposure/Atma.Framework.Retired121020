namespace Atma.Entities
{
    using Atma.Common;
    using Atma.Profiling;
    using System;
    using System.Collections.Generic;
    using System.Text;

    using static Atma.Debug;

    public sealed class ComponentSystemList : ComponentSystemBase
    {
        private SystemManager _manager;
        private bool _sorted = false;
        private DirectedGraph<ComponentSystemBase> _graph = new DirectedGraph<ComponentSystemBase>();
        private List<ComponentSystemBase> _allSystems = new List<ComponentSystemBase>();
        private List<ComponentSystem> _systems = new List<ComponentSystem>();
        private List<ComponentSystemList> _groups = new List<ComponentSystemList>();
        private List<ComponentSystemBase> _ordered = new List<ComponentSystemBase>();

        internal ComponentSystemList(SystemManager manager, Type type, IProfileService profiler) : base(profiler)
        {
            _manager = manager;
            Type = type;
        }

        internal void Dirty()
        {
            _sorted = false;
        }

        internal override void InternalUpdate()
        {
            using var scope = Profiler.Current.Begin(Type.Name);
            foreach (var it in Ordered)
                it.InternalUpdate();
        }

        public void Update()
        {
            InternalUpdate();
        }

        internal void AddSystem(ComponentSystemBase systemImpl)
        {
            _sorted = false;
            _allSystems.Add(systemImpl);

            if (systemImpl is ComponentSystemList list)
                _groups.Add(list);
            else if (systemImpl is ComponentSystem system)
            {
                _systems.Add(system);
                Assert(system.Group == null);
                system.Group = this;
            }
        }

        public ComponentSystemBase GetByType(Type type)
        {
            foreach (var it in _allSystems)
                if (it.Type == type)
                    return it;

            return null;
        }

        public IReadOnlyCollection<ComponentSystemBase> All => _allSystems;

        public IReadOnlyCollection<ComponentSystem> Systems => _systems;

        public IReadOnlyCollection<ComponentSystemList> Groups => _groups;

        public IReadOnlyCollection<ComponentSystemBase> Ordered
        {
            get
            {
                if (!_sorted)
                    Sort();

                return _ordered;
            }
        }

        internal void Sort()
        {
            if (!_sorted && _allSystems.Count > 0)
            {
                _graph.Clear();
                foreach (var it in _allSystems)
                    _graph.AddNode(it);

                foreach (var it in _manager.Resolvers)
                    it.Resolve(this);

                if (!_graph.Validate(true))
                    throw new Exception("Failed to validate the component graph.");

                _ordered.Clear();
                _ordered.AddRange(_graph.ReversePostOrder());

            }

            _sorted = true;
            foreach (var it in _groups)
                it.Sort();
        }

        internal void AddDependency(ComponentSystemBase system, ComponentSystemBase dependent)
        {
            _sorted = false;
            _graph.AddEdge(system, dependent);
            system.AddDependency(dependent);
        }

        internal override void ToString(StringBuilder sb, int depth)
        {
            sb.Append(new string(' ', depth * 2));
            sb.Append('[');
            sb.Append(Type.Name);
            sb.AppendLine("]");

            depth++;
            foreach (var it in DependsOn)
            {
                sb.Append(new string(' ', depth * 2));
                sb.Append('*');
                sb.AppendLine(it.Type.Name);
            }

            foreach (var it in Ordered)
                it.ToString(sb, depth);
        }
    }
}
