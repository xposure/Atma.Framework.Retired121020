namespace Atma.Memory
{
    using System.Collections.Generic;

    public sealed class PagedObjectPool<T>
        where T : unmanaged
    {
        private IAllocator _memory;
        public const int OBJECT_BITS = 12; //4096
        public const int OBJECTS_PER_POOL = 1 << OBJECT_BITS;
        public const int OBJECTS_MASK = OBJECTS_PER_POOL - 1;

        private List<NativeArray<T>> _objectMap;

        private NativeStack<uint> _freeIds = new NativeStack<uint>(Allocator.Persistent, OBJECTS_PER_POOL);

        private int _free;
        private int _capacity;
        private uint _version;

        public int Capacity => _capacity;

        public int Count => _capacity - _free;

        public int Free => _free;

        public PagedObjectPool()
        {
            _memory = new DynamicAllocator();
            _objectMap = new List<NativeArray<T>>();
            AddPage();
        }

        public ref T this[uint objectId]
        {
            get
            {
                var version = objectId >> 24;
                var id = (int)(objectId & 0xffffff);

                var index = id & OBJECTS_MASK;
                var page = id >> OBJECT_BITS;

                Assert.GreatherThanEqualTo(page, 0);
                Assert.LessThan(page, _objectMap.Count);
                Assert.GreatherThanEqualTo(index, 0);
                Assert.LessThan(index, _objectMap[page].Length);
                //Assert(page >= 0 && page < _objectMap.Count, $"{page} was out of range[{0}-{_objectMap.Count}]");
                //Assert(index >= 0 && index < _objectMap[page].Length, $"{index} was out of range[{0}-{_objectMap[page].Length}]");

                return ref _objectMap[page][index];
            }
        }

        private void AddPage()
        {
            var arr = new NativeArray<T>(_memory, OBJECTS_PER_POOL);

            _freeIds.EnsureCapacity(OBJECTS_PER_POOL);
            var newMax = (uint)_objectMap.Count * (uint)OBJECTS_PER_POOL + (uint)OBJECTS_PER_POOL;
            for (uint i = 0; i < OBJECTS_PER_POOL; i++)
                _freeIds.Push(newMax - i - 1);
            _free += OBJECTS_PER_POOL;
            _capacity += OBJECTS_PER_POOL;
            _objectMap.Add(arr);
        }

        public void Return(uint id)
        {
            ref var e = ref this[id];
            e = default;
            _freeIds.Push(id);
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
    }
}
