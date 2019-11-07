namespace Atma.Jobs
{
    using Atma.Common;
    using System;

    public interface IJobPool
    {
        Type Type { get; }

        Job Take();

        void Return(Job job);
    }

    public class JobPool<T> : IJobPool
        where T : Job, new()
    {
        private ObjectPoolRef<T> _pool = new ObjectPoolRef<T>();

        public Type Type { get; }

        public JobPool()
        {
            Type = typeof(T);
        }

        public Job Take()
        {
            return _pool.Take();
        }

        public void Return(Job job)
        {
            _pool.Return((T)job);
        }

    }
}
