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

    class Program
    {

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
            HeapAllocationShouldFillGap();
            // var cl = new ComponentList();
            // var em = new EntityManager(cl);

            // var type = em.CreateArchetype(typeof(Test));
            // var e = em.CreateEntity(type);

            // for (var y = 0; y < 3; y++)
            // {
            //     var sw = Stopwatch.StartNew();
            //     var em = new EntityManager();
            //     var spec = new EntitySpec(
            //         ComponentType<Position>.Type
            //     );

            //     for (var i = 0; i < 50000; i++)
            //         em.Create(spec);


            //     var counter = 0;
            //     for (var x = 0; x < 10000; x++)
            //     {
            //         em.ForEach((uint entity, ref Position p) =>
            //         {
            //             counter++;
            //             p.X = 10;
            //             p.Y = 10;
            //         });
            //     }

            //     Console.WriteLine(counter);
            //     Console.WriteLine(sw.Elapsed.ToString());
            // }
        }

        public static unsafe void HeapAllocationShouldFillGap()
        {
            //arrange
            var blocks = 256;
            var size = blocks * HeapAllocation.HeapSize;
            var memory = stackalloc HeapAllocation[blocks];
            var span = new System.Span<HeapAllocation>(memory, blocks);
            *memory = new HeapAllocation(size);

            //act
            HeapAllocation.Split(memory, 1); //255
            HeapAllocation.Split(&memory[2], 1); //252
            HeapAllocation.Split(&memory[4], 1); //249

            HeapAllocation.Free(&memory[2]);

            //assert
            Console.WriteLine("**assert**");
            Console.WriteLine(span[0]);
            Console.WriteLine(span[2]);
            Console.WriteLine(span[4]);

            span[0].Blocks.ShouldBe(1u);
            span[2].Blocks.ShouldBe(1u);

            span[4].Blocks.ShouldBe(1u);
            span[6].Blocks.ShouldBe((uint)blocks - 7);

            span[0].SizeInBytes.ShouldBe((uint)HeapAllocation.HeapSize);
            span[2].SizeInBytes.ShouldBe((uint)HeapAllocation.HeapSize);
            span[4].SizeInBytes.ShouldBe((uint)HeapAllocation.HeapSize);
            span[6].SizeInBytes.ShouldBe((uint)(blocks - 7) * HeapAllocation.HeapSize);

            HeapAllocation.CountFreeBlocks(memory).ShouldBe((uint)blocks - 6u);
            HeapAllocation.CountUsedBlocks(memory, out var allocations).ShouldBe((uint)6u);
            allocations.ShouldBe(2);

            Assert(span[0].Previous == null);
            Assert(span[2].Previous == &memory[0]);
            Assert(span[4].Previous == &memory[2]);
            Assert(span[6].Previous == &memory[4]);

            Assert(span[0].Next == &memory[2]);
            Assert(span[2].Next == &memory[4]);
            Assert(span[4].Next == &memory[6]);
            Assert(span[6].Next == null);

            span[0].Flags.ShouldBe(1u);
            span[2].Flags.ShouldBe(0u);
            span[4].Flags.ShouldBe(1u);
            span[6].Flags.ShouldBe(0u);

            //act2
            HeapAllocation.Free(&memory[0]);

            //assert2
            Console.WriteLine("**assert2**");
            Console.WriteLine(span[0]);
            Console.WriteLine(span[2]);
            Console.WriteLine(span[4]);

            span[0].Blocks.ShouldBe(3u);
            span[2].Blocks.ShouldBe(0u);
            span[4].Blocks.ShouldBe(1u);
            span[6].Blocks.ShouldBe((uint)blocks - 7);

            span[0].SizeInBytes.ShouldBe((uint)HeapAllocation.HeapSize * 3);
            span[2].SizeInBytes.ShouldBe(0u);
            span[4].SizeInBytes.ShouldBe((uint)HeapAllocation.HeapSize);
            span[6].SizeInBytes.ShouldBe((uint)(blocks - 7) * HeapAllocation.HeapSize);

            HeapAllocation.CountFreeBlocks(memory).ShouldBe((uint)blocks - 4u);
            HeapAllocation.CountUsedBlocks(memory, out allocations).ShouldBe((uint)4u);
            allocations.ShouldBe(1);

            Assert(span[0].Previous == null);
            Assert(span[2].Previous == null);
            Assert(span[4].Previous == &memory[0]);
            Assert(span[6].Previous == &memory[4]);

            Assert(span[0].Next == &memory[4]);
            Assert(span[2].Next == null);
            Assert(span[4].Next == &memory[6]);
            Assert(span[6].Next == null);

            span[0].Flags.ShouldBe(0u);
            span[2].Flags.ShouldBe(0u);
            span[4].Flags.ShouldBe(1u);
            span[6].Flags.ShouldBe(0u);


            //act3
            HeapAllocation.Free(&memory[4]);

            //assert3
            Console.WriteLine("**assert3**");
            Console.WriteLine(span[0]);
            Console.WriteLine(span[2]);
            Console.WriteLine(span[4]);

            span[0].Blocks.ShouldBe((uint)(blocks - 1));
            span[2].Blocks.ShouldBe(0u);
            span[4].Blocks.ShouldBe(0u);
            span[6].Blocks.ShouldBe(0u);

            span[0].SizeInBytes.ShouldBe((uint)(HeapAllocation.HeapSize * (blocks - 1)));
            span[2].SizeInBytes.ShouldBe(0u);
            span[4].SizeInBytes.ShouldBe(0u);
            span[6].SizeInBytes.ShouldBe(0u);

            HeapAllocation.CountFreeBlocks(memory).ShouldBe((uint)(blocks - 1u));
            HeapAllocation.CountUsedBlocks(memory, out allocations).ShouldBe(1u);
            allocations.ShouldBe(0);

            Assert(span[0].Previous == null);
            Assert(span[2].Previous == null);
            Assert(span[4].Previous == null);
            Assert(span[6].Previous == null);

            Assert(span[0].Next == null);
            Assert(span[2].Next == null);
            Assert(span[4].Next == null);
            Assert(span[6].Next == null);

            span[0].Flags.ShouldBe(0u);
            span[2].Flags.ShouldBe(0u);
            span[4].Flags.ShouldBe(0u);
            span[6].Flags.ShouldBe(0u);

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
