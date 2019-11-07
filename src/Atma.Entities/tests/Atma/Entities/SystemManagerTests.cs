namespace Atma.Entities
{
    using Shouldly;

    using System;
    using System.Collections.Generic;

    public class SystemManagerTests
    {
        [UpdateBefore(typeof(PreUpdate))]
        public class Init { }

        [UpdateGroup(typeof(Init))]
        public class InitA : ComponentSystem
        {
            protected override void Update()
            {
                throw new NotImplementedException();
            }
        }

        [UpdateGroup(typeof(Init))]
        [UpdateAfter(typeof(InitA))]
        public class InitB : ComponentSystem
        {
            protected override void Update()
            {
                throw new NotImplementedException();
            }
        }


        [UpdateBefore(typeof(Update))]
        public class PreUpdate { }
        [UpdateGroup(typeof(PreUpdate))]
        [UpdateBefore(typeof(PreUpdateB))]
        public class PreUpdateA : ComponentSystem
        {
            protected override void Update()
            {
                throw new NotImplementedException();
            }
        }

        [UpdateGroup(typeof(PreUpdate))]
        public class PreUpdateB : ComponentSystem
        {
            protected override void Update()
            {
                throw new NotImplementedException();
            }
        }
        public class Update { }

        [UpdateGroup(typeof(Update))]
        public class UpdateA : ComponentSystem
        {
            protected override void Update()
            {
                throw new NotImplementedException();
            }
        }

        [UpdateGroup(typeof(Update))]
        public class UpdateB : ComponentSystem
        {
            protected override void Update()
            {
                throw new NotImplementedException();
            }
        }

        [UpdateAfter(typeof(Update))]
        public class PostUpdate { }
        [UpdateGroup(typeof(PostUpdate))]
        public class PostUpdateA : ComponentSystem
        {
            protected override void Update()
            {
                throw new NotImplementedException();
            }
        }

        [UpdateGroup(typeof(PostUpdate))]
        [UpdateAfter(typeof(PostUpdateA))]
        public class PostUpdateB : ComponentSystem
        {
            protected override void Update()
            {
                throw new NotImplementedException();
            }
        }

        public class FixedUpdate { }
        [UpdateGroup(typeof(FixedUpdate))]
        public class FixedUpdateA : ComponentSystem
        {
            protected override void Update()
            {
                throw new NotImplementedException();
            }
        }

        [UpdateGroup(typeof(FixedUpdate))]
        public class FixedUpdateB : ComponentSystem
        {
            protected override void Update()
            {
                throw new NotImplementedException();
            }
        }


        public void ShouldGroupCorrectly()
        {
            //var data = new List<string>();
            var manager = new SystemManager(null, null);
            manager.AddDefaultDependencyResolvers();

            manager.AddSystem(new PreUpdateB());
            manager.AddSystem(new PreUpdateA());
            manager.AddSystem(new UpdateA());
            manager.AddSystem(new UpdateB());
            manager.AddSystem(new PostUpdateB());
            manager.AddSystem(new PostUpdateA());
            manager.AddSystem(new FixedUpdateA());
            manager.AddSystem(new FixedUpdateB());
            manager.AddSystem(new InitA());
            manager.AddSystem(new InitB());
            manager.SortComponentSystems();

            Helpers.Expand(manager.Root).ShouldBe(
                new KeyValuePair<int, string>[]
                {
                    new KeyValuePair<int, string>(0, nameof(SystemManager)),
                    new KeyValuePair<int, string>(1, nameof(Init)),
                    new KeyValuePair<int, string>(2, nameof(InitA)),
                    new KeyValuePair<int, string>(2, nameof(InitB)),
                    new KeyValuePair<int, string>(1, nameof(PreUpdate)),
                    new KeyValuePair<int, string>(2, nameof(PreUpdateA)),
                    new KeyValuePair<int, string>(2, nameof(PreUpdateB)),
                    new KeyValuePair<int, string>(1, nameof(Update)),
                    new KeyValuePair<int, string>(2, nameof(UpdateA)),
                    new KeyValuePair<int, string>(2, nameof(UpdateB)),
                    new KeyValuePair<int, string>(1, nameof(PostUpdate)),
                    new KeyValuePair<int, string>(2, nameof(PostUpdateA)),
                    new KeyValuePair<int, string>(2, nameof(PostUpdateB)),
                    new KeyValuePair<int, string>(1, nameof(FixedUpdate)),
                    new KeyValuePair<int, string>(2, nameof(FixedUpdateA)),
                    new KeyValuePair<int, string>(2, nameof(FixedUpdateB))
                });
            ////manager.Root.

            //data.ShouldBe(new[] { "B", "A", "C" });
        }

        public void ShouldNotThrowWhenEmpty()
        {
            //var data = new List<string>();
            var manager = new SystemManager(null, null);
            manager.AddDefaultDependencyResolvers();

            Should.NotThrow(() => manager.SortComponentSystems());

        
        }


        public ref struct JobData
        {
            public Span<float3> X;
        }

        public struct float3
        {
            public float x, y, z;
        }

        //public class Query0 : ComponentSystem
        //{

        //    private EntityQuery<JobData> query;

        //    protected override void Created()
        //    {
        //        base.Created();
        //        query = CreateQuery<JobData>();
        //    }
        //    protected override void Update()
        //    {

        //    }

        //}
    }



    static class Helpers
    {
        public static IEnumerable<KeyValuePair<int, string>> Expand(ComponentSystemBase system, int depth = 0)
        {
            yield return new KeyValuePair<int, string>(depth, system.Type?.Name);
            if (system is ComponentSystemList list)
                foreach (var it in list.Ordered)
                    foreach (var kvp in Expand(it, depth + 1))
                        yield return kvp;
        }
    }

}
