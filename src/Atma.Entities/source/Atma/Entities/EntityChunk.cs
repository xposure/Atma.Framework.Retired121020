namespace Atma.Entities
{
    using System;
    using Atma.Memory;
    using Microsoft.Extensions.Logging;

    [System.Diagnostics.DebuggerStepThrough]
    public readonly struct MovedEntity
    {
        public readonly uint ID;
        public readonly int Index;

        public bool DidMove => ID > 0;

        public MovedEntity(uint id, int index)
        {
            ID = id;
            Index = index;
        }

    }

    public sealed class EntityChunk : UnmanagedDispose
    {
        private ILogger _logger;
        private ILoggerFactory _logFactory;
        private IAllocator _allocator;
        private NativeArray<uint> _entities;
        private ComponentPackedArray _packedArray;

        private int _entityCount = 0;

        public int Count => _entityCount;
        public int Free => Entity.ENTITY_MAX - _entityCount;

        internal ReadOnlySpan<uint> Entities => _entities.Slice();

        public EntitySpec Specification { get; }
        internal ComponentPackedArray PackedArray => _packedArray;

        internal EntityChunk(ILoggerFactory logFactory, IAllocator allocator, EntitySpec specifcation)
        {
            _logFactory = logFactory;
            _logger = _logFactory.CreateLogger<EntityChunk>();
            _allocator = allocator;
            Specification = specifcation;
            _packedArray = new ComponentPackedArray(logFactory, _allocator, specifcation);
            _entities = new NativeArray<uint>(_allocator, _packedArray.Length);
        }

        internal void Create(int specIndex, int chunkIndex, EntityRef entity)
        {
            Span<EntityRef> entities = stackalloc[] { entity };
            Create(specIndex, chunkIndex, entities);
        }

        internal int Create(int specIndex, int chunkIndex, Span<EntityRef> entities)
        {
            var amountToCreate = entities.Length > Free ? Free : entities.Length;
            for (var i = 0; i < amountToCreate; i++)
            {
                ref var entity = ref entities[i];
                var id = entity.ID;
                _entities[_entityCount] = id;
                entity.Replace(new Entity(id, specIndex, chunkIndex, _entityCount++));
            }
            return amountToCreate;
        }

        internal unsafe void Delete(EntityRef entity, EntityPool entityPool)
        {
            Span<EntityRef> slice = stackalloc[] { entity };
            Delete(slice, entityPool);
        }

        internal void Delete(Span<EntityRef> entities, EntityPool entityPool)
        {
            //originally I didn't want the entity pool in here because it crosses the boundaries
            //but I realize its required due to bulk deleting shifting entities in the same chunk around
            //and we need real time entity location updates for EntityRef reflection
            for (var i = 0; i < entities.Length; i++)
            {
                ref var entity = ref entities[i];
                var index = entity.Index;
                //Assert.Range(index, 0, _entityCount);
                _entityCount--;

                if (index < _entityCount) //removing the last element, no need to patch
                {
                    _packedArray.Move(_entityCount, index);
                    _entities[index] = _entities[_entityCount];

                    //update the moved entity so that EntityRefs reflect the change
                    ref var movedEntity = ref entityPool[_entities[_entityCount]];
                    movedEntity.Index = index;
                }

                _entities[_entityCount] = 0;
            }
        }

        public Span<T> GetComponentData<T>() where T : unmanaged => _packedArray.GetComponentData<T>();

        protected override void OnManagedDispose()
        {
            _packedArray.Dispose();
            _packedArray = null;
        }

        protected override void OnUnmanagedDispose()
        {
            _entities.Dispose();
        }
    }
}