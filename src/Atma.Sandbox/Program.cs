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

        public struct Position
        {
            public float X;
            public float Y;

            public Position(float x, float y)
            {
                X = x;
                Y = y;
            }
        }

        public struct Velocity
        {
            public float VX;
            public float VY;

            public Velocity(float vx, float vy)
            {
                VX = vx;
                VY = vy;
            }
        }

        public interface ISystemProcessor
        {

        }

        public struct Test //: ISystemProcessor
        {
            // public readonly ReadOnlySpan<EntityRef> entities;
            // public readonly Span<Position> positions;
            // public readonly ReadOnlySpan<Velocity> velocities;

            public void Execute(int length)
            {
                Console.WriteLine("hello");
            }
        }

        public readonly ref partial struct Test2 //: ISystemProcessor
        {
            public readonly ReadOnlySpan<EntityRef> entities;
            public readonly Span<Position> positions;
            public readonly ReadOnlySpan<Velocity> velocities;

            partial void Execute(int length);
        }

        public readonly ref partial struct Test2
        {
            partial void Execute(int length)
            {
                Console.WriteLine("Hello world!");
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

            var type = typeof(Test);

            type.GetConstructors().WriteLine();


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
