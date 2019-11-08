namespace Atma.Entities
{
    using Atma.Common;
    using Atma.Memory;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using static Atma.Debug;

    // public interface IEntityView2
    // {

    // }

    // public interface IEntityView2<T> : IEntityView
    // {
    //     void ForEach(T t, int length);
    // }

    public interface IEntityManager2
    {
        //IEntityView View<T>() where T : unmanaged;

        uint Create(EntitySpec spec);

        void Assign<T>(uint entity, in T t) where T : unmanaged;
        void Replace<T>(uint entity, in T t) where T : unmanaged;
        void Update<T>(uint entity, in T t) where T : unmanaged;

        bool Has(uint entity);
        bool Has<T>(uint entity) where T : unmanaged;

        void Remove(uint entity);
        void Remove<T>(uint entity) where T : unmanaged;

        void Reset(uint entity);
        void Reset<T>(uint entity) where T : unmanaged;

        void Move(uint entity, EntitySpec spec);
    }

    public sealed partial class EntityManager2 : UnmanagedDispose, IEntityManager2
    {
        // //private DynamicMemoryPool _persistentMemory = new DynamicMemoryPool();
        // private List<EntityArchetype> _archetypes = new List<EntityArchetype>();
        // private LookupList<EntityArchetype> _archetypeIDLookup = new LookupList<EntityArchetype>();
        // //private LookupList<ComponentType> _componentTypes = new LookupList<ComponentType>();

        // private ComponentList _componentList = new ComponentList();
        // private ComponentViewList _componentViewList;
        // private EntityPool _entityPool = new EntityPool();

        // internal IReadOnlyList<EntityArchetype> Archetypes => _archetypes;
        // internal ComponentList ComponentList => _componentList;

        // public EntityManager2(ComponentList componentList)
        // {
        //     // _componentList = componentList;
        //     // _componentViewList = new ComponentViewList(componentList);
        //     // _archetypes.Add(new EntityArchetype(this, 0, 0));

        //     // _entityPool.Take(); //we take the first entity because we use ID as INVALID;
        // }
        private EntityPool2 _entityPool = new EntityPool2();
        private LookupList<EntitySpec> _knownSpecs = new LookupList<EntitySpec>();
        private List<EntityChunkArray> _entityArrays = new List<EntityChunkArray>();

        public EntityManager2()
        {
            //take the first one to reserve 0 as invalid
            _entityPool.Take(0, 0, 0);
        }

        private int GetOrCreateSpec(EntitySpec spec)
        {
            var specIndex = _knownSpecs.IndexOf(spec.ID);
            if (specIndex == -1)
            {
                //we are limited to this many unique specs per EM
                specIndex = _knownSpecs.Count;
                Assert(specIndex < Entity.SPEC_MAX);
                _knownSpecs.Add(spec.ID, spec);
                _entityArrays.Add(new EntityChunkArray(spec));
            }
            return specIndex;
        }

        private int GetOrCreateSpec(Span<ComponentType> componentTypes)
        {
            var specId = ComponentType.CalculateId(componentTypes);
            var specIndex = _knownSpecs.IndexOf(specId);
            if (specIndex == -1)
            {
                var spec = new EntitySpec(specId, componentTypes.ToArray());
                //we are limited to this many unique specs per EM
                specIndex = _knownSpecs.Count;
                Assert(specIndex < Entity.SPEC_MAX);
                _knownSpecs.Add(spec.ID, spec);
                _entityArrays.Add(new EntityChunkArray(spec));
            }
            return specIndex;
        }

        public uint Create(EntitySpec spec)
        {
            var specIndex = GetOrCreateSpec(spec);
            var index = _entityArrays[specIndex].Create(out var chunkIndex);
            return _entityPool.Take(specIndex, chunkIndex, index);
        }

        public void Assign<T>(uint entity, in T t)
            where T : unmanaged
        {
            Assert(!Has<T>(entity));

            ref readonly var e = ref _entityPool.Get(entity);
            var spec = _knownSpecs[e.SpecIndex];


            //we need to move the entity to the new spec
            Span<ComponentType> componentTypes = stackalloc ComponentType[spec.ComponentTypes.Length + 1];
            spec.ComponentTypes.CopyTo(componentTypes);

            var newComponentType = ComponentType<T>.Type;
            componentTypes[spec.ComponentTypes.Length] = newComponentType;

            var specId = ComponentType.CalculateId(componentTypes);
            var specIndex = GetOrCreateSpec(componentTypes);
            var newSpec = _knownSpecs[specIndex];

            Move(entity, spec);
            Replace<T>(entity, t);
        }

        public bool Has(uint entity)
        {
            return _entityPool.IsValid(entity);
        }

        public bool Has<T>(uint entity)
            where T : unmanaged
        {
            // Assert(Has(entity));
            // var componentType = ComponentType<T>.Type;
            throw new NotImplementedException();

        }

        public void Remove(uint entity)
        {
            _entityPool.Return(entity);
        }

        public void Remove<T>(uint entity)
            where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void Replace<T>(uint entity, in T t)
            where T : unmanaged
        {
            Assert(Has<T>(entity));
            ref readonly var e = ref _entityPool.Get(entity);
            var array = _entityArrays[e.SpecIndex];
            //var chunk = array.AllChunks

            //using var writelock
            throw new NotImplementedException();
        }

        public void Reset(uint entity)
        {
            throw new NotImplementedException();
        }

        public void Reset<T>(uint entity)
            where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void Update<T>(uint entity, in T t)
            where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void Move(uint enitty, EntitySpec spec)
        {

        }

        // public IEntityView View<T>() where T : unmanaged
        // {
        //     throw new NotImplementedException();
        // }


        // internal void GetEntityStorage(int entity, out EntityArchetype archetype, out ArchetypeChunk chunk, out int index)
        // {
        //     ref var e = ref _entityPool[entity];
        //     Assert(e.IsValid);

        //     archetype = _archetypes[e.ArchetypeIndex];
        //     chunk = archetype.Chunks[e.ChunkIndex];
        //     index = e.Index;
        // }


    }
}
