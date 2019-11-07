namespace Atma.Entities
{
    using Atma.Common;
    using Atma.Memory;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using static Atma.Debug;

    public interface IEntityView
    {

    }

    public interface IEntityView<T> : IEntityView
    {
        void ForEach(T t, int length);
    }

    public interface IEntityManager
    {
        IEntityView View<T>() where T : unmanaged;

        void Assign<T>(int entity, in T t);
        void Replace<T>(int entity, in T t);
        void Update<T>(int entity, in T t);

        void Has(int entity);
        void Has<T>(int entity);

        void Remove(int entity);
        void Remove<T>(int entity);

        void Reset(int entity);
        void Reset<T>(int entity);
    }


    public sealed partial class EntityManager : IService
    {
        //private DynamicMemoryPool _persistentMemory = new DynamicMemoryPool();
        private List<EntityArchetype> _archetypes = new List<EntityArchetype>();
        private LookupList<EntityArchetype> _archetypeIDLookup = new LookupList<EntityArchetype>();
        //private LookupList<ComponentType> _componentTypes = new LookupList<ComponentType>();

        private ComponentList _componentList = new ComponentList();
        private ComponentViewList _componentViewList;
        private EntityPool _entityPool = new EntityPool();

        internal IReadOnlyList<EntityArchetype> Archetypes => _archetypes;
        internal ComponentList ComponentList => _componentList;

        public EntityManager(ComponentList componentList)
        {
            _componentList = componentList;
            _componentViewList = new ComponentViewList(componentList);
            _archetypes.Add(new EntityArchetype(this, 0, 0));

            _entityPool.Take(); //we take the first entity because we use ID as INVALID;
        }

        public int Count => _archetypeIDLookup.AllObjects.Sum(x => x.Count);

        public int CreateEntity(EntityArchetype archetype)
        {
            AssertMainThread();
            Assert(archetype.IsValid);

            var entityId = _entityPool.Take();
            _entityPool[entityId] = archetype.CreateEntityInternal(entityId);

            return entityId;
        }

        public void CreateEntity(EntityArchetype archetype, int count, out NativeArray<int> entities)
        {
            AssertMainThread();

            Assert(archetype.IsValid);
            entities = new NativeArray<int>(Allocator.Temp, count);
            for (var i = 0; i < count; i++)
                entities[i] = _entityPool.Take();

            archetype.CreateEntity(_entityPool, entities);
        }

        public void CreateEntity(EntityArchetype archetype, int count)
        {
            CreateEntity(archetype, count, out var entities);
            entities.Dispose();
        }

        public void Update<T>(in T t)
            where T : unmanaged
        {
            var view = _componentViewList.GetView<T>(this);
            view.UpdateEntity(this, t);
        }

        public void Update<T>(in T t, int entity)
           where T : unmanaged
        {
            var view = _componentViewList.GetView<T>(this);
            view.UpdateEntity(this, t, entity);
        }

        public bool HasEntity(int id)
        {
            AssertMainThread();
            return _entityPool[id].IsValid;
            //return _entityArchetypeLookup.ContainsKey(id);
        }

        public void DestroyEntity(int entity)
        {
            AssertMainThread();
            Assert(_entityPool[entity].IsValid);

            GetEntityStorage(entity, out var archetype, out var chunk, out var index);

            //i'm not a fan of passing in the entity pool here
            //but there is house keeping that needs to happen
            //to the global entity array
            archetype.DestroyEntity(_entityPool, entity, chunk, index);
            _entityPool.Return(entity);
        }

        public void DestroyEntity(NativeSlice<int> entities)
        {
            AssertMainThread();
            for (var i = 0; i < entities.Length; i++)
                DestroyEntity(entities[i]);

        }

        public void MoveEntity(int entity, EntityArchetype newArchetype)
        {
            AssertMainThread();
            Assert(_entityPool[entity].IsValid);

            GetEntityStorage(entity, out var archetype, out var chunk, out var index);

            MoveEntity(entity, chunk, index, archetype, newArchetype);
        }

        internal void MoveEntity(int entity, ArchetypeChunk chunkSrc, int indexSrc, EntityArchetype archetype, EntityArchetype newArchetype)
        {
            AssertMainThread();

            //_entityArchetypeLookup[entity.EntityID] = newArchetype.ID;            
            EntityArchetype.MoveEntity(_entityPool, entity, chunkSrc, indexSrc, archetype, newArchetype);
        }

        public bool HasComponentData<T>(int entity)
        {
            AssertMainThread();

            GetEntityStorage(entity, out var archetype, out var chunk, out var index);
            return archetype.HasComponentType(typeof(T));
        }

        internal unsafe void SetComponentData(int entity, int componentTypeID, void* componentData)
        {
            AssertMainThread();

            GetEntityStorage(entity, out var archetype, out var chunk, out var entityIndex);

            var foundComponent = _componentList.Find(componentTypeID, out var componentType);
            Assert(foundComponent);

            if (!archetype.HasComponentType(componentType))
            {
                Span<ComponentType> types = stackalloc ComponentType[archetype.ComponentTypes.Length + 1];
                for (var i = 0; i < archetype.ComponentTypes.Length; i++)
                    types[i] = archetype.ComponentTypes[i];

                types[types.Length - 1] = componentType;

                var newArchetype = CreateArchetype(types);

                MoveEntity(entity, chunk, entityIndex, archetype, newArchetype);
                GetEntityStorage(entity, out archetype, out chunk, out entityIndex);
            }

            chunk.SetComponentData(entityIndex, componentType, componentData);
        }

        public unsafe void SetComponentData<T>(int entity, in T component)
            where T : unmanaged
        {
            var copy = component;
            SetComponentData(entity, typeof(T).GetHashCode(), &copy);
        }

        public T GetComponentData<T>(int entity)
            where T : unmanaged
        {
            AssertMainThread();

            GetEntityStorage(entity, out var archetype, out var chunk, out var index);
            Assert(archetype.HasComponentType(typeof(T)));

            return chunk.GetComponentData<T>(index);
        }

        public void RemoveComponentData(int entity, int componentTypeID)
        {
            AssertMainThread();

            GetEntityStorage(entity, out var archetype, out var chunk, out var index);
            var foundComponentType = _componentList.Find(componentTypeID, out var componentType);
            Assert(foundComponentType);
            Assert(archetype.HasComponentType(componentType));

            var t = 0;
            Span<ComponentType> types = stackalloc ComponentType[archetype.ComponentTypes.Length - 1];
            for (var i = 0; i < archetype.ComponentTypes.Length; i++)
                if (archetype.ComponentTypes[i].ID != componentTypeID)
                    types[t++] = archetype.ComponentTypes[i];

            var newArchetype = CreateArchetype(types);

            MoveEntity(entity, chunk, index, archetype, newArchetype);
        }

        public void RemoveComponentData<T>(int entity)
            where T : unmanaged
        {
            RemoveComponentData(entity, typeof(T).GetHashCode());
        }

        //TODO: Should this just be an extension?
        public EntityView<T> CreateView<T>()
            where T : unmanaged
        {
            return new EntityView<T>(this);
        }

        public EntityArchetype CreateArchetype(Span<ComponentType> types)
        {
            AssertMainThread();
            Assert(types.Length > 0);

            var id = EntityArchetype.GetId(types);

            if (!_archetypeIDLookup.TryGetValue(id, out var archetype))
            {
                var set = new HashSet<int>();
                var componentTypes = new ComponentTypeInfo[types.Length];
                for (var i = 0; i < types.Length; i++)
                {
                    Assert(set.Add(types[i].GetHashCode()));
                    componentTypes[i] = _componentList.GetTypeInfo(types[i]);
                    Assert(componentTypes[i] != null);
                }

                archetype = new EntityArchetype(this, _archetypes.Count, id, componentTypes);
                _archetypes.Add(archetype);
                _archetypeIDLookup.Add(archetype.ID, archetype);
                _componentViewList.EntityArchetypeCreated(archetype);
            }

            return archetype;
        }

        public EntityArchetype CreateArchetype(params Type[] types)
        {
            var set = new HashSet<Type>();
            Span<ComponentType> componentTypes = stackalloc ComponentType[types.Length];
            for (var i = 0; i < types.Length; i++)
            {
                Assert(set.Add(types[i]));
                if (!_componentList.Find(types[i], out var componentType))
                {
                    componentType = _componentList.AddComponent(types[i]);
                    //Assert(false);
                }
                componentTypes[i] = componentType;
            }

            return CreateArchetype(componentTypes);
        }

        internal void GetEntityStorage(int entity, out EntityArchetype archetype, out ArchetypeChunk chunk, out int index)
        {
            ref var e = ref _entityPool[entity];
            Assert(e.IsValid);

            archetype = _archetypes[e.ArchetypeIndex];
            chunk = archetype.Chunks[e.ChunkIndex];
            index = e.Index;
        }

        public EntityArchetype GetEntityArchetype(int entity)
        {
            AssertMainThread();
            return _archetypes[_entityPool[entity].ArchetypeIndex];
        }

        internal ComponentView GetView<T>()
            where T : unmanaged
        {
            return _componentViewList.GetView<T>(this);
        }

        public bool First<T>(out T entity) where T : unmanaged => First(out entity, false);

        public bool FirstOrDefault<T>(out T entity) where T : unmanaged => First(out entity, true);

        internal unsafe bool First<T>(out T entity, bool throwOnNone)
            where T : unmanaged
        {
            foreach (var it in Project<T>())
            {
                entity = it;
                return true;
            }

            if (throwOnNone)
                throw new Exception($"Could not find any {typeof(T).Name}");
            entity = default;
            return false;
        }

        //public delegate void QuerySetup<T>(ref T t) where T : unmanaged;

        public IEnumerable<T> Project<T>()
            where T : unmanaged
        {
            var view = _componentViewList.GetView<T>(this);
            foreach (var it in view.Project<T>())
            {
                //we need to copy the data
                var t = it;
                yield return t;
            }
        }

        public void ProjectTo<T>(in NativeSlice<T> slice)
            where T : unmanaged
        {
            var view = _componentViewList.GetView<T>(this);
            view.ProjectTo(slice);
        }

        public NativeList<T> ProjectToList<T>(Allocator allocator = Allocator.Temp)
            where T : unmanaged
        {
            var view = _componentViewList.GetView<T>(this);
            //NativeList takes ownership of the array, so we don't need to dispose
            var array = new NativeArray<T>(allocator, view.EntityCount);
            view.ProjectTo<T>(array);

            return new NativeList<T>(ref array);
        }

        public IEnumerable<EntityArchetype> FilterArchetypes<T>(bool activeOnly = true)
           where T : unmanaged
        {
            var view = _componentViewList.GetView<T>(this);
            foreach (var archetype in view.Archetypes)
                if (!activeOnly || archetype.Count > 0)
                    yield return archetype;
        }

        public IEnumerable<ArchetypeChunk> FilterChunks<T>(bool activeOnly = true)
            where T : unmanaged
        {
            var view = _componentViewList.GetView<T>(this);
            foreach (var archetype in view.Archetypes)
                foreach (var chunk in archetype.Chunks)
                    if (!activeOnly || chunk.Count > 0)
                        yield return chunk;
        }
    }
}
