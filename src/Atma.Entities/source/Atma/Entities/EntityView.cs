namespace Atma.Entities
{
    using Atma.Common;
    using Atma.DI;
    using Atma.Jobs;
    using Atma.Memory;
    using System;
    using System.Collections.Generic;

    public abstract class EntityView
    {
        private readonly ComponentView _view;
        internal ComponentView View => _view;

        protected readonly EntityManager _entityManager;

        internal EntityView(EntityManager entityManager, ComponentView view)
        {
            _entityManager = entityManager;
            _view = view;
        }
    }

    [Injectable]
    public sealed class EntityView<T> : EntityView
        where T : unmanaged
    {
        public delegate void OnSetup(ref T t);

        private OnSetup _setup;

        public EntityView(EntityManager entityManager)
            : base(entityManager, entityManager.GetView<T>())
        {
        }

        public EntityView<T> Setup(OnSetup setup)
        {
            _setup = setup;
            return this;
        }

        public int EntityCount => View.EntityCount;

        public T First()
        {
            _entityManager.First<T>(out var t, true);
            return t;
        }

        public T FirstOrDefault()
        {
            _entityManager.FirstOrDefault<T>(out var t);
            return t;
        }

        public NativeArray<T> ToArray(Allocator allocator = Allocator.Temp)
        {
            var array = new NativeArray<T>(allocator, View.ProjectCount);
            if (array.Length > 0)
            {
                View.ProjectTo<T>(array);
            }
            return array;
        }

        public IEnumerable<T> ForEach()
        {
            foreach (var it in _entityManager.Project<T>())
            {
                //TODO: Project is also doing the same thing, is this going to cause 2 copies?
                var t = it;
                _setup?.Invoke(ref t);
                yield return t;
            }
        }

        private class ParallelJob : Job
        {
            public Action<T> job;
            public T data;
            public EntityView<T> view;

            protected override void Execute()
            {
                view._setup?.Invoke(ref data);
                job(data);
            }
        }

        private ObjectPoolRef<ParallelJob> _parallelPool;
        private List<ParallelJob> _runners;

        public void ForEachParallel(JobManager jobManager, Action<T> job)
        {
            if (_parallelPool == null)
            {
                _parallelPool = new ObjectPoolRef<ParallelJob>();
                _runners = new List<ParallelJob>();
            }

            foreach (var it in _entityManager.Project<T>())
            {
                var j = _parallelPool.Take();
                j.job = job;
                j.data = it;
                j.view = this;
                _runners.Add(j);
            }

            jobManager.Schedule(_runners);
            jobManager.Complete();

            foreach (var it in _runners)
                _parallelPool.Return(it);

            _runners.Clear();
        }
    }
}
