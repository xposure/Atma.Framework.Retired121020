namespace Atma.Jobs
{
    using Atma.Common;
    using Atma.Profiling;
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    public class JobWorkerPool : IDisposable
    {
        private IProfileService _profiler;
        private ConcurrentQueue<JobHandle> _queuedJobs = new ConcurrentQueue<JobHandle>();
        private JobWorker[] _workerThreads;

        private volatile int _totalRunning = 0;

        public int Workers => _workerThreads.Length;
        public int TotalRunning => _totalRunning;

        public int JobsQueued => _queuedJobs.Count;

        public bool Idle
        {
            get
            {
                if (_queuedJobs.Count > 0)
                    return false;

                for (var i = 0; i < _workerThreads.Length; i++)
                    if (!_workerThreads[i].Idle)
                        return false;

                return true;
            }
        }

        public JobWorkerPool(int workers = -1, IProfileService profiler = null)
        {
            _profiler = profiler;
            if (workers == -1)
                workers = Environment.ProcessorCount;

            _workerThreads = new JobWorker[workers];
            for (var i = 0; i < _workerThreads.Length; i++)
            {
                _workerThreads[i] = new JobWorker(i, Execute);
                _workerThreads[i].Start();
            }
        }

        private void Execute(JobWorker worker)
        {
            Interlocked.Increment(ref _totalRunning);
            try
            {
                while (worker.IsRunning)
                {
                    worker.Idle = true;
                    worker.Signal.Wait();
                    worker.Signal.Reset();
                    worker.Idle = false;

                    ProcessJobs();
                }
            }
            finally
            {
                worker.Idle = true;
                Interlocked.Decrement(ref _totalRunning);
            }
        }

        internal void ExecuteOne()
        {
            if (_queuedJobs.TryDequeue(out var jobHandle))
                jobHandle.Execute();

        }

        private void ProcessJobs()
        {
            var current = _profiler.Current;
            while (_queuedJobs.TryDequeue(out var job))
            {
                //try
                {
                    using var scope = current.Begin($"Job {job.JobID.ID}");
                    job.Execute();
                }
                //catch //(Exception ex)
                {
                    //TODO: how should we handle job exceptions?

                    //job handle completion is tracked by if the id still exists
                    //in the handle list, if it doesn't then it was completed
                    //since there is no reference type there is no way to signal a failed job
                    //and even if there was, what would a game actually do differently other than
                    //tell the user and crash?

                    //TODO: figure out how to gracefully handle a failure and stop the application
                }
            }
        }

        internal void SignalWork()
        {
            foreach (var it in _workerThreads)
                it.Signal.Set();
        }

        internal void Schedule(JobHandle[] jobHandle)
        {
            for(var i = 0;i < jobHandle.Length;i++)
                _queuedJobs.Enqueue(jobHandle[i]);
            SignalWork();
        }

        internal void Schedule(in JobHandle jobHandle)
        {            
            _queuedJobs.Enqueue(jobHandle);
            //SignalWork();
            //return jobHandle;
        }

        internal bool Flush()
        {
            SignalWork();
            //ProcessJobs();

            return _queuedJobs.Count > 0;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_workerThreads != null)
                    {
                        for (var i = 0; i < _workerThreads.Length; i++)
                            _workerThreads[i].Stop();

                        for (var i = 0; i < _workerThreads.Length; i++)
                            _workerThreads[i].Join(1000);

                        _workerThreads = null;
                    }
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
