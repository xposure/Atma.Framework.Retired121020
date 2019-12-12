namespace Atma.Systems
{
    using System;
    using System.Reflection;
    using Atma.Common;
    using Atma.Entities;

    public abstract class SystemBase : ISystem
    {
        protected bool _inited { get; private set; }
        protected readonly Type _type;

        private DependencyList _dependencies;

        public DependencyList Dependencies => _dependencies;

        public int Priority { get; }
        public string Name { get; }
        public string Group { get; }

        public string[] Stages { get; }
        public bool Disabled { get; set; }

        protected internal SystemBase(string name = null, string group = null, int? priority = null, string[] stages = null)
        {
            _type = this.GetType();

            Name = name ?? _type.GetCustomAttribute<NameAttribute>()?.Name ?? _type.Name;
            Group = group ?? _type.GetCustomAttribute<GroupAttribute>()?.Name ?? null;
            Priority = priority ?? _type.GetCustomAttribute<PriorityAttribute>()?.Priority ?? 0;
            Stages = stages ?? _type.GetCustomAttribute<StagesAttribute>()?.Stages ?? null;
        }

        public void Tick(SystemManager systemManager, EntityManager entityManager)
        {
            if (!Disabled)
                OnTick(systemManager, entityManager);
        }

        protected abstract void OnTick(SystemManager systemManager, EntityManager entityManager);

        public virtual void Init()
        {
            if (_inited)
                return;

            _inited = true;
            OnInit();

            _dependencies = new DependencyList(Name, Priority, GatherDependencies);
        }

        protected abstract void OnInit();

        private void GatherDependencies(DependencyListConfig config)
        {
            config.Type(_type);
            OnGatherDependencies(config);
        }

        protected abstract void OnGatherDependencies(DependencyListConfig config);

        public override string ToString() => $"Name: {Name}, Dep: {_dependencies.ToString()}";
    }
}