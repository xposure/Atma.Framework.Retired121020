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
    public class ForEntityBench
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

        [GlobalCleanup]
        public void Cleanup()
        {
            _entities.Dispose();
            _memory.Dispose();


        }

        [IterationSetup]
        public void IterationSetup()
        {
            var r = new Random(123456789);
            _entities.ForEntity((uint entity, ref Position position, ref Velocity velocity) =>
            {
                position = new Position(r.Next(0, 1024), r.Next(0, 1024));
                velocity = new Velocity(r.Next(-500, 500), r.Next(-500, 500));
            });
        }

        [IterationCleanup]
        public void IterationCleanup()
        {

        }

        [GlobalSetup]
        public void Setup()
        {
            _logFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            _memory = new HeapAllocator(_logFactory);
            _entities = new EntityManager(_logFactory, _memory);

            var spec = EntitySpec.Create<Position, Velocity>();
            for (var i = 0; i < N; i++)
            {
                //TODO: bulk insert API
                var entity = _entities.Create(spec);
            }
        }

        [Benchmark]
        public void Raw()
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] {
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            };

            var entityArrays = _entities.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var t0i = array.Specification.GetComponentIndex(componentTypes[0]);
                    var t1i = array.Specification.GetComponentIndex(componentTypes[1]);

                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;

                        var t0 = chunk.PackedArray.GetComponentSpan<Position>(t0i, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentSpan<Velocity>(t1i, componentTypes[1]);
                        for (var j = 0; j < length; j++)
                        {
                            ref var position = ref t0[j];
                            ref var velocity = ref t1[j];
                            position.x += velocity.x * dt;
                            position.y += velocity.y * dt;

                            //WTF: Well I learned the hard way today about denormalized floats... 
                            //need to reset the data on each iteration
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
        }

        [Benchmark]
        public void ForChunk()
        {
            _entities.ForChunk((int length, ReadOnlySpan<uint> entities, Span<Position> positions, Span<Velocity> velocities) =>
            {
                for (var i = 0; i < length; i++)
                {
                    ref var position = ref positions[i];
                    ref var velocity = ref velocities[i];

                    position.x += velocity.x * dt;
                    position.y += velocity.y * dt;

                    velocity.x -= velocity.x * dt;
                    velocity.y -= velocity.y * dt;

                    if ((position.x > maxx && velocity.x > 0) || (position.x < 0 && velocity.x < 0))
                        velocity.x = -velocity.x;

                    if ((position.y > maxy && velocity.y > 0) || (position.y < 0 && velocity.y < 0))
                        velocity.y = -velocity.y;
                }
            });
        }
        [Benchmark]
        public void ForEntity()
        {
            _entities.ForEntity((uint entity, ref Position position, ref Velocity velocity) =>
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

    [SimpleJob(launchCount: 1, warmupCount: 3, targetCount: 5, invocationCount: 2000, id: "SIMDMaybe")]
    //[MediumRunJob]
    public class ForChunkBench
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

        [GlobalCleanup]
        public void Cleanup()
        {
            _entities.Dispose();
            _memory.Dispose();


        }

        [IterationSetup]
        public void IterationSetup()
        {
            var r = new Random(123456789);
            _entities.ForEntity((uint entity, ref Position position, ref Velocity velocity) =>
            {
                position = new Position(r.Next(0, 1024), r.Next(0, 1024));
                velocity = new Velocity(r.Next(-500, 500), r.Next(-500, 500));
            });
        }

        [IterationCleanup]
        public void IterationCleanup()
        {

        }

        [GlobalSetup]
        public void Setup()
        {
            _logFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            _memory = new HeapAllocator(_logFactory);
            _entities = new EntityManager(_logFactory, _memory);

            var spec = EntitySpec.Create<Position, Velocity>();
            for (var i = 0; i < N; i++)
            {
                //TODO: bulk insert API
                var entity = _entities.Create(spec);
            }
        }

        [Benchmark]
        public void RawSinglePass()
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] {
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            };

            var entityArrays = _entities.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var t0i = array.Specification.GetComponentIndex(componentTypes[0]);
                    var t1i = array.Specification.GetComponentIndex(componentTypes[1]);

                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;

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
                        }
                    }
                }
            }
        }

        [Benchmark]
        public void ForEntitySinglePass()
        {
            _entities.ForEntity((uint entity, ref Position position, ref Velocity velocity) =>
            {
                position.x += velocity.x * dt;
                position.y += velocity.y * dt;

                velocity.x -= velocity.x * dt;
                velocity.y -= velocity.y * dt;
            });
        }


        [Benchmark]
        public void RawTwoPass()
        {
            Span<ComponentType> componentTypes = stackalloc ComponentType[] {
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            };

            var entityArrays = _entities.EntityArrays;
            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var t0i = array.Specification.GetComponentIndex(componentTypes[0]);
                    var t1i = array.Specification.GetComponentIndex(componentTypes[1]);

                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;

                        var t0 = chunk.PackedArray.GetComponentSpan<Position>(t0i, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentSpan<Velocity>(t1i, componentTypes[1]);
                        for (var j = 0; j < length; j++)
                        {
                            ref var position = ref t0[j];
                            ref var velocity = ref t1[j];
                            position.x += velocity.x * dt;
                            position.y += velocity.y * dt;
                        }
                    }
                }
            }

            for (var i = 0; i < entityArrays.Count; i++)
            {
                var array = entityArrays[i];
                if (array.Specification.HasAll(componentTypes))
                {
                    var t1i = array.Specification.GetComponentIndex(componentTypes[1]);

                    for (var k = 0; k < array.AllChunks.Count; k++)
                    {
                        var chunk = array.AllChunks[k];
                        var length = chunk.Count;
                        var t1 = chunk.PackedArray.GetComponentSpan<Velocity>(t1i, componentTypes[1]);
                        for (var j = 0; j < length; j++)
                        {
                            ref var velocity = ref t1[j];
                            velocity.x -= velocity.x * dt;
                            velocity.y -= velocity.y * dt;
                        }
                    }
                }
            }
        }

        [Benchmark]
        public void ForEntityTwoPass()
        {
            _entities.ForEntity((uint entity, ref Position position, ref Velocity velocity) =>
            {
                position.x += velocity.x * dt;
                position.y += velocity.y * dt;
            });

            _entities.ForEntity((uint entity, ref Velocity velocity) =>
            {
                velocity.x -= velocity.x * dt;
                velocity.y -= velocity.y * dt;
            });
        }
    }


    class Program
    {
        static void RunOnce(int iterations)
        {
            var xyz = new ForEntityBench();
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
            //for (var i = 0; i < 15; i++)
            //    RunOnce(1000);
            var summary = BenchmarkRunner.Run<ForChunkBench>();
        }
    }
}
