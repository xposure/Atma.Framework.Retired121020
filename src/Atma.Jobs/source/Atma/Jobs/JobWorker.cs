namespace Atma.Jobs
{
    using Atma.Common;
    using static Atma.Debug;

    using System;
    using System.Threading;
    using Atma.Profiling;

    public class JobWorker
    {
        internal ManualResetEventSlim Signal { get; } = new ManualResetEventSlim();
        private DistributedThread _distributedThread { get; }

        private Action<JobWorker> _worker;
        private volatile bool _isStarted = false;
        private volatile bool _isRunning = false;

        private int _core = 0;

        public bool IsRunning => _isRunning;
        public bool Idle { get; internal set; }

        public JobWorker(int core, Action<JobWorker> worker)
        {
            _core = core;
            _worker = worker;
            _distributedThread = new DistributedThread(new ParameterizedThreadStart(Run));
            _distributedThread.ProcessorAffinity = 1 << core;
            _distributedThread.ManagedThread.Name = $"Worker Thread (Core {core})";
        }

        private void Run(object o)
        {
            _isRunning = true;
            try
            {
                _worker((JobWorker)o);
            }
            finally
            {
                _isStarted = false;
                _isRunning = false;
            }
        }

        public void Start()
        {
            Assert(!_isStarted);
            _isStarted = true;
            _distributedThread.Start(this);
        }

        public void Stop()
        {
            Assert(_isRunning);
            Signal.Set();
            _isRunning = false;
        }

        public void Join(int ms)
        {
            if (_isStarted)
            {
                Signal.Set();
                _distributedThread.ManagedThread.Join();
                //{ not supported error?
                //_distributedThread.ManagedThread.Abort();
                //}
            }
        }
    }
}
