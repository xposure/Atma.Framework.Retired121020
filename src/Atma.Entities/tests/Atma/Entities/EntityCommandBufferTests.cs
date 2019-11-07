using Atma.Memory;

using Shouldly;

namespace Atma.Entities
{
    public class EntityCommandBufferTests
    {
        public void ShouldCreate()
        {
            var e = GetEntityManager();
            var archetype1 = e.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3));
            var archetype = e.CreateArchetype(typeof(Valid), typeof(Valid2));
            var buffer = new EntityCommandBuffer(Allocator.Temp, 1024);
            buffer.CreateEntity(archetype);

            buffer.Playback(e);

            archetype.Count.ShouldBe(1);
            e.HasEntity(1);
            e.HasComponentData<Valid>(1).ShouldBe(true);
            e.HasComponentData<Valid2>(1).ShouldBe(true);
            e.HasComponentData<Valid3>(1).ShouldBe(false);
        }

        public void ShouldDelete()
        {
            var e = GetEntityManager();
            var archetype1 = e.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3));
            var archetype = e.CreateArchetype(typeof(Valid), typeof(Valid2));
            var buffer = new EntityCommandBuffer(Allocator.Temp, 1024);
            e.CreateEntity(archetype1); //1
            e.CreateEntity(archetype1); //2
            e.CreateEntity(archetype); //3
            e.CreateEntity(archetype); //4

            buffer.DeleteEntity(2);
            buffer.DeleteEntity(3);

            buffer.Playback(e);

            e.HasEntity(1).ShouldBe(true);
            e.HasComponentData<Valid>(1).ShouldBe(true);
            e.HasComponentData<Valid2>(1).ShouldBe(true);
            e.HasComponentData<Valid3>(1).ShouldBe(true);
            e.HasEntity(2).ShouldBe(false);

            e.HasEntity(4).ShouldBe(true);
            e.HasComponentData<Valid>(4).ShouldBe(true);
            e.HasComponentData<Valid2>(4).ShouldBe(true);
            e.HasComponentData<Valid3>(4).ShouldBe(false);
            e.HasEntity(3).ShouldBe(false);
        }

        public void ShouldSetComponent()
        {
            var e = GetEntityManager();
            var archetype1 = e.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3));
            var archetype = e.CreateArchetype(typeof(Valid), typeof(Valid2));
            var buffer = new EntityCommandBuffer(Allocator.Temp, 1024);
            e.CreateEntity(archetype1); //1
            e.CreateEntity(archetype1); //2
            e.CreateEntity(archetype); //3
            e.CreateEntity(archetype); //4

            buffer.SetComponent(1, new Valid() { X = 10, Y = 11 });
            buffer.SetComponent(2, new Valid3() { W = 22 });
            buffer.SetComponent(3, new Valid2() { Z = 33 });
            buffer.SetComponent(4, new Valid3() { W = 42 });

            buffer.Playback(e);

            e.HasEntity(1).ShouldBe(true);
            e.HasComponentData<Valid>(1).ShouldBe(true);
            e.HasComponentData<Valid2>(1).ShouldBe(true);
            e.HasComponentData<Valid3>(1).ShouldBe(true);
            e.GetComponentData<Valid>(1).X.ShouldBe(10);
            e.GetComponentData<Valid>(1).Y.ShouldBe(11);

            e.HasEntity(2).ShouldBe(true);
            e.HasComponentData<Valid>(2).ShouldBe(true);
            e.HasComponentData<Valid2>(2).ShouldBe(true);
            e.HasComponentData<Valid3>(2).ShouldBe(true);
            e.GetComponentData<Valid3>(2).W.ShouldBe(22);

            e.HasEntity(3).ShouldBe(true);
            e.HasComponentData<Valid>(3).ShouldBe(true);
            e.HasComponentData<Valid2>(3).ShouldBe(true);
            e.HasComponentData<Valid3>(3).ShouldBe(false);
            e.GetComponentData<Valid2>(3).Z.ShouldBe(33);

            //4 should move archetypes
            e.HasEntity(4).ShouldBe(true);
            e.HasComponentData<Valid>(4).ShouldBe(true);
            e.HasComponentData<Valid2>(4).ShouldBe(true);
            e.HasComponentData<Valid3>(4).ShouldBe(true);
            e.GetComponentData<Valid3>(4).W.ShouldBe(42);

        }

        public void ShouldRemoveComponent()
        {
            var e = GetEntityManager();
            var archetype1 = e.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3));
            var archetype = e.CreateArchetype(typeof(Valid), typeof(Valid2));
            var buffer = new EntityCommandBuffer(Allocator.Temp, 1024);
            e.CreateEntity(archetype1); //1
            e.CreateEntity(archetype1); //2
            e.CreateEntity(archetype); //3
            e.CreateEntity(archetype); //4

            buffer.RemoveComponent<Valid>(1);
            buffer.RemoveComponent<Valid3>(2);
            buffer.RemoveComponent<Valid2>(3);

            buffer.Playback(e);

            e.HasEntity(1).ShouldBe(true);
            e.HasComponentData<Valid>(1).ShouldBe(false);
            e.HasComponentData<Valid2>(1).ShouldBe(true);
            e.HasComponentData<Valid3>(1).ShouldBe(true);

            e.HasEntity(2).ShouldBe(true);
            e.HasComponentData<Valid>(2).ShouldBe(true);
            e.HasComponentData<Valid2>(2).ShouldBe(true);
            e.HasComponentData<Valid3>(2).ShouldBe(false);

            e.HasEntity(3).ShouldBe(true);
            e.HasComponentData<Valid>(3).ShouldBe(true);
            e.HasComponentData<Valid2>(3).ShouldBe(false);
            e.HasComponentData<Valid3>(3).ShouldBe(false);

            //4 should move archetypes
            e.HasEntity(4).ShouldBe(true);
            e.HasComponentData<Valid>(4).ShouldBe(true);
            e.HasComponentData<Valid2>(4).ShouldBe(true);
            e.HasComponentData<Valid3>(4).ShouldBe(false);

        }

        public void ShouldSetComponentOnPreviousCreate()
        {
            //for (var k = 0; k < 1000; k++)
            {

                var e = GetEntityManager();
                var archetype1 = e.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3));
                var archetype = e.CreateArchetype(typeof(Valid), typeof(Valid2));
                var buffer = new EntityCommandBuffer(Allocator.Temp, 1024);

                buffer.CreateEntity(archetype); // 1
                buffer.SetComponent(new Valid() { X = 10, Y = 11 });
                buffer.Playback(e);

                e.HasEntity(1).ShouldBe(true);
                e.HasComponentData<Valid>(1).ShouldBe(true);
                e.HasComponentData<Valid2>(1).ShouldBe(true);
                e.GetComponentData<Valid>(1).X.ShouldBe(10);
                e.GetComponentData<Valid>(1).Y.ShouldBe(11);
                var f = e.GetComponentData<Valid2>(1);
                f.Z.ShouldBe(0);

                buffer.CreateEntity(archetype, 3);

                //should also move them
                buffer.SetComponent(new Valid2() { Z = 22 });
                buffer.SetComponent(new Valid3() { W = 33 });

                buffer.Playback(e);

                for (var i = 2; i < 5; i++)
                {
                    e.HasEntity(i).ShouldBe(true);
                    e.HasComponentData<Valid>(i).ShouldBe(true);
                    e.HasComponentData<Valid2>(i).ShouldBe(true);
                    e.HasComponentData<Valid3>(i).ShouldBe(true);
                    e.GetComponentData<Valid>(i).X.ShouldBe(0);
                    e.GetComponentData<Valid>(i).Y.ShouldBe(0);
                    e.GetComponentData<Valid2>(i).Z.ShouldBe(22);
                    e.GetComponentData<Valid3>(i).W.ShouldBe(33);
                }
            }
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
}
