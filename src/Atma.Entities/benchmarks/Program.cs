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

namespace Atma.Entities.Benchmarks
{
    public struct Color
    {
        public int ARGB;
        public Color(int a, int r, int g, int b)
        {
            ARGB = a << 24 + r << 16 + g << 8 + b;
        }
    }

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

    [SimpleJob(launchCount: 1, warmupCount: 3, targetCount: 5, invocationCount: 10000, id: "QuickJob")]
    //[MediumRunJob]
    public class Md5VsSha256
    {
        public float dt = 0.016f;
        public float maxx = 1024;
        public float maxy = 1024;

        private ILoggerFactory _logFactory;
        private ILogger _logger;
        private EntityManager _entities;


        private IAllocator _memory;


        [Params(100000)]
        public int N;

        [IterationCleanup]
        public void Cleanup()
        {
            _entities.Dispose();
            _memory.Dispose();

            // for (var i = 0; i < index; i++)
            //     Console.WriteLine($"{i}: {elapsed[i]}");

        }

        [IterationSetup]
        public void Setup()
        {
            elapsed = new float[N];
            _logFactory = new NullLoggerFactory();

            _memory = new HeapAllocator(_logFactory);
            _entities = new EntityManager(_logFactory, _memory);

            var r = new Random();
            var spec = EntitySpec.Create<Position, Velocity, Color>();
            for (var i = 0; i < N; i++)
            {
                //TODO: bulk insert API
                var entity = _entities.Create(spec);
                _entities.Replace(entity, new Position(r.Next(0, 1024), r.Next(0, 1024)));
                _entities.Replace(entity, new Velocity(r.Next(-500, 500), r.Next(-500, 500)));
                _entities.Replace(entity, new Color(r.Next(255), r.Next(255), r.Next(255), 255));
            }
        }

        public static int index = 0;
        public static float[] elapsed;

        [Benchmark(Baseline = true)]
        public void Raw()
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] {
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            };

            var sw = Stopwatch.StartNew();
            var entityArrays = _entities.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var t0i = -1;
                    var t1i = -1;

                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        if (t0i == -1) t0i = chunk.PackedArray.GetComponentIndex(componentTypes[0]);
                        if (t1i == -1) t1i = chunk.PackedArray.GetComponentIndex(componentTypes[1]);

                        var t0 = chunk.PackedArray.GetComponentSpan<Position>(t0i, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentSpan<Velocity>(t1i, componentTypes[1]);
                        for (var j = 0; j < length; j++)
                        {
                            ref var position = ref t0[j];
                            ref var velocity = ref t1[j];
                            position.x += velocity.x * dt;
                            position.y += velocity.y * dt;

                            velocity.x -= velocity.x * dt;
                            velocity.y -= velocity.y * dt;

                            if ((position.x > maxx && velocity.x > 0) || (position.x < 0 && velocity.x < 0))
                                velocity.x = -velocity.x;

                            if ((position.y > maxy && velocity.y > 0) || (position.y < 0 && velocity.y < 0))
                                velocity.y = -velocity.y;
                        }
                    }
                }
            }
            sw.Stop();
            elapsed[Interlocked.Increment(ref index)] = (float)sw.Elapsed.TotalSeconds;
        }

        //[Benchmark]
        public void ForEach()
        {
            _entities.ForEach((uint entity, ref Position position, ref Velocity velocity) =>
            {
                position.x += velocity.x * dt;
                position.y += velocity.y * dt;

                velocity.x -= velocity.x * dt;
                velocity.y -= velocity.y * dt;

                if ((position.x > maxx && velocity.x > 0) || (position.x < 0 && velocity.x < 0))
                    velocity.x = -velocity.x;

                if ((position.y > maxy && velocity.y > 0) || (position.y < 0 && velocity.y < 0))
                    velocity.y = -velocity.y;

            });
        }
    }

    class Program
    {
        static void RunOnce(int iterations)
        {
            var xyz = new Md5VsSha256();
            xyz.N = 100000;
            xyz.Setup();
            var sw = Stopwatch.StartNew();
            while (iterations-- > 0)
                xyz.Raw();
            sw.Stop();
            xyz.Cleanup();
            Console.WriteLine($"{sw.Elapsed}");
        }
        static void Main(string[] args)
        {
            //RunOnce(1000);
            //RunOnce(1000);
            //RunOnce(1000);
            var summary = BenchmarkRunner.Run<Md5VsSha256>();
        }
    }
}
