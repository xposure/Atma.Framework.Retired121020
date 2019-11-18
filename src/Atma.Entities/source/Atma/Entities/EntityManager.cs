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
        public IReadOnlyList<EntityChunkArray> EntityArrays => _entityArrays;

        private ILogger _logger;
        private ILoggerFactory _logFactory;
        private IAllocator _allocator;

        private EntityPool _entityPool;// = new EntityPool2();
        private LookupList<EntitySpec> _knownSpecs = new LookupList<EntitySpec>();
        private List<EntityChunkArray> _entityArrays = new List<EntityChunkArray>();

        public EntityManager(ILoggerFactory logFactory, IAllocator allocator)
        {
            _logFactory = logFactory;
            _logger = _logFactory.CreateLogger<EntityManager>();
            _allocator = allocator;

            _entityPool = new EntityPool(_logFactory, _allocator);
            //take the first one to reserve 0 as invalid
            _entityPool.Take();
        }


        private int GetOrCreateSpec(EntitySpec spec)
        {
            var specIndex = _knownSpecs.IndexOf(spec.ID);
            if (specIndex == -1)
            {
                //we are limited to this many unique specs per EM
                specIndex = _knownSpecs.Count;
                Assert.LessThan(specIndex, Entity.SPEC_MAX);
                _knownSpecs.Add(spec.ID, spec);
                _entityArrays.Add(new EntityChunkArray(_logFactory, _allocator, spec));
            }
            return specIndex;
        }

        private int GetOrCreateSpec(Span<ComponentType> componentTypes)
        {
            var specId = ComponentType.CalculateId(componentTypes);
            var specIndex = _knownSpecs.IndexOf(specId);
            if (specIndex == -1)
            {
                var spec = new EntitySpec(specId, componentTypes);
                //we are limited to this many unique specs per EM
                specIndex = _knownSpecs.Count;
                Assert.LessThan(specIndex, Entity.SPEC_MAX);
                _knownSpecs.Add(spec.ID, spec);
                _entityArrays.Add(new EntityChunkArray(_logFactory, _allocator, spec));
            }
            return specIndex;
        }

        public uint Create(EntitySpec spec)
        {
            var specIndex = GetOrCreateSpec(spec);
            var entity = _entityPool.Take();
            ref var e = ref _entityPool[entity];

            var index = _entityArrays[specIndex].Create(entity, out var chunkIndex);
            e = new Entity(entity, specIndex, chunkIndex, index);
            return entity;
        }

        public uint Create(Span<ComponentType> componentTypes)
        {
            var specIndex = GetOrCreateSpec(componentTypes);
            var entity = _entityPool.Take();
            ref var e = ref _entityPool[entity];

            var index = _entityArrays[specIndex].Create(entity, out var chunkIndex);
            e = new Entity(entity, specIndex, chunkIndex, index);
            return entity;
        }

        public void Create(EntitySpec spec, NativeArray<uint> entities) => Create(spec.ComponentTypes, entities);

        public unsafe void Create(Span<ComponentType> componentTypes, Span<uint> entities)
        {
            var specId = ComponentType.CalculateId(componentTypes);
            var specIndex = GetOrCreateSpec(componentTypes);
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

        // internal uint Create(int specId)
        // {
        //     var specIndex = _knownSpecs.IndexOf(specId);
        //     Assert.GreatherThan(specIndex, -1);
        //     var spec = _knownSpecs[specIndex];
        //     return Create(spec);
        // }
        private void GetEntities(Span<uint> ids, Span<Entity> entities)
        {

        }

        // internal unsafe void Assign(uint entity, ComponentType* type, ref void* src, bool oneToMany)
        // {
        //     ref var e = ref _entityPool[entity];
        //     var entities = stackalloc Entity[] { e };
        //     var slice = new NativeSlice<Entity>(entities, 1);

        //     Move(slice, type);
        //     Replace(entity, type, ref src, oneToMany);
        // }

        internal unsafe void Assign(Span<uint> entities, ComponentType* type, ref void* src, bool oneToMany)
        {
            using var entityRefs = new NativeArray<EntityRef>(_allocator, entities.Length);

            for (var i = 0; i < entities.Length; i++)
            {
                entityRefs[i] = new EntityRef(_entityPool.GetPointer(entities[i]));
                Assert.EqualTo(Has(ref entityRefs[i], type->ID), false);
            }

            Move(entityRefs, type);

            //TODO: pass the entity ref array in instead of looking it up again
            SetComponentInternal(type, entities, src, oneToMany, false);
        }

        internal unsafe void Move(Span<EntityRef> entities, ComponentType* type)
        {
            var lastIndex = 0;
            for (var i = 0; i < entities.Length; i++)
            {
                ref var entityInfo = ref entities[i];
                Assert.EqualTo(Has(ref entityInfo, type->ID), false);
                var srcSpecIndex = entityInfo.SpecIndex;
                var srcSpec = _entityArrays[entityInfo.SpecIndex].Specification;

                //we need to move the entity to the new spec
                Span<ComponentType> componentTypes = stackalloc ComponentType[srcSpec.ComponentTypes.Length + 1];
                srcSpec.ComponentTypes.CopyTo(componentTypes);

                componentTypes[srcSpec.ComponentTypes.Length] = *type;

                var specId = ComponentType.CalculateId(componentTypes);
                var dstSpecIndex = GetOrCreateSpec(componentTypes);
                //var dstSpec = _knownSpecs[specIndex];


                while (i < entities.Length - 1 && entities[i + 1].SpecIndex == srcSpecIndex)
                    i++;


                Move(entities.Slice(lastIndex, i - lastIndex + 1), dstSpecIndex);
                lastIndex = i + 1;

                // Move(ref entityInfo, dstSpecIndex);
                // _entityPool[entityInfo.ID] = entityInfo;
                // while (i < entities.Length - 1 && entities[i + 1].SpecIndex == srcSpecIndex)
                // {
                //     Move(ref entities[++i], dstSpecIndex);
                //     _entityPool[entityInfo.ID] = entityInfo;
                // }
            }
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

        private bool Has(ref EntityRef entityInfo, int componentId)
        {
            var spec = _entityArrays[entityInfo.SpecIndex].Specification;
            return spec.Has(componentId);
        }

        public unsafe void Remove(uint entity)
        {
            var entities = stackalloc[] { entity };
            Remove(new Span<uint>(entities, 1));
        }

        public unsafe void Remove(Span<uint> entities)
        {
            var temp = stackalloc EntityRef[256];
            var entityRefs = new NativeFixedList<EntityRef>(temp, 256);

            for (var i = 0; i < entities.Length; i++)
            {
                entityRefs.Add(new EntityRef(_entityPool.GetPointer(entities[i])));
                if (entityRefs.Free == 0)
                {
                    Remove(entityRefs, true);
                    entityRefs.Reset();
                }
            }

            if (entityRefs.Length > 0)
                Remove(entityRefs, true);
        }

        private unsafe void Remove(ref EntityRef e, bool returnId)
        {
            var temp = stackalloc[] { e };
            var slice = new Span<EntityRef>(temp, 1);
            Remove(slice, returnId);
        }

        private unsafe void Remove(ref NativeFixedList<EntityRef> removeEntities, int specIndex, int chunkIndex, bool returnId)
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

        internal unsafe void Remove(Span<EntityRef> entities, bool returnId)
        {
            var tempEntities = stackalloc EntityRef[128];
            var removeEntities = new NativeFixedList<EntityRef>(tempEntities, 128);

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
            var srcSpec = _entityArrays[entityInfo.SpecIndex].Specification;

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
                var temp = stackalloc[] { new EntityRef(_entityPool.GetPointer(entity)) };
                var slice = new Span<EntityRef>(temp, 1);

                Move(slice, dstSpecIndex);
                return true;
            }
        }

        public void Replace<T>(uint entity, in T t)
            where T : unmanaged
        {
            ref var e = ref _entityPool[entity];
            Replace<T>(ref e, t);
        }

        internal unsafe void Replace(uint entity, ComponentType* type, ref void* ptr, bool oneToMany)
        {
            ref var e = ref _entityPool[entity];
            Assert.Equals(Has(ref e, type->ID), true);
            var array = _entityArrays[e.SpecIndex];
            var chunk = array.AllChunks[e.ChunkIndex];
            var componentIndex = array.Specification.GetComponentIndex(type->ID);
            chunk.PackedArray.Copy(componentIndex, ref ptr, e.Index, 1, oneToMany);
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
            SetComponentInternal(componentType, entities, t.RawPointer, false, false);
        }

        internal unsafe void SetComponentInternal(EntityChunkArray array, Span<Entity> slice, ComponentType* componentType, ref void* src, bool oneToMany)
        {
            while (slice.Length > 0)
            {
                ref var e = ref slice[0];
                var length = slice.Length;
                slice = array.Copy(componentType, ref src, slice, !oneToMany);
                Assert.NotEqualTo(slice.Length, length);
            }
        }

        internal unsafe void SetComponentInternal(ComponentType* componentType, Span<uint> entities, void* src, bool oneToMany, bool allowUpdate)
        {
            //Assert.Equals(Has(ref entity, ComponentType<T>.Type.ID), true);
            //we are going to put replaces in front of the array and assigns in the back if we are allowed
            using var entityRefs = new NativeList<Entity>(_allocator, entities.Length);

            //TODO: come back to this
            //Assert.EqualTo(allowUpdate, false);

            var specIndex = -1;
            for (var i = 0; i < entities.Length; i++)
            {
                ref var e = ref _entityPool[entities[i]];
                if (e.SpecIndex != specIndex)
                {
                    //flush
                    if (entityRefs.Length > 0)
                    {
                        var array = _entityArrays[specIndex];
                        SetComponentInternal(array, entityRefs.Slice(), componentType, ref src, oneToMany);
                        entityRefs.Reset();
                    }
                    specIndex = e.SpecIndex;
                }
                if (Has(ref e, componentType->ID))
                {
                    entityRefs.Add(e);
                }
                else
                {
                    //TODO: modifying data can have serious considerations during looping but I think we really only need to worry aboout remove, since the rest is append only
                    //TODO: assign is already one at a time and slow, so might as well stackalloc
                    var ep = stackalloc[] { entities[i] };
                    Assign(new Span<uint>(ep, 1), componentType, ref src, oneToMany);
                    Assert.EqualTo(true, false); //no update yet
                    //Assert.EqualTo(allowAssign, true);
                    //entityRefs[assign--] = e;
                }
            }

            if (entityRefs.Length > 0)
            {
                var array = _entityArrays[specIndex];
                SetComponentInternal(array, entityRefs.Slice(), componentType, ref src, oneToMany);
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

        internal unsafe void UpdateInternal(ComponentType* componentType, Span<uint> entities, void* src, bool oneToMany)
        {
            //Assert.Equals(Has(ref entity, ComponentType<T>.Type.ID), true);
            using var entityRefs = new NativeArray<Entity>(_allocator, entities.Length);
            for (var i = 0; i < entities.Length; i++)
            {
                ref var e = ref _entityPool[entities[i]];
                Assert.Equals(Has(ref e, componentType->ID), true);
                entityRefs[i] = e;
            }

            var slice = entityRefs.Slice();
            while (slice.Length > 0)
            {
                ref var e = ref slice[0];
                var array = _entityArrays[e.SpecIndex];
                var length = slice.Length;
                slice = array.Copy(componentType, ref src, slice, oneToMany);
                Assert.NotEqualTo(slice.Length, length);
            }
        }

        public unsafe void Move(uint entity, EntitySpec spec)
        {
            var specIndex = GetOrCreateSpec(spec);
            var temp = stackalloc[] { new EntityRef(_entityPool.GetPointer(entity)) };
            var slice = new Span<EntityRef>(temp, 1);

            Move(slice, specIndex);
        }

        private void Move(Span<EntityRef> entities, int dstSpecIndex)
        {
            for (var i = 0; i < entities.Length; i++)
            {
                ref var entityInfo = ref entities[i];
                var src = _entityArrays[entityInfo.SpecIndex];
                var dst = _entityArrays[dstSpecIndex];

                //we want to create the data at the dst first
                var srcIndex = entityInfo.Index;
                var dstIndex = dst.Create(entityInfo.ID, out var dstChunkIndex);
                var srcChunkIndex = entityInfo.ChunkIndex;

                var srcChunk = src.AllChunks[srcChunkIndex];
                var dstChunk = dst.AllChunks[dstChunkIndex];

                ComponentPackedArray.CopyTo(srcChunk.PackedArray, srcIndex, dstChunk.PackedArray, dstIndex);

                //this internal function will swap the entity at the end of the array
                //without returning the id to the pool
                Remove(ref entityInfo, false);

                //then we want to remap the entity to its new location
                entityInfo.Replace(new Entity(entityInfo.ID, dstSpecIndex, dstChunkIndex, dstIndex));
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
