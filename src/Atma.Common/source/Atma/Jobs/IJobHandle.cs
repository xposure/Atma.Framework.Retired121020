namespace Atma.Jobs
{
    public interface IJobHandle
    {
        public JobRef JobDependency { get; }
        public JobRef JobID { get; }

        bool IsCompleted { get; }

        bool Equals(IJobHandle other);
        void Run();
        void Wait();
    }
}