namespace Atma.Sandbox
{
    using System;
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

            EntitySetComponent();

            Console.WriteLine("Hello World!");
        }
        public static void EntitySetComponent()
        {
            var _manager = GetEntityManager();
            var archetype = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3));
            var entity1 = _manager.CreateEntity(archetype);
            var entity2 = _manager.CreateEntity(archetype);

            var setComponentData = new Valid() { X = 1, Y = 2 };
            _manager.SetComponentData(entity2, setComponentData);

            var getComponentData = _manager.GetComponentData<Valid>(entity2);

            getComponentData.X.ShouldBe(setComponentData.X);
            getComponentData.Y.ShouldBe(setComponentData.Y);

            _manager.DestroyEntity(entity1);

            //check if the data moves
            getComponentData = _manager.GetComponentData<Valid>(entity2);

            getComponentData.X.ShouldBe(setComponentData.X);
            getComponentData.Y.ShouldBe(setComponentData.Y);

            //data is cleared on delete, not sure if this will change in the future
            entity1 = _manager.CreateEntity(archetype);
            getComponentData = _manager.GetComponentData<Valid>(entity1);

            getComponentData.X.ShouldBe(0);
            getComponentData.Y.ShouldBe(0);
        }

        public static void BulkCreateArray()
        {
            var _manager = GetEntityManager();
            var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid5));

            _manager.CreateEntity(archetype0, 8192, out var entities);
            for (var i = 0; i < entities.Length; i++)
                entities[i].ShouldBe(i + 1);
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
