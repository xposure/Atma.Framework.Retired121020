namespace Atma.Common
{
    using static Atma.Debug;

    using System.Collections.Generic;

    public class LookupList<Key, Value>
    {
        private List<Key> _indexLookup;
        private List<Value> _data;

        public int Count => _indexLookup.Count;

        public LookupList(int initialSize = 8)
        {
            _indexLookup = new List<Key>(initialSize);
            _data = new List<Value>(initialSize);
        }

        public IEnumerable<Key> AllKeys => _indexLookup;
        public IEnumerable<Value> AllObjects => _data;

        public Value this[Key id]
        {
            get
            {
                var index = IndexOf(id);
                Assert(index >= 0);

                return _data[index];
            }
            set
            {
                var index = IndexOf(id);
                Assert(index >= 0);

                _data[index] = value;
            }
        }

        public void Add(Key id, Value t)
        {
            _indexLookup.Add(id);
            _data.Add(t);
        }

        public int IndexOf(Key id)
        {
            for (var i = 0; i < _indexLookup.Count; i++)
                if (_indexLookup[i].Equals(id))
                    return i;

            return -1;
        }

        public bool TryGetValue(Key id, out Value t)
        {
            var index = IndexOf(id);
            if (index == -1)
            {
                t = default;
                return false;
            }

            t = _data[index];
            return true;
        }

        public void Remove(Key id)
        {
            var index = IndexOf(id);
            Assert(index >= 0);

            _indexLookup.RemoveAt(index);
            _data.RemoveAt(index);
        }

        public void RemoveFast(Key id)
        {
            var index = IndexOf(id);
            Assert(index >= 0);

            _indexLookup.RemoveFast(index);
            _data.RemoveFast(index);
        }


    }
    public class LookupList<T>
    {
        private List<int> _indexLookup;
        private List<T> _data;

        public int Count => _indexLookup.Count;

        public LookupList(int initialSize = 8)
        {
            _indexLookup = new List<int>(initialSize);
            _data = new List<T>(initialSize);
        }

        public IEnumerable<int> AllIDs => _indexLookup;
        public IEnumerable<T> AllObjects => _data;

        public T this[int id]
        {
            get
            {
                var index = IndexOf(id);
                Assert(index >= 0);

                return _data[index];
            }
            set
            {
                var index = IndexOf(id);
                Assert(index >= 0);

                _data[index] = value;
            }
        }

        public T Add(int id, T t)
        {
            _indexLookup.Add(id);
            _data.Add(t);
            return t;
        }

        public int IndexOf(int id)
        {
            for (var i = 0; i < _indexLookup.Count; i++)
                if (_indexLookup[i] == id)
                    return i;

            return -1;
        }

        public bool TryGetValue(int id, out T t)
        {
            var index = IndexOf(id);
            if (index == -1)
            {
                t = default;
                return false;
            }

            t = _data[index];
            return true;
        }

        public void Remove(int id)
        {
            var index = IndexOf(id);
            Assert(index >= 0);

            _indexLookup.RemoveAt(index);
            _data.RemoveAt(index);
        }

        public void RemoveFast(int id)
        {
            var index = IndexOf(id);
            Assert(index >= 0);

            _indexLookup.RemoveFast(index);
            _data.RemoveFast(index);
        }

        public void Clear()
        {
            _indexLookup.Clear();
            _data.Clear();
        }
    }
}
