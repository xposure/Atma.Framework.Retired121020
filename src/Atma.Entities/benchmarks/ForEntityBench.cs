namespace Atma.Entities
{
    using System;
    using Atma.Memory;
    using BenchmarkDotNet.Attributes;
    using Microsoft.Extensions.Logging;

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

                            //WTF: Well I learned the hard way today about denormalized floats... 
                            //need to reset the data on each iteration
                            velocity.X -= velocity.X * dt;
                            velocity.Y -= velocity.Y * dt;

                            if ((position.X > maxx && velocity.X > 0) || (position.X < 0 && velocity.X < 0))
                                velocity.X = -velocity.X;

                            if ((position.Y > maxy && velocity.Y > 0) || (position.Y < 0 && velocity.Y < 0))
                                velocity.Y = -velocity.Y;
                        }
                    }
                }
            }
        }

        [Benchmark]
        public void ForChunk()
        {
            _entities.ForChunk((int length, ReadOnlySpan<EntityRef> entities, Span<Position> positions, Span<Velocity> velocities) =>
            {
                for (var i = 0; i < length; i++)
                {
                    ref var position = ref positions[i];
                    ref var velocity = ref velocities[i];

                    position.X += velocity.X * dt;
                    position.Y += velocity.Y * dt;

                    velocity.X -= velocity.X * dt;
                    velocity.Y -= velocity.Y * dt;

                    if ((position.X > maxx && velocity.X > 0) || (position.X < 0 && velocity.X < 0))
                        velocity.X = -velocity.X;

                    if ((position.Y > maxy && velocity.Y > 0) || (position.Y < 0 && velocity.Y < 0))
                        velocity.Y = -velocity.Y;
                }
            });
        }
        [Benchmark]
        public void ForEntity()
        {
            _entities.ForEntity((uint entity, ref Position position, ref Velocity velocity) =>
            {
                position.X += velocity.X * dt;
                position.Y += velocity.Y * dt;

                velocity.X -= velocity.X * dt;
                velocity.Y -= velocity.Y * dt;

                if ((position.X > maxx && velocity.X > 0) || (position.X < 0 && velocity.X < 0))
                    velocity.X = -velocity.X;

                if ((position.Y > maxy && velocity.Y > 0) || (position.Y < 0 && velocity.Y < 0))
                    velocity.Y = -velocity.Y;

            });
        }
    }
}