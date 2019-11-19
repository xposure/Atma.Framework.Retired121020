[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Atma.Entities.Tests")]
namespace Atma.Entities
{
    using Atma.Common;
    using Atma.Memory;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed partial class EntityManager : UnmanagedDispose//, IEntityManager2
    {

        public int EntityCount { get => _entityArrays.Sum(x => x.EntityCount); }
        public int SpecCount { get => _knownSpecs.Count; }
        public IReadOnlyList<EntityChunkList> EntityArrays => _entityArrays;

        private ILogger _logger;
        private ILoggerFactory _logFactory;
        private IAllocator _allocator;

        private EntityPool _entityPool;// = new EntityPool2();
        private LookupList<EntitySpec> _knownSpecs = new LookupList<EntitySpec>();
        private List<EntityChunkList> _entityArrays = new List<EntityChunkList>();

        public EntityManager(ILoggerFactory logFactory, IAllocator allocator)
        {
            _logFactory = logFactory;
            _logger = _logFactory.CreateLogger<EntityManager>();
            _allocator = allocator;

            _entityPool = new EntityPool(_logFactory, _allocator);
            //take the first one to reserve 0 as invalid
            _entityPool.Take();
        }

        private int GetOrCreateSpec(in EntitySpec spec)
        {
            var specIndex = _knownSpecs.IndexOf(spec.ID);
            if (specIndex == -1)
            {
                //we are limited to this many unique specs per EM
                specIndex = _knownSpecs.Count;
                Assert.LessThan(specIndex, Entity.SPEC_MAX);
                _knownSpecs.Add(spec.ID, spec);
                _entityArrays.Add(new EntityChunkList(_logFactory, _allocator, spec));
            }
            return specIndex;
        }

        private int GetOrCreateSpec(Span<ComponentType> componentTypes)
        {
            var specId = ComponentType.CalculateId(componentTypes);
            var specIndex = _knownSpecs.IndexOf(specId);
            if (specIndex == -1)
                return GetOrCreateSpec(new EntitySpec(specId, componentTypes));

            return specIndex;
        }

        public uint Create(in EntitySpec spec)
        {
            var specIndex = GetOrCreateSpec(spec);
            Span<uint> createdEntities = stackalloc uint[1];
            CreateInternal(specIndex, createdEntities);
            return createdEntities[0];
        }

        public uint Create(Span<ComponentType> componentTypes)
        {
            var specIndex = GetOrCreateSpec(componentTypes);
            Span<uint> createdEntities = stackalloc uint[1];
            CreateInternal(specIndex, createdEntities);
            return createdEntities[0];
        }

        public unsafe void Create(in EntitySpec spec, Span<uint> createdEntities)
        {
            var specIndex = GetOrCreateSpec(spec);
            CreateInternal(specIndex, createdEntities);
        }

        public unsafe void Create(Span<ComponentType> componentTypes, Span<uint> createdEntities)
        {
            var specIndex = GetOrCreateSpec(componentTypes);
            CreateInternal(specIndex, createdEntities);
        }

        internal unsafe void CreateInternal(int specIndex, Span<uint> entities)
        {
            var array = _entityArrays[specIndex];

            //TODO: we should work with entity refs here so we aren't looking these up again
            _entityPool.Take(entities);

            var stackCreated = stackalloc CreatedEntity[entities.Length];
            var created = new Span<CreatedEntity>(stackCreated, entities.Length);

            array.Create(entities, created);
            for (var i = 0; i < entities.Length; i++)
            {
                ref var entity = ref _entityPool[entities[i]];
                ref var createdEntity = ref created[i];
                entity = new Entity(entities[i], specIndex, createdEntity.ChunkIndex, createdEntity.Index);
            }
        }

        // internal unsafe void Assign(uint entity, ComponentType* type, ref void* src, bool oneToMany)
        // {
        //     ref var e = ref _entityPool[entity];
        //     var entities = stackalloc Entity[] { e };
        //     var slice = new NativeSlice<Entity>(entities, 1);

        //     Move(slice, type);
        //     Replace(entity, type, ref src, oneToMany);
        // }

        internal unsafe void Assign(Span<uint> entities, ComponentType* type, ref void* src, bool incrementSource)
        {
            Move(entities, type);

            //TODO: pass the entity ref array in instead of looking it up again
            SetComponentInternal(type, entities, src, incrementSource, false);
        }

        private unsafe void Move(ref NativeFixedList2<uint> entities, int srcSpecIndex, ComponentType* type)
        {
            var srcSpec = _entityArrays[srcSpecIndex].Specification;

            //we need to move the entity to the new spec
            Span<ComponentType> componentTypes = stackalloc ComponentType[srcSpec.ComponentTypes.Length + 1];
            srcSpec.ComponentTypes.CopyTo(componentTypes);

            componentTypes[srcSpec.ComponentTypes.Length] = *type;

            var specId = ComponentType.CalculateId(componentTypes);
            var dstSpecIndex = GetOrCreateSpec(componentTypes);
            Move(entities.Slice(), srcSpecIndex, dstSpecIndex);
            entities.Reset();
        }

        internal unsafe void Move(Span<uint> entities, ComponentType* type)
        {
            NativeFixedList2<uint> batch = stackalloc uint[128];

            var srcSpecIndex = -1;
            for (var i = 0; i < entities.Length; i++)
            {
                ref var entityInfo = ref _entityPool[entities[i]];
                Assert.EqualTo(Has(ref entityInfo, type->ID), false);

                //var srcSpecIndex = entityInfo.SpecIndex;
                if (entityInfo.SpecIndex != srcSpecIndex || batch.Free == 0)
                {
                    if (batch.Length > 0)
                        Move(ref batch, srcSpecIndex, type);

                    srcSpecIndex = entityInfo.SpecIndex;
                }

                batch.Add(entities[i]);
            }

            if (batch.Length > 0)
                Move(ref batch, srcSpecIndex, type);
        }

        public unsafe void Assign<T>(uint entity, in T t)
           where T : unmanaged
        {
            //assign has too move logic with moving data around so we are just going to put things on the stack and call a single function
            var componentType = stackalloc[] { ComponentType<T>.Type };
            void* data = stackalloc[] { t };
            var entities = stackalloc[] { entity };
            Assign(new Span<uint>(entities, 1), componentType, ref data, true);

            // Assert.EqualTo(Has<T>(entity), false);

            // ref var entityInfo = ref _entityPool[entity];
            // var srcSpec = _entityArrays[entityInfo.SpecIndex].Specification;

            // //we need to move the entity to the new spec
            // Span<ComponentType> componentTypes = stackalloc ComponentType[srcSpec.ComponentTypes.Length + 1];
            // srcSpec.ComponentTypes.CopyTo(componentTypes);

            // var newComponentType = ComponentType<T>.Type;
            // componentTypes[srcSpec.ComponentTypes.Length] = newComponentType;

            // var specId = ComponentType.CalculateId(componentTypes);
            // var dstSpecIndex = GetOrCreateSpec(componentTypes);
            // //var dstSpec = _knownSpecs[specIndex];

            // Move(ref entityInfo, dstSpecIndex);
            // //Move(entity, dstSpec);
            // Replace<T>(ref entityInfo, t);
        }


        public ref T Get<T>(uint entity)
            where T : unmanaged
        {
            Assert.EqualTo(Has<T>(entity), true);

            ref readonly var e = ref _entityPool[entity];

            var array = _entityArrays[e.SpecIndex];
            var chunk = array.AllChunks[e.ChunkIndex];
            var span = chunk.PackedArray.GetComponentData<T>();

            return ref span[e.Index];
        }

        public bool Has(uint entity)
        {
            return _entityPool.IsValid(entity);
        }

        public bool Has<T>(uint entity)
            where T : unmanaged
        {
            ref var e = ref _entityPool[entity];
            return Has(ref e, ComponentType<T>.Type.ID);
        }

        private bool Has(uint entity, int componentId)
        {
            ref var e = ref _entityPool[entity];
            return Has(ref e, componentId);
        }

        private bool Has(ref Entity entityInfo, int componentId)
        {
            var spec = _entityArrays[entityInfo.SpecIndex].Specification;
            return spec.Has(componentId);
        }

        public unsafe void Remove(uint entity)
        {
            Span<uint> entities = stackalloc[] { entity };
            Remove(entities);
        }

        public unsafe void Remove(Span<uint> entities) => RemoveInternal(entities, true);

        private unsafe void Remove(ref NativeFixedList2<EntityRef> removeEntities, int specIndex, int chunkIndex, bool returnId)
        {
            var array = _entityArrays[specIndex];
            var stackIndicies = stackalloc int[removeEntities.Length];
            var sliceIndicies = new Span<int>(stackIndicies, removeEntities.Length);

            var stackMovedEntities = stackalloc MovedEntity[removeEntities.Length];
            var movedEntities = new NativeFixedList<MovedEntity>(stackMovedEntities, removeEntities.Length);

            array.Delete(chunkIndex, sliceIndicies, ref movedEntities);
            for (var i = 0; i < movedEntities.Length; i++)
            {
                ref var moved = ref movedEntities[i];
                _entityPool[moved.ID] = new Entity(moved.ID, specIndex, chunkIndex, moved.Index);
            }

            if (returnId)
            {
                for (var i = 0; i < removeEntities.Length; i++)
                    _entityPool.Return(removeEntities[i].ID);
            }

            removeEntities.Reset();
        }

        internal unsafe void RemoveInternal(Span<uint> entities, bool returnId)
        {
            Span<EntityRef> batch = stackalloc EntityRef[128];
            var index = 0;
            while (index < entities.Length)
            {
                var remaining = entities.Length - index;
                var count = remaining > batch.Length ? batch.Length : remaining;

                for (var i = 0; i < count; i++)
                    batch[i] = new EntityRef(_entityPool.GetPointer(entities[index + i]));

                Remove(batch.Slice(0, count), returnId);
                index += batch.Length;
            }
        }

        internal unsafe void Remove(Span<EntityRef> entities, bool returnId)
        {
            NativeFixedList2<EntityRef> removeEntities = stackalloc EntityRef[128];

            var specIndex = -1;
            var chunkIndex = -1;
            for (var i = 0; i < entities.Length; i++)
            {
                ref var e = ref entities[i];
                if (e.SpecIndex != specIndex || e.ChunkIndex != chunkIndex || removeEntities.Free == 0)
                {
                    //flush
                    if (removeEntities.Length > 0)
                        Remove(ref removeEntities, specIndex, chunkIndex, returnId);

                    specIndex = e.SpecIndex;
                    chunkIndex = e.ChunkIndex;
                }

                removeEntities.Add(e);
            }

            if (removeEntities.Length > 0)
                Remove(ref removeEntities, specIndex, chunkIndex, returnId);
        }

        public void Remove<T>(uint entity)
          where T : unmanaged
        {
            Remove(entity, ComponentType<T>.Type.ID);
        }

        internal unsafe bool Remove(uint entity, int componentId)
        {
            Assert.EqualTo(Has(entity, componentId), true);

            ref var entityInfo = ref _entityPool[entity];
            var srcSpecIndex = entityInfo.SpecIndex;
            var srcSpec = _entityArrays[srcSpecIndex].Specification;

            var srcComponentCount = srcSpec.ComponentTypes.Length - 1;
            if (srcComponentCount == 0)
            {
                //TODO: Should we throw an exception instead?
                Remove(entity); //if there are no more components, delete the entity
                return true;
            }
            else
            {
                //we need to move the entity to the new spec
                var copyIndex = 0;
                Span<ComponentType> componentTypes = stackalloc ComponentType[srcSpec.ComponentTypes.Length - 1];
                for (var i = 0; i < srcSpec.ComponentTypes.Length; i++)
                    if (srcSpec.ComponentTypes[i].ID != componentId)
                        componentTypes[copyIndex++] = srcSpec.ComponentTypes[i];

                var specId = ComponentType.CalculateId(componentTypes);
                var dstSpecIndex = GetOrCreateSpec(componentTypes);
                Span<uint> entities = stackalloc[] { entity };

                Move(entities, srcSpecIndex, dstSpecIndex);
                return true;
            }
        }

        public void Replace<T>(uint entity, in T t)
            where T : unmanaged
        {
            ref var e = ref _entityPool[entity];
            Replace<T>(ref e, t);
        }

        public void Replace<T>(ref Entity entity, in T t)
          where T : unmanaged
        {
            Assert.Equals(Has(ref entity, ComponentType<T>.Type.ID), true);
            var array = _entityArrays[entity.SpecIndex];
            var chunk = array.AllChunks[entity.ChunkIndex];
            var span = chunk.PackedArray.GetComponentData<T>();
            span[entity.Index] = t;
        }

        public unsafe void Replace<T>(NativeArray<uint> entities, in T t)
            where T : unmanaged
        {
            var data = stackalloc[] { t };
            var componentType = stackalloc[] { ComponentType<T>.Type };
            SetComponentInternal(componentType, entities, data, true, false);
        }

        public unsafe void Replace<T>(NativeArray<uint> entities, in NativeArray<T> t)
            where T : unmanaged
        {
            Assert.EqualTo(entities.Length, t.Length);

            var componentType = stackalloc[] { ComponentType<T>.Type };
            SetComponentInternal(componentType, entities, t.RawPointer, true, false);
        }

        // internal unsafe void SetComponentInternal(EntityChunkList array, Span<Entity> slice, ComponentType* componentType, ref void* src, bool oneToMany)
        // {
        //     while (slice.Length > 0)
        //     {
        //         ref var e = ref slice[0];
        //         //TODO: sort array first 
        //         var length = slice.Length;
        //         slice = array.Copy(componentType, ref src, slice, oneToMany);
        //         Assert.NotEqualTo(slice.Length, length);
        //     }
        // }

        internal unsafe void SetComponentInternal(ComponentType* componentType, Span<uint> entities, void* src, bool incrementSource, bool allowUpdate)
        {
            //Assert.Equals(Has(ref entity, ComponentType<T>.Type.ID), true);
            //we are going to put replaces in front of the array and assigns in the back if we are allowed
            NativeFixedList2<Entity> entityRefs = stackalloc Entity[128];

            //TODO: come back to this
            //Assert.EqualTo(allowUpdate, false);

            var specIndex = -1;
            for (var i = 0; i < entities.Length; i++)
            {
                ref var e = ref _entityPool[entities[i]];
                var hasComponent = Has(ref e, componentType->ID);
                if (!allowUpdate)
                    Assert.EqualTo(hasComponent, true);

                //TODO: I think we need to flush here because moving data can call remove and reorder entities
                //if array is changed to work with entityrefs this could be fixed
                if (e.SpecIndex != specIndex || !hasComponent || entityRefs.Free == 0)
                {
                    //flush
                    if (entityRefs.Length > 0)
                    {
                        var array = _entityArrays[specIndex];
                        array.Copy(specIndex, componentType, ref src, entityRefs.Slice(), incrementSource);
                        //SetComponentInternal(array, entityRefs.Slice(), componentType, ref src, incrementSource);
                        entityRefs.Reset();
                    }
                    specIndex = e.SpecIndex;
                }

                if (!hasComponent)
                    Move(entities.Slice(i, 1), componentType);

                entityRefs.Add(e);
            }

            if (entityRefs.Length > 0)
            {
                var array = _entityArrays[specIndex];
                array.Copy(specIndex, componentType, ref src, entityRefs.Slice(), incrementSource);
            }
        }

        public void Reset(uint entity)
        {
            ref var e = ref _entityPool[entity];
            var array = _entityArrays[e.SpecIndex];
            var chunk = array.AllChunks[e.ChunkIndex];
            chunk.PackedArray.Reset(e.Index);
        }

        public void Reset<T>(uint entity)
            where T : unmanaged
        {
            ref var e = ref _entityPool[entity];
            var array = _entityArrays[e.SpecIndex];
            var chunk = array.AllChunks[e.ChunkIndex];
            chunk.PackedArray.Reset<T>(e.Index);
        }

        public void Update<T>(uint entity, in T t)
            where T : unmanaged
        {
            ref var e = ref _entityPool[entity];
            var spec = _entityArrays[e.SpecIndex].Specification;
            var componentType = ComponentType<T>.Type;
            if (spec.Has(componentType))
                Replace<T>(ref e, t);
            else
                Assign<T>(entity, t);
        }

        // internal unsafe void Update(ComponentType* type, uint entity, ref void* ptr, bool oneToMany)
        // {
        //     ref var e = ref _entityPool[entity];
        //     var spec = _entityArrays[e.SpecIndex].Specification;
        //     if (spec.Has(type->ID))
        //         Replace(entity, type, ref ptr, oneToMany);
        //     else
        //         Assign(entity, type, ref ptr, oneToMany);
        // }

        internal unsafe void UpdateInternal(ComponentType* componentType, Span<uint> entities, void* src, bool incrementSource)
        {
            //Assert.Equals(Has(ref entity, ComponentType<T>.Type.ID), true);

            NativeFixedList2<Entity> entityRefs = stackalloc Entity[128];
            //Move(entities, componentType);

            var specIndex = -1;
            for (var i = 0; i < entities.Length; i++)
            {
                ref var e = ref _entityPool[entities[i]];
                Assert.Equals(Has(ref e, componentType->ID), true);
                if (e.SpecIndex != specIndex)
                {
                    //flush
                    if (entityRefs.Length > 0 || entityRefs.Free == 0)
                    {
                        var array = _entityArrays[specIndex];
                        array.Copy(specIndex, componentType, ref src, entityRefs.Slice(), incrementSource);
                        //SetComponentInternal(array, entityRefs.Slice(), componentType, ref src, incrementSource);
                        entityRefs.Reset();
                    }
                    specIndex = e.SpecIndex;
                }

                entityRefs.Add(e);
            }

            if (entityRefs.Length > 0)
            {
                var array = _entityArrays[specIndex];
                array.Copy(specIndex, componentType, ref src, entityRefs.Slice(), incrementSource);
            }
        }

        public unsafe void Move(uint entity, in EntitySpec spec)
        {
            var dstSpecIndex = GetOrCreateSpec(spec);
            ref var e = ref _entityPool[entity];
            Span<uint> entities = stackalloc[] { entity };

            Move(entities, e.SpecIndex, dstSpecIndex);
        }

        private void Move(Span<uint> entities, int srcSpecIndex, int dstSpecIndex)
        {
            var index = 0;
            var src = _entityArrays[srcSpecIndex];
            var dst = _entityArrays[dstSpecIndex];

            Span<CreatedEntity> createdEntities = stackalloc CreatedEntity[1024];
            while (index < entities.Length)
            {
                //var segment = index * createdEntities.Length;
                var remaining = entities.Length - index;
                var count = remaining > createdEntities.Length ? createdEntities.Length : remaining;

                var workingEntities = entities.Slice(index, count);

                dst.Create(workingEntities, createdEntities);

                for (var i = 0; i < count; i++)
                {
                    ref var createdEntity = ref createdEntities[i];
                    ref var entity = ref _entityPool[entities[index + i]];

                    var srcChunk = src.AllChunks[entity.ChunkIndex];
                    var dstChunk = dst.AllChunks[createdEntity.ChunkIndex];

                    //TODO: see if we can batch this? it would require both src and dst to be linear
                    //src can be anywhere and unordered
                    //dst will be linear but could span chunks
                    ComponentPackedArray.CopyTo(srcChunk.PackedArray, entity.Index, dstChunk.PackedArray, createdEntity.Index);

                    var moved = src.Delete(entity.ChunkIndex, entity.Index);
                    if (moved.DidMove)
                    {
                        ref var movedEntity = ref _entityPool[moved.ID];
                        movedEntity = new Entity(moved.ID, srcSpecIndex, movedEntity.ChunkIndex, moved.Index);
                    }

                    entity = new Entity(entity.ID, dstSpecIndex, createdEntity.ChunkIndex, createdEntity.Index);
                }

                index += count;
            }
        }

        protected override void OnUnmanagedDispose()
        {
            _entityArrays.DisposeAll();
            _entityArrays.Clear();

            _entityPool.Dispose();
        }
    }
}
