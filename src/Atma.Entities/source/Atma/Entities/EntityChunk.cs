namespace Atma.Entities
{
    using Atma.Memory;
    using Microsoft.Extensions.Logging;

    public sealed class EntityChunk : UnmanagedDispose
    {
        private ILogger _logger;
        private ILoggerFactory _logFactory;
        private IAllocator _allocator;
        private NativeArray<uint> _entities;
        private EntityPackedArray _packedArray;

        private int _entityCount = 0;

        public int Count => _entityCount;
        public int Free => Entity.ENTITY_MAX - _entityCount;

        public NativeSlice<uint> Entities => _entities.Slice();

        public EntitySpec Specification { get; }
        public EntityPackedArray PackedArray => _packedArray;

        public EntityChunk(ILoggerFactory logFactory, IAllocator allocator, EntitySpec specifcation)
        {
            _logFactory = logFactory;
            _logger = _logFactory.CreateLogger<EntityChunk>();
            _allocator = allocator;
            Specification = specifcation;
            _packedArray = new EntityPackedArray(logFactory, _allocator, specifcation);
            _entities = new NativeArray<uint>(_allocator, _packedArray.Length);
        }

        public uint GetEntity(int index)
        {
            Assert.Range(index, 0, _entityCount);
            return _entities[index];
        }

        public int Create(uint entity)
        {
            Assert.GreatherThan(Free, 0);

            var index = _entityCount++;
            _entities[index] = entity;
            return index;
        }

        // internal unsafe NativeSlice<Entity> Copy(ComponentType* componentType, ref void* src, in NativeSlice<Entity> entities)
        // {
        //     for (var i = 0; i < entities.Length; i++)
        //     {

        //     }


        //     return entities;
        // }

        public int Delete(int index)
        {
            Assert.Range(index, 0, _entityCount);
            _entityCount--;

            var movedIndex = -1;
            if (index < _entityCount) //removing the last element, no need to patch
            {
                movedIndex = index;
                _packedArray.Move(_entityCount, index);
                _entities[index] = _entities[_entityCount];
            }

            _entities[_entityCount] = 0;
            return movedIndex;
        }

        protected override void OnManagedDispose()
        {
            _entities.Dispose();
            _packedArray = null;
        }

        protected override void OnUnmanagedDispose()
        {
            _packedArray.Dispose();
        }
    }
}