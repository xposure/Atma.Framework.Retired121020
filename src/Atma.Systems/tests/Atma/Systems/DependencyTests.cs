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
            private DependencyList _dependencies;
            public DependencyList Dependencies => _dependencies;

            public bool Disabled { get; set; } = false;

            public int Priority { get; } = 0;

            public string Name { get; }

            private int* _execution;
            public int ExecutionOrder { get; set; }

            public DummySystem(ILoggerFactory logFactory, string name, int priority, int* execution, Action<DependencyListConfig> config = null)
            {
                _logger = logFactory.CreateLogger(name);
                Name = name;
                Priority = priority;
                _execution = execution;
                _dependencies = new DependencyList(name, priority, config);
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
        public void WriteShouldTrumpRead()
        {
            var a = new DependencyList("a", 0, deps => deps.Write<Position>());
            var b = new DependencyList("b", 0, deps => deps.Read<Position>());

            a.IsDependentOn(b).ShouldBe(false);
            b.IsDependentOn(a).ShouldBe(true);
        }

        [Fact]
        public void PriorityShouldTrumpRead()
        {
            var a = new DependencyList("a", -1, deps => deps.Read<Position>());
            var b = new DependencyList("b", 0, deps => deps.Read<Position>());

            a.IsDependentOn(b).ShouldBe(false);
            b.IsDependentOn(a).ShouldBe(true);
        }

        [Fact]
        public void PriorityShouldTrumpWrite()
        {
            var a = new DependencyList("a", -1, deps => deps.Read<Position>());
            var b = new DependencyList("b", 0, deps => deps.Write<Position>());

            a.IsDependentOn(b).ShouldBe(false);
            b.IsDependentOn(a).ShouldBe(true);
        }

        [Fact]
        public void BeforeShouldTrumpPriority()
        {
            var a = new DependencyList("a", 0, deps => deps.Before("b"));
            var b = new DependencyList("b", -1, deps => deps.Read<Position>());

            a.IsDependentOn(b).ShouldBe(false);
            b.IsDependentOn(a).ShouldBe(true);
        }

        [Fact]
        public void BeforeShouldTrumpWrite()
        {
            var a = new DependencyList("a", 0, deps => deps.Before("b"));
            var b = new DependencyList("b", 0, deps => deps.Write<Position>());

            a.IsDependentOn(b).ShouldBe(false);
            b.IsDependentOn(a).ShouldBe(true);
        }

        [Fact]
        public void AfterShouldTrumpWrite()
        {
            var a = new DependencyList("a", 0, deps => deps.After("b").Write<Position>());
            var b = new DependencyList("b", 0, deps => deps.Read<Position>());

            a.IsDependentOn(b).ShouldBe(true);
            b.IsDependentOn(a).ShouldBe(false);
        }

        [Fact]
        public void AfterShouldTrumpPriority()
        {
            var a = new DependencyList("a", 0, deps => deps.Read<Position>());
            var b = new DependencyList("b", -1, deps => deps.After("a"));

            a.IsDependentOn(b).ShouldBe(false);
            b.IsDependentOn(a).ShouldBe(true);
        }

        [Fact]
        public void ShouldMergeReads()
        {
            var a = new DependencyList("a", 0, deps => deps.Read<Position>());
            var b = new DependencyList("b", 0, deps => deps.Read<Velocity>());

            var c = DependencyList.MergeComponents("c", 0, new[] { a, b });
            c._readComponents.Count.ShouldBe(2);
            c._readComponents.ShouldContain(ComponentType<Position>.Type);
            c._readComponents.ShouldContain(ComponentType<Velocity>.Type);
            c._writeComponents.Count.ShouldBe(0);
            c._allComponents.Count.ShouldBe(2);
        }

        [Fact]
        public void ShouldMergeWrites()
        {
            var a = new DependencyList("a", 0, deps => deps.Write<Position>());
            var b = new DependencyList("b", 0, deps => deps.Write<Velocity>());

            var c = DependencyList.MergeComponents("c", 0, new[] { a, b });
            c._writeComponents.Count.ShouldBe(2);
            c._writeComponents.ShouldContain(ComponentType<Position>.Type);
            c._writeComponents.ShouldContain(ComponentType<Velocity>.Type);
            c._readComponents.Count.ShouldBe(0);
            c._allComponents.Count.ShouldBe(2);
        }

        [Fact]
        public void ShouldMergeReadsAndWrites()
        {
            var a = new DependencyList("a", 0, deps => deps.Read<Position>());
            var b = new DependencyList("b", 0, deps => deps.Write<Velocity>());

            var c = DependencyList.MergeComponents("c", 0, new[] { a, b });
            c._readComponents.Count.ShouldBe(1);
            c._readComponents.ShouldContain(ComponentType<Position>.Type);
            c._writeComponents.Count.ShouldBe(1);
            c._writeComponents.ShouldContain(ComponentType<Velocity>.Type);
            c._allComponents.Count.ShouldBe(2);
        }

        [Fact]
        public void ShouldPromoteReadToWrite()
        {
            var a = new DependencyList("a", 0, deps => deps.Read<Position>());
            var b = new DependencyList("b", 0, deps => deps.Write<Position>());

            var c = DependencyList.MergeComponents("c", 0, new[] { a, b });
            c._writeComponents.Count.ShouldBe(1);
            c._writeComponents.ShouldContain(ComponentType<Position>.Type);
            c._readComponents.Count.ShouldBe(0);
            c._allComponents.Count.ShouldBe(1);
        }

        [Fact]
        public void WriteIsDependentOnWrite()
        {
            var a = new DependencyList("a", 0, deps => deps.Write<Position>());
            var b = new DependencyList("b", 0, deps => deps.Write<Position>());

            a.IsDependentOn(b).ShouldBe(true);
            b.IsDependentOn(a).ShouldBe(true);
        }

        [Fact]
        public void WriteIsNotDependentOnWriteWithPriorty()
        {
            var a = new DependencyList("a", 0, deps => deps.Write<Position>());
            var b = new DependencyList("b", -1, deps => deps.Write<Position>());

            a.IsDependentOn(b).ShouldBe(true);
            b.IsDependentOn(a).ShouldBe(false);
        }

        [Fact]
        public void WriteIsNotDependentOnWriteWithBefore()
        {
            var a = new DependencyList("a", 0, deps => deps.Write<Position>());
            var b = new DependencyList("b", 0, deps => deps.Write<Position>().Before("a"));

            a.IsDependentOn(b).ShouldBe(true);
            b.IsDependentOn(a).ShouldBe(false);
        }

        [Fact]
        public void WriteIsNotDependentOnWriteWithAfter()
        {
            var a = new DependencyList("a", 0, deps => deps.Write<Position>().After("b"));
            var b = new DependencyList("b", 0, deps => deps.Write<Position>());

            a.IsDependentOn(b).ShouldBe(true);
            b.IsDependentOn(a).ShouldBe(false);
        }
    }
}