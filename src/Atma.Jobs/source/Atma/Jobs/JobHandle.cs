namespace Atma.Jobs
{
    using System;

    public struct JobHandle : IEquatable<IJobHandle>, IJobHandle
    {
        private readonly JobManager _jobManager;
        private readonly JobRef _jobDependency;
        private readonly JobRef _jobID;

        public JobRef JobID => _jobID;
        public JobRef JobDependency => _jobDependency;

        internal JobHandle(JobManager jobManager, JobRef jobID, JobRef jobDependencyID)
        {
            _jobManager = jobManager;
            _jobID = jobID;
            _jobDependency = jobDependencyID;
        }

        public void Run()
        {
            if (JobDependency.IsValid)
                JobDependency.Job.Run();

            JobID.Job.Run();
        }

        internal void Execute()
        {
            if (JobDependency.IsValid)
            {
                while (!_jobManager.IsJobCompleted(JobDependency))
                    Idle();
            }

            JobID.Job.Run();
            _jobManager.JobCompleted(this);
        }

        private void Idle()
        {
            _jobManager.ExecuteOne();
        }

        public bool IsCompleted => _jobManager.IsJobCompleted(JobID);

        public void Wait()
        {
            while (!IsCompleted)
                Idle();

            //we need to first see if we are done, if so, return
            //if not we need to start polling tasks off the jobManager
            //if we don't we could have multiple jobhandles all waiting with
            //no free workers to do anything

            //_jobManager.IsCompleted(JobID);
        }

        public bool Equals(IJobHandle other)
        {
            return JobID.ID == other.JobID.ID && JobID.Version == other.JobID.Version;
        }
    }
}
