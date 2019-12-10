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
    using System.Reflection.Emit;

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
        //variable float dt
        //before <system> (example RenderSystem)
        //after <system> (example InputSystem)
        //group <group> (defaults to Update)
        //write position
        //read velocity
        //entities entities
        //buffer buffer [65536]

        //generate file: obj/VelocitySystem.cs
        public ref partial struct VelocitySystem
        {
            public float dt;
            public Span<Position> positions;
            public ReadOnlySpan<EntityRef> entites;
            public ReadOnlySpan<Velocity> velocities;
            public EntityCommandBuffer buffer;

            public static void Process(SystemManager sm, EntityManager em)
            {
                var system = new VelocitySystem();
                system.dt = sm.Variable<float>("dt");
                system.buffer = em.CreateCommandBuffer();

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
                            system.positions = chunk.GetComponentData<Position>(c0, componentTypes[0]);
                            system.velocities = chunk.GetComponentData<Velocity>(c1, componentTypes[1]);
                            system.entites = chunk.Entities;
                            system.Execute(chunk.Count);
                        }
                    }
                }

                system.buffer.Execute();
            }

            partial void Execute(int length);
        }

        public ref partial struct VelocitySystem
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

        public sealed class VelocitySystemProcessor : ISystem
        {
            public string Group => "Update";

            public object Dependencies
            {
                get
                {
                    return null;
                }
            }

            public void Tick(SystemManager systemManager, EntityManager entityManager)
            {
                VelocitySystem.Process(systemManager, entityManager);
            }
        }

        protected static ILoggerFactory _logFactory;
        protected static ILogger _logger;

        public struct ActorTest
        {
            public Position position;
            public Velocity velocity;
        }

        public unsafe class SystemTest
        {


            private delegate void caller(SystemTest test, void* it);

            public void Execute()
            {
                var memory = new HeapAllocator(_logFactory);
                var actors = memory.Take<ActorTest>(10000);
                var actor = (ActorTest*)actors.Address;
                for (var j = 0; j < 10000; j++)
                    actor[j] = new ActorTest() { position = new Position(j), velocity = new Velocity(j + 1) };

                var method = new DynamicMethod("Test", null, new Type[] { typeof(SystemTest), typeof(void*) });

                var gen = method.GetILGenerator();
                var i = gen.DeclareLocal(typeof(int));
                //var b = gen.DeclareLocal(typeof(bool));

                //var label = gen.DefineLabel();

                //i = 0
                gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Stloc_0);

                //gen.Emit(OpCodes.Br_S, label);



                gen.Emit(OpCodes.Ldarg_0); //void*, systemtest
                gen.Emit(OpCodes.Ldarg_2);
                gen.Emit(OpCodes.Ldloc_0);
                gen.Emit(OpCodes.Conv_I);
                gen.Emit(OpCodes.Sizeof, typeof(ActorTest));
                gen.Emit(OpCodes.Mul);
                gen.Emit(OpCodes.Add);
                gen.Emit(OpCodes.Ldflda, typeof(ActorTest).GetField("position")); //void*   



                gen.Emit(OpCodes.Ldarg_0); //void*, systemtest
                gen.Emit(OpCodes.Ldarg_2);
                gen.Emit(OpCodes.Ldloc_0);
                gen.Emit(OpCodes.Conv_I);
                gen.Emit(OpCodes.Sizeof, typeof(ActorTest));
                gen.Emit(OpCodes.Mul);
                gen.Emit(OpCodes.Add);
                gen.Emit(OpCodes.Ldflda, typeof(ActorTest).GetField("velocity")); //void*   

                gen.Emit(OpCodes.Call, typeof(SystemTest).GetMethod("ExecuteMe2"));

                gen.Emit(OpCodes.Ret);

                var f = (caller)method.CreateDelegate(typeof(caller));
                //Console.WriteLine(new IntPtr(actor));
                //ExecuteMe(actor);
                f(this, actor);
            }

            public void ExecuteMe(int length, ActorTest* actor)
            {
                for (var i = 0; i < length; i++)
                    ExecuteMe2(ref actor[i].position, actor[i].velocity);
                // Console.WriteLine(new IntPtr(actor));
                // Console.WriteLine(actor[0].x);
                // Console.WriteLine(actor[0].y);
                // Console.WriteLine(actor[1].x);
                // Console.WriteLine(actor[1].y);
            }
            public void ExecuteMe2(ref Position position, in Velocity velocity)
            {
                //Console.WriteLine(new IntPtr(actor));
                Console.WriteLine(position.X);
                Console.WriteLine(position.Y);
                Console.WriteLine(velocity.X);
                Console.WriteLine(velocity.Y);
            }


        }


        static void Main(string[] args)
        {
            _logFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = _logFactory.CreateLogger("Sandbox");


            var sys = new SystemTest();
            sys.Execute();
            return;


            var memory = new HeapAllocator(_logFactory);
            var em = new EntityManager(_logFactory, memory);

            var spec = new EntitySpec(ComponentType<Position>.Type, ComponentType<Velocity>.Type);
            var e = em.Create(spec);

            em.Replace(e, new Velocity(1, -1));

            var sm = new SystemManager(_logFactory);
            sm.Variable("dt", 0.16f);

            var vs = new VelocitySystemProcessor();
            vs.Tick(sm, em);

            var p = em.Get<Position>(e);
            Console.WriteLine(p);

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
