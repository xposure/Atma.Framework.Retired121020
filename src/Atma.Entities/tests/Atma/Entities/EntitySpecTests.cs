namespace Atma.Entities
{
    using System;
    using System.Collections.Generic;
    using Shouldly;

    public class EntitySpecTests
    {
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

        public void ArchetypeComponentsShouldBeSorted()
        {
            var specs = new[]
            {
                new EntitySpec(ComponentType<Valid>.Type, ComponentType<Valid2>.Type, ComponentType<Valid3>.Type),
                new EntitySpec(ComponentType<Valid2>.Type, ComponentType<Valid>.Type, ComponentType<Valid3>.Type),
                new EntitySpec(ComponentType<Valid3>.Type, ComponentType<Valid>.Type, ComponentType<Valid2>.Type)

            };

            for (var x = 0; x < specs.Length; x++)
            {
                var archetype = specs[x];
                for (var y = 0; y < archetype.ComponentTypes.Length - 1; y++)
                {
                    var c0 = archetype.ComponentTypes[y];
                    var c1 = archetype.ComponentTypes[y + 1];

                    c0.ID.ShouldBeLessThan(c1.ID);
                }
            }
        }

        public void ShouldHaveAll()
        {
            var spec = new EntitySpec(ComponentType<Valid>.Type, ComponentType<Valid2>.Type, ComponentType<Valid3>.Type, ComponentType<Valid4>.Type);

            var valid = new[]
            {
                new EntitySpec(ComponentType<Entity>.Type, ComponentType<Valid>.Type, ComponentType<Valid2>.Type, ComponentType<Valid3>.Type),
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


        public void ArchetypeShouldFindMatches()
        {
            var specs = new[]
            {
                new EntitySpec(ComponentType<Valid>.Type, ComponentType<Valid2>.Type, ComponentType<Valid6>.Type, ComponentType<Valid5>.Type),
                new EntitySpec(ComponentType<Valid4>.Type,ComponentType<Valid2>.Type,ComponentType<Valid3>.Type, ComponentType<Valid5>.Type),
                new EntitySpec(ComponentType<Valid6>.Type, ComponentType<Valid3>.Type, ComponentType<Valid>.Type, ComponentType<Valid4>.Type)
            };

            Span<ComponentType> temp = stackalloc ComponentType[24];

            var c0 = specs[0].FindMatches(specs[1], temp);
            var m0 = new List<ComponentType>(temp.Slice(c0).ToArray());

            var c1 = specs[1].FindMatches(specs[2], temp);
            var m1 = new List<ComponentType>(temp.Slice(c1).ToArray());

            var c2 = specs[2].FindMatches(specs[0], temp);
            var m2 = new List<ComponentType>(temp.Slice(c2).ToArray());

            m0.ShouldContain(x => x.ID == ComponentType<Valid2>.Type.ID);
            m0.ShouldContain(x => x.ID == ComponentType<Valid5>.Type.ID);
            m0.ShouldNotContain(x => x.ID == ComponentType<Valid>.Type.ID);
            m0.ShouldNotContain(x => x.ID == ComponentType<Valid3>.Type.ID);
            m0.ShouldNotContain(x => x.ID == ComponentType<Valid6>.Type.ID);

            m1.ShouldContain(x => x.ID == ComponentType<Valid4>.Type.ID);
            m1.ShouldContain(x => x.ID == ComponentType<Valid3>.Type.ID);
            m1.ShouldNotContain(x => x.ID == ComponentType<Valid>.Type.ID);
            m1.ShouldNotContain(x => x.ID == ComponentType<Valid6>.Type.ID);
            m1.ShouldNotContain(x => x.ID == ComponentType<Valid2>.Type.ID);

            m2.ShouldContain(x => x.ID == ComponentType<Valid6>.Type.ID);
            m2.ShouldContain(x => x.ID == ComponentType<Valid>.Type.ID);
            m2.ShouldNotContain(x => x.ID == ComponentType<Valid3>.Type.ID);
            m2.ShouldNotContain(x => x.ID == ComponentType<Valid4>.Type.ID);
            m2.ShouldNotContain(x => x.ID == ComponentType<Valid2>.Type.ID);

        }
    }
}