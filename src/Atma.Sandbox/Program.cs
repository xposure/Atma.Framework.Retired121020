namespace Atma.Sandbox
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Atma.Entities;
    using Shouldly;

    class Program
    {
        public struct Test
        {
            public float x;
            public float y;
        }

        static void Main(string[] args)
        {
            // var cl = new ComponentList();
            // var em = new EntityManager(cl);

            // var type = em.CreateArchetype(typeof(Test));
            // var e = em.CreateEntity(type);

            SpecShouldFindMatches();

            Console.WriteLine("Hello World!");
        }


        public static void SpecShouldFindMatches()
        {
            var specs = new[]
            {
                new EntitySpec(ComponentType<Valid>.Type, ComponentType<Valid2>.Type, ComponentType<Valid6>.Type, ComponentType<Valid5>.Type),
                new EntitySpec(ComponentType<Valid4>.Type,ComponentType<Valid2>.Type,ComponentType<Valid3>.Type, ComponentType<Valid5>.Type),
                new EntitySpec(ComponentType<Valid6>.Type, ComponentType<Valid3>.Type, ComponentType<Valid>.Type, ComponentType<Valid4>.Type)
            };

            Span<ComponentType> componentTypes0 = stackalloc ComponentType[8];
            var c0 = specs[0].FindMatches(specs[1], componentTypes0);
            var m0 = new List<ComponentType>(componentTypes0.Slice(0, c0).ToArray());
            var componentTypes1 = new ComponentType[8];
            //var c1 = specs[1].FindMatches(specs[2], componentTypes1.AsSpan());
            var m1 = new List<ComponentType>(componentTypes1);// new List<ComponentType>(componentTypes1.AsSpan().Slice(c1).ToArray()).ToList();
            var componentTypes2 = new ComponentType[8];
            //var c2 = specs[2].FindMatches(specs[0], componentTypes2.AsSpan());
            var m2 = new List<ComponentType>(componentTypes2);// new List<ComponentType>(componentTypes2.AsSpan().Slice(c2).ToArray()).ToList();

            var valid2 = ComponentType<Valid2>.Type.ID;
            m0.Any(x => x.ID == ComponentType<Valid2>.Type.ID).ShouldBe(true);
            //`m0.ShouldContain(x => x.ID == (() => valid2)());
            // m0.ShouldContain(x => x.ID == ComponentType<Valid5>.Type.ID);
            // m0.ShouldNotContain(x => x.ID == ComponentType<Valid>.Type.ID);
            // m0.ShouldNotContain(x => x.ID == ComponentType<Valid3>.Type.ID);
            // m0.ShouldNotContain(x => x.ID == ComponentType<Valid6>.Type.ID);

            // m1.ShouldContain(x => x.ID == ComponentType<Valid4>.Type.ID);
            // m1.ShouldContain(x => x.ID == ComponentType<Valid3>.Type.ID);
            // m1.ShouldNotContain(x => x.ID == ComponentType<Valid>.Type.ID);
            // m1.ShouldNotContain(x => x.ID == ComponentType<Valid6>.Type.ID);
            // m1.ShouldNotContain(x => x.ID == ComponentType<Valid2>.Type.ID);

            // m2.ShouldContain(x => x.ID == ComponentType<Valid6>.Type.ID);
            // m2.ShouldContain(x => x.ID == ComponentType<Valid>.Type.ID);
            // m2.ShouldNotContain(x => x.ID == ComponentType<Valid3>.Type.ID);
            // m2.ShouldNotContain(x => x.ID == ComponentType<Valid4>.Type.ID);
            // m2.ShouldNotContain(x => x.ID == ComponentType<Valid2>.Type.ID);

        }

        private static EntityManager GetEntityManager()
        {
            var componentList = new ComponentList();
            componentList.AddComponent<Valid>();
            componentList.AddComponent<Valid2>();
            componentList.AddComponent<Valid3>();
            componentList.AddComponent<Valid4>();
            componentList.AddComponent<Valid5>();
            componentList.AddComponent<Valid6>();
            return new EntityManager(componentList);
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
