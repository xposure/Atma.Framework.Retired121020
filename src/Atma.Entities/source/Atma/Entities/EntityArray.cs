namespace Atma.Common
{
    using Atma.Entities;
    using Atma.Memory;
    using static Atma.Debug;
    using System.Collections.Generic;


    public class EntityPool
    {
        public const int ENTITIES_PER_POOL = 4096;

        private List<NativeArray<Entity>> _entityMap;

        private NativeStack<int> _freeIds = new NativeStack<int>(Allocator.Persistent, ENTITIES_PER_POOL);

        private int _free;
        private int _capacity;

        public int Capacity => _capacity;

        public int Count => _capacity - _free;

        public int Free => _free;


        public EntityPool()
        {
            _entityMap = new List<NativeArray<Entity>>();
            AddPage();
        }

        public ref Entity this[int id]
        {
            get
            {
                var index = id % ENTITIES_PER_POOL;
                var page = id / ENTITIES_PER_POOL; // should we do >> 12 is the compiler ompitizing that
                Assert(page >= 0 && page < _entityMap.Count, $"{page} was out of range[{0}-{_entityMap.Count}]");
                Assert(index >= 0 && index < _entityMap[page].Length, $"{index} was out of range[{0}-{_entityMap[page].Length}]");
                return ref _entityMap[page][index];
            }
        }

        private void AddPage()
        {
            var arr = new NativeArray<Entity>(Allocator.Persistent, ENTITIES_PER_POOL);

            _freeIds.EnsureCapacity(ENTITIES_PER_POOL);
            var newMax = _entityMap.Count * ENTITIES_PER_POOL + ENTITIES_PER_POOL;
            for (int i = 0; i < ENTITIES_PER_POOL; i++)
                _freeIds.Push(newMax - i - 1);
            _free += ENTITIES_PER_POOL;
            _capacity += ENTITIES_PER_POOL;
            _entityMap.Add(arr);
        }

        public void Return(int id)
        {
            this[id] = new Entity(0, 0, 0, 0);
            _freeIds.Push(id);
            _free++;
        }

        public int Take()
        {
            if (_freeIds.Length == 0)
                AddPage();

            _free--;

            return _freeIds.Pop();
        }

        public void Take(NativeSlice<int> items)
        {
            while (_free < items.Length)
                AddPage();

            for (var i = 0; i < items.Length; i++)
            {
                _free--;
                items[i] = _freeIds.Pop();
            }
        }
    }
}
