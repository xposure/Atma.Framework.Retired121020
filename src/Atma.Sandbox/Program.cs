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
    using System.Reflection;

    public interface IDummyFace
    {

    }
    public static class AutoWire
    {
        public delegate void WireUp(IDummyFace face, string name, Delegate o);

        public static void Subscribe<T>(this IDummyFace it, string name, Action<T> callback)
        {

        }

        public static WireUp Build(MethodInfo _method)
        {
            //TODO: we don't need the method, just the type argues
            //TODO: Need to store the funcs in the dictionary with ID based on hashid of types
            var autoType = typeof(AutoWire);
            var autoMethod = autoType.GetMethods(BindingFlags.Static | BindingFlags.Public).First(x => x.Name == "Subscribe" && x.GetGenericArguments().Length == 1);

            var genericMethod = autoMethod.MakeGenericMethod(_method.GetParameters().Select(x => x.ParameterType).ToArray());

            var parms = _method.GetParameters();
            var method = new DynamicMethod(_method.ToString(), null, new Type[] { typeof(IDummyFace), typeof(string), typeof(Delegate) });

            var gen = method.GetILGenerator();

            //call method
            gen.Emit(OpCodes.Ldarg_0);
            //gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldarg_2);
            gen.Emit(OpCodes.Call, genericMethod);

            gen.Emit(OpCodes.Ret);

            return (WireUp)method.CreateDelegate(typeof(WireUp));
        }
    }
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

        public class DummyFace : IDummyFace
        {

        }


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

        static unsafe AllocationHandle Take(IAllocator memory, int size)
        {
            var handle = memory.Take(size);
            var addr = (byte*)handle.Address;
            for (var k = 0; k < size; k++)
                addr[k] = 0xfe;
            return handle;
        }

        static unsafe void Verify(in AllocationHandle handle, int size)
        {
            var addr = (byte*)handle.Address;
            for (var k = 0; k < size; k++)
                if (addr[k] != 0xfe)
                    throw new Exception($"{handle}[{k}] was {addr[k].ToString("X2")}");
        }

        public class TestWire
        {
            public void Hello(float dt)
            {
                System.Console.WriteLine(dt);
            }

        }

        static unsafe void Main(string[] args)
        {

            var df = new DummyFace();
            var tw = new TestWire();
            var mi = typeof(TestWire).GetMethod("Hello");
            var dg = mi.CreateDelegate(typeof(Action<float>), tw);
            var wireUp = AutoWire.Build(mi);
            wireUp(df, "Hello", dg);

            _logFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = _logFactory.CreateLogger("Sandbox");

            var memory = new HeapAllocator(_logFactory);

            var handle_50331651_48 = Take(memory, 9150);
            memory.Validate("1");

            memory.Free(ref handle_50331651_48);
            memory.Validate("2");

            var handle_67108866_64 = Take(memory, 22);
            memory.Validate("5");

            return;

            var r = new Random();
            var handles = new List<AllocationHandle>();
            var sizes = new List<int>();
            for (var i = 0; i < 10000; i++)
            {
                var action = r.Next(2);
                if (action == 0)
                {
                    var size = 10;
                    var sizeType = r.Next(0, 3);
                    if (sizeType == 0)
                        size = r.Next(10, 32);
                    else if (sizeType == 1)
                        size = r.Next(100, 10000);
                    else if (sizeType == 2)
                        size = r.Next(10000, 100000);

                    var handle = memory.Take(size);

                    handles.Add(handle);
                    sizes.Add(size);
                    var addr = (byte*)handle.Address;
                    for (var k = 0; k < size; k++)
                        addr[k] = 0xfe;
                }
                else if (action == 1 && handles.Count > 0)
                {
                    var index = r.Next(handles.Count);
                    var handle = handles[index];
                    var size = sizes[index];

                    handles.RemoveFast(index);
                    sizes.RemoveFast(index);

                    var addr = (byte*)handle.Address;
                    for (var k = 0; k < size; k++)
                        if (addr[k] != 0xfe)
                            throw new Exception();

                    memory.Free(ref handle);
                }
            }


            return;

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
