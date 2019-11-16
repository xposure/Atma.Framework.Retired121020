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

        public void Create(Span<ComponentType> componentTypes, NativeSlice<uint> entities)
        {
            var specId = ComponentType.CalculateId(componentTypes);
            var specIndex = GetOrCreateSpec(componentTypes);
            var array = _entityArrays[specIndex];

            _entityPool.Take(entities);
            array.Create(entities);
        }

        // internal uint Create(int specId)
        // {
        //     var specIndex = _knownSpecs.IndexOf(specId);
        //     Assert.GreatherThan(specIndex, -1);
        //     var spec = _knownSpecs[specIndex];
        //     return Create(spec);
        // }
        private void GetEntities(NativeSlice<uint> ids, NativeSlice<Entity> entities)
        {

        }

        internal unsafe void Assign(uint entity, ComponentType* type, void* src, bool oneToMany)
        {
            ref var e = ref _entityPool[entity];
            var entities = stackalloc Entity[] { e };
            var slice = new NativeSlice<Entity>(entities, 1);

            Move(slice, type);
            Replace(entity, type, src);
        }

        internal unsafe void Assign(NativeSlice<uint> entities, ComponentType* type, void* src, bool oneToMany)
        {
            using var entityRefs = new NativeArray<Entity>(_allocator, entities.Length);

            for (var i = 0; i < entities.Length; i++)
            {
                ref var e = ref _entityPool[entities[i]];
                entityRefs[i] = e;
                Assert.EqualTo(Has(ref e, type->ID), false);
            }

            Move(entityRefs, type);

            //TODO: pass the entity ref array in instead of looking it up again
            SetComponentInternal(type, entities, src, oneToMany, false);
        }

        internal unsafe void Move(NativeSlice<Entity> entities, ComponentType* type)
        {
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

                Move(ref entityInfo, dstSpecIndex);
                while (i < entities.Length && entities[i + 1].SpecIndex == srcSpecIndex)
                    Move(ref entities[++i], dstSpecIndex);
            }
        }


        public unsafe void Assign<T>(uint entity, in T t)
           where T : unmanaged
        {
            //assign has too move logic with moving data around so we are just going to put things on the stack and call a single function
            var componentType = stackalloc[] { ComponentType<T>.Type };
            var data = stackalloc[] { t };
            var entities = stackalloc[] { entity };
            Assign(new NativeSlice<uint>(entities, 1), componentType, data, true);

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
            var span = chunk.PackedArray.GetComponentSpan<T>();

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
            var entities = stackalloc[] { entity };
            Remove(new NativeSlice<uint>(entities, 1));
        }

        public void Remove(NativeSlice<uint> entities)
        {
            for (var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                ref var e = ref _entityPool[entity];
                Remove(ref e);
                _entityPool.Return(entity);
            }
        }

        private void Remove(ref Entity e)
        {
            var array = _entityArrays[e.SpecIndex];

            //we are moving the last element to the element deleted
            //we need to patch the array, but the array doesn't store ids            
            var movedIndex = array.Delete(e.ChunkIndex, e.Index);
            if (movedIndex > -1)
            {
                var chunk = array.AllChunks[e.ChunkIndex];
                var movedId = chunk.GetEntity(movedIndex);
                ref var movedEntity = ref _entityPool[movedId];
                movedEntity = new Entity(movedId, movedEntity.SpecIndex, movedEntity.ChunkIndex, movedIndex);
            }
        }

        public void Remove<T>(uint entity)
          where T : unmanaged
        {
            Remove(entity, ComponentType<T>.Type.ID);
        }

        internal bool Remove(uint entity, int componentId)
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

                Move(ref entityInfo, dstSpecIndex);
                return true;
            }
        }

        public void Replace<T>(uint entity, in T t)
            where T : unmanaged
        {
            ref var e = ref _entityPool[entity];
            Replace<T>(ref e, t);
        }

        internal unsafe void Replace(uint entity, ComponentType* type, void* ptr)
        {
            ref var e = ref _entityPool[entity];
            Assert.Equals(Has(ref e, type->ID), true);
            var array = _entityArrays[e.SpecIndex];
            var chunk = array.AllChunks[e.ChunkIndex];
            chunk.PackedArray.Copy(type, ptr, e.Index);
        }

        public void Replace<T>(ref Entity entity, in T t)
          where T : unmanaged
        {
            Assert.Equals(Has(ref entity, ComponentType<T>.Type.ID), true);
            var array = _entityArrays[entity.SpecIndex];
            var chunk = array.AllChunks[entity.ChunkIndex];
            var span = chunk.PackedArray.GetComponentSpan<T>();
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

        internal unsafe void SetComponentInternal(EntityChunkArray array, NativeSlice<Entity> slice, ComponentType* componentType, void* src, bool oneToMany)
        {
            while (slice.Length > 0)
            {
                ref var e = ref slice[0];
                var length = slice.Length;
                slice = array.Copy(componentType, ref src, slice, oneToMany);
                Assert.NotEqualTo(slice.Length, length);
            }
        }

        internal unsafe void SetComponentInternal(ComponentType* componentType, NativeSlice<uint> entities, void* src, bool oneToMany, bool allowAssign)
        {
            //Assert.Equals(Has(ref entity, ComponentType<T>.Type.ID), true);
            //we are going to put replaces in front of the array and assigns in the back if we are allowed
            using var entityRefs = new NativeList<Entity>(_allocator, entities.Length);

            //TODO: come back to this
            Assert.EqualTo(allowAssign, false);

            //this whole function needs rewrote, we need to do this in a loop and group specIndices together
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
                        SetComponentInternal(array, entityRefs.Slice(), componentType, src, oneToMany);
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
                    Assert.EqualTo(true, false); //no update yet
                    //Assert.EqualTo(allowAssign, true);
                    //entityRefs[assign--] = e;
                }
            }

            if (entityRefs.Length > 0)
            {
                var array = _entityArrays[specIndex];
                SetComponentInternal(array, entityRefs.Slice(), componentType, src, oneToMany);
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

        internal unsafe void Update(uint entity, ComponentType* type, void* ptr)
        {
            ref var e = ref _entityPool[entity];
            var spec = _entityArrays[e.SpecIndex].Specification;
            if (spec.Has(type->ID))
                Replace(entity, type, ptr);
            else
                Assign(entity, type, ptr, true);
        }

        internal unsafe void UpdateInternal(ComponentType* componentType, NativeSlice<uint> entities, void* src, bool oneToMany)
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

        public void Move(uint entity, EntitySpec spec)
        {
            ref var e = ref _entityPool[entity];
            var specIndex = GetOrCreateSpec(spec);
            Move(ref e, specIndex);
        }

        private void Move(ref Entity entityInfo, int dstSpecIndex)
        {
            var src = _entityArrays[entityInfo.SpecIndex];
            var dst = _entityArrays[dstSpecIndex];

            //we want to create the data at the dst first
            var srcIndex = entityInfo.Index;
            var dstIndex = dst.Create(entityInfo.ID, out var dstChunkIndex);
            var srcChunkIndex = entityInfo.ChunkIndex;

            var srcChunk = src.AllChunks[srcChunkIndex];
            var dstChunk = dst.AllChunks[dstChunkIndex];

            EntityPackedArray.CopyTo(srcChunk.PackedArray, srcIndex, dstChunk.PackedArray, dstIndex);

            //this internal function will swap the entity at the end of the array
            //without returning the id to the pool
            Remove(ref entityInfo);

            //then we want to remap the entity to its new location
            entityInfo = new Entity(entityInfo.ID, dstSpecIndex, dstChunkIndex, dstIndex);
        }

        protected override void OnUnmanagedDispose()
        {
            _entityArrays.DisposeAll();
            _entityArrays.Clear();

            _entityPool.Dispose();
        }
    }
}
