namespace Atma.Common
{
    using static Atma.Debug;

    using System.Collections.Generic;
    using System.Threading;

    public class ConcurrentLookupList<T>
    {
        private ReaderWriterLockSlim _lock;
        private List<int> _indexLookup;
        private List<T> _data;

        public ConcurrentLookupList(int initialSize = 8)
        {
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _indexLookup = new List<int>(initialSize);
            _data = new List<T>(initialSize);
        }

        public IEnumerable<int> AllIDs => _indexLookup;
        public IEnumerable<T> AllObjects => _data;

        public T this[int id]
        {
            get
            {
                try
                {
                    _lock.EnterReadLock();
                    var index = indexOf(id);
                    Assert(index >= 0);

                    return _data[index];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    _lock.EnterWriteLock();
                    var index = indexOf(id);
                    Assert(index >= 0);

                    _data[index] = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public void Add(int id, T t)
        {
            try
            {
                _lock.EnterWriteLock();
                _indexLookup.Add(id);
                _data.Add(t);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void TryAdd(int id, T t)
        {
            try
            {
                _lock.EnterWriteLock();
                var index = indexOf(id);
                if (index == -1)
                {
                    _indexLookup.Add(id);
                    _data.Add(t);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public int IndexOf(int id)
        {
            try
            {
                _lock.EnterReadLock();
                return indexOf(id);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private int indexOf(int id)
        {
            for (var i = 0; i < _indexLookup.Count; i++)
                if (_indexLookup[i] == id)
                    return i;

            return -1;
        }

        public bool TryGetValue(int id, out T t)
        {
            try
            {
                _lock.EnterReadLock();

                var index = indexOf(id);
                if (index == -1)
                {
                    t = default;
                    return false;
                }

                t = _data[index];
                return true;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Remove(int id)
        {
            try
            {
                _lock.EnterWriteLock();
                var index = indexOf(id);
                Assert(index >= 0);

                _indexLookup.RemoveAt(index);
                _data.RemoveAt(index);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

        }

        public void RemoveFast(int id)
        {
            try
            {
                _lock.EnterWriteLock();
                var index = indexOf(id);
                Assert(index >= 0);

                _indexLookup.RemoveFast(index);
                _data.RemoveFast(index);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
