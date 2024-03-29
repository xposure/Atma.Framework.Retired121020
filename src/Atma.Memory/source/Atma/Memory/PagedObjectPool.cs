namespace Atma.Memory
{
    using System.Collections.Generic;

    [System.Diagnostics.DebuggerStepThrough]
    public sealed class PagedObjectPool<T> : UnmanagedDispose
        where T : unmanaged
    {
        private IAllocator _memory;
        public const int OBJECT_BITS = 12; //4096
        public const int OBJECTS_PER_POOL = 1 << OBJECT_BITS;
        public const int OBJECTS_MASK = OBJECTS_PER_POOL - 1;

        private List<NativeArray<T>> _objectMap;

        private NativeStack<uint> _freeIds;// = new NativeStack<uint>(Allocator.Persistent, OBJECTS_PER_POOL);

        private int _free;
        private int _capacity;
        private uint _version;

        public int Capacity => _capacity;

        public int Count => _capacity - _free;

        public int Free => _free;

        public PagedObjectPool(IAllocator allocator)
        {
            _memory = allocator;
            _objectMap = new List<NativeArray<T>>();
            _freeIds = new NativeStack<uint>(_memory, OBJECTS_PER_POOL);
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

                Assert.Range(page, 0, _objectMap.Count);
                Assert.Range(index, 0, _objectMap[page].Length);

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

        protected override void OnUnmanagedDispose()
        {
            _objectMap.DisposeAll();
            _freeIds.Dispose();
        }
    }
}
