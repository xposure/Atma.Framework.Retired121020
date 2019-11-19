namespace Atma.Entities
{
    using System;
    using Atma.Memory;
    using Microsoft.Extensions.Logging;

    public sealed class EntityChunk : UnmanagedDispose
    {
        private ILogger _logger;
        private ILoggerFactory _logFactory;
        private IAllocator _allocator;
        private NativeList<uint> _entities2;

        internal ReadOnlySpan<uint> Entities => _entities2.Slice();
        internal readonly ComponentPackedArray PackedArray;

        public int Count => _entities2.Length;
        public int Free => Entity.ENTITY_MAX - Count;

        public readonly EntitySpec Specification;


        internal EntityChunk(ILoggerFactory logFactory, IAllocator allocator, EntitySpec specifcation)
        {
            _logFactory = logFactory;
            _logger = _logFactory.CreateLogger<EntityChunk>();
            _allocator = allocator;
            Specification = specifcation;
            PackedArray = new ComponentPackedArray(logFactory, _allocator, specifcation);
            _entities2 = new NativeList<uint>(_allocator, PackedArray.Length);
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

                entity.Replace(new Entity(id, specIndex, chunkIndex, _entities2.Length));
                _entities2.Add(id);
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
                //_entityCount--;
                if (_entities2.RemoveAtWithSwap(index, true))
                {
                    PackedArray.Move(_entities2.Length, index);
                    //update the moved entity so that EntityRefs reflect the change
                    ref var movedEntity = ref entityPool[_entities2[index]];
                    movedEntity.Index = index;
                }
            }
        }

        public Span<T> GetComponentData<T>(int index = -1) where T : unmanaged => PackedArray.GetComponentData<T>(index);

        protected override void OnManagedDispose()
        {
            PackedArray.Dispose();
        }

        protected override void OnUnmanagedDispose()
        {
            _entities2.Dispose();
        }
    }
}