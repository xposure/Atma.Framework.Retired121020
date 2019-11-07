namespace Atma.Entities
{
    using Atma.Jobs;
    using Atma.Profiling;
    using System;
    using System.Collections.Generic;
    using static Atma.Debug;

    public class DefaultComponentList { }

    public sealed class SystemManager : IInitService
    {
        private readonly EntityManager _entityManager;
        private readonly JobManager _jobManager;
        private readonly IProfileService _profiler;

        private ComponentSystemList _default;
        private ComponentSystemList _root;
        private Dictionary<Type, ComponentSystemList> _groups = new Dictionary<Type, ComponentSystemList>();
        private List<DependencyResolver> _resolvers = new List<DependencyResolver>();
        private List<ComponentSystem> _newSystems = new List<ComponentSystem>();

        public SystemManager(EntityManager entityManager, JobManager jobManager, IProfileService profiler = null)
        {
            _entityManager = entityManager;
            _jobManager = jobManager;
            _profiler = profiler ?? NullProfileService.Null;
            _root = new ComponentSystemList(this, typeof(SystemManager), _profiler);
            _default = new ComponentSystemList(this, typeof(DefaultComponentList), _profiler);
        }

        public IEnumerable<DependencyResolver> Resolvers => _resolvers;

        public void Initialize()
        {
            AddDefaultDependencyResolvers();

            foreach (var it in _newSystems)
                it.Created();

            _newSystems.Clear();

            SortComponentSystems();
        }

        public void AddSystem(ComponentSystem system)
        {
            //system.EntityManager = _entityManager;
            //if (system is JobComponentSystem jobSystem)
            //    jobSystem.JobManager = _jobManager;

            var group = GetGroup(system);
            group.AddSystem(system);
            _newSystems.Add(system);
        }

        public void AddDefaultDependencyResolvers()
        {
            _resolvers.Add(new DependencyUpdateBefore());
            _resolvers.Add(new DependencyUpdateAfter());
            _resolvers.Add(new DependencyView());
        }

        private ComponentSystemList GetGroup(ComponentSystemBase system)
        {
            var group = GetGroup(system.Type);
            if (group == null)
                return _root;

            if (!_groups.TryGetValue(group, out var list))
            {
                list = new ComponentSystemList(this, group, _profiler);

                _groups.Add(list.Type, list);
                var parent = GetGroup(list);
                parent.AddSystem(list);
            }
            return list;
        }

        internal static Type GetGroup(Type type)
        {
            var groupAttrs = type.GetCustomAttributes(typeof(UpdateGroup), true);
            if (groupAttrs.Length == 1)
            {
                var groupAttr = (UpdateGroup)groupAttrs[0];
                return groupAttr.Type;
            }

            return null;
        }

        public void SortComponentSystems()
        {
            _root.Sort();
        }

        public ComponentSystemList Root => _root;

        public void UpdateGroup<T>()
            where T : unmanaged
        {
            if (_groups.TryGetValue(typeof(T), out var group))
                group.Update();
            //Assert(group != null);
            //group.InternalUpdate();
        }
    }
}
