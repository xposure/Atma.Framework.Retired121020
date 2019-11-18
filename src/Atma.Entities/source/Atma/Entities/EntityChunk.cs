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

        public void Create(NativeSlice<uint> entities)
        {
            Assert.GreatherThanEqualTo(Free, entities.Length);
            for (var i = 0; i < entities.Length; i++)
            {
                var index = _entityCount++;
                _entities[index] = entities[i];
            }
        }

        // internal unsafe NativeSlice<Entity> Copy(ComponentType* componentType, ref void* src, in NativeSlice<Entity> entities)
        // {
        //     for (var i = 0; i < entities.Length; i++)
        //     {

        //     }


        //     return entities;
        // }

        public unsafe MovedEntity Delete(int index)
        {
            var data = stackalloc int[] { index };
            var result = stackalloc MovedEntity[1];

            var slice = new NativeSlice<int>(data, 1);
            var moved = new NativeFixedList<MovedEntity>(data, 1);

            Delete(slice, ref moved);
            return moved[0];
        }


        internal void Delete(NativeSlice<int> indicies, ref NativeFixedList<MovedEntity> movedEntities)
        {
            for (var index = 0; index < indicies.Length; index++)
            {
                Assert.Range(index, 0, _entityCount);
                _entityCount--;

                if (index < _entityCount) //removing the last element, no need to patch
                {
                    var movedIndex = index;
                    _packedArray.Move(_entityCount, index);
                    _entities[index] = _entities[_entityCount];
                    movedEntities.Add(new MovedEntity(_entities[movedIndex], movedIndex));
                }

                _entities[_entityCount] = 0;
            }
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