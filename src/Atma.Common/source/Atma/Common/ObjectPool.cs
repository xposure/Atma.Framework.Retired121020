namespace Atma.Common
{
    using System;
    using System.Collections.Generic;

    public abstract class AbstractObjectPool<T>
    {
        //private List<T> _pool;
        private readonly List<T> _free;

        //public int Capacity => _pool.Count + _free.Count;

        //public int Count => _pool.Count;

        //public IEnumerable<T> Active => _pool;

        //public IEnumerable<T> Free => _free;

        public AbstractObjectPool(int initialSize = 8)
        {
            _free = new List<T>(initialSize);

        }

        protected void Initialize()
        {
            for (var i = 0; i < _free.Capacity; i++)
                _free.Add(Next());

            //we want them in the opposite order in case its a sequence id
            //one time hit should be ok against perf
            _free.Reverse();
        }

        protected abstract T Next();

        public void Return(T t)
        {
            _free.Add(t);
        }

        public T Take()
        {
            if (_free.Count == 0)
                return Next();

            var index = _free.Count - 1;
            var obj = _free[index];
            _free.RemoveAt(index);
            return obj;
        }

        public void Take(T[] items)
        {
            //if(_free.Count < items.Length)
            //{
            //    var freeSpace = _free.Capacity - _free.Count;
            //    if(freeSpace < items.Length)
            //    {
            //        var newCapacity = _free.Capacity;
            //        freeSpace = newCapacity - _free.Count;
            //        while(freeSpace < items.Length)
            //            newCapacity = (newCapacity * 3) / 2;

            //        _free.Capacity = newCapacity;
            //    }
            //}


            //todo: needs optimated
            for (var i = 0; i < items.Length; i++)
                items[i] = Take();
        }
    }

    public sealed class ObjectPoolInt : AbstractObjectPool<int>
    {
        private int _sequenceId = 0;

        public ObjectPoolInt(int initialSize = 8, int startId = 0) : base(initialSize)
        {
            _sequenceId = startId;
            Initialize();
        }

        protected override int Next()
        {
            return _sequenceId++;
        }
    }

    public sealed class ObjectPoolRef<T> : AbstractObjectPool<T>
        where T : new()
    {
        protected override T Next()
        {
            return new T();
        }
    }

    public sealed class ObjectPool<T> : AbstractObjectPool<T>
    {
        private Func<T> _create;
        public ObjectPool(Func<T> create)
        {
            _create = create;
        }

        protected override T Next() => _create();
    }


}
