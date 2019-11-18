using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;
using Atma.Memory;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Atma.Memory.Benchmarks
{
    public struct Position
    {
        public float x;
        public float y;

        public Position(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public struct Velocity
    {
        public float x;
        public float y;
        public Velocity(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [SimpleJob(launchCount: 1, warmupCount: 3, targetCount: 5, invocationCount: 2000, id: "QuickJob")]
    //[MediumRunJob]
    public unsafe class SpanVsNativeSlice
    {
        private ILoggerFactory _logFactory;
        private ILogger _logger;
        private IAllocator _memory;

        private AllocationHandle _handle;


        [Params(100000)]
        public int N;

        [GlobalCleanup]
        public void Cleanup()
        {
            _memory.Dispose();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _handle = _memory.Take(N * sizeof(float));
            var addr = (float*)_handle.Address;
            for (var i = 0; i < N; i++)
                addr[i] = 0.0001f;
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            _memory.Free(ref _handle);
        }

        [GlobalSetup]
        public void Setup()
        {
            _logFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            _memory = new HeapAllocator(_logFactory);
        }

        [Benchmark]
        public float Span()
        {
            var span = new Span<float>((void*)_handle.Address, N);
            var r = 0f;
            for (var i = 0; i < span.Length; i++)
                r += span[i];

            return r;
        }

        [Benchmark]
        public float ReadOnlySpan()
        {
            var span = new ReadOnlySpan<float>((void*)_handle.Address, N);
            var r = 0f;
            for (var i = 0; i < span.Length; i++)
                r += span[i];

            return r;
        }

        [Benchmark]
        public float NativeSlice()
        {
            var span = new NativeSlice<float>((void*)_handle.Address, N);
            var r = 0f;
            for (var i = 0; i < span.Length; i++)
                r += span[i];

            return r;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //for (var i = 0; i < 15; i++)
            //    RunOnce(1000);
            var summary = BenchmarkRunner.Run<SpanVsNativeSlice>();
        }
    }
}
