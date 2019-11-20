namespace Atma.Entities
{
    using System;
    using Atma.Memory;
    using BenchmarkDotNet.Attributes;
    using Microsoft.Extensions.Logging;

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
            System.Span<ComponentType> componentTypes = stackalloc ComponentType[] {
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

                        var t0 = chunk.PackedArray.GetComponentData<Position>(t0i, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentData<Velocity>(t1i, componentTypes[1]);
                        for (var j = 0; j < length; j++)
                        {
                            ref var position = ref t0[j];
                            ref var velocity = ref t1[j];
                            position.X += velocity.X * dt;
                            position.Y += velocity.Y * dt;
                            velocity.X -= velocity.X * dt;
                            velocity.Y -= velocity.Y * dt;
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
                position.X += velocity.X * dt;
                position.Y += velocity.Y * dt;

                velocity.X -= velocity.X * dt;
                velocity.Y -= velocity.Y * dt;
            });
        }


        [Benchmark]
        public void RawTwoPass()
        {
            System.Span<ComponentType> componentTypes = stackalloc ComponentType[] {
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

                        var t0 = chunk.PackedArray.GetComponentData<Position>(t0i, componentTypes[0]);
                        var t1 = chunk.PackedArray.GetComponentData<Velocity>(t1i, componentTypes[1]);
                        for (var j = 0; j < length; j++)
                        {
                            ref var position = ref t0[j];
                            ref var velocity = ref t1[j];
                            position.X += velocity.X * dt;
                            position.Y += velocity.Y * dt;
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
                        var t1 = chunk.PackedArray.GetComponentData<Velocity>(t1i, componentTypes[1]);
                        for (var j = 0; j < length; j++)
                        {
                            ref var velocity = ref t1[j];
                            velocity.X -= velocity.X * dt;
                            velocity.Y -= velocity.Y * dt;
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
                position.X += velocity.X * dt;
                position.Y += velocity.Y * dt;
            });

            _entities.ForEntity((uint entity, ref Velocity velocity) =>
            {
                velocity.X -= velocity.X * dt;
                velocity.Y -= velocity.Y * dt;
            });
        }
    }
}