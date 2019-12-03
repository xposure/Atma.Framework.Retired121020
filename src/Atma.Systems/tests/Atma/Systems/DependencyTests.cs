namespace Atma.Systems
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Atma.Entities;
    using Divergic.Logging.Xunit;
    using Microsoft.Extensions.Logging;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public unsafe class DependencyTests
    {
        public struct Position
        {
            public int x, y;
        }

        public struct Velocity
        {
            public int x, y;
        }
        private readonly ILoggerFactory _logFactory;

        public DependencyTests(ITestOutputHelper output)
        {
            _logFactory = LogFactory.Create(output);
        }

        private unsafe class DummySystem : ISystem
        {
            private ILogger _logger;
            private Dependency[] _dependencies;
            public IEnumerable<Dependency> Dependencies => _dependencies;

            public bool Disabled { get; set; } = false;

            public int Priority { get; } = 0;

            public string Name { get; }

            private int* _execution;
            public int ExecutionOrder { get; set; }

            public DummySystem(ILoggerFactory logFactory, string name, int priority, int* execution, params Dependency[] deps)
            {
                _logger = logFactory.CreateLogger(name);
                Name = name;
                Priority = priority;
                _execution = execution;
                _dependencies = deps;
            }

            public void Init()
            {

            }

            public void Tick(SystemManager systemManager, EntityManager entityManager)
            {
                _logger.LogDebug(Name);
                ExecutionOrder = (*_execution)++;
            }

            public override string ToString() => Name;
        }

        [Fact]
        public void ShouldUpdateBefore()
        {
            var sm = new SystemManager(_logFactory, null);
            var execution = stackalloc int[1];

            var render = sm.Add(new DummySystem(_logFactory, "Render", 0, execution));
            var update = sm.Add(new DummySystem(_logFactory, "Update", 0, execution, new BeforeDependency("Render")));

            sm.Init();
            sm.Tick();

            update.ExecutionOrder.ShouldBe(0);
            render.ExecutionOrder.ShouldBe(1);
        }


        [Fact]
        public void ShouldUpdateAfter()
        {
            var sm = new SystemManager(_logFactory, null);
            var execution = stackalloc int[1];

            var render = sm.Add(new DummySystem(_logFactory, "Render", 0, execution, new AfterDependency("Update")));
            var update = sm.Add(new DummySystem(_logFactory, "Update", 0, execution));

            sm.Init();
            sm.Tick();

            update.ExecutionOrder.ShouldBe(0);
            render.ExecutionOrder.ShouldBe(1);
        }

        [Fact]
        public void ShouldUpdateReadAndWrite()
        {
            var sm = new SystemManager(_logFactory, null);
            var execution = stackalloc int[1];

            var readPosition = sm.Add(new DummySystem(_logFactory, "read", 0, execution, new ReadDependency<Position>()));
            var writePosition = sm.Add(new DummySystem(_logFactory, "write", 0, execution, new WriteDependency<Position>(), new ReadDependency<Velocity>()));

            sm.Init();
            sm.Tick();

            writePosition.ExecutionOrder.ShouldBe(0);
            readPosition.ExecutionOrder.ShouldBe(1);
        }

        [Fact]
        public void ShouldThrowCyclic()
        {
            var sm = new SystemManager(_logFactory, null);
            var execution = stackalloc int[1];

            var readPosition = sm.Add(new DummySystem(_logFactory, "read", 0, execution, new ReadDependency<Position>(), new WriteDependency<Velocity>()));
            var writePosition = sm.Add(new DummySystem(_logFactory, "write", 0, execution, new WriteDependency<Position>(), new ReadDependency<Velocity>()));

            Should.Throw<Exception>(() => sm.Init());
        }

        [Fact]
        public void ShouldNotThrowCyclic()
        {
            var sm = new SystemManager(_logFactory, null);
            var execution = stackalloc int[1];

            var readPosition = sm.Add(new DummySystem(_logFactory, "read", 0, execution, new ReadDependency<Position>(), new WriteDependency<Velocity>()));
            var writePosition = sm.Add(new DummySystem(_logFactory, "write", -1, execution, new WriteDependency<Position>(), new ReadDependency<Velocity>()));

            sm.Init();
            sm.Tick();

            writePosition.ExecutionOrder.ShouldBe(0);
            readPosition.ExecutionOrder.ShouldBe(1);
        }

        [Fact]
        public void ShouldAggregateAndPromoteDependencies()
        {
            var sm = new SystemManager(_logFactory, null);
            var execution = stackalloc int[1];

            var updateGroup = sm.Add(new SystemGroup(_logFactory, "Update", -1));
            var renderGroup = sm.Add(new SystemGroup(_logFactory, "Render", 0));

            var inputGroup = updateGroup.Add(new SystemGroup(_logFactory, "Input", 0));
            var inputb = inputGroup.Add(new DummySystem(_logFactory, "inputb", 0, execution, new ReadDependency<Position>(), new ReadDependency<Velocity>()));
            var inputa = inputGroup.Add(new DummySystem(_logFactory, "inputa", 0, execution, new WriteDependency<Position>()));

            var readPosition = sm.Add(new DummySystem(_logFactory, "read", 0, execution, new ReadDependency<Position>(), new WriteDependency<Velocity>()));
            var writePosition = sm.Add(new DummySystem(_logFactory, "write", -1, execution, new WriteDependency<Position>(), new ReadDependency<Velocity>()));

            sm.Init();
            sm.Tick();

            updateGroup.Dependencies.ShouldContain(new WriteDependency<Position>());
            updateGroup.Dependencies.ShouldContain(new ReadDependency<Velocity>());
            updateGroup.Dependencies.Count().ShouldBe(2);

            inputa.ExecutionOrder.ShouldBe(0);
            inputb.ExecutionOrder.ShouldBe(1);
            writePosition.ExecutionOrder.ShouldBe(2);
            readPosition.ExecutionOrder.ShouldBe(3);
        }

        // public void ShouldExecuteGroupsInOrder()
        // {
        //     var sm = new SystemManager(_logFactory);
        //     var execution = stackalloc int[1];

        //     var updateGroup = sm.Add(new SystemGroup(_logFactory,"Update", 0));
        //     var renderGroup = sm.Add(new SystemGroup(_logFactory,"Render", 0));

        //     var inputGroup = updateGroup.Add(new SystemGroup(_logFactory,"Input", 0));
        //     var inputb = inputGroup.Add(new DummySystem(_logFactory, "inputb", 0, execution, new ReadDependency<Position>(), new ReadDependency<Velocity>()));
        //     var inputa = inputGroup.Add(new DummySystem(_logFactory, "inputa", 0, execution, new WriteDependency<Position>()));


        //     var readPosition = sm.Add(new DummySystem(_logFactory, "read", 0, execution, new ReadDependency<Position>(), new WriteDependency<Velocity>()));
        //     var writePosition = sm.Add(new DummySystem(_logFactory, "write", 0, execution, new WriteDependency<Position>(), new ReadDependency<Velocity>()));

        //     sm.Init();
        //     sm.Tick(null);

        //     writePosition.ExecutionOrder.ShouldBe(0);
        //     readPosition.ExecutionOrder.ShouldBe(1);
        // }
    }
}