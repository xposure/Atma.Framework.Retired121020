namespace Atma.Entities
{
    using Atma.Common;
    using Atma.Memory;

    using static Atma.Debug;

    public unsafe sealed class ArchetypeChunk
    {

        private readonly MemoryBlock _data;


        private readonly Entity* _entities;
        private readonly ComponentDataArray<Entity> _entityArray;
        private readonly ComponentDataArray[] _componentData;
        private long _version;
        private readonly int _chunkIndex;

        public int Index => _chunkIndex;

        public EntityArchetype Archetype { get; }

        public int Capacity { get; }

        public int Count { get; private set; }

        public int Free => Capacity - Count;

        public long Version => _version;

        public bool IsFull => Capacity == Count;

        internal ArchetypeChunk(EntityArchetype archetype, int chunkIndex, int size)
        {
            Archetype = archetype;
            Capacity = size;
            _chunkIndex = chunkIndex;
            //entity id index array
            var entitySize = archetype.EntitySize;
            entitySize += SizeOf<Entity>.Size;

            var totalSize = entitySize * size;

            _data = new MemoryBlock(Allocator.Persistent, totalSize);
            _entities = _data.Take<Entity>(size);

            _componentData = new ComponentDataArray[archetype.ComponentTypes.Length];
            for (var i = 0; i < archetype.ComponentTypes.Length; i++)
            {
                var componentDataArray = (ComponentDataArray)archetype.ComponentTypeInfo[i].CreateArray();// ComponentDataArray.Create(archetype.ComponentTypes[i]);
                componentDataArray.Initialize(_data, size);

                _componentData[i] = componentDataArray;
            }

            _entityArray = new ComponentDataArray<Entity>(_entities, size);
            var entitySpan = _entityArray.Span;
            for (var i = 0; i < entitySpan.Length; i++)
                entitySpan[i] = new Entity(0, archetype.Index, _chunkIndex, i);
        }

        internal void Deconstruct<T, K>(out ReadComponentDataSpanRef<T> t, out ReadComponentDataSpanRef<K> k)
            where T : unmanaged
            where K : unmanaged
        {
            t = GetReadComponent<T>();
            k = GetReadComponent<K>();
        }

        internal int CreateEntityInternal(int id)
        {
            //Assert(READERS == 0 && WRITERS == 0);
            var index = Count;
            Count++;
            //TODO: should creating an entity change the version?
            //it wouldn't invalidate any entities (but delete would)
            //_version++;

            ref var entity = ref _entities[index];
            entity.ID = id;

            return index;
        }

        internal void CreateEntity(EntityPool entityPool, in NativeSlice<int> entities, ref int index)
        {
            //Assert(READERS == 0 && WRITERS == 0);
            //var span = _entities.Span;
            var archetypeIndex = Archetype.Index;
            while (index < entities.Length && Count < _entityArray.Length)
            {
                ref var entity = ref _entities[Count];
                entity.ID = entities[index];
                entityPool[entity.ID] = new Entity(entity.ID, archetypeIndex, this.Index, Count++);
                index++;
            }

            //_version++;
        }

        internal void DeleteIndex(EntityPool pool, int index, bool clearToZero)
        {
            Assert(Count > 0);
            ref var newPosition = ref _entities[index];
            ref var oldPosition = ref _entities[Count - 1];
            ref var meta = ref pool[oldPosition.ID];
            meta.Index = index;
            newPosition.ID = oldPosition.ID;
            oldPosition.ID = 0;
            //we need to move all component data too
            for (var i = 0; i < _componentData.Length; i++)
                _componentData[i].DeleteIndex(index, Count, clearToZero);
            Count--;
            //_version++;
        }

        internal unsafe void SetComponentData(int entityIndex, in ComponentType componentType, void* componentData)
        {
            //Assert(typeof(T) != typeof(Entity));
            var componentIndex = Archetype.GetComponentIndex(componentType);
            Assert(componentIndex > -1);
            //_version++;
            using var writable = GetWriteComponentArray(componentIndex);
            writable.Array.SetComponentData(entityIndex, componentData);
        }


        internal void SetComponentData<T>(int entityIndex, in T componentData)
            where T : unmanaged
        {
            using var array = GetWriteComponent<T>();
            array[entityIndex] = componentData;
        }

        internal T GetComponentData<T>(int entityIndex)
            where T : unmanaged
        {
            using var array = GetReadComponent<T>();
            return array[entityIndex];
        }

        internal unsafe void* GetComponentAddress(int componentIndex) => _componentData[componentIndex]._rawData;

        internal ReadComponentDataArrayRef GetReadComponentArray(int componentIndex)
        {
            return new ReadComponentDataArrayRef(_componentData[componentIndex]);
        }

        internal WriteComponentDataArrayRef GetWriteComponentArray(int componentIndex)
        {
            return new WriteComponentDataArrayRef(_componentData[componentIndex]);
        }

        public ReadComponentDataSpanRef<T> GetReadComponent<T>()
            where T : unmanaged
        {
            if (typeof(T) == typeof(Entity))
                return new ReadComponentDataSpanRef<T>(_entityArray, Count);

            var index = Archetype.GetComponentIndex<T>();
            Assert(index > -1);

            return new ReadComponentDataSpanRef<T>(_componentData[index], Count);
        }

        public WriteComponentDataSpanRef<T> GetWriteComponent<T>()
            where T : unmanaged
        {
            Assert(typeof(T) != typeof(Entity));
            var index = Archetype.GetComponentIndex<T>();
            Assert(index > -1);
            //_version++;
            return new WriteComponentDataSpanRef<T>(_componentData[index], Count);
        }
    }
}
