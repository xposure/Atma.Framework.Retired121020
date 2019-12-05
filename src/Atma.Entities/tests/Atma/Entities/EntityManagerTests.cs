namespace Atma.Entities
{
    using Atma.Common;
    using Atma.Memory;
    using Divergic.Logging.Xunit;
    using Microsoft.Extensions.Logging;
    using Shouldly;

    using System;
    using System.Linq;
    using Xunit;
    using Xunit.Abstractions;

    public class EntityManagerTests
    {
        [System.Diagnostics.DebuggerStepThrough]
        private struct Position
        {
            public int X;
            public int Y;

            public Position(int v)
            {
                X = v;
                Y = v;
            }
            public Position(int x, int y)
            {
                X = x;
                Y = y;
            }

            public override string ToString() => $"X:{X}, Y:{Y}";
        }

        [System.Diagnostics.DebuggerStepThrough]
        private struct Velocity
        {
            public int VX;
            public int VY;

            public Velocity(int v)
            {
                VX = v;
                VY = v;
            }
            public Velocity(int vx, int vy)
            {
                VX = vx;
                VY = vy;
            }
            public override string ToString() => $"VX:{VX}, VY:{VY}";
        }

        private readonly ILoggerFactory _logFactory;

        public EntityManagerTests(ITestOutputHelper output)
        {
            _logFactory = LogFactory.Create(output);
        }

        // [Fact]
        // public void ShouldGetOrCreateSpecByEntitySpec()
        // {
        //     using var memory = new HeapAllocator(_logFactory);
        //     using var entities = new EntityManager(_logFactory, memory);

        //     Span<ComponentType> c0 = stackalloc[] { ComponentType<Position>.Type };
        //     Span<ComponentType> c1 = stackalloc[] { ComponentType<Velocity>.Type };
        //     Span<ComponentType> c2 = stackalloc[] { ComponentType<Velocity>.Type, ComponentType<Position>.Type };
        //     Span<ComponentType> c3 = stackalloc[] { ComponentType<Position>.Type, ComponentType<Velocity>.Type };

        //     var si0 = entities.GetOrCreateSpec(new EntitySpec(c0));
        //     var si1 = entities.GetOrCreateSpec(new EntitySpec(c1));
        //     var si2 = entities.GetOrCreateSpec(new EntitySpec(c2));
        //     var si3 = entities.GetOrCreateSpec(new EntitySpec(c3));
        //     var si4 = entities.GetOrCreateSpec(new EntitySpec(c0));
        //     var si5 = entities.GetOrCreateSpec(new EntitySpec(c1));

        //     si0.ShouldBe(0);
        //     si1.ShouldBe(1);
        //     si2.ShouldBe(2);
        //     si3.ShouldBe(2);
        //     si4.ShouldBe(0);
        //     si5.ShouldBe(1);
        // }

        // [Fact]
        // public void ShouldGetOrCreateSpecByComponentTypes()
        // {
        //     using var memory = new HeapAllocator(_logFactory);
        //     using var entities = new EntityManager(_logFactory, memory);

        //     Span<ComponentType> c0 = stackalloc[] { ComponentType<Position>.Type };
        //     Span<ComponentType> c1 = stackalloc[] { ComponentType<Velocity>.Type };
        //     Span<ComponentType> c2 = stackalloc[] { ComponentType<Velocity>.Type, ComponentType<Position>.Type };
        //     Span<ComponentType> c3 = stackalloc[] { ComponentType<Position>.Type, ComponentType<Velocity>.Type };

        //     var si0 = entities.GetOrCreateSpec(c0);
        //     var si1 = entities.GetOrCreateSpec(c1);
        //     var si2 = entities.GetOrCreateSpec(c2);
        //     var si3 = entities.GetOrCreateSpec(c3);
        //     var si4 = entities.GetOrCreateSpec(c0);
        //     var si5 = entities.GetOrCreateSpec(c1);

        //     si0.ShouldBe(0);
        //     si1.ShouldBe(1);
        //     si2.ShouldBe(2);
        //     si3.ShouldBe(2);
        //     si4.ShouldBe(0);
        //     si5.ShouldBe(1);
        // }

        [Fact]
        public void ShouldCreateOne()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec = EntitySpec.Create<Position, Velocity>();
            var id0 = entities.Create(spec);
            var id1 = entities.Create(spec.ComponentTypes);

            id0.ShouldNotBe(0u);
            id1.ShouldNotBe(0u);
            id0.ShouldNotBe(id1);

            entities.EntityCount.ShouldBe(2);
            entities.EntityArrays[0].EntityCount.ShouldBe(2);
        }

        [Fact]
        public void ShouldCreateWithGroupedData()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var speca = EntitySpec.Create<Position, Velocity>(new GroupA() { HashCode = 1 });
            var specb = EntitySpec.Create<Position, Velocity>(new GroupA() { HashCode = 2 });
            var id0 = entities.Create(speca);
            var id1 = entities.Create(specb);

            id0.ShouldNotBe(0u);
            id1.ShouldNotBe(0u);
            id0.ShouldNotBe(id1);

            entities.EntityCount.ShouldBe(2);
            entities.EntityArrays.Count.ShouldBe(2);
            entities.EntityArrays[0].EntityCount.ShouldBe(1);
            entities.EntityArrays[1].EntityCount.ShouldBe(1);

            entities.GetGroupData<GroupA>(id0).HashCode.ShouldBe(1);
            entities.GetGroupData<GroupA>(id1).HashCode.ShouldBe(2);
        }

        [Fact]
        public void ShouldMoveEntityWithGroupedData()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var speca = EntitySpec.Create<Position, Velocity>(new GroupA() { HashCode = 1 });
            var specb = EntitySpec.Create<Position, Velocity>(new GroupA() { HashCode = 2 });
            var id0 = entities.Create(speca);
            var id1 = entities.Create(specb);

            id0.ShouldNotBe(0u);
            id1.ShouldNotBe(0u);
            id0.ShouldNotBe(id1);

            var newSpec = entities.SetGroupData(id0, new GroupA() { HashCode = 2 });

            newSpec.ID.ShouldBe(specb.ID);

            entities.EntityCount.ShouldBe(2);
            entities.EntityArrays.Count.ShouldBe(2);
            entities.EntityArrays[0].EntityCount.ShouldBe(0);
            entities.EntityArrays[1].EntityCount.ShouldBe(2);

            entities.GetGroupData<GroupA>(id0).HashCode.ShouldBe(2);
            entities.GetGroupData<GroupA>(id1).HashCode.ShouldBe(2);
        }

        [Fact]
        public void ShouldCreateBySpan()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec = EntitySpec.Create<Position, Velocity>();

            var ids = Enumerable.Range(0, 8192).Select(x => 0u).ToArray();
            entities.Create(spec, ids);

            ids.ShouldNotContain(0u);
            ids.ShouldBeUnique();

            entities.EntityCount.ShouldBe(8192);
            entities.EntityArrays[0].EntityCount.ShouldBe(8192);
        }


        [Fact]
        public void ShouldAssignOne()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec0 = EntitySpec.Create<Position>();
            //var spec1 = EntitySpec.Create<Position, Velocity>();

            var id0 = entities.Create(spec0);
            entities.Replace(id0, new Position(20));
            //var id1 = entities.Create(spec1);

            entities.Assign<Velocity>(id0, new Velocity(10));

            entities.EntityCount.ShouldBe(1);
            entities.EntityArrays.Count.ShouldBe(2);
            entities.EntityArrays[0].EntityCount.ShouldBe(0);
            entities.EntityArrays[1].EntityCount.ShouldBe(1);
            entities.Get<Position>(id0).ShouldBe(new Position(20));
            entities.Get<Velocity>(id0).ShouldBe(new Velocity(10));
        }

        [Fact]
        public void ShouldAssignManyWithOneValue()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec = EntitySpec.Create<Position>();

            var ids = Enumerable.Range(0, 8192).Select(x => 0u).ToArray();
            entities.Create(spec, ids);


            entities.Assign<Velocity>(ids, new Velocity(10));

            entities.EntityCount.ShouldBe(8192);
            entities.EntityArrays.Count.ShouldBe(2);
            entities.EntityArrays[0].EntityCount.ShouldBe(0);
            entities.EntityArrays[1].EntityCount.ShouldBe(8192);

            for (var i = 0; i < ids.Length; i++)
                entities.Get<Velocity>(ids[i]).ShouldBe(new Velocity(10));
        }

        [Fact]
        public void ShouldAssignManyWithManyValues()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec = EntitySpec.Create<Position>();

            var ids = Enumerable.Range(0, 8192).Select(x => 0u).ToArray();
            entities.Create(spec, ids);

            var velocities = Enumerable.Range(0, ids.Length).Select(x => new Velocity(x)).ToArray();
            using var array = new NativeArray<Velocity>(memory, ids.Length);
            velocities.CopyTo(array);

            entities.Assign<Velocity>(ids, array);

            entities.EntityCount.ShouldBe(8192);
            entities.EntityArrays.Count.ShouldBe(2);
            entities.EntityArrays[0].EntityCount.ShouldBe(0);
            entities.EntityArrays[1].EntityCount.ShouldBe(8192);

            for (var i = 0; i < ids.Length; i++)
                entities.Get<Velocity>(ids[i]).ShouldBe(velocities[i]);
        }

        [Fact]
        public void ShouldRemoveOneEntity()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec = EntitySpec.Create<Position, Velocity>();
            var id = entities.Create(spec);
            entities.Delete(id);

            id.ShouldNotBe(0u);

            entities.EntityCount.ShouldBe(0);
            entities.EntityArrays[0].EntityCount.ShouldBe(0);
            entities.EntityArrays.Count.ShouldBe(1);
        }

        [Fact]
        public void ShouldRemoveEntitiesBySpan()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec = EntitySpec.Create<Position, Velocity>();

            var ids = Enumerable.Range(0, 8192).Select(x => 0u).ToArray();
            entities.Create(spec, ids);

            var removeIds = Enumerable.Range(1024, 1024).Select(x => ids[x]).ToArray();
            entities.Delete(removeIds);

            entities.EntityCount.ShouldBe(8192 - 1024);
            entities.EntityArrays[0].EntityCount.ShouldBe(8192 - 1024);
            for (var i = 0; i < ids.Length; i++)
                entities.Has(ids[i]).ShouldBe((i >= 1024 && i < 2048) ? false : true);
        }

        [Fact]
        public void ShouldRemoveComponent()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec = EntitySpec.Create<Position, Velocity>();
            var id = entities.Create(spec);

            entities.Remove<Velocity>(id);

            entities.EntityCount.ShouldBe(1);
            entities.EntityArrays[0].EntityCount.ShouldBe(0);
            entities.EntityArrays[1].EntityCount.ShouldBe(1);
        }

        [Fact]
        public void ShouldRemoveComponentBySpan()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec = EntitySpec.Create<Position, Velocity>();

            var ids = Enumerable.Range(0, 8192).Select(x => 0u).ToArray();
            entities.Create(spec, ids);

            var removeIds = Enumerable.Range(1024, 1024).Select(x => ids[x]).ToArray();
            entities.Remove<Velocity>(removeIds);

            entities.EntityCount.ShouldBe(8192);
            entities.EntityArrays[0].EntityCount.ShouldBe(8192 - 1024);
            entities.EntityArrays[1].EntityCount.ShouldBe(1024);
            for (var i = 0; i < ids.Length; i++)
                entities.Has<Velocity>(ids[i]).ShouldBe((i >= 1024 && i < 2048) ? false : true);
        }


        [Fact]
        public void ShouldReplaceOne()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec0 = EntitySpec.Create<Position>();
            //var spec1 = EntitySpec.Create<Position, Velocity>();

            var id0 = entities.Create(spec0);
            entities.Replace(id0, new Position(20));

            entities.EntityCount.ShouldBe(1);
            entities.EntityArrays.Count.ShouldBe(1);
            entities.EntityArrays[0].EntityCount.ShouldBe(1);
            entities.Get<Position>(id0).ShouldBe(new Position(20));
        }

        [Fact]
        public void ShouldReplaceManyWithOneValue()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec = EntitySpec.Create<Position>();

            var ids = Enumerable.Range(0, 8192).Select(x => 0u).ToArray();
            entities.Create(spec, ids);

            entities.Replace<Position>(ids, new Position(10));

            entities.EntityCount.ShouldBe(8192);
            entities.EntityArrays.Count.ShouldBe(1);
            entities.EntityArrays[0].EntityCount.ShouldBe(8192);

            for (var i = 0; i < ids.Length; i++)
                entities.Get<Position>(ids[i]).ShouldBe(new Position(10));
        }

        [Fact]
        public void ShouldReplaceManyWithManyValues()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec = EntitySpec.Create<Position>();

            var ids = Enumerable.Range(0, 8192).Select(x => 0u).ToArray();
            entities.Create(spec, ids);

            var positions = Enumerable.Range(0, ids.Length).Select(x => new Position(x)).ToArray();
            using var array = new NativeArray<Position>(memory, ids.Length);
            positions.CopyTo(array);

            entities.Replace<Position>(ids, array);

            entities.EntityCount.ShouldBe(8192);
            entities.EntityArrays.Count.ShouldBe(1);
            entities.EntityArrays[0].EntityCount.ShouldBe(8192);

            for (var i = 0; i < ids.Length; i++)
                entities.Get<Position>(ids[i]).ShouldBe(positions[i]);
        }

        [Fact]
        public void ShouldUpdateOne()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec0 = EntitySpec.Create<Position>();
            var spec1 = EntitySpec.Create<Position, Velocity>();

            var id0 = entities.Create(spec0);
            var id1 = entities.Create(spec1);
            entities.Update(id0, new Velocity(10));
            entities.Update(id1, new Velocity(20));

            entities.EntityCount.ShouldBe(2);
            entities.EntityArrays.Count.ShouldBe(2);
            entities.EntityArrays[0].EntityCount.ShouldBe(0);
            entities.EntityArrays[1].EntityCount.ShouldBe(2);
            entities.Get<Velocity>(id0).ShouldBe(new Velocity(10));
            entities.Get<Velocity>(id1).ShouldBe(new Velocity(20));

        }

        [Fact]
        public void ShouldUpdateManyWithOneValue()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec0 = EntitySpec.Create<Position>();
            var spec1 = EntitySpec.Create<Position, Velocity>();

            var ids0 = Enumerable.Range(0, 4096).Select(x => 0u).ToArray();
            var ids1 = Enumerable.Range(0, 4096).Select(x => 0u).ToArray();
            entities.Create(spec0, ids0);
            entities.Create(spec1, ids1);

            entities.Update<Velocity>(ids0, new Velocity(10));
            entities.Update<Velocity>(ids1, new Velocity(20));

            entities.EntityCount.ShouldBe(8192);
            entities.EntityArrays.Count.ShouldBe(2);
            entities.EntityArrays[0].EntityCount.ShouldBe(0);
            entities.EntityArrays[1].EntityCount.ShouldBe(8192);

            for (var i = 0; i < ids0.Length; i++)
                entities.Get<Velocity>(ids0[i]).ShouldBe(new Velocity(10));
            for (var i = 0; i < ids1.Length; i++)
                entities.Get<Velocity>(ids1[i]).ShouldBe(new Velocity(20));
        }

        [Fact]
        public void ShouldUpdateManyWithManyValues()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec0 = EntitySpec.Create<Position>();
            var spec1 = EntitySpec.Create<Position, Velocity>();

            var ids0 = Enumerable.Range(0, 4096).Select(x => 0u).ToArray();
            var ids1 = Enumerable.Range(0, 4096).Select(x => 0u).ToArray();
            entities.Create(spec0, ids0);
            entities.Create(spec1, ids1);

            var velocities0 = Enumerable.Range(0, ids0.Length).Select(x => new Velocity(x)).ToArray();
            var velocities1 = Enumerable.Range(0, ids1.Length).Select(x => new Velocity(x + 4096)).ToArray();
            using var array0 = new NativeArray<Velocity>(memory, ids0.Length);
            using var array1 = new NativeArray<Velocity>(memory, ids1.Length);
            velocities0.CopyTo(array0);
            velocities1.CopyTo(array1);

            entities.Update<Velocity>(ids0, array0);
            entities.Update<Velocity>(ids1, array1);

            entities.EntityCount.ShouldBe(8192);
            entities.EntityArrays.Count.ShouldBe(2);
            entities.EntityArrays[0].EntityCount.ShouldBe(0);
            entities.EntityArrays[1].EntityCount.ShouldBe(8192);

            for (var i = 0; i < ids0.Length; i++)
                entities.Get<Velocity>(ids0[i]).ShouldBe(velocities0[i]);
            for (var i = 0; i < ids1.Length; i++)
                entities.Get<Velocity>(ids1[i]).ShouldBe(velocities1[i]);
        }

        [Fact]
        public void ShouldUpdateManyWithManyValuesInterleaved()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec0 = EntitySpec.Create<Position>();
            var spec1 = EntitySpec.Create<Position, Velocity>();

            var ids0 = Enumerable.Range(0, 4096).Select(x => 0u).ToArray();
            var ids1 = Enumerable.Range(0, 4096).Select(x => 0u).ToArray();
            entities.Create(spec0, ids0);
            entities.Create(spec1, ids1);

            var velocities0 = Enumerable.Range(0, ids0.Length).Select(x => new Velocity(x)).ToArray();
            var velocities1 = Enumerable.Range(0, ids1.Length).Select(x => new Velocity(x + 4096)).ToArray();
            using var array = new NativeArray<Velocity>(memory, ids0.Length + ids1.Length);
            using var ids = new NativeArray<uint>(memory, ids0.Length + ids1.Length);
            var index0 = 0;
            var index1 = 0;
            for (var i = 0; i < array.Length; i++)
            {
                if ((i % 2) == 0)
                {
                    ids[i] = ids0[index0];
                    array[i] = velocities0[index0++];
                }
                else
                {
                    ids[i] = ids1[index1];
                    array[i] = velocities1[index1++];
                }
            }

            entities.Update<Velocity>(ids, array);

            entities.EntityCount.ShouldBe(8192);
            entities.EntityArrays.Count.ShouldBe(2);
            entities.EntityArrays[0].EntityCount.ShouldBe(0);
            entities.EntityArrays[1].EntityCount.ShouldBe(8192);

            for (var i = 0; i < ids0.Length; i++)
                entities.Get<Velocity>(ids0[i]).ShouldBe(velocities0[i]);
            for (var i = 0; i < ids1.Length; i++)
                entities.Get<Velocity>(ids1[i]).ShouldBe(velocities1[i]);
        }

        [Fact]
        public void ShouldResetOneEntity()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec0 = EntitySpec.Create<Position, Velocity>();

            var id0 = entities.Create(spec0);
            entities.Replace(id0, new Position(20));
            entities.Replace(id0, new Velocity(10));
            entities.Reset(id0);

            entities.EntityCount.ShouldBe(1);
            entities.EntityArrays.Count.ShouldBe(1);
            entities.EntityArrays[0].EntityCount.ShouldBe(1);
            entities.Get<Position>(id0).ShouldBe(new Position(0));
            entities.Get<Velocity>(id0).ShouldBe(new Velocity(0));
        }

        [Fact]
        public void ShouldResetManyEntities()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec = EntitySpec.Create<Position, Velocity>();

            var ids = Enumerable.Range(0, 16).Select(x => 0u).ToArray();
            entities.Create(spec, ids);

            for (var i = 0; i < ids.Length; i++)
            {
                entities.Replace<Position>(ids, new Position(10 * i));
                entities.Replace<Velocity>(ids, new Velocity(20 * i));
            }

            entities.Reset(ids);

            entities.EntityCount.ShouldBe(16);
            entities.EntityArrays.Count.ShouldBe(1);
            entities.EntityArrays[0].EntityCount.ShouldBe(16);

            for (var i = 0; i < ids.Length; i++)
            {
                entities.Get<Position>(ids[i]).ShouldBe(new Position(0));
                entities.Get<Velocity>(ids[i]).ShouldBe(new Velocity(0));
            }
        }

        [Fact]
        public void ShouldResetOneComponent()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec0 = EntitySpec.Create<Position, Velocity>();

            var id0 = entities.Create(spec0);
            entities.Replace(id0, new Position(20));
            entities.Replace(id0, new Velocity(10));
            entities.Reset<Position>(id0);

            entities.EntityCount.ShouldBe(1);
            entities.EntityArrays.Count.ShouldBe(1);
            entities.EntityArrays[0].EntityCount.ShouldBe(1);
            entities.Get<Position>(id0).ShouldBe(new Position(0));
            entities.Get<Velocity>(id0).ShouldBe(new Velocity(10));
        }

        [Fact]
        public void ShouldResetManyComponents()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec = EntitySpec.Create<Position, Velocity>();

            var ids = Enumerable.Range(0, 16).Select(x => 0u).ToArray();
            entities.Create(spec, ids);

            for (var i = 0; i < ids.Length; i++)
            {
                entities.Replace<Position>(ids[i], new Position(10 * i));
                entities.Replace<Velocity>(ids[i], new Velocity(20 * i));
            }

            entities.Reset<Position>(ids);

            entities.EntityCount.ShouldBe(16);
            entities.EntityArrays.Count.ShouldBe(1);
            entities.EntityArrays[0].EntityCount.ShouldBe(16);

            for (var i = 0; i < ids.Length; i++)
            {
                entities.Get<Position>(ids[i]).ShouldBe(new Position(0));
                entities.Get<Velocity>(ids[i]).ShouldBe(new Velocity(20 * i));
            }
        }

        [Fact]
        public void ShouldMoveOne()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec0 = EntitySpec.Create<Position>();
            var spec1 = EntitySpec.Create<Position, Velocity>();

            var id0 = entities.Create(spec0);
            var id1 = entities.Create(spec1);
            entities.Replace(id0, new Position(10));
            entities.Replace(id1, new Position(20));

            entities.Move(id0, spec1);

            entities.EntityCount.ShouldBe(2);
            entities.EntityArrays.Count.ShouldBe(2);
            entities.EntityArrays[0].EntityCount.ShouldBe(0);
            entities.EntityArrays[1].EntityCount.ShouldBe(2);
            entities.Get<Position>(id0).ShouldBe(new Position(10));
            entities.Get<Position>(id1).ShouldBe(new Position(20));

        }

        [Fact]
        public void ShouldMoveMany()
        {
            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);

            var spec0 = EntitySpec.Create<Position>();
            var spec1 = EntitySpec.Create<Position, Velocity>();

            var ids0 = Enumerable.Range(0, 4096).Select(x => 0u).ToArray();
            var ids1 = Enumerable.Range(0, 4096).Select(x => 0u).ToArray();
            entities.Create(spec0, ids0);
            entities.Create(spec1, ids1);

            entities.Replace<Position>(ids0, new Position(10));
            entities.Replace<Position>(ids1, new Position(20));

            entities.Move(ids0, spec1);

            entities.EntityCount.ShouldBe(8192);
            entities.EntityArrays.Count.ShouldBe(2);
            entities.EntityArrays[0].EntityCount.ShouldBe(0);
            entities.EntityArrays[1].EntityCount.ShouldBe(8192);

            for (var i = 0; i < ids0.Length; i++)
                entities.Get<Position>(ids0[i]).ShouldBe(new Position(10));
            for (var i = 0; i < ids1.Length; i++)
                entities.Get<Position>(ids1[i]).ShouldBe(new Position(20));

        }


        [Fact]
        public void ShouldCreateEntityManager()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            using var em = new EntityManager(_logFactory, memory);
            var spec = new EntitySpec(ComponentType<Position>.Type);

            //act
            var id0 = em.Create(spec);
            var id1 = em.Create(spec);
            var id2 = em.Create(spec);
            var id3 = em.Create(spec);

            //assert
            (id0 & 0xffffff).ShouldBe(1u);
            (id1 & 0xffffff).ShouldBe(2u);
            (id2 & 0xffffff).ShouldBe(3u);
            (id3 & 0xffffff).ShouldBe(4u);
            em.EntityArrays.Count.ShouldBe(1);
            em.EntityArrays[0].EntityCount.ShouldBe(4);
            em.EntityCount.ShouldBe(4);

        }

        [Fact]
        public void ShouldBulkCreateEntities()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            using var em = new EntityManager(_logFactory, memory);
            var spec = new EntitySpec(ComponentType<Position>.Type);

            //act
            using var entities = new NativeArray<uint>(memory, 32);
            em.Create(spec, entities);

            //assert
            for (var i = 0; i < entities.Length; i++)
            {
                var id = (int)(entities[i] & 0xffffff);
                id.ShouldBe(i + 1);
            }

        }

        [Fact]
        public void ShouldShouldDeleteEntity()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            using var em = new EntityManager(_logFactory, memory);
            var spec = new EntitySpec(ComponentType<Position>.Type);

            //act
            var id0 = em.Create(spec);

            //assert
            em.Has(id0).ShouldBe(true);
            em.Delete(id0);
            em.Has(id0).ShouldBe(false);
        }

        [Fact]
        public void ShouldShiftAllEntitiesOnDelete()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            using var em = new EntityManager(_logFactory, memory);
            var spec = new EntitySpec(ComponentType<Position>.Type);

            //act
            var id0 = em.Create(spec);

            em.Replace<Position>(id0, new Position(10, 20));
            var id1 = em.Create(spec);
            em.Replace<Position>(id1, new Position(11, 21));
            var id2 = em.Create(spec);
            em.Replace<Position>(id2, new Position(12, 22));

            em.Delete(id0);

            //assert
            var p1 = em.Get<Position>(id1);
            p1.X.ShouldBe(11);
            p1.Y.ShouldBe(21);

            var p2 = em.Get<Position>(id2);
            p2.X.ShouldBe(12);
            p2.Y.ShouldBe(22);

            em.EntityArrays.Count.ShouldBe(1);
            em.EntityArrays[0].EntityCount.ShouldBe(2);
            em.EntityCount.ShouldBe(2);
        }

        [Fact]
        public void ShouldAssignEntity()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            using var em = new EntityManager(_logFactory, memory);
            var spec = new EntitySpec(ComponentType<Position>.Type);

            //act
            var id0 = em.Create(spec);
            em.Assign<Velocity>(id0, new Velocity(10, 20));

            //assert
            em.Has<Velocity>(id0).ShouldBe(true);
            em.Get<Velocity>(id0).VX.ShouldBe(10);
            em.Get<Velocity>(id0).VY.ShouldBe(20);

            em.EntityArrays.Count.ShouldBe(2);
            em.EntityArrays[0].EntityCount.ShouldBe(0);
            em.EntityArrays[1].EntityCount.ShouldBe(1);
            em.EntityCount.ShouldBe(1);
        }

        [Fact]
        public void ShouldReplaceEntity()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            using var em = new EntityManager(_logFactory, memory);
            var spec = new EntitySpec(ComponentType<Position>.Type);

            //act
            var id0 = em.Create(spec);
            em.Replace<Position>(id0, new Position(10, 20));

            //assert
            em.Has<Position>(id0).ShouldBe(true);
            em.Get<Position>(id0).X.ShouldBe(10);
            em.Get<Position>(id0).Y.ShouldBe(20);

            em.EntityArrays.Count.ShouldBe(1);
            em.EntityArrays[0].EntityCount.ShouldBe(1);
            em.EntityCount.ShouldBe(1);
        }

        [Fact]
        public void ShouldUpdateEntity()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            using var em = new EntityManager(_logFactory, memory);
            var spec = new EntitySpec(
                ComponentType<Position>.Type
            );

            //act
            var id0 = em.Create(spec);
            var id1 = em.Create(spec);
            em.Update(id0, new Velocity(20, 10));


            //assert
            var v0 = em.Get<Velocity>(id0);
            v0.VX.ShouldBe(20);
            v0.VY.ShouldBe(10);
            em.EntityArrays.Count.ShouldBe(2);
            em.EntityArrays[0].EntityCount.ShouldBe(1);
            em.EntityArrays[1].EntityCount.ShouldBe(1);
            em.EntityCount.ShouldBe(2);
        }

        [Fact]
        public void ShouldMoveEntity()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            using var em = new EntityManager(_logFactory, memory);
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
            p1.X.ShouldBe(10);
            p1.Y.ShouldBe(20);

            em.EntityArrays.Count.ShouldBe(2);
            em.EntityArrays[0].EntityCount.ShouldBe(1);
            em.EntityArrays[1].EntityCount.ShouldBe(1);
            em.EntityCount.ShouldBe(2);
        }


        [Fact]
        public void ShouldRemoveEntity()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            using var em = new EntityManager(_logFactory, memory);
            var spec = new EntitySpec(ComponentType<Position>.Type);

            //act
            var id0 = em.Create(spec);
            em.Delete(id0);

            //assert
            em.EntityArrays.Count.ShouldBe(1);
            em.EntityArrays[0].EntityCount.ShouldBe(0);
            em.EntityCount.ShouldBe(0);
        }

        [Fact]
        public void ShouldRemoveEntityWithNoComponents()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            using var em = new EntityManager(_logFactory, memory);
            var spec = new EntitySpec(
                ComponentType<Position>.Type
            );

            //act
            var id0 = em.Create(spec);
            em.Remove<Position>(id0);

            //assert
            em.EntityArrays.Count.ShouldBe(1);
            em.EntityArrays[0].EntityCount.ShouldBe(0);
            em.EntityCount.ShouldBe(0);
        }

        [Fact]
        public void ShouldResetEntity()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            using var em = new EntityManager(_logFactory, memory);
            var spec = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            //act
            var id0 = em.Create(spec);
            em.Replace(id0, new Position(10, 20));
            em.Replace(id0, new Velocity(20, 10));
            em.Reset(id0);

            //assert
            var p = em.Get<Position>(id0);
            var v = em.Get<Velocity>(id0);
            p.X.ShouldBe(0);
            p.Y.ShouldBe(0);
            v.VX.ShouldBe(0);
            v.VY.ShouldBe(0);
            em.EntityArrays.Count.ShouldBe(1);
            em.EntityArrays[0].EntityCount.ShouldBe(1);
            em.EntityCount.ShouldBe(1);
        }

        [Fact]
        public void ShouldResetEntityComponent()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            using var em = new EntityManager(_logFactory, memory);
            var spec = new EntitySpec(
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type
            );

            //act
            var id0 = em.Create(spec);
            em.Replace(id0, new Position(10, 20));
            em.Replace(id0, new Velocity(20, 10));
            em.Reset<Velocity>(id0);

            //assert
            var p = em.Get<Position>(id0);
            var v = em.Get<Velocity>(id0);
            p.X.ShouldBe(10);
            p.Y.ShouldBe(20);
            v.VX.ShouldBe(0);
            v.VY.ShouldBe(0);
            em.EntityArrays.Count.ShouldBe(1);
            em.EntityArrays[0].EntityCount.ShouldBe(1);
            em.EntityCount.ShouldBe(1);
        }

        [Fact]
        public unsafe void ShouldBulkReplaceWithNativeArrays()
        {
            //arrange
            using var memory = new DynamicAllocator(_logFactory);
            using var em = new EntityManager(_logFactory, memory);
            var spec0 = new EntitySpec(ComponentType<Position>.Type, ComponentType<Velocity>.Type);
            var spec1 = new EntitySpec(ComponentType<Position>.Type);

            //act
            var id0 = em.Create(spec0);
            var id1 = em.Create(spec1);
            var id2 = em.Create(spec1);
            var id3 = em.Create(spec0);

            using var entities = new NativeArray<uint>(memory, 4);
            entities[0] = id0;
            entities[1] = id1;
            entities[2] = id2;
            entities[3] = id3;

            using var data = new NativeArray<Position>(memory, 4);
            data[0] = new Position(100, 100);
            data[1] = new Position(200, 200);
            data[2] = new Position(400, 100);
            data[3] = new Position(100, 400);

            em.Replace(entities, data);

            //assert
            var p0 = em.Get<Position>(id0);
            var p1 = em.Get<Position>(id1);
            var p2 = em.Get<Position>(id2);
            var p3 = em.Get<Position>(id3);

            p0.X.ShouldBe(100);
            p0.Y.ShouldBe(100);
            p1.X.ShouldBe(200);
            p1.Y.ShouldBe(200);
            p2.X.ShouldBe(400);
            p2.Y.ShouldBe(100);
            p3.X.ShouldBe(100);
            p3.Y.ShouldBe(400);
        }

        // public void ShouldCreateValidArchetype()
        // {
        //     var _manager = GetEntityManager();
        //     var archetype = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3));

        //     archetype.IsValid.ShouldBe(true);
        //     archetype.EntitySize.ShouldBe(16);
        // }

        // public void ShouldThrowOnInvalidType()
        // {
        //     var _manager = GetEntityManager();
        //     Should.Throw<Exception>(() =>
        //     {
        //         var archetype = _manager.CreateArchetype(typeof(Invalid));
        //     });
        // }

        //TODO: appveyor
        // public void ShouldThrowOnDuplicateType()
        // {
        //     var _manager = GetEntityManager();
        //     Should.Throw<Exception>(() =>
        //     {
        //         var archetype = _manager.CreateArchetype(typeof(Valid), typeof(Valid));
        //     });
        // }



        // public void ShouldManagesEntity()
        // {
        //     var _manager = GetEntityManager();
        //     var archetype = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3));
        //     var entity = _manager.CreateEntity(archetype);

        //     //should create a new entity
        //     entity.ShouldBe(1);

        //     //should have this entity
        //     _manager.HasEntity(entity).ShouldBe(true);

        //     //should delete this entity
        //     _manager.DestroyEntity(entity);

        //     //should no longer have this entity
        //     _manager.HasEntity(entity).ShouldBe(false);

        //     //should create a new entity and reuse the id
        //     entity = _manager.CreateEntity(archetype);
        //     entity.ShouldBe(1);

        //     //should increment id
        //     _manager.CreateEntity(archetype).ShouldBe(2);

        // }

        // public void EntitySetComponent()
        // {
        //     var _manager = GetEntityManager();
        //     var archetype = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3));
        //     var entity1 = _manager.CreateEntity(archetype);
        //     var entity2 = _manager.CreateEntity(archetype);

        //     var setComponentData = new Valid() { X = 1, Y = 2 };
        //     _manager.SetComponentData(entity2, setComponentData);

        //     var getComponentData = _manager.GetComponentData<Valid>(entity2);

        //     getComponentData.X.ShouldBe(setComponentData.X);
        //     getComponentData.Y.ShouldBe(setComponentData.Y);

        //     _manager.DestroyEntity(entity1);

        //     //check if the data moves
        //     getComponentData = _manager.GetComponentData<Valid>(entity2);

        //     getComponentData.X.ShouldBe(setComponentData.X);
        //     getComponentData.Y.ShouldBe(setComponentData.Y);

        //     //data is cleared on delete, not sure if this will change in the future
        //     entity1 = _manager.CreateEntity(archetype);
        //     getComponentData = _manager.GetComponentData<Valid>(entity1);

        //     getComponentData.X.ShouldBe(0);
        //     getComponentData.Y.ShouldBe(0);
        // }

        // public void EntityShouldChangeArchetypes()
        // {
        //     var _manager = GetEntityManager();
        //     var archetypeBase = _manager.CreateArchetype(typeof(Valid), typeof(Valid2));
        //     var entity = _manager.CreateEntity(archetypeBase);

        //     var archetype1 = _manager.GetEntityArchetype(entity);

        //     //should auto move the entity to new archetype
        //     var setComponentData = new Valid3() { W = 1 };
        //     _manager.SetComponentData(entity, setComponentData);

        //     var archetype2 = _manager.GetEntityArchetype(entity);
        //     archetype1.ID.ShouldNotBe(archetype2.ID);

        //     //should move the entity back to the original archetype
        //     _manager.RemoveComponentData<Valid3>(entity);
        //     archetype2 = _manager.GetEntityArchetype(entity);

        //     archetype2.ID.ShouldBe(archetype1.ID);

        //     //should move the entity back to the 2nd archetype
        //     archetype2 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3));
        //     _manager.MoveEntity(entity, archetype2);
        //     _manager.GetEntityArchetype(entity).ID.ShouldBe(archetype2.ID);
        // }






        /* MOSTLY OLD PROJECTION CODE BELOW THIS POINT */
        /* MOSTLY OLD PROJECTION CODE BELOW THIS POINT */
        /* MOSTLY OLD PROJECTION CODE BELOW THIS POINT */
        /* MOSTLY OLD PROJECTION CODE BELOW THIS POINT */
        /* MOSTLY OLD PROJECTION CODE BELOW THIS POINT */
        /* MOSTLY OLD PROJECTION CODE BELOW THIS POINT */
        /* MOSTLY OLD PROJECTION CODE BELOW THIS POINT */
        /* MOSTLY OLD PROJECTION CODE BELOW THIS POINT */
        /* MOSTLY OLD PROJECTION CODE BELOW THIS POINT */
        /* MOSTLY OLD PROJECTION CODE BELOW THIS POINT */
        /* MOSTLY OLD PROJECTION CODE BELOW THIS POINT */
        /* MOSTLY OLD PROJECTION CODE BELOW THIS POINT */



        // public void ShouldFilter()
        // {
        //     var _manager = GetEntityManager();
        //     var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2));
        //     var archetype1 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3));
        //     var archetype2 = _manager.CreateArchetype(typeof(Valid), typeof(Valid4));
        //     var archetype3 = _manager.CreateArchetype(typeof(Valid6));

        //     var entity0 = _manager.CreateEntity(archetype0);
        //     var entity1 = _manager.CreateEntity(archetype0);
        //     var entity2 = _manager.CreateEntity(archetype0);

        //     var entity3 = _manager.CreateEntity(archetype1);
        //     var entity4 = _manager.CreateEntity(archetype1);

        //     var entity5 = _manager.CreateEntity(archetype2);
        //     var entity6 = _manager.CreateEntity(archetype2);

        //     var entity7 = _manager.CreateEntity(archetype3);
        //     var entity8 = _manager.CreateEntity(archetype3);

        //     _manager.FilterArchetypes<ShouldFilterStruct>().Count().ShouldBe(3);
        //     _manager.FilterArchetypes<ShouldFilterStruct>().Sum(x => x.Count).ShouldBe(7);

        //     _manager.DestroyEntity(entity5);
        //     _manager.DestroyEntity(entity6);

        //     _manager.FilterArchetypes<ShouldFilterStruct>().Count().ShouldBe(2);
        //     _manager.FilterArchetypes<ShouldFilterStruct>().Sum(x => x.Count).ShouldBe(5);

        // }

        // //TODO: appveyor
        // // public void EntityArchetypeArrayAccess()
        // // {
        // //     var _manager = GetEntityManager();
        // //     var archetype0 = _manager.CreateArchetype(typeof(Valid));
        // //     var entity0 = _manager.CreateEntity(archetype0);

        // //     //var query = EntityQueryBuilder.Create(b =>
        // //     //    b.Query(q =>
        // //     //        q.Writes<Valid>()
        // //     //    )
        // //     //);

        // //     foreach (var archetype in _manager.FilterArchetypes<ShouldFilterStruct>())
        // //     {
        // //         archetype.GetComponentIndex<Valid>().ShouldBe(0);
        // //         archetype.GetComponentIndex<Valid2>().ShouldBe(-1);

        // //         foreach (var chunk in archetype.ActiveChunks)
        // //         {
        // //             using var valids = chunk.GetReadComponent<Valid>();
        // //             valids.Length.ShouldBe(1);

        // //             Should.NotThrow(() =>
        // //             {
        // //                 using var x = chunk.GetReadComponent<Valid>();
        // //             });

        // //             Should.Throw<Exception>(() =>
        // //             {
        // //                 using var x = chunk.GetWriteComponent<Valid>();
        // //             });
        // //         }
        // //     }
        // // }

        // //TODO: appveyor
        // // public void ShouldThrowWhileReadWrite()
        // // {
        // //     var _manager = GetEntityManager();
        // //     var archetype0 = _manager.CreateArchetype(typeof(Valid));
        // //     var entity0 = _manager.CreateEntity(archetype0);

        // //     foreach (var archetype in _manager.FilterArchetypes<ShouldFilterStruct>())
        // //     {
        // //         archetype.GetComponentIndex<Valid>().ShouldBe(0);
        // //         archetype.GetComponentIndex<Valid2>().ShouldBe(-1);

        // //         foreach (var chunk in archetype.ActiveChunks)
        // //         {
        // //             using var valids = chunk.GetReadComponent<Valid>();
        // //             valids.Length.ShouldBe(1);

        // //             Should.NotThrow(() =>
        // //             {
        // //                 using var x = chunk.GetReadComponent<Valid>();
        // //             });

        // //             Should.Throw<Exception>(() =>
        // //             {
        // //                 using var x = chunk.GetWriteComponent<Valid>();
        // //             });
        // //         }
        // //     }
        // // }

        // public void EntityViewXRefArchetypes()
        // {
        //     var _manager = GetEntityManager();
        //     var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2));
        //     var archetype1 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3), typeof(Valid2));
        //     var entity0 = _manager.CreateEntity(archetype0);
        //     var entity1 = _manager.CreateEntity(archetype1);

        //     _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
        //     _manager.SetComponentData(entity0, new Valid2() { Z = 12 });

        //     _manager.SetComponentData(entity1, new Valid() { X = 20, Y = 21 });
        //     _manager.SetComponentData(entity1, new Valid2() { Z = 22 });
        //     _manager.SetComponentData(entity1, new Valid3() { W = 23 });

        //     var result = _manager.Project<Group>().ToArray();
        //     result.Length.ShouldBe(2);
        //     result[0].valid.X.ShouldBe(10);
        //     result[0].valid.Y.ShouldBe(11);
        //     result[0].valid2.Z.ShouldBe(12);

        //     result[1].valid.X.ShouldBe(20);
        //     result[1].valid.Y.ShouldBe(21);
        //     result[1].valid2.Z.ShouldBe(22);
        // }

        // public void EntityProjectArrays()
        // {
        //     var _manager = GetEntityManager();
        //     var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2));
        //     var archetype1 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3), typeof(Valid2));
        //     var entity0 = _manager.CreateEntity(archetype0);
        //     var entity1 = _manager.CreateEntity(archetype1);
        //     var entity2 = _manager.CreateEntity(archetype1);

        //     _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
        //     _manager.SetComponentData(entity0, new Valid2() { Z = 12 });

        //     _manager.SetComponentData(entity1, new Valid() { X = 20, Y = 21 });
        //     _manager.SetComponentData(entity1, new Valid2() { Z = 22 });
        //     _manager.SetComponentData(entity1, new Valid3() { W = 23 });

        //     var result = _manager.Project<GroupArray>().ToArray();
        //     result.Length.ShouldBe(2);
        //     result[0].entity[0].ID.ShouldBe(1);
        //     result[0].valid[0].X.ShouldBe(10);
        //     result[0].valid[0].Y.ShouldBe(11);
        //     result[0].valid2[0].Z.ShouldBe(12);
        //     result[0].length.ShouldBe(1);

        //     result[1].entity[0].ID.ShouldBe(2);
        //     result[1].valid[0].X.ShouldBe(20);
        //     result[1].valid[0].Y.ShouldBe(21);
        //     result[1].valid2[0].Z.ShouldBe(22);
        //     result[1].length.ShouldBe(2);

        //     _manager.SetComponentData(entity0, new Valid() { X = 33, Y = 34 });
        //     result[0].entity[0].ID.ShouldBe(1);
        //     result[0].valid[0].X.ShouldBe(33);
        //     result[0].valid[0].Y.ShouldBe(34);

        //     _manager.DestroyEntity(entity2);
        //     result = _manager.Project<GroupArray>().ToArray();
        //     result[1].length.ShouldBe(1);
        // }

        // public void EntityFirst()
        // {
        //     var _manager = GetEntityManager();
        //     var archetype1 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3), typeof(Valid2));
        //     var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2));
        //     var entity1 = _manager.CreateEntity(archetype1);
        //     var entity0 = _manager.CreateEntity(archetype0);

        //     _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
        //     _manager.SetComponentData(entity0, new Valid2() { Z = 12 });

        //     _manager.SetComponentData(entity1, new Valid() { X = 20, Y = 21 });
        //     _manager.SetComponentData(entity1, new Valid2() { Z = 22 });
        //     _manager.SetComponentData(entity1, new Valid3() { W = 23 });

        //     _manager.First<Group>(out var entityGroup).ShouldBe(true);
        //     entityGroup.valid.X.ShouldBe(20);
        //     entityGroup.valid.Y.ShouldBe(21);
        //     entityGroup.valid2.Z.ShouldBe(22);
        // }

        // public void EntityUpdate()
        // {
        //     var _manager = GetEntityManager();
        //     var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3), typeof(Valid2));
        //     var entity0 = _manager.CreateEntity(archetype0);

        //     _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
        //     _manager.SetComponentData(entity0, new Valid2() { Z = 12 });

        //     _manager.First<Group>(out var entityGroup).ShouldBe(true);
        //     entityGroup.valid = new Valid() { X = 20, Y = 21 };
        //     _manager.Update(entityGroup, entity0);
        //     _manager.First<Group>(out entityGroup).ShouldBe(true);
        //     entityGroup.valid.X.ShouldBe(20);
        //     entityGroup.valid.Y.ShouldBe(21);
        // }

        // public unsafe void EntityProjectPointer()
        // {
        //     var _manager = GetEntityManager();
        //     var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3), typeof(Valid2));
        //     var entity0 = _manager.CreateEntity(archetype0);
        //     var entity1 = _manager.CreateEntity(archetype0);

        //     _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
        //     _manager.SetComponentData(entity1, new Valid() { X = 20, Y = 21 });

        //     var results = _manager.Project<ValidPtr>().ToList();

        //     results.Count.ShouldBe(2);
        //     results[0].Valid->X.ShouldBe(10);
        //     results[0].Valid->Y.ShouldBe(11);
        //     results[1].Valid->X.ShouldBe(20);
        //     results[1].Valid->Y.ShouldBe(21);

        // }

        // public void EntityUpdateWithEntity()
        // {
        //     var _manager = GetEntityManager();
        //     var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3), typeof(Valid2));
        //     var entity0 = _manager.CreateEntity(archetype0);

        //     _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
        //     _manager.SetComponentData(entity0, new Valid2() { Z = 12 });

        //     _manager.First<GroupWithEntity2>(out var entityGroup).ShouldBe(true);
        //     entityGroup.valid = new Valid() { X = 20, Y = 21 };
        //     _manager.Update(entityGroup);
        //     _manager.First(out entityGroup).ShouldBe(true);
        //     entityGroup.valid.X.ShouldBe(20);
        //     entityGroup.valid.Y.ShouldBe(21);
        // }

        // public void AnyComponent()
        // {
        //     var _manager = GetEntityManager();
        //     var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid4));
        //     var archetype1 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3), typeof(Valid2));
        //     var archetype2 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3), typeof(Valid5));
        //     var entity0 = _manager.CreateEntity(archetype0);
        //     var entity1 = _manager.CreateEntity(archetype1);
        //     var entity2 = _manager.CreateEntity(archetype2);

        //     _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
        //     _manager.SetComponentData(entity0, new Valid2() { Z = 12 });
        //     _manager.SetComponentData(entity0, new Valid4() { _ = 13 });

        //     _manager.SetComponentData(entity1, new Valid() { X = 20, Y = 21 });
        //     _manager.SetComponentData(entity1, new Valid2() { Z = 22 });
        //     _manager.SetComponentData(entity1, new Valid3() { W = 23 });

        //     _manager.SetComponentData(entity2, new Valid() { X = 30, Y = 31 });
        //     _manager.SetComponentData(entity2, new Valid2() { Z = 32 });
        //     _manager.SetComponentData(entity2, new Valid3() { W = 33 });
        //     _manager.SetComponentData(entity2, new Valid4() { _ = 34 });

        //     var entityGroup = _manager.Project<GroupConditional>().ToArray();
        //     entityGroup.Length.ShouldBe(3);
        //     entityGroup[0].valid.X.ShouldBe(10);
        //     entityGroup[0].valid.Y.ShouldBe(11);
        //     entityGroup[0].valid2.Z.ShouldBe(12);
        //     entityGroup[0].valid3.W.ShouldBe(0);

        //     entityGroup[1].valid.X.ShouldBe(20);
        //     entityGroup[1].valid.Y.ShouldBe(21);
        //     entityGroup[1].valid2.Z.ShouldBe(22);
        //     entityGroup[1].valid3.W.ShouldBe(23);

        //     entityGroup[2].valid.X.ShouldBe(30);
        //     entityGroup[2].valid.Y.ShouldBe(31);
        //     entityGroup[2].valid2.Z.ShouldBe(32);
        //     entityGroup[2].valid3.W.ShouldBe(33);

        // }

        // public void IgnoreComponent()
        // {
        //     var _manager = GetEntityManager();
        //     var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid5));
        //     var archetype1 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3), typeof(Valid2));
        //     var archetype2 = _manager.CreateArchetype(typeof(Valid), typeof(Valid5), typeof(Valid2), typeof(Valid6));
        //     var entity0 = _manager.CreateEntity(archetype0);
        //     var entity1 = _manager.CreateEntity(archetype1);
        //     var entity2 = _manager.CreateEntity(archetype2);

        //     _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
        //     _manager.SetComponentData(entity0, new Valid2() { Z = 12 });
        //     _manager.SetComponentData(entity0, new Valid5() { _ = 13 });

        //     _manager.SetComponentData(entity1, new Valid() { X = 20, Y = 21 });
        //     _manager.SetComponentData(entity1, new Valid2() { Z = 22 });
        //     _manager.SetComponentData(entity1, new Valid3() { W = 23 });

        //     _manager.SetComponentData(entity2, new Valid() { X = 30, Y = 31 });
        //     _manager.SetComponentData(entity2, new Valid2() { Z = 32 });
        //     _manager.SetComponentData(entity2, new Valid5() { _ = 34 });

        //     var entityGroup = _manager.Project<GroupIgnore>().ToArray();
        //     entityGroup.Length.ShouldBe(2);

        // }

        // public void GetsGroupWithEntity()
        // {
        //     var _manager = GetEntityManager();
        //     var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid5));
        //     var entity0 = _manager.CreateEntity(archetype0);

        //     _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
        //     _manager.SetComponentData(entity0, new Valid2() { Z = 12 });
        //     _manager.SetComponentData(entity0, new Valid5() { _ = 13 });

        //     var entityGroup = _manager.Project<GroupWithEntity>().ToArray();
        //     entityGroup.Length.ShouldBe(1);
        //     entityGroup[0].entity.ArchetypeIndex.ShouldBe(1);
        //     entityGroup[0].entity.ChunkIndex.ShouldBe(0);
        //     entityGroup[0].entity.Index.ShouldBe(0);
        //     entityGroup[0].entity.ID.ShouldBe(entity0);
        // }


        // public void ShouldProjectToList()
        // {
        //     var _manager = GetEntityManager();
        //     var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid5));

        //     for (var i = 0; i < 100; i++)
        //     {
        //         var entity0 = _manager.CreateEntity(archetype0);

        //         _manager.SetComponentData(entity0, new Valid() { X = i, Y = i + 11 });
        //         _manager.SetComponentData(entity0, new Valid2() { Z = i + 12 });
        //         _manager.SetComponentData(entity0, new Valid5() { _ = i + 13 });
        //     }


        //     using var entities = _manager.ProjectToList<GroupWithEntity>();
        //     entities.Length.ShouldBe(100);
        //     for (var i = 0; i < 100; i++)
        //     {
        //         entities[i].entity.ID.ShouldBe(i + 1);
        //         entities[i].valid2.Z.ShouldBe(i + 12);
        //         entities[i].valid5._.ShouldBe(i + 13);
        //     }
        // }

        // public void BulkCreateArray()
        // {
        //     var _manager = GetEntityManager();
        //     var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid5));

        //     _manager.CreateEntity(archetype0, 8192, out var entities);
        //     for (var i = 0; i < entities.Length; i++)
        //         entities[i].ShouldBe(i + 1);
        // }

        // public void EntityShouldSetIndex()
        // {
        //     var entity = new Entity(0, 0, 0, 0);
        //     for (var i = 0; i < Entity.ENTITY_MAX; i++)
        //     {
        //         entity.Index = i;
        //         entity.Index.ShouldBe(i);
        //     }
        // }

        // public void BulkEntityCreateAndDelete()
        // {
        //     var sample = 4096 * 3;
        //     var _manager = GetEntityManager();
        //     var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid5));

        //     for (var k = 0; k < 4; k++)
        //     {
        //         for (var i = 1; i < sample; i++)
        //         {
        //             //if(i == 4095)
        //             //_manager.CreateEntity(archetype0);
        //             //else
        //             var x = _manager.CreateEntity(archetype0);

        //             archetype0.Count.ShouldBe(i);
        //             _manager.HasEntity(i).ShouldBe(true);
        //         }

        //         for (int i = sample - 1; i >= 1; i--)
        //         {
        //             _manager.DestroyEntity(i);
        //             archetype0.Count.ShouldBe(i - 1);
        //             _manager.HasEntity(i).ShouldBe(false);
        //         }
        //     }
        // }

        // public void EntityViewShouldUpdate()
        // {
        //     var sample = 4096;
        //     var _manager = GetEntityManager();
        //     var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid5));

        //     for (var i = 0; i < sample; i++)
        //     {
        //         var entity0 = _manager.CreateEntity(archetype0);
        //         _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
        //         _manager.SetComponentData(entity0, new Valid2() { Z = 12 });
        //         _manager.SetComponentData(entity0, new Valid5() { _ = 13 });
        //     }

        //     var view = _manager.CreateView<GroupArray>();
        //     var result = view.ForEach().ToArray();
        //     result.Length.ShouldBe(sample / (int)Entity.ENTITY_MAX);

        //     foreach (var entityGroup in result)
        //         entityGroup.length.ShouldBe((int)Entity.ENTITY_MAX);

        //     for (var i = 0; i < sample; i += 2)
        //         _manager.RemoveComponentData<Valid2>(i + 1);

        //     //_manager.RemoveComponentData<Valid2>(1024 + 1);


        //     view = _manager.CreateView<GroupArray>();
        //     result = view.ForEach().ToArray();
        //     result.Length.ShouldBe(sample / (int)Entity.ENTITY_MAX);

        //     foreach (var entityGroup in result)
        //         entityGroup.length.ShouldBe((int)Entity.ENTITY_MAX / 2);


        // }

    }
}
