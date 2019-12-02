namespace Atma.Sandbox
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Atma.Entities;
    using Atma.Memory;
    using Shouldly;
    using System.Diagnostics;
    using static Atma.Debug;
    using Microsoft.Extensions.Logging;
    using System.Linq.Expressions;
    using Atma.Common;

    public static class Helpers
    {
        public static void WriteLine<T>(this IEnumerable<T> it)
        {
            foreach (var item in it)
                Console.WriteLine(item.ToString());
        }
    }

    class Program
    {

        public abstract class SystemVariable
        {
            public abstract Type VariableType { get; }
        }

        public sealed class SystemVariable<T> : SystemVariable
        {
            private readonly static Type _type = typeof(T);

            public override Type VariableType => _type;

            public T Value;

            public SystemVariable(T value)
            {
                Value = value;
            }


        }

        public sealed class SystemManager
        {
            private ILoggerFactory _logFactory;
            private ILogger _logger;

            private LookupList<SystemVariable> _variables = new LookupList<SystemVariable>();

            public SystemManager(ILoggerFactory logFactory)
            {
                _logger = logFactory.CreateLogger<SystemManager>();
            }

            public T Variable<T>(string name)
            {
                var id = name.GetHashCode();
                if (_variables.TryGetValue(id, out var sys))
                {
                    var sysVar = sys as SystemVariable<T>;
                    if (sysVar == null)
                    {
                        _logger.LogError($"Variable [{name}] type mismatch, expected [{typeof(T).FullName}] but got variable type [{sys.VariableType.FullName}].");
                        return default;
                    }

                    return sysVar.Value;
                }
                _logger.LogWarning($"Variable [{name}] was not found, returning default. Register the variable in the system manager.");
                return default;
            }

            public void Variable<T>(string name, T value)
            {
                var id = name.GetHashCode();
                if (!_variables.TryGetValue(id, out var sys))
                {
                    _variables.Add(id, new SystemVariable<T>(value));
                    return;
                }

                var sysVar = sys as SystemVariable<T>;
                if (sysVar == null)
                {
                    _logger.LogError($"Variable [{name}] type mismatch, expected [{typeof(T).FullName}] but got variable type [{sys.VariableType.FullName}].");
                    return;
                }

                sysVar.Value = value;
            }

        }

        public interface ISystem
        {
            void Tick(SystemManager systemManager, EntityManager entityManager);
        }


        //file: VelocitySystem.ecs
        //namespace (optional, get folders, get project namespace?)
        //variable dt
        //before <system>
        //after <system>
        //group <group>
        //write position
        //read velocity
        //entities entities
        //buffer buffer [65536]

        //generate file: obj/VelocitySystem.cs
        public ref partial struct VelocitySystemProcessor
        {
            public float dt;
            public Span<Position> positions;
            public ReadOnlySpan<EntityRef> entites;
            public ReadOnlySpan<Velocity> velocities;
            public EntityCommandBuffer buffer;

            public static void Process(SystemManager sm, EntityManager em)
            {
                var processor = new VelocitySystemProcessor();
                processor.dt = sm.Variable<float>("dt");
                processor.buffer = em.CreateCommandBuffer();

                Span<ComponentType> componentTypes = stackalloc ComponentType[] { ComponentType<Position>.Type, ComponentType<Velocity>.Type };
                var entityArrays = em.EntityArrays;
                for (var i = 0; i < entityArrays.Count; i++)
                {
                    var array = entityArrays[i];
                    if (array.Specification.HasAll(componentTypes))
                    {
                        var c0 = array.Specification.GetComponentIndex(componentTypes[0]);
                        var c1 = array.Specification.GetComponentIndex(componentTypes[1]);
                        for (var k = 0; k < array.AllChunks.Count; k++)
                        {
                            var chunk = array.AllChunks[k];
                            var length = chunk.Count;
                            var entities = chunk.Entities;
                            var t0 = chunk.GetComponentData<Position>(c0, componentTypes[0]);
                            var t1 = chunk.GetComponentData<Velocity>(c1, componentTypes[1]);

                            processor.Execute(entities.Length);
                        }
                    }
                }

                processor.buffer.Execute(em);
            }

            partial void Execute(int length);
        }

        public ref partial struct VelocitySystemProcessor
        {
            partial void Execute(int length)
            {
                for (var i = 0; i < length; i++)
                {
                    ref readonly var e = ref entites[i];
                    ref var p = ref positions[i];
                    ref readonly var v = ref velocities[i];
                    p.X += v.X * dt;
                    p.Y += v.Y * dt;
                }
            }
        }

        public sealed class VelocitySystem : ISystem
        {
            public object Dependencies
            {
                get
                {
                    return null;
                }
            }

            public void Tick(SystemManager systemManager, EntityManager entityManager)
            {
                VelocitySystemProcessor.Process(systemManager, entityManager);
            }
        }


        protected static ILoggerFactory _logFactory;
        protected static ILogger _logger;

        static void Main(string[] args)
        {
            _logFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = _logFactory.CreateLogger("Sandbox");

            var memory = new HeapAllocator(_logFactory);
            var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Position>.Type, ComponentType<Velocity>.Type);
            var e = em.Create(spec);

            em.ForChunk((int length, ReadOnlySpan<EntityRef> entities, Span<Position> positions, Span<Velocity> velocities) =>
            {

            });

            //var test2 = new Test2();
            //test2.Execute(1);

            // var type = typeof(Test);

            // type.GetConstructors().WriteLine();


            em.Dispose();
            memory.Dispose();
        }
    }
    public struct Valid
    {
        public int X, Y;
        public override string ToString()
        {
            return $"X: {X}, Y: {Y}";
        }
    }

    public struct Valid2
    {
        public int Z;
        public override string ToString()
        {
            return $"Z: {Z}";
        }
    }

    public struct Valid3
    {
        public int W;
        public override string ToString()
        {
            return $"W: {W}";
        }
    }

    public struct Valid4
    {
        public int _;

        public override string ToString()
        {
            return $"_: {_}";
        }
    }

    public struct Valid5
    {
        public int _;
        public override string ToString()
        {
            return $"_: {_}";
        }

    }

    public struct Valid6
    {
        public int _;
        public override string ToString()
        {
            return $"_: {_}";
        }

    }

    public struct Invalid
    {
        public Object obj;
        public override string ToString()
        {
            return $"*";
        }
    }

    public struct ShouldFilterStruct
    {
        public Write<Valid> valid;
    }

    public struct GroupArray
    {
        public bool oddSize;
        public float dt;
        public int length;
        public Read<Entity> entity;
        public Read<Valid> valid;
        public Write<Valid2> valid2;
    }

    public struct GroupWithEntity2
    {
        public Entity entity;
        public Valid valid;
        public Valid2 valid2;
    }

    public struct Group
    {
        public Valid valid;
        public Valid2 valid2;

        public override string ToString()
        {
            return $"Group {{ valid: {valid}, valid2: {valid2} }}";
        }
    }
}
