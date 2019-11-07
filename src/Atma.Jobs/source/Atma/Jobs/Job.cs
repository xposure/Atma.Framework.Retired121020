namespace Atma.Jobs
{
    public abstract class Job : IJob
    {
        private IJobManager _jobManager;

        internal void Initialize(IJobManager pool)
        {
            _jobManager = pool;
        }

        public void Run()
        {
            Execute();
        }

        public void Schedule()
        {
            _jobManager.Schedule(this);
        }

        protected abstract void Execute();
    }
}
