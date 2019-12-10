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
        ActorSystem view = new ActorSystem();

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

        [Benchmark(Baseline = true)]
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



        public readonly unsafe struct Actor : Atma.Systems.IExecutor
        {
            public readonly Position* position;
            public readonly Velocity* velocity;

            public void Execute(int length)
            {
                for (var i = 0; i < length; i++)
                    Execute(ref position[i], ref velocity[i]);
            }

            private void Execute(ref Position position, ref Velocity velocity)
            {
                float dt = 0.016f;
                float maxx = 1024;
                float maxy = 1024;

                position.X += velocity.X * dt;
                position.Y += velocity.Y * dt;

                velocity.X -= velocity.X * dt;
                velocity.Y -= velocity.Y * dt;

                if ((position.X > maxx && velocity.X > 0) || (position.X < 0 && velocity.X < 0))
                    velocity.X = -velocity.X;

                if ((position.Y > maxy && velocity.Y > 0) || (position.Y < 0 && velocity.Y < 0))
                    velocity.Y = -velocity.Y;
            }
        }

        private unsafe void ExecuteActor(Actor* actor)
        {
            actor->position->X += actor->velocity->X * dt;
            actor->position->Y += actor->velocity->Y * dt;

            actor->velocity->X -= actor->velocity->X * dt;
            actor->velocity->Y -= actor->velocity->Y * dt;

            if ((actor->position->X > maxx && actor->velocity->X > 0) || (actor->position->X < 0 && actor->velocity->X < 0))
                actor->velocity->X = -actor->velocity->X;

            if ((actor->position->Y > maxy && actor->velocity->Y > 0) || (actor->position->Y < 0 && actor->velocity->Y < 0))
                actor->velocity->Y = -actor->velocity->Y;
        }

        [Benchmark]
        public unsafe void ForEntityStructPtr()
        {
            Span<ComponentType> componentTypes = stackalloc[] { ComponentType<Position>.Type, ComponentType<Velocity>.Type };

            _entities.EntityArrays.Filter(componentTypes, array =>
            {
                var actor = stackalloc[] { new Actor() };

                var ptr0Size = sizeof(Position);
                var ptr1Size = sizeof(Velocity);

                byte** ptr0 = (byte**)&actor->position;
                byte** ptr1 = (byte**)&actor->velocity;

                var c0 = array.Specification.GetComponentIndex(ComponentType<Position>.Type);
                var c1 = array.Specification.GetComponentIndex(ComponentType<Velocity>.Type);

                for (var i = 0; i < array.AllChunks.Count; i++)
                {
                    var chunk = array.AllChunks[i];
                    (*ptr0) = (byte*)chunk.PackedArray[c0].Memory;
                    (*ptr1) = (byte*)chunk.PackedArray[c1].Memory;

                    for (var j = 0; j < chunk.Count; j++)
                    {
                        ExecuteActor(actor);
                        (*ptr0) += ptr0Size;
                        (*ptr1) += ptr1Size;
                    }
                }
            });
        }

        [Benchmark]
        public unsafe void ForEntityViewStructPtr()
        {
            view.Execute(_entities);
        }

        public sealed unsafe partial class ActorSystem : Atma.Systems.System<Actor>
        {
            public float dt = 0.016f;
            public float maxx = 1024;
            public float maxy = 1024;

            protected override void Execute(in Actor actor, int length)
            {
                for (var i = 0; i < length; i++)
                    Execute(ref actor.position[i], ref actor.velocity[i]);
                // actor.position->X += actor.velocity->X * dt;
                // actor.position->Y += actor.velocity->Y * dt;

                // actor.velocity->X -= actor.velocity->X * dt;
                // actor.velocity->Y -= actor.velocity->Y * dt;

                // if ((actor.position->X > maxx && actor.velocity->X > 0) || (actor.position->X < 0 && actor.velocity->X < 0))
                //     actor.velocity->X = -actor.velocity->X;

                // if ((actor.position->Y > maxy && actor.velocity->Y > 0) || (actor.position->Y < 0 && actor.velocity->Y < 0))
                //     actor.velocity->Y = -actor.velocity->Y;
            }

            partial void Execute(ref Position position, ref Velocity velocity);
        }

        public partial class ActorSystem : Atma.Systems.System<Actor>
        {
            partial void Execute(ref Position position, ref Velocity velocity)
            {
                float dt = 0.016f;
                float maxx = 1024;
                float maxy = 1024;

                position.X += velocity.X * dt;
                position.Y += velocity.Y * dt;

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