namespace Atma.Entities
{
    using BenchmarkDotNet.Attributes;
    using Microsoft.Extensions.Logging;

    [SimpleJob(launchCount: 1, warmupCount: 3, targetCount: 5, invocationCount: 2000)]
    public class Benchmark
    {
        protected ILoggerFactory _logFactory;
        protected ILogger _logger;

        protected Benchmark()
        {
            _logFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            _logger = _logFactory.CreateLogger("Benchmark");
        }

        [GlobalSetup] public void Setup() => OnSetup();
        [GlobalCleanup] public void Cleanup() => OnCleanup();
        [IterationSetup] public void IterationSetup() => OnIterationSetup();
        [IterationCleanup] public void IterationCleanup() => OnIterationCleanup();

        protected virtual void OnSetup() { }
        protected virtual void OnCleanup() { }
        protected virtual void OnIterationSetup() { }
        protected virtual void OnIterationCleanup() { }

    }
}