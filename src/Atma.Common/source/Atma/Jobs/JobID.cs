namespace Atma.Jobs
{
    public struct JobRef
    {
        public readonly int ID;
        public readonly int Version;
        public readonly IJob Job;
        public bool IsValid => Job != null;


        public JobRef(IJob job, int id, int version)
        {
            Job = job;
            ID = id;
            Version = version;
        }
    }
}
