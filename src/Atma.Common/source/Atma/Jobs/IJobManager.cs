using System;

namespace Atma.Jobs
{
    public interface IJobManager :  IDisposable
    {
        IJobHandle CombineDependencies(params IJobHandle[] jobHandles);
        void Complete();
        bool IsJobCompleted(in IJobHandle jobHandle);
        bool IsJobCompleted(in JobRef jobID);
        IJobHandle Schedule(IJob job, in IJobHandle dependency = default);
        void WaitForIdle();
    }
}