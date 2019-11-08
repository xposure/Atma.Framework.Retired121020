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

    // public interface IEntityManager2
    // {
    //     //IEntityView View<T>() where T : unmanaged;

    //     uint Create(EntitySpec spec);

    //     void Assign<T>(uint entity, in T t) where T : unmanaged;
    //     void Replace<T>(uint entity, in T t) where T : unmanaged;
    //     void Update<T>(uint entity, in T t) where T : unmanaged;

    //     bool Has(uint entity);
    //     bool Has<T>(uint entity) where T : unmanaged;

    //     ref T Get<T>(uint entity) where T : unmanaged;

    //     void Remove(uint entity);
    //     void Remove<T>(uint entity) where T : unmanaged;

    //     void Reset(uint entity);
    //     void Reset<T>(uint entity) where T : unmanaged;

    //     void Move(uint entity, EntitySpec spec);

    //     int EntityCount { get; }
    //     int SpecCount { get; }
    // }

    public sealed partial class EntityManager : UnmanagedDispose//, IEntityManager2
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

        public int EntityCount { get => _entityArrays.Sum(x => x.EntityCount); }
        public int SpecCount { get => _knownSpecs.Count; }
        public IReadOnlyList<EntityChunkArray> EntityArrays => _entityArrays;

        private EntityPool2 _entityPool = new EntityPool2();
        private LookupList<EntitySpec> _knownSpecs = new LookupList<EntitySpec>();
        private List<EntityChunkArray> _entityArrays = new List<EntityChunkArray>();

        public EntityManager()
        {
            //take the first one to reserve 0 as invalid
            _entityPool.Take();
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
            var entity = _entityPool.Take();
            ref var e = ref _entityPool[entity];

            var index = _entityArrays[specIndex].Create(entity, out var chunkIndex);
            e = new Entity2(entity, specIndex, chunkIndex, index);
            return entity;
        }

        public void Assign<T>(uint entity, in T t)
            where T : unmanaged
        {
            Assert(!Has<T>(entity));

            ref var entityInfo = ref _entityPool[entity];
            var srcSpec = _entityArrays[entityInfo.SpecIndex].Specification;

            //we need to move the entity to the new spec
            Span<ComponentType> componentTypes = stackalloc ComponentType[srcSpec.ComponentTypes.Length + 1];
            srcSpec.ComponentTypes.CopyTo(componentTypes);

            var newComponentType = ComponentType<T>.Type;
            componentTypes[srcSpec.ComponentTypes.Length] = newComponentType;

            var specId = ComponentType.CalculateId(componentTypes);
            var dstSpecIndex = GetOrCreateSpec(componentTypes);
            //var dstSpec = _knownSpecs[specIndex];

            Move(ref entityInfo, dstSpecIndex);
            //Move(entity, dstSpec);
            Replace<T>(ref entityInfo, t);
        }

        public ref T Get<T>(uint entity)
            where T : unmanaged
        {
            Assert(Has<T>(entity));

            ref readonly var e = ref _entityPool[entity];

            var array = _entityArrays[e.SpecIndex];
            var chunk = array.AllChunks[e.ChunkIndex];
            var span = chunk.PackedArray.GetComponentSpan<T>();

            return ref span[e.Index];
        }

        public bool Has(uint entity)
        {
            return _entityPool.IsValid(entity);
        }

        public bool Has<T>(uint entity)
            where T : unmanaged
        {
            ref var e = ref _entityPool[entity];
            return Has<T>(ref e);
        }

        private bool Has<T>(ref Entity2 entityInfo)
            where T : unmanaged
        {
            var spec = _entityArrays[entityInfo.SpecIndex].Specification;
            return spec.Has(ComponentType<T>.Type);
        }

        public void Remove(uint entity)
        {
            ref var e = ref _entityPool[entity];
            Remove(ref e);
            _entityPool.Return(entity);
        }

        private void Remove(ref Entity2 e)
        {
            var array = _entityArrays[e.SpecIndex];

            //we are moving the last element to the element deleted
            //we need to patch the array, but the array doesn't store ids            
            var movedIndex = array.Delete(e.ChunkIndex, e.Index);
            if (movedIndex > -1)
            {
                var chunk = array.AllChunks[e.ChunkIndex];
                var movedId = chunk.GetEntity(movedIndex);
                ref var movedEntity = ref _entityPool[movedId];
                movedEntity = new Entity2(movedId, movedEntity.SpecIndex, movedEntity.ChunkIndex, movedIndex);
            }
        }

        public void Remove<T>(uint entity)
            where T : unmanaged
        {
            Assert(Has<T>(entity));

            var oldComponentType = ComponentType<T>.Type;
            var oldComponentId = oldComponentType.ID;
            ref var entityInfo = ref _entityPool[entity];
            var srcSpec = _entityArrays[entityInfo.SpecIndex].Specification;

            var srcComponentCount = srcSpec.ComponentTypes.Length - 1;
            if (srcComponentCount == 0)
                //TODO: Should we throw an exception instead?
                Remove(entity); //if there are no more components, delete the entity
            else
            {
                //we need to move the entity to the new spec
                var copyIndex = 0;
                Span<ComponentType> componentTypes = stackalloc ComponentType[srcSpec.ComponentTypes.Length - 1];
                for (var i = 0; i < srcSpec.ComponentTypes.Length; i++)
                    if (srcSpec.ComponentTypes[i].ID != oldComponentId)
                        componentTypes[copyIndex++] = srcSpec.ComponentTypes[i];

                var specId = ComponentType.CalculateId(componentTypes);
                var dstSpecIndex = GetOrCreateSpec(componentTypes);

                Move(ref entityInfo, dstSpecIndex);
            }
        }

        public void Replace<T>(uint entity, in T t)
            where T : unmanaged
        {
            ref var e = ref _entityPool[entity];
            Replace<T>(ref e, t);
        }

        public void Replace<T>(ref Entity2 entity, in T t)
          where T : unmanaged
        {
            Assert(Has<T>(ref entity));
            var array = _entityArrays[entity.SpecIndex];
            var chunk = array.AllChunks[entity.ChunkIndex];
            var span = chunk.PackedArray.GetComponentSpan<T>();
            span[entity.Index] = t;
        }

        public void Reset(uint entity)
        {
            ref var e = ref _entityPool[entity];
            var array = _entityArrays[e.SpecIndex];
            var chunk = array.AllChunks[e.ChunkIndex];
            chunk.PackedArray.Reset(e.Index);
        }

        public void Reset<T>(uint entity)
            where T : unmanaged
        {
            ref var e = ref _entityPool[entity];
            var array = _entityArrays[e.SpecIndex];
            var chunk = array.AllChunks[e.ChunkIndex];
            chunk.PackedArray.Reset<T>(e.Index);
        }

        public void Update<T>(uint entity, in T t)
            where T : unmanaged
        {
            ref var e = ref _entityPool[entity];
            var spec = _entityArrays[e.SpecIndex].Specification;
            var componentType = ComponentType<T>.Type;
            if (spec.Has(componentType))
                Replace<T>(ref e, t);
            else
                Assign<T>(entity, t);
        }

        public void Move(uint entity, EntitySpec spec)
        {
            ref var e = ref _entityPool[entity];
            var specIndex = GetOrCreateSpec(spec);
            Move(ref e, specIndex);
        }

        private void Move(ref Entity2 entityInfo, int dstSpecIndex)
        {
            var src = _entityArrays[entityInfo.SpecIndex];
            var dst = _entityArrays[dstSpecIndex];


            //we want to create the data at the dst first
            var srcIndex = entityInfo.Index;
            var dstIndex = dst.Create(entityInfo.ID, out var dstChunkIndex);
            var srcChunkIndex = entityInfo.ChunkIndex;

            var srcChunk = src.AllChunks[srcChunkIndex];
            var dstChunk = dst.AllChunks[dstChunkIndex];

            EntityPackedArray.CopyTo(srcChunk.PackedArray, srcIndex, dstChunk.PackedArray, dstIndex);

            //this internal function will swap the entity at the end of the array
            //without returning the id to the pool
            Remove(ref entityInfo);

            //then we want to remap the entity to its new location
            entityInfo = new Entity2(entityInfo.ID, dstSpecIndex, dstChunkIndex, dstIndex);
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
        // public IEnumerable<EntityChunkArray> EntityArrays(Func<EntitySpec, bool> filter)
        // {
        //     for (var i = 0; i < _entityArrays.Count; i++)
        //     {
        //         var array = _entityArrays[i];
        //         if (filter(array.Specification))
        //             yield return array;
        //     }
        // }


    }
}
