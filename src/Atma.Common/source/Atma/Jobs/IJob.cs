namespace Atma.Jobs
{
    public interface IJob
    {
        void Run();
        void Schedule();
    }
}