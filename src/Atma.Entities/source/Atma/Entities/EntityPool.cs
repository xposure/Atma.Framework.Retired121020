namespace Atma.Common
{
    using Atma.Entities;
    using Atma.Memory;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;

    public sealed class EntityPool : UnmanagedDispose
    {
        public const int ENTITIES_BITS = 12; //4096
        public const int ENTITIES_PER_POOL = 1 << ENTITIES_BITS;
        public const int ENTITIES_MASK = ENTITIES_PER_POOL - 1;

        private ILogger _logger;
        private ILoggerFactory _logFactory;

        private IAllocator _memory;
        private List<NativeArray<Entity>> _entityMap;

        private NativeStack<uint> _freeIds;

        private int _free;
        private int _capacity;
        private uint _version;

        public int Capacity => _capacity;

        public int Count => _capacity - _free;

        public int Free => _free;

        public EntityPool(ILoggerFactory logFactory, IAllocator allocator)
        {
            _logFactory = logFactory;
            _logger = _logFactory.CreateLogger<EntityPool>();
            _memory = allocator;
            _freeIds = new NativeStack<uint>(_memory, ENTITIES_PER_POOL);
            _entityMap = new List<NativeArray<Entity>>();
            AddPage();
        }

        public ref Entity this[uint entity]
        {
            get
            {
                var version = entity >> 24;
                var id = (int)(entity & 0xffffff);

                var index = id & ENTITIES_MASK;
                var page = id >> ENTITIES_BITS;

                Assert.Range(page, 0, _entityMap.Count);
                Assert.Range(index, 0, _entityMap[page].Length);

                return ref _entityMap[page][index];
            }
        }

        public bool IsValid(uint id)
        {
            ref var e = ref this[id];
            return e.ID == id;
        }

        private void AddPage()
        {
            var arr = new NativeArray<Entity>(_memory, ENTITIES_PER_POOL);

            _freeIds.EnsureCapacity(ENTITIES_PER_POOL);
            var newMax = (uint)_entityMap.Count * (uint)ENTITIES_PER_POOL + (uint)ENTITIES_PER_POOL;
            for (uint i = 0; i < ENTITIES_PER_POOL; i++)
                _freeIds.Push(newMax - i - 1);
            _free += ENTITIES_PER_POOL;
            _capacity += ENTITIES_PER_POOL;
            _entityMap.Add(arr);
        }

        public void Return(uint id)
        {
            ref var e = ref this[id];
            Assert.GreatherThan(e.ID, 0);
            Assert.EqualTo(e.ID, id);

            e = new Entity(0, 0, 0, 0);
            _freeIds.Push(id & 0xffffff);
            _free++;
        }

        public uint Take()
        {
            if (_freeIds.Length == 0)
                AddPage();

            _free--;

            var id = _freeIds.Pop();
            Assert.LessThanEqualTo(id, 0xffffff);
            var version = _version++;
            id |= version << 24;

            return id;
        }

        public void Take(NativeSlice<uint> array)
        {
            //TODO: later we should grab entire pages at a time
            for (var i = 0; i < array.Length; i++)
                array[i] = Take();
        }

        protected override void OnUnmanagedDispose()
        {
            _entityMap.DisposeAll();
            _entityMap.Clear();
            _freeIds.Dispose();
        }
    }
}
