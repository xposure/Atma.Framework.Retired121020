namespace Atma.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Shouldly;
    using Xunit;

    public class EntitySpecTests
    {
        [Fact]
        public void ShouldGenerateSameHashcode()
        {
            var specs = new[]
            {
                new EntitySpec(ComponentType<Valid>.Type, ComponentType<Valid2>.Type, ComponentType<Valid3>.Type),
                new EntitySpec(ComponentType<Valid2>.Type, ComponentType<Valid>.Type, ComponentType<Valid3>.Type),
                new EntitySpec(ComponentType<Valid3>.Type, ComponentType<Valid>.Type, ComponentType<Valid2>.Type)
            };

            for (var x = 0; x < specs.Length - 1; x++)
            {
                var a1 = specs[x];
                for (var y = x + 1; y < specs.Length; y++)
                {
                    var a2 = specs[y];
                    a1.EntitySize.ShouldBe(a2.EntitySize);
                    a1.ID.ShouldBe(a2.ID);
                }
            }
        }

        //this is no longer a requirement ComponentType stack methods do the sort using ints and insertion sort
        // [Fact]
        // public void SpecComponentsShouldBeSorted()
        // {
        //     var specs = new[]
        //     {
        //         new EntitySpec(ComponentType<Valid>.Type, ComponentType<Valid2>.Type, ComponentType<Valid3>.Type),
        //         new EntitySpec(ComponentType<Valid2>.Type, ComponentType<Valid>.Type, ComponentType<Valid3>.Type),
        //         new EntitySpec(ComponentType<Valid3>.Type, ComponentType<Valid>.Type, ComponentType<Valid2>.Type)
        //     };

        //     for (var x = 0; x < specs.Length; x++)
        //     {
        //         var spec = specs[x];
        //         for (var y = 0; y < spec.ComponentTypes.Length - 1; y++)
        //         {
        //             var c0 = spec.ComponentTypes[y];
        //             var c1 = spec.ComponentTypes[y + 1];

        //             c0.ID.ShouldBeLessThan(c1.ID);
        //         }
        //     }
        // }

        [Fact]
        public void ShouldHaveAll()
        {
            var spec = new EntitySpec(ComponentType<Valid>.Type, ComponentType<Valid2>.Type, ComponentType<Valid3>.Type, ComponentType<Valid4>.Type);

            var valid = new[]
            {
                new EntitySpec(ComponentType<Valid>.Type, ComponentType<Valid2>.Type, ComponentType<Valid3>.Type),
                new EntitySpec(ComponentType<Valid2>.Type, ComponentType<Valid3>.Type, ComponentType<Valid4>.Type),
                new EntitySpec(ComponentType<Valid>.Type, ComponentType<Valid4>.Type),
                new EntitySpec(ComponentType<Valid>.Type),
                new EntitySpec(ComponentType<Valid>.Type, ComponentType<Valid2>.Type, ComponentType<Valid3>.Type, ComponentType<Valid4>.Type)
            };

            for (var i = 0; i < valid.Length; i++)
                spec.HasAll(valid[i]).ShouldBe(true);

            var invalid = new[]
            {
                new EntitySpec(ComponentType<Valid>.Type, ComponentType<Valid2>.Type, ComponentType<Valid3>.Type, ComponentType<Valid6>.Type),
                new EntitySpec(ComponentType<Valid2>.Type, ComponentType<Valid3>.Type, ComponentType<Valid6>.Type, ComponentType<Valid4>.Type),
                new EntitySpec(ComponentType<Valid5>.Type, ComponentType<Valid>.Type, ComponentType<Valid4>.Type, ComponentType<Valid6>.Type),
                new EntitySpec(ComponentType<Valid>.Type, ComponentType<Valid2>.Type, ComponentType<Valid3>.Type, ComponentType<Valid5>.Type, ComponentType<Valid4>.Type),
                new EntitySpec(ComponentType<Valid6>.Type)
            };

            for (var i = 0; i < invalid.Length; i++)
                spec.HasAll(invalid[i]).ShouldBe(false);

            var spec0 = new EntitySpec(ComponentType<Valid6>.Type);
            var spec1 = new EntitySpec(ComponentType<Valid>.Type);
            spec0.HasAll(spec1).ShouldBe(false);
        }

        [Fact]
        public void ShouldHaveAny()
        {
            var spec = new EntitySpec(
                ComponentType<Valid>.Type,
                ComponentType<Valid2>.Type,
                ComponentType<Valid3>.Type,
                ComponentType<Valid4>.Type
            );

            var valid = new[]
            {
                new EntitySpec(ComponentType<Valid>.Type, ComponentType<Valid2>.Type, ComponentType<Valid3>.Type),
                new EntitySpec(ComponentType<Valid2>.Type, ComponentType<Valid6>.Type, ComponentType<Valid4>.Type),
                new EntitySpec(ComponentType<Valid>.Type, ComponentType<Valid4>.Type),
                new EntitySpec(ComponentType<Valid>.Type),
                new EntitySpec(ComponentType<Valid5>.Type, ComponentType<Valid2>.Type, ComponentType<Valid3>.Type, ComponentType<Valid4>.Type)
            };

            for (var i = 0; i < valid.Length; i++)
                spec.HasAny(valid[i]).ShouldBe(true);

            var invalid = new[]
            {
                new EntitySpec(ComponentType<Valid6>.Type),
                new EntitySpec(ComponentType<Valid5>.Type),
                new EntitySpec(ComponentType<Valid6>.Type, ComponentType<Valid5>.Type)
            };

            for (var i = 0; i < invalid.Length; i++)
                spec.HasAny(invalid[i]).ShouldBe(false);
        }


        [Fact]
        public void SpecShouldFindMatches()
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
            var c1 = specs[1].FindMatches(specs[2], componentTypes0);
            var m1 = new List<ComponentType>(componentTypes0.Slice(0, c1).ToArray());
            var c2 = specs[2].FindMatches(specs[0], componentTypes0);
            var m2 = new List<ComponentType>(componentTypes0.Slice(0, c2).ToArray());

            m0.Any(x => x.ID == ComponentType<Valid2>.Type.ID).ShouldBe(true);
            m0.Any(x => x.ID == ComponentType<Valid5>.Type.ID).ShouldBe(true);
            m0.Any(x => x.ID == ComponentType<Valid>.Type.ID).ShouldBe(false);
            m0.Any(x => x.ID == ComponentType<Valid3>.Type.ID).ShouldBe(false);
            m0.Any(x => x.ID == ComponentType<Valid6>.Type.ID).ShouldBe(false);

            m1.Any(x => x.ID == ComponentType<Valid4>.Type.ID).ShouldBe(true);
            m1.Any(x => x.ID == ComponentType<Valid3>.Type.ID).ShouldBe(true);
            m1.Any(x => x.ID == ComponentType<Valid>.Type.ID).ShouldBe(false);
            m1.Any(x => x.ID == ComponentType<Valid6>.Type.ID).ShouldBe(false);
            m1.Any(x => x.ID == ComponentType<Valid2>.Type.ID).ShouldBe(false);

            m2.Any(x => x.ID == ComponentType<Valid6>.Type.ID).ShouldBe(true);
            m2.Any(x => x.ID == ComponentType<Valid>.Type.ID).ShouldBe(true);
            m2.Any(x => x.ID == ComponentType<Valid3>.Type.ID).ShouldBe(false);
            m2.Any(x => x.ID == ComponentType<Valid4>.Type.ID).ShouldBe(false);
            m2.Any(x => x.ID == ComponentType<Valid2>.Type.ID).ShouldBe(false);

        }

        [Fact]
        public void EntitySpecGroupShouldMatch()
        {
            var a = new EntitySpec(new IEntitySpecGroup[] { new GroupA() { HashCode = 1 } }, ComponentType<Valid>.Type, ComponentType<Valid2>.Type);
            var b = new EntitySpec(new IEntitySpecGroup[] { new GroupA() { HashCode = 1 } }, ComponentType<Valid2>.Type, ComponentType<Valid>.Type);

            a.ID.ShouldBe(b.ID);
            a.GetGroupData<GroupA>().HashCode.ShouldBe(1);
            b.GetGroupData<GroupA>().HashCode.ShouldBe(1);
        }

        [Fact]
        public void EntitySpecGroupShouldMatchAll()
        {
            var a = new EntitySpec(new IEntitySpecGroup[] { new GroupA() { HashCode = 1 }, new GroupB() { HashCode = 2 } }, ComponentType<Valid>.Type, ComponentType<Valid2>.Type);
            var b = new EntitySpec(new IEntitySpecGroup[] { new GroupB() { HashCode = 2 }, new GroupA() { HashCode = 1 } }, ComponentType<Valid2>.Type, ComponentType<Valid>.Type);

            a.ID.ShouldBe(b.ID);
            a.GetGroupData<GroupA>().HashCode.ShouldBe(1);
            b.GetGroupData<GroupA>().HashCode.ShouldBe(1);
            a.GetGroupData<GroupB>().HashCode.ShouldBe(2);
            b.GetGroupData<GroupB>().HashCode.ShouldBe(2);
        }

        [Fact]
        public void EntitySpecGroupShouldNotMatchHash()
        {
            var a = new EntitySpec(new IEntitySpecGroup[] { new GroupA() { HashCode = 1 } }, ComponentType<Valid>.Type, ComponentType<Valid2>.Type);
            var b = new EntitySpec(new IEntitySpecGroup[] { new GroupA() { HashCode = 2 } }, ComponentType<Valid2>.Type, ComponentType<Valid>.Type);

            a.ID.ShouldNotBe(b.ID);
            a.GetGroupData<GroupA>().HashCode.ShouldBe(1);
            b.GetGroupData<GroupA>().HashCode.ShouldBe(2);
        }

        [Fact]
        public void EntitySpecGroupShouldNotMatchDifferentGroup()
        {
            var a = new EntitySpec(new IEntitySpecGroup[] { new GroupA() { HashCode = 1 } }, ComponentType<Valid>.Type, ComponentType<Valid2>.Type);
            var b = new EntitySpec(new IEntitySpecGroup[] { new GroupB() { HashCode = 1 } }, ComponentType<Valid2>.Type, ComponentType<Valid>.Type);

            a.ID.ShouldNotBe(b.ID);
            a.GetGroupData<GroupA>().HashCode.ShouldBe(1);
            b.GetGroupData<GroupB>().HashCode.ShouldBe(1);
        }
    }
}