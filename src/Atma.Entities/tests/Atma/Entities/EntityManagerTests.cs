namespace Atma.Entities
{
    using Atma.Common;
    using Shouldly;

    using System;
    using System.Linq;

    public class EntityManagerTests
    {
        public void ShouldCreateValidArchetype()
        {
            var _manager = GetEntityManager();
            var archetype = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3));

            archetype.IsValid.ShouldBe(true);
            archetype.EntitySize.ShouldBe(16);
        }

        public void ShouldThrowOnInvalidType()
        {
            var _manager = GetEntityManager();
            Should.Throw<Exception>(() =>
            {
                var archetype = _manager.CreateArchetype(typeof(Invalid));
            });
        }
        //TODO: appveyor
        // public void ShouldThrowOnDuplicateType()
        // {
        //     var _manager = GetEntityManager();
        //     Should.Throw<Exception>(() =>
        //     {
        //         var archetype = _manager.CreateArchetype(typeof(Valid), typeof(Valid));
        //     });
        // }

        public void ShouldGenerateSameHashcode()
        {
            var _manager = GetEntityManager();
            var archetypes = new[]{ _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3)),
                _manager.CreateArchetype(typeof(Valid2), typeof(Valid), typeof(Valid3)),
                _manager.CreateArchetype(typeof(Valid3), typeof(Valid), typeof(Valid2))
            };

            for (var x = 0; x < archetypes.Length - 1; x++)
            {
                var a1 = archetypes[x];
                for (var y = x + 1; y < archetypes.Length; y++)
                {
                    var a2 = archetypes[y];
                    a1.EntitySize.ShouldBe(a2.EntitySize);
                }
            }
        }

        public void ArchetypeComponentsShouldBeSorted()
        {
            var _manager = GetEntityManager();
            var archetypes = new[]
            {
                _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3)),
                _manager.CreateArchetype(typeof(Valid2), typeof(Valid), typeof(Valid3)),
                _manager.CreateArchetype(typeof(Valid3), typeof(Valid), typeof(Valid2))
            };

            for (var x = 0; x < archetypes.Length; x++)
            {
                var archetype = archetypes[x];
                for (var y = 0; y < archetype.ComponentTypes.Length - 1; y++)
                {
                    var c0 = archetype.ComponentTypes[y];
                    var c1 = archetype.ComponentTypes[y + 1];

                    c0.ID.ShouldBeLessThan(c1.ID);
                }
            }
        }

        public void ArchetypeShouldFindMatches()
        {
            var _manager = GetEntityManager();
            var archetypes = new[]
            {
                _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid6), typeof(Valid5)),
                _manager.CreateArchetype(typeof(Valid4),typeof(Valid2),typeof(Valid3), typeof(Valid5)),
                _manager.CreateArchetype( typeof(Valid6), typeof(Valid3), typeof(Valid), typeof(Valid4))
            };

            var m0 = EntityArchetype.FindMatches(archetypes[0], archetypes[1]).ToList();
            var m1 = EntityArchetype.FindMatches(archetypes[1], archetypes[2]).ToList();
            var m2 = EntityArchetype.FindMatches(archetypes[2], archetypes[0]).ToList();

            m0.ShouldContain(x => x.ID == typeof(Valid2).GetHashCode());
            m0.ShouldContain(x => x.ID == typeof(Valid5).GetHashCode());
            m0.ShouldNotContain(x => x.ID == typeof(Valid).GetHashCode());
            m0.ShouldNotContain(x => x.ID == typeof(Valid3).GetHashCode());
            m0.ShouldNotContain(x => x.ID == typeof(Valid6).GetHashCode());

            m1.ShouldContain(x => x.ID == typeof(Valid4).GetHashCode());
            m1.ShouldContain(x => x.ID == typeof(Valid3).GetHashCode());
            m1.ShouldNotContain(x => x.ID == typeof(Valid).GetHashCode());
            m1.ShouldNotContain(x => x.ID == typeof(Valid6).GetHashCode());
            m1.ShouldNotContain(x => x.ID == typeof(Valid2).GetHashCode());

            m2.ShouldContain(x => x.ID == typeof(Valid6).GetHashCode());
            m2.ShouldContain(x => x.ID == typeof(Valid).GetHashCode());
            m2.ShouldNotContain(x => x.ID == typeof(Valid3).GetHashCode());
            m2.ShouldNotContain(x => x.ID == typeof(Valid4).GetHashCode());
            m2.ShouldNotContain(x => x.ID == typeof(Valid2).GetHashCode());

        }

        public void ShouldManagesEntity()
        {
            var _manager = GetEntityManager();
            var archetype = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3));
            var entity = _manager.CreateEntity(archetype);

            //should create a new entity
            entity.ShouldBe(1);

            //should have this entity
            _manager.HasEntity(entity).ShouldBe(true);

            //should delete this entity
            _manager.DestroyEntity(entity);

            //should no longer have this entity
            _manager.HasEntity(entity).ShouldBe(false);

            //should create a new entity and reuse the id
            entity = _manager.CreateEntity(archetype);
            entity.ShouldBe(1);

            //should increment id
            _manager.CreateEntity(archetype).ShouldBe(2);

        }

        public void EntityCountOnArchetype()
        {
            var _manager = GetEntityManager();
            var archetype = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3));
            var entity = _manager.CreateEntity(archetype);
            archetype.Count.ShouldBe(1);

            entity = _manager.CreateEntity(archetype);
            archetype.Count.ShouldBe(2);

            _manager.DestroyEntity(1);
            archetype.Count.ShouldBe(1);

            _manager.DestroyEntity(2);
            archetype.Count.ShouldBe(0);
        }

        public void EntitySetComponent()
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

        public void EntityShouldChangeArchetypes()
        {
            var _manager = GetEntityManager();
            var archetypeBase = _manager.CreateArchetype(typeof(Valid), typeof(Valid2));
            var entity = _manager.CreateEntity(archetypeBase);

            var archetype1 = _manager.GetEntityArchetype(entity);

            //should auto move the entity to new archetype
            var setComponentData = new Valid3() { W = 1 };
            _manager.SetComponentData(entity, setComponentData);

            var archetype2 = _manager.GetEntityArchetype(entity);
            archetype1.ID.ShouldNotBe(archetype2.ID);

            //should move the entity back to the original archetype
            _manager.RemoveComponentData<Valid3>(entity);
            archetype2 = _manager.GetEntityArchetype(entity);

            archetype2.ID.ShouldBe(archetype1.ID);

            //should move the entity back to the 2nd archetype
            archetype2 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3));
            _manager.MoveEntity(entity, archetype2);
            _manager.GetEntityArchetype(entity).ID.ShouldBe(archetype2.ID);
        }

        public void ShouldHaveAll()
        {
            var _manager = GetEntityManager();
            var archetypeBase = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3), typeof(Valid4));

            var valid = new[]
            {
                _manager.CreateArchetype(typeof(Entity), typeof(Valid), typeof(Valid2), typeof(Valid3)),
                _manager.CreateArchetype(typeof(Valid2), typeof(Valid3), typeof(Valid4)),
                _manager.CreateArchetype(typeof(Valid), typeof(Valid4)),
                _manager.CreateArchetype(typeof(Valid)),
                _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3), typeof(Valid4))
            };

            for (var i = 0; i < valid.Length; i++)
                EntityArchetype.HasAll(archetypeBase, valid[i]).ShouldBe(true);

            var invalid = new[]
            {
                _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3), typeof(Valid6)),
                _manager.CreateArchetype(typeof(Valid2), typeof(Valid3), typeof(Valid6), typeof(Valid4)),
                _manager.CreateArchetype(typeof(Valid5), typeof(Valid), typeof(Valid4), typeof(Valid6)),
                _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3), typeof(Valid5), typeof(Valid4)),
                _manager.CreateArchetype(typeof(Valid6))
            };

            for (var i = 0; i < invalid.Length; i++)
                EntityArchetype.HasAll(archetypeBase, invalid[i]).ShouldBe(false);

            var archetype0 = _manager.CreateArchetype(typeof(Valid6));
            var archetype1 = _manager.CreateArchetype(typeof(Valid));
            EntityArchetype.HasAll(archetype0, archetype1).ShouldBe(false);

        }

        public void ShouldHaveAny()
        {
            var _manager = GetEntityManager();
            var archetypeBase = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3), typeof(Valid4));

            var valid = new[]
            {
                _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid3)),
                _manager.CreateArchetype(typeof(Valid2), typeof(Valid6), typeof(Valid4)),
                _manager.CreateArchetype(typeof(Valid), typeof(Valid4)),
                _manager.CreateArchetype(typeof(Valid)),
                _manager.CreateArchetype(typeof(Valid5), typeof(Valid2), typeof(Valid3), typeof(Valid4))
            };

            for (var i = 0; i < valid.Length; i++)
                EntityArchetype.HasAny(archetypeBase, valid[i]).ShouldBe(true);

            var invalid = new[]
            {
                _manager.CreateArchetype(typeof(Valid6)),
                _manager.CreateArchetype(typeof(Valid5)),
                _manager.CreateArchetype(typeof(Valid6), typeof(Valid5))
            };

            for (var i = 0; i < invalid.Length; i++)
                EntityArchetype.HasAny(archetypeBase, invalid[i]).ShouldBe(false);
        }

        public void ShouldFilter()
        {
            var _manager = GetEntityManager();
            var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2));
            var archetype1 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3));
            var archetype2 = _manager.CreateArchetype(typeof(Valid), typeof(Valid4));
            var archetype3 = _manager.CreateArchetype(typeof(Valid6));

            var entity0 = _manager.CreateEntity(archetype0);
            var entity1 = _manager.CreateEntity(archetype0);
            var entity2 = _manager.CreateEntity(archetype0);

            var entity3 = _manager.CreateEntity(archetype1);
            var entity4 = _manager.CreateEntity(archetype1);

            var entity5 = _manager.CreateEntity(archetype2);
            var entity6 = _manager.CreateEntity(archetype2);

            var entity7 = _manager.CreateEntity(archetype3);
            var entity8 = _manager.CreateEntity(archetype3);

            _manager.FilterArchetypes<ShouldFilterStruct>().Count().ShouldBe(3);
            _manager.FilterArchetypes<ShouldFilterStruct>().Sum(x => x.Count).ShouldBe(7);

            _manager.DestroyEntity(entity5);
            _manager.DestroyEntity(entity6);

            _manager.FilterArchetypes<ShouldFilterStruct>().Count().ShouldBe(2);
            _manager.FilterArchetypes<ShouldFilterStruct>().Sum(x => x.Count).ShouldBe(5);

        }

        //TODO: appveyor
        // public void EntityArchetypeArrayAccess()
        // {
        //     var _manager = GetEntityManager();
        //     var archetype0 = _manager.CreateArchetype(typeof(Valid));
        //     var entity0 = _manager.CreateEntity(archetype0);

        //     //var query = EntityQueryBuilder.Create(b =>
        //     //    b.Query(q =>
        //     //        q.Writes<Valid>()
        //     //    )
        //     //);

        //     foreach (var archetype in _manager.FilterArchetypes<ShouldFilterStruct>())
        //     {
        //         archetype.GetComponentIndex<Valid>().ShouldBe(0);
        //         archetype.GetComponentIndex<Valid2>().ShouldBe(-1);

        //         foreach (var chunk in archetype.ActiveChunks)
        //         {
        //             using var valids = chunk.GetReadComponent<Valid>();
        //             valids.Length.ShouldBe(1);

        //             Should.NotThrow(() =>
        //             {
        //                 using var x = chunk.GetReadComponent<Valid>();
        //             });

        //             Should.Throw<Exception>(() =>
        //             {
        //                 using var x = chunk.GetWriteComponent<Valid>();
        //             });
        //         }
        //     }
        // }

        //TODO: appveyor
        // public void ShouldThrowWhileReadWrite()
        // {
        //     var _manager = GetEntityManager();
        //     var archetype0 = _manager.CreateArchetype(typeof(Valid));
        //     var entity0 = _manager.CreateEntity(archetype0);

        //     foreach (var archetype in _manager.FilterArchetypes<ShouldFilterStruct>())
        //     {
        //         archetype.GetComponentIndex<Valid>().ShouldBe(0);
        //         archetype.GetComponentIndex<Valid2>().ShouldBe(-1);

        //         foreach (var chunk in archetype.ActiveChunks)
        //         {
        //             using var valids = chunk.GetReadComponent<Valid>();
        //             valids.Length.ShouldBe(1);

        //             Should.NotThrow(() =>
        //             {
        //                 using var x = chunk.GetReadComponent<Valid>();
        //             });

        //             Should.Throw<Exception>(() =>
        //             {
        //                 using var x = chunk.GetWriteComponent<Valid>();
        //             });
        //         }
        //     }
        // }

        public void EntityViewXRefArchetypes()
        {
            var _manager = GetEntityManager();
            var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2));
            var archetype1 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3), typeof(Valid2));
            var entity0 = _manager.CreateEntity(archetype0);
            var entity1 = _manager.CreateEntity(archetype1);

            _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
            _manager.SetComponentData(entity0, new Valid2() { Z = 12 });

            _manager.SetComponentData(entity1, new Valid() { X = 20, Y = 21 });
            _manager.SetComponentData(entity1, new Valid2() { Z = 22 });
            _manager.SetComponentData(entity1, new Valid3() { W = 23 });

            var result = _manager.Project<Group>().ToArray();
            result.Length.ShouldBe(2);
            result[0].valid.X.ShouldBe(10);
            result[0].valid.Y.ShouldBe(11);
            result[0].valid2.Z.ShouldBe(12);

            result[1].valid.X.ShouldBe(20);
            result[1].valid.Y.ShouldBe(21);
            result[1].valid2.Z.ShouldBe(22);
        }

        public void EntityProjectArrays()
        {
            var _manager = GetEntityManager();
            var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2));
            var archetype1 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3), typeof(Valid2));
            var entity0 = _manager.CreateEntity(archetype0);
            var entity1 = _manager.CreateEntity(archetype1);
            var entity2 = _manager.CreateEntity(archetype1);

            _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
            _manager.SetComponentData(entity0, new Valid2() { Z = 12 });

            _manager.SetComponentData(entity1, new Valid() { X = 20, Y = 21 });
            _manager.SetComponentData(entity1, new Valid2() { Z = 22 });
            _manager.SetComponentData(entity1, new Valid3() { W = 23 });

            var result = _manager.Project<GroupArray>().ToArray();
            result.Length.ShouldBe(2);
            result[0].entity[0].ID.ShouldBe(1);
            result[0].valid[0].X.ShouldBe(10);
            result[0].valid[0].Y.ShouldBe(11);
            result[0].valid2[0].Z.ShouldBe(12);
            result[0].length.ShouldBe(1);

            result[1].entity[0].ID.ShouldBe(2);
            result[1].valid[0].X.ShouldBe(20);
            result[1].valid[0].Y.ShouldBe(21);
            result[1].valid2[0].Z.ShouldBe(22);
            result[1].length.ShouldBe(2);

            _manager.SetComponentData(entity0, new Valid() { X = 33, Y = 34 });
            result[0].entity[0].ID.ShouldBe(1);
            result[0].valid[0].X.ShouldBe(33);
            result[0].valid[0].Y.ShouldBe(34);

            _manager.DestroyEntity(entity2);
            result = _manager.Project<GroupArray>().ToArray();
            result[1].length.ShouldBe(1);
        }

        public void EntityFirst()
        {
            var _manager = GetEntityManager();
            var archetype1 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3), typeof(Valid2));
            var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2));
            var entity1 = _manager.CreateEntity(archetype1);
            var entity0 = _manager.CreateEntity(archetype0);

            _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
            _manager.SetComponentData(entity0, new Valid2() { Z = 12 });

            _manager.SetComponentData(entity1, new Valid() { X = 20, Y = 21 });
            _manager.SetComponentData(entity1, new Valid2() { Z = 22 });
            _manager.SetComponentData(entity1, new Valid3() { W = 23 });

            _manager.First<Group>(out var entityGroup).ShouldBe(true);
            entityGroup.valid.X.ShouldBe(20);
            entityGroup.valid.Y.ShouldBe(21);
            entityGroup.valid2.Z.ShouldBe(22);
        }

        public void EntityUpdate()
        {
            var _manager = GetEntityManager();
            var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3), typeof(Valid2));
            var entity0 = _manager.CreateEntity(archetype0);

            _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
            _manager.SetComponentData(entity0, new Valid2() { Z = 12 });

            _manager.First<Group>(out var entityGroup).ShouldBe(true);
            entityGroup.valid = new Valid() { X = 20, Y = 21 };
            _manager.Update(entityGroup, entity0);
            _manager.First<Group>(out entityGroup).ShouldBe(true);
            entityGroup.valid.X.ShouldBe(20);
            entityGroup.valid.Y.ShouldBe(21);
        }

        public unsafe void EntityProjectPointer()
        {
            var _manager = GetEntityManager();
            var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3), typeof(Valid2));
            var entity0 = _manager.CreateEntity(archetype0);
            var entity1 = _manager.CreateEntity(archetype0);

            _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
            _manager.SetComponentData(entity1, new Valid() { X = 20, Y = 21 });

            var results = _manager.Project<ValidPtr>().ToList();

            results.Count.ShouldBe(2);
            results[0].Valid->X.ShouldBe(10);
            results[0].Valid->Y.ShouldBe(11);
            results[1].Valid->X.ShouldBe(20);
            results[1].Valid->Y.ShouldBe(21);

        }

        public void EntityUpdateWithEntity()
        {
            var _manager = GetEntityManager();
            var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3), typeof(Valid2));
            var entity0 = _manager.CreateEntity(archetype0);

            _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
            _manager.SetComponentData(entity0, new Valid2() { Z = 12 });

            _manager.First<GroupWithEntity2>(out var entityGroup).ShouldBe(true);
            entityGroup.valid = new Valid() { X = 20, Y = 21 };
            _manager.Update(entityGroup);
            _manager.First(out entityGroup).ShouldBe(true);
            entityGroup.valid.X.ShouldBe(20);
            entityGroup.valid.Y.ShouldBe(21);
        }

        public void AnyComponent()
        {
            var _manager = GetEntityManager();
            var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid4));
            var archetype1 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3), typeof(Valid2));
            var archetype2 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3), typeof(Valid5));
            var entity0 = _manager.CreateEntity(archetype0);
            var entity1 = _manager.CreateEntity(archetype1);
            var entity2 = _manager.CreateEntity(archetype2);

            _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
            _manager.SetComponentData(entity0, new Valid2() { Z = 12 });
            _manager.SetComponentData(entity0, new Valid4() { _ = 13 });

            _manager.SetComponentData(entity1, new Valid() { X = 20, Y = 21 });
            _manager.SetComponentData(entity1, new Valid2() { Z = 22 });
            _manager.SetComponentData(entity1, new Valid3() { W = 23 });

            _manager.SetComponentData(entity2, new Valid() { X = 30, Y = 31 });
            _manager.SetComponentData(entity2, new Valid2() { Z = 32 });
            _manager.SetComponentData(entity2, new Valid3() { W = 33 });
            _manager.SetComponentData(entity2, new Valid4() { _ = 34 });

            var entityGroup = _manager.Project<GroupConditional>().ToArray();
            entityGroup.Length.ShouldBe(3);
            entityGroup[0].valid.X.ShouldBe(10);
            entityGroup[0].valid.Y.ShouldBe(11);
            entityGroup[0].valid2.Z.ShouldBe(12);
            entityGroup[0].valid3.W.ShouldBe(0);

            entityGroup[1].valid.X.ShouldBe(20);
            entityGroup[1].valid.Y.ShouldBe(21);
            entityGroup[1].valid2.Z.ShouldBe(22);
            entityGroup[1].valid3.W.ShouldBe(23);

            entityGroup[2].valid.X.ShouldBe(30);
            entityGroup[2].valid.Y.ShouldBe(31);
            entityGroup[2].valid2.Z.ShouldBe(32);
            entityGroup[2].valid3.W.ShouldBe(33);

        }

        public void IgnoreComponent()
        {
            var _manager = GetEntityManager();
            var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid5));
            var archetype1 = _manager.CreateArchetype(typeof(Valid), typeof(Valid3), typeof(Valid2));
            var archetype2 = _manager.CreateArchetype(typeof(Valid), typeof(Valid5), typeof(Valid2), typeof(Valid6));
            var entity0 = _manager.CreateEntity(archetype0);
            var entity1 = _manager.CreateEntity(archetype1);
            var entity2 = _manager.CreateEntity(archetype2);

            _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
            _manager.SetComponentData(entity0, new Valid2() { Z = 12 });
            _manager.SetComponentData(entity0, new Valid5() { _ = 13 });

            _manager.SetComponentData(entity1, new Valid() { X = 20, Y = 21 });
            _manager.SetComponentData(entity1, new Valid2() { Z = 22 });
            _manager.SetComponentData(entity1, new Valid3() { W = 23 });

            _manager.SetComponentData(entity2, new Valid() { X = 30, Y = 31 });
            _manager.SetComponentData(entity2, new Valid2() { Z = 32 });
            _manager.SetComponentData(entity2, new Valid5() { _ = 34 });

            var entityGroup = _manager.Project<GroupIgnore>().ToArray();
            entityGroup.Length.ShouldBe(2);

        }

        public void GetsGroupWithEntity()
        {
            var _manager = GetEntityManager();
            var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid5));
            var entity0 = _manager.CreateEntity(archetype0);

            _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
            _manager.SetComponentData(entity0, new Valid2() { Z = 12 });
            _manager.SetComponentData(entity0, new Valid5() { _ = 13 });

            var entityGroup = _manager.Project<GroupWithEntity>().ToArray();
            entityGroup.Length.ShouldBe(1);
            entityGroup[0].entity.ArchetypeIndex.ShouldBe(1);
            entityGroup[0].entity.ChunkIndex.ShouldBe(0);
            entityGroup[0].entity.Index.ShouldBe(0);
            entityGroup[0].entity.ID.ShouldBe(entity0);
        }


        public void ShouldProjectToList()
        {
            var _manager = GetEntityManager();
            var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid5));

            for (var i = 0; i < 100; i++)
            {
                var entity0 = _manager.CreateEntity(archetype0);

                _manager.SetComponentData(entity0, new Valid() { X = i, Y = i + 11 });
                _manager.SetComponentData(entity0, new Valid2() { Z = i + 12 });
                _manager.SetComponentData(entity0, new Valid5() { _ = i + 13 });
            }


            using var entities = _manager.ProjectToList<GroupWithEntity>();
            entities.Length.ShouldBe(100);
            for (var i = 0; i < 100; i++)
            {
                entities[i].entity.ID.ShouldBe(i + 1);
                entities[i].valid2.Z.ShouldBe(i + 12);
                entities[i].valid5._.ShouldBe(i + 13);
            }
        }

        public void EntityPool()
        {
            var entityPool = new EntityPool();

            for (var i = 0; i < 10000; i++)
                entityPool.Take().ShouldBe(i);
        }

        public void BulkCreateArray()
        {
            var _manager = GetEntityManager();
            var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid5));

            _manager.CreateEntity(archetype0, 8192, out var entities);
            for (var i = 0; i < entities.Length; i++)
                entities[i].ShouldBe(i + 1);
        }
        public void EntityShouldSetIndex()
        {
            var entity = new Entity(0, 0, 0, 0);
            for (var i = 0; i < Entity.MAX_ENTITIES_PER_CHUNK; i++)
            {
                entity.Index = i;
                entity.Index.ShouldBe(i);
            }
        }

        public void PancakesBug()
        {
            var _manager = GetEntityManager();
            var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid5));


        }

        public void BulkEntityCreateAndDelete()
        {
            var sample = 4096 * 3;
            var _manager = GetEntityManager();
            var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid5));

            for (var k = 0; k < 4; k++)
            {
                for (var i = 1; i < sample; i++)
                {
                    //if(i == 4095)
                    //_manager.CreateEntity(archetype0);
                    //else
                    var x = _manager.CreateEntity(archetype0);

                    archetype0.Count.ShouldBe(i);
                    _manager.HasEntity(i).ShouldBe(true);
                }

                for (int i = sample - 1; i >= 1; i--)
                {
                    _manager.DestroyEntity(i);
                    archetype0.Count.ShouldBe(i - 1);
                    _manager.HasEntity(i).ShouldBe(false);
                }
            }
        }

        public void EntityViewShouldUpdate()
        {
            var sample = 4096;
            var _manager = GetEntityManager();
            var archetype0 = _manager.CreateArchetype(typeof(Valid), typeof(Valid2), typeof(Valid5));

            for (var i = 0; i < sample; i++)
            {
                var entity0 = _manager.CreateEntity(archetype0);
                _manager.SetComponentData(entity0, new Valid() { X = 10, Y = 11 });
                _manager.SetComponentData(entity0, new Valid2() { Z = 12 });
                _manager.SetComponentData(entity0, new Valid5() { _ = 13 });
            }

            var view = _manager.CreateView<GroupArray>();
            var result = view.ForEach().ToArray();
            result.Length.ShouldBe(sample / (int)Entity.MAX_ENTITIES_PER_CHUNK);

            foreach (var entityGroup in result)
                entityGroup.length.ShouldBe((int)Entity.MAX_ENTITIES_PER_CHUNK);

            for (var i = 0; i < sample; i += 2)
                _manager.RemoveComponentData<Valid2>(i + 1);

            //_manager.RemoveComponentData<Valid2>(1024 + 1);


            view = _manager.CreateView<GroupArray>();
            result = view.ForEach().ToArray();
            result.Length.ShouldBe(sample / (int)Entity.MAX_ENTITIES_PER_CHUNK);

            foreach (var entityGroup in result)
                entityGroup.length.ShouldBe((int)Entity.MAX_ENTITIES_PER_CHUNK / 2);


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
