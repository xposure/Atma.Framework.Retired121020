namespace Atma.Entities
{
    using Atma.DI;
    using Atma.Memory;
    using static Atma.Debug;

    using System.Collections.Generic;
    using Atma.Profiling;

    [Injectable]
    public abstract class ComponentSystem : ComponentSystemBase
    {

        protected readonly EntityManager EntityManager;

        private HashSet<ComponentType> _readComponents = new HashSet<ComponentType>();
        private HashSet<ComponentType> _writeComponents = new HashSet<ComponentType>();

        private HashSet<ComponentView> _views = new HashSet<ComponentView>();

        internal IReadOnlyCollection<ComponentType> ReadComponents => _readComponents;
        internal IReadOnlyCollection<ComponentType> WriteComponents => _writeComponents;

        internal IReadOnlyCollection<ComponentView> Views => _views;

        internal ComponentSystemList Group { get; set; }

        protected EntityCommandBuffer PostUpdateCommands = new EntityCommandBuffer(Allocator.Persistent, 1024);

        private void AddView(ComponentView view)
        {
            if (_views.Add(view))
            {
                foreach (var it in view.Fields)
                {
                    if (it.IsWriteable)
                        _writeComponents.Add(it.ComponentType);
                    else
                        _readComponents.Add(it.ComponentType);
                }

                Group?.Dirty();
            }
        }

        protected EntityView<T> AddView<T>(EntityView<T> entityView)
            where T : unmanaged
        {
            AddView(entityView.View);
            return entityView;
        }

        public void AddView(EntityView entityView)
        {
            AddView(entityView.View);
        }

        internal override void InternalUpdate()
        {
            using var scope = Profiler.Current.Begin(Type.Name);
            Assert(EntityManager != null);
            Update();
            PostUpdateCommands.Playback(EntityManager);
        }

        protected abstract void Update();
    }

}
