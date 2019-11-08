namespace Atma.Sandbox
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Atma.Entities;
    using Shouldly;
    using System.Diagnostics;

    class Program
    {
        public struct Test
        {
            public float x;
            public float y;
        }

        private struct Position
        {
            public int X;
            public int Y;

            public Position(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        private struct Velocity
        {
            public int VX;
            public int VY;

            public Velocity(int vx, int vy)
            {
                VX = vx;
                VY = vy;
            }
        }

        static void Main(string[] args)
        {
            // var cl = new ComponentList();
            // var em = new EntityManager(cl);

            // var type = em.CreateArchetype(typeof(Test));
            // var e = em.CreateEntity(type);

            for (var y = 0; y < 3; y++)
            {
                var sw = Stopwatch.StartNew();
                var em = new EntityManager2();
                var spec = new EntitySpec(
                    ComponentType<Position>.Type
                );

                for (var i = 0; i < 50000; i++)
                    em.Create(spec);


                var counter = 0;
                for (var x = 0; x < 10000; x++)
                {
                    em.ForEach((uint entity, ref Position p) =>
                    {
                        counter++;
                        p.X = 10;
                        p.Y = 10;
                    });
                }

                Console.WriteLine(counter);
                Console.WriteLine(sw.Elapsed.ToString());
            }
        }


        public static void ShouldMoveEntity()
        {
            //arrange
            using var em = new EntityManager2();
            var srcSpec = new EntitySpec(ComponentType<Position>.Type);
            var dstSpec = new EntitySpec(ComponentType<Position>.Type, ComponentType<Velocity>.Type);

            //act
            var id0 = em.Create(srcSpec);
            em.Replace(id0, new Position(20, 10));

            var id1 = em.Create(srcSpec);
            em.Replace(id1, new Position(10, 20));

            em.Move(id0, dstSpec);

            //assert
            var p0 = em.Get<Position>(id0);
            p0.X.ShouldBe(20);
            p0.Y.ShouldBe(10);

            var p1 = em.Get<Position>(id1);
            p0.X.ShouldBe(10);
            p0.Y.ShouldBe(20);

            em.EntityArrays.Count.ShouldBe(2);
            em.EntityArrays[0].EntityCount.ShouldBe(1);
            em.EntityArrays[1].EntityCount.ShouldBe(1);
            em.EntityCount.ShouldBe(2);
        }


    }

    // public unsafe struct ValidPtr
    // {
    //     public Valid* Valid;

    //     public override string ToString()
    //     {
    //         return Valid->ToString();
    //     }
    // }

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
