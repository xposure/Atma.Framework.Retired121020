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
        private NativeList<EntityRef> _entities2;

        internal ReadOnlySpan<EntityRef> Entities => _entities2.Slice();
        internal readonly ComponentPackedArray PackedArray;

        public int Count => _entities2.Length;
        public int Free => Entity.ENTITY_MAX - Count;

        public readonly EntitySpec Specification;

        public readonly int SpecIndex;
        public readonly int ChunkIndex;


        internal EntityChunk(ILoggerFactory logFactory, IAllocator allocator, EntitySpec specifcation, int specIndex, int chunkIndex)
        {
            _logFactory = logFactory;
            _logger = _logFactory.CreateLogger<EntityChunk>();
            _allocator = allocator;
            Specification = specifcation;
            PackedArray = new ComponentPackedArray(logFactory, _allocator, specifcation);
            _entities2 = new NativeList<EntityRef>(_allocator, PackedArray.Length);
            SpecIndex = specIndex;
            ChunkIndex = chunkIndex;
        }

        internal void Create(in EntityRef entity)
        {
            Span<EntityRef> entities = stackalloc[] { entity };
            Create(entities);
        }

        internal int Create(Span<EntityRef> entities)
        {
            var amountToCreate = entities.Length > Free ? Free : entities.Length;
            for (var i = 0; i < amountToCreate; i++)
            {
                ref var entity = ref entities[i];
                var id = entity.ID;

                entity.Replace(new Entity(id, SpecIndex, ChunkIndex, _entities2.Length));
                _entities2.Add(entity);
            }
            return amountToCreate;
        }

        internal unsafe void Delete(in EntityRef entity)
        {
            Span<EntityRef> slice = stackalloc[] { entity };
            Delete(slice);
        }

        internal void Delete(Span<EntityRef> entities)
        {
            for (var i = 0; i < entities.Length; i++)
            {
                ref var entity = ref entities[i];
                var index = entity.Index;
                if (_entities2.RemoveAtWithSwap(index, true))
                {
                    PackedArray.Move(_entities2.Length, index);
                    _entities2[index].Index = index;
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