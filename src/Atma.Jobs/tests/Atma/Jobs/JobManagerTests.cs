#pragma warning disable 0649
namespace Atma.Jobs
{
    using System.Threading;
    using Shouldly;

    public class JobManagerTests
    {
        private struct JobData
        {
            public int i;
        }

        public class JobRunner : Job
        {
            public int result = 0;

            protected override void Execute()
            {
                result += 10;
            }
        }

        public class JobDelay : Job
        {
            public int Delay = 10;
            protected override void Execute()
            {
                Thread.Sleep(Delay);
            }
        }

        public void ShouldRunJob()
        {
            using (var manager = JobManager.Create(1))
            {
                manager.WaitForIdle();

                var job = new JobRunner();
                var handle = manager.Schedule(job);

                manager.WaitForIdle();

                handle.IsCompleted.ShouldBe(true);
                job.result.ShouldBe(10);
            }
        }

        public void ShouldRunJobWithDeps()
        {
            using (var manager = JobManager.Create(1))
            {
                manager.WaitForIdle();

                var job0 = new JobDelay() { Delay = 1 };
                var handle0 = manager.Schedule(job0);
                var job1 = new JobRunner();
                var handle1 = manager.Schedule(job1, handle0);

                manager.WaitForIdle();

                handle0.IsCompleted.ShouldBe(true);
                handle1.IsCompleted.ShouldBe(true);

                job1.result.ShouldBe(10);
            }
        }

        public void MainThreadShouldHelpOnComplete()
        {
            using (var manager = JobManager.Create(1))
            {
                manager.WaitForIdle();

                var job0 = new JobDelay() { Delay = 1 };
                var handle0 = manager.Schedule(job0);
                var job1 = new JobRunner();
                var handle1 = manager.Schedule(job1, handle0);

                manager.Complete();

                handle0.IsCompleted.ShouldBe(true);
                handle1.IsCompleted.ShouldBe(true);

                job1.result.ShouldBe(10);
            }
        }

        public void ShouldWaitOnCombinedDeps()
        {
            using (var manager = JobManager.Create(1))
            {
                manager.WaitForIdle();

                var job0 = new JobDelay() { Delay = 2 };
                var handle0 = manager.Schedule(job0);
                var job1 = new JobDelay() { Delay = 2 };
                var handle1 = manager.Schedule(job1);
                var job2 = new JobDelay() { Delay = 2 };
                var handle2 = manager.Schedule(job2);
                var job3 = new JobDelay() { Delay = 2 };
                var handle3 = manager.Schedule(job3);
                var job4 = new JobRunner();
                var handle4 = manager.Schedule(job4);

                var handle = manager.CombineDependencies(handle0, handle1, handle2, handle3, handle4);
                handle.Wait();

                handle0.IsCompleted.ShouldBe(true);
                handle1.IsCompleted.ShouldBe(true);
                handle2.IsCompleted.ShouldBe(true);
                handle3.IsCompleted.ShouldBe(true);
                handle4.IsCompleted.ShouldBe(true);

                job4.result.ShouldBe(10);
            }
        }

    }
}
