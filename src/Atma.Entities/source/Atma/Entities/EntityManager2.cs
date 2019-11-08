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

        void Assign<T>(uint entity, in T t);
        void Replace<T>(uint entity, in T t);
        void Update<T>(uint entity, in T t);

        bool Has(uint entity);
        bool Has<T>(uint entity) where T : unmanaged;

        void Remove(uint entity);
        void Remove<T>(uint entity);

        void Reset(uint entity);
        void Reset<T>(uint entity);
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
        private List<EntityArray> _entityArrays = new List<EntityArray>();

        public EntityManager2()
        {
            //take the first one to reserve 0 as invalid
            _entityPool.Take(0, 0, 0);
        }

        public uint Create(EntitySpec spec)
        {
            var specIndex = _knownSpecs.IndexOf(spec.ID);
            if (specIndex == -1)
            {
                //we are limited to this many unique specs per EM
                specIndex = _knownSpecs.Count;
                Assert(specIndex < Entity.SPEC_MAX);
                _knownSpecs.Add(spec.ID, spec);
                _entityArrays.Add(new EntityArray(spec));
            }

            var index = _entityArrays[specIndex].Create(out var chunkIndex);
            return _entityPool.Take(specIndex, chunkIndex, index);
        }

        public void Assign<T>(uint entity, in T t)
        {
            throw new NotImplementedException();
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
        {
            throw new NotImplementedException();
        }

        public void Replace<T>(uint entity, in T t)
        {
            throw new NotImplementedException();
        }

        public void Reset(uint entity)
        {
            throw new NotImplementedException();
        }

        public void Reset<T>(uint entity)
        {
            throw new NotImplementedException();
        }

        public void Update<T>(uint entity, in T t)
        {
            throw new NotImplementedException();
        }

        public IEntityView View<T>() where T : unmanaged
        {
            throw new NotImplementedException();
        }


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
