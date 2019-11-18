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

        public ReadOnlySpan<uint> Entities => _entities.Span;

        public EntitySpec Specification { get; }
        public ComponentPackedArray PackedArray => _packedArray;

        public EntityChunk(ILoggerFactory logFactory, IAllocator allocator, EntitySpec specifcation)
        {
            _logFactory = logFactory;
            _logger = _logFactory.CreateLogger<EntityChunk>();
            _allocator = allocator;
            Specification = specifcation;
            _packedArray = new ComponentPackedArray(logFactory, _allocator, specifcation);
            _entities = new NativeArray<uint>(_allocator, _packedArray.Length);
        }

        public uint Get(int index)
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

        public int Create(NativeSlice<uint> entities)
        {
            var amountToCreate = entities.Length > Free ? Free : entities.Length;
            for (var i = 0; i < amountToCreate; i++)
                _entities[_entityCount++] = entities[i];
            return amountToCreate;
        }

        public unsafe MovedEntity Delete(int index)
        {
            var data = stackalloc int[] { index };
            var result = stackalloc MovedEntity[1];

            var slice = new NativeSlice<int>(data, 1);
            var moved = new NativeFixedList<MovedEntity>(data, 1);

            Delete(slice, ref moved);
            return moved[0];
        }

        //This would be faster but exposes implementation details further up the chain :/
        // internal void Delete(NativeSlice<int> indicies, EntityPool entityPool)
        // {
        //     for (var index = 0; index < indicies.Length; index++)
        //     {
        //         Assert.Range(index, 0, _entityCount);
        //         _entityCount--;

        //         if (index < _entityCount) //removing the last element, no need to patch
        //         {
        //             _packedArray.Move(_entityCount, index);
        //             _entities[index] = _entities[_entityCount];
        //             ref var entity = ref entityPool[_entities[index]];
        //             entity.Index = index;
        //         }

        //         _entities[_entityCount] = 0;
        //     }
        // }

        internal void Delete(NativeSlice<int> indicies, ref NativeFixedList<MovedEntity> movedEntities)
        {
            for (var i = 0; i < indicies.Length; i++)
            {
                var index = indicies[i];
                Assert.Range(index, 0, _entityCount);
                _entityCount--;

                if (index < _entityCount) //removing the last element, no need to patch
                {
                    _packedArray.Move(_entityCount, index);
                    _entities[index] = _entities[_entityCount];
                    movedEntities.Add(new MovedEntity(_entities[_entityCount], index));
                }

                _entities[_entityCount] = 0;
            }
        }

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