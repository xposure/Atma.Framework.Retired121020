namespace Atma.Jobs
{
    internal sealed class JobWaiter : Job
    {
        private JobHandle[] _handles;

        internal void SetHandles(IJobHandle[] handles)
        {
            _handles = new JobHandle[handles.Length];
            for (var i = 0; i < handles.Length; i++)
                _handles[i] = (JobHandle)handles[i];
        }

        protected override void Execute()
        {
            for (var i = 0; i < _handles.Length; i++)
                _handles[i].Wait();
        }
    }
}
