namespace Atma.Common
{
    using Atma.Entities;
    using Atma.Memory;
    using static Atma.Debug;
    using System.Collections.Generic;

    public class EntityPool2 : IEntityPool2
    {
        public const int ENTITIES_BITS = 12; //4096
        public const int ENTITIES_PER_POOL = 1 << ENTITIES_BITS;
        public const int ENTITIES_MASK = ENTITIES_PER_POOL - 1;

        private List<NativeArray<Entity2>> _entityMap;

        private NativeStack<uint> _freeIds = new NativeStack<uint>(Allocator.Persistent, ENTITIES_PER_POOL);

        private int _free;
        private int _capacity;
        private uint _version;

        public int Capacity => _capacity;

        public int Count => _capacity - _free;

        public int Free => _free;

        public EntityPool2()
        {
            _entityMap = new List<NativeArray<Entity2>>();
            AddPage();
        }

        public ref Entity2 this[uint entity]
        {
            get
            {
                var version = entity >> 24;
                var id = (int)(entity & 0xffffff);

                var index = id & ENTITIES_MASK;
                var page = id >> ENTITIES_BITS;

                Assert(page >= 0 && page < _entityMap.Count, $"{page} was out of range[{0}-{_entityMap.Count}]");
                Assert(index >= 0 && index < _entityMap[page].Length, $"{index} was out of range[{0}-{_entityMap[page].Length}]");

                return ref _entityMap[page][index];
            }
        }

        //public ref Entity2 Get(uint entity) => ref this[entity];

        public bool IsValid(uint id)
        {
            ref var e = ref this[id];
            return e.ID == id;
        }

        private void AddPage()
        {
            var arr = new NativeArray<Entity2>(Allocator.Persistent, ENTITIES_PER_POOL);

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
            Assert(e.ID > 0);
            Assert(e.ID == id);

            e = new Entity2(0, 0, 0, 0);
            _freeIds.Push(id);
            _free++;
        }

        public uint Take()
        {
            if (_freeIds.Length == 0)
                AddPage();

            _free--;

            var id = _freeIds.Pop();
            Assert(id <= 0xffffff);
            var version = _version++;
            id |= version << 24;

            //this[id] = new Entity2(id, specIndex, chunkIndex, index);

            return id;
        }

        // public void Take(NativeSlice<int> items)
        // {
        //     while (_free < items.Length)
        //         AddPage();

        //     for (var i = 0; i < items.Length; i++)
        //     {
        //         _free--;
        //         items[i] = _freeIds.Pop();
        //     }
        // }
    }
}
