namespace Atma.Jobs
{
    using Atma.Common;
    using Atma.Profiling;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public class JobManager : IService, IDisposable
    {
        private IProfileService _profiler;
        private JobWorkerPool _workerPool;
        private ConcurrentLookupList<JobHandle> _jobHandles = new ConcurrentLookupList<JobHandle>();
        private ObjectPoolRef<JobWaiter> _jobWaiterPool = new ObjectPoolRef<JobWaiter>();
        private LookupList<IJobPool> _jobPools = new LookupList<IJobPool>();

        private volatile int _sequenceID = 0;
        private volatile int _version = 0;

        static JobManager()
        {
            Thread.CurrentThread.Name = "Main Thread";
        }

        public JobManager(IProfileService profiler = null) : this(-1, profiler)
        {
        }

        private JobManager(int workers, IProfileService profiler = null)
        {
            _profiler = profiler ?? NullProfileService.Null;
            _workerPool = new JobWorkerPool(workers, _profiler);
        }

        public static JobManager Create(int workers) => new JobManager(-1);

        public T GetFromJobPool<T>()
            where T : Job, new()
        {
            var type = typeof(T);
            var hashcode = type.GetHashCode();
            if (!_jobPools.TryGetValue(hashcode, out var jobPool))
            {
                jobPool = new JobPool<T>();
                _jobPools.Add(hashcode, jobPool);
            }

            return (T)jobPool.Take();
        }

        public void ReturnToJobPool<T>(T job)
            where T : Job
        {
            var type = typeof(T);
            var hashcode = type.GetHashCode();
            if (!_jobPools.TryGetValue(hashcode, out var jobPool))
            {
                throw new Exception("Could not find job pool?");
            }

            jobPool.Return(job);
        }

        public void Update()
        {

        }

        public void Initialize()
        {
            while (_workerPool.TotalRunning != _workerPool.Workers)
                Thread.Sleep(0);
        }

        public void WaitForIdle()
        {
            Initialize();
            while (!_workerPool.Idle)
                Thread.Sleep(0);
        }

        private List<IJobHandle> handles = new List<IJobHandle>();

        //public void Schedule(List<IJob> job, IJobHandle dependency = default)
        //{
        //    //in todays world, i would think an int increment is an atomic operation...
        //    handles.Clear();
        //    for (var i = 0; i < job.Count; i++)
        //    {
        //        var id = Interlocked.Increment(ref _sequenceID);
        //        var handle = new JobHandle(this, new JobRef(job[i], id, _version), dependency?.JobID ?? default);
        //        _jobHandles.Add(id, handle);
        //        handles.Add(handle);
        //    }

        //    var arr = handles.ToArray();
        //    JobWaiter waiter;
        //    lock (_jobWaiterPool)
        //        waiter = _jobWaiterPool.Take();

        //    waiter.SetHandles(arr);
        //    return Schedule(waiter);
        //}

        public void Schedule(IEnumerable<IJob> job, IJobHandle dependency = default)
        {
            foreach (var it in job)
            {
                var id = Interlocked.Increment(ref _sequenceID);

                var jobHandle = new JobHandle(this, new JobRef(it, id, _version), dependency?.JobID ?? default);
                _jobHandles.Add(id, jobHandle);
                _workerPool.Schedule(jobHandle);
            }
            _workerPool.SignalWork();
        }

        public IJobHandle Schedule(IJob job, in IJobHandle dependency = default)
        {
            //in todays world, i would think an int increment is an atomic operation...
            var id = Interlocked.Increment(ref _sequenceID);

            var jobHandle = new JobHandle(this, new JobRef(job, id, _version), dependency?.JobID ?? default);
            _jobHandles.Add(id, jobHandle);
            _workerPool.Schedule(jobHandle);
            _workerPool.SignalWork();
            return jobHandle;
        }

        public IJobHandle CombineDependencies(params IJobHandle[] jobHandles)
        {
            //object pool is always write, never reading anything
            //is lock the best way to handle a single access object
            //is the overhead of lock worse than the gc pressure
            //newing a class up would cost?

            JobWaiter waiter;
            lock (_jobWaiterPool)
                waiter = _jobWaiterPool.Take();

            waiter.SetHandles(jobHandles);
            return Schedule(waiter);
        }

        public void Complete()
        {
            //TODO: we need to think about main thread checks to throw useful errors in debug mode

            while (_workerPool.Flush())
            {
                //flush will use our thread to help finish the rest of the work
                //once flush returns false, there is nothing left in the queue
            }
            WaitForIdle();
            //we are using this to protect us against int roll over
            //but its probably not really helping since removefast is still single int based
            Interlocked.Increment(ref _version);
        }

        internal void ExecuteOne()
        {
            _workerPool.ExecuteOne();
        }

        internal void JobCompleted(in JobHandle jobHandle)
        {
            if (jobHandle.JobID.Job is JobWaiter jobWaiter)
            {
                lock (_jobWaiterPool)
                    _jobWaiterPool.Return(jobWaiter);
            }

            _jobHandles.RemoveFast(jobHandle.JobID.ID);
        }

        public bool IsJobCompleted(in JobRef jobID)
        {
            //lets try to get the job handle by id
            if (!_jobHandles.TryGetValue(jobID.ID, out var jh))
                return true; //job handle was not found, it was completed

            //we need to check if this handle has the same version, if not, its completed
            //this should never happen, but never say never
            return jobID.Version != jh.JobID.Version;
        }

        public bool IsJobCompleted(in IJobHandle jobHandle) => IsJobCompleted(jobHandle.JobID);

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _workerPool?.Dispose();
                    _workerPool = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
