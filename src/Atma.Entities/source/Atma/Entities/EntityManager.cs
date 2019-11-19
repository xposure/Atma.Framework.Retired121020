﻿namespace Atma.Entities
{
    using Atma.Common;
    using Atma.Memory;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed partial class EntityManager : UnmanagedDispose//, IEntityManager2
    {
        private const int BATCH_SIZE = 256;

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
        }

        internal int GetOrCreateSpec(in EntitySpec spec)
        {
            var specIndex = _knownSpecs.IndexOf(spec.ID);
            if (specIndex == -1)
            {
                //we are limited to this many unique specs per EM
                specIndex = _knownSpecs.Count;
                Assert.LessThan(specIndex, Entity.SPEC_MAX);
                _knownSpecs.Add(spec.ID, spec);
                _entityArrays.Add(new EntityChunkList(_logFactory, _allocator, spec, specIndex));
            }
            return specIndex;
        }

        internal int GetOrCreateSpec(Span<ComponentType> componentTypes)
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
            for (var i = 0; i < entities.Length;)
            {
                var remaining = entities.Length - i;
                var take = remaining > BATCH_SIZE ? BATCH_SIZE : remaining;
                Span<EntityRef> entityRefs = stackalloc EntityRef[take];
                _entityPool.Take(entityRefs);
                array.Create(entityRefs);
                for (var j = 0; j < take; j++)
                    entities[i + j] = entityRefs[j].ID;
                i += take;
            }
        }

        public unsafe void Assign<T>(uint entity, in T t)
           where T : unmanaged
        {
            var componentType = stackalloc[] { ComponentType<T>.Type };
            Assert.Equals(Has(entity, componentType->ID), false);

            Span<uint> entities = stackalloc[] { entity };
            MoveInternal(entities, componentType);

            ref var entityRef = ref _entityPool[entity];

            var array = _entityArrays[entityRef.SpecIndex];
            var chunk = array.AllChunks[entityRef.ChunkIndex];
            var span = chunk.GetComponentData<T>();
            span[entityRef.Index] = t;
        }

        public unsafe void Assign<T>(Span<uint> entities, in T t)
           where T : unmanaged
        {
            var componentType = stackalloc[] { ComponentType<T>.Type };
            //we want to do move first so that it throws an exception
            //if the entity already has the component (update won't)
            MoveInternal(entities, componentType);

            var data = stackalloc[] { t };
            UpdateInternal(componentType, entities, data, false, false);
        }

        public unsafe void Assign<T>(Span<uint> entities, NativeArray<T> array)
            where T : unmanaged
        {
            Assert.EqualTo(entities.Length, array.Length);
            var componentType = stackalloc[] { ComponentType<T>.Type };
            //we want to do move first so that it throws an exception
            //if the entity already has the component (update won't)
            MoveInternal(entities, componentType);

            var data = array.RawPointer;
            UpdateInternal(componentType, entities, data, true, false);
        }

        internal unsafe void AssignInternal(Span<uint> entities, ComponentType* type, ref void* src, bool incrementSource)
        {
            UpdateInternal(type, entities, src, incrementSource, true);
        }

        public ref T Get<T>(uint entity)
            where T : unmanaged
        {
            Assert.EqualTo(Has<T>(entity), true);

            ref readonly var e = ref _entityPool[entity];

            var array = _entityArrays[e.SpecIndex];
            var chunk = array.AllChunks[e.ChunkIndex];
            var span = chunk.GetComponentData<T>();

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


        public void Remove<T>(uint entity)
            where T : unmanaged
        {
            RemoveInternal(entity, ComponentType<T>.Type.ID);
        }

        public unsafe void Remove(uint entity)
        {
            Span<uint> entities = stackalloc[] { entity };
            Remove(entities);
        }

        public unsafe void Remove(Span<uint> entities) => RemoveInternal(entities, true);

        private unsafe void RemoveInternal(ref SpanList<EntityRef> removeEntities, int specIndex, int chunkIndex, bool returnId)
        {
            var array = _entityArrays[specIndex];
            array.Delete(chunkIndex, removeEntities);

            if (returnId)
            {
                for (var i = 0; i < removeEntities.Length; i++)
                    _entityPool.Return(removeEntities[i].ID);
            }

            removeEntities.Reset();
        }

        internal unsafe void RemoveInternal(Span<uint> entities, bool returnId)
        {
            Span<EntityRef> batch = stackalloc EntityRef[BATCH_SIZE];
            var index = 0;
            while (index < entities.Length)
            {
                var remaining = entities.Length - index;
                var count = remaining > batch.Length ? batch.Length : remaining;

                for (var i = 0; i < count; i++)
                    batch[i] = _entityPool.GetRef(entities[index + i]);

                RemoveInternal(batch.Slice(0, count), returnId);
                index += batch.Length;
            }
        }

        internal unsafe void RemoveInternal(Span<EntityRef> entities, bool returnId)
        {
            SpanList<EntityRef> removeEntities = stackalloc EntityRef[BATCH_SIZE];

            var specIndex = -1;
            var chunkIndex = -1;
            for (var i = 0; i < entities.Length; i++)
            {
                ref var e = ref entities[i];
                if (e.SpecIndex != specIndex || e.ChunkIndex != chunkIndex || removeEntities.Free == 0)
                {
                    //flush
                    if (removeEntities.Length > 0)
                        RemoveInternal(ref removeEntities, specIndex, chunkIndex, returnId);

                    specIndex = e.SpecIndex;
                    chunkIndex = e.ChunkIndex;
                }

                removeEntities.Add(e);
            }

            if (removeEntities.Length > 0)
                RemoveInternal(ref removeEntities, specIndex, chunkIndex, returnId);
        }



        internal unsafe bool RemoveInternal(uint entity, int componentId)
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

                MoveInternal(entities, srcSpecIndex, dstSpecIndex);
                return true;
            }
        }

        public void Replace<T>(uint entity, in T t)
            where T : unmanaged
        {
            ref var e = ref _entityPool[entity];
            ReplaceInternal<T>(ref e, t);
        }

        public unsafe void Replace<T>(NativeArray<uint> entities, in T t)
            where T : unmanaged
        {
            var data = stackalloc[] { t };
            var componentType = stackalloc[] { ComponentType<T>.Type };
            UpdateInternal(componentType, entities, data, false, false);
        }

        public unsafe void Replace<T>(NativeArray<uint> entities, in NativeArray<T> t)
            where T : unmanaged
        {
            Assert.EqualTo(entities.Length, t.Length);

            var componentType = stackalloc[] { ComponentType<T>.Type };
            UpdateInternal(componentType, entities, t.RawPointer, true, false);
        }

        internal void ReplaceInternal<T>(ref Entity entity, in T t)
          where T : unmanaged
        {
            Assert.Equals(Has(ref entity, ComponentType<T>.Type.ID), true);
            var array = _entityArrays[entity.SpecIndex];
            var chunk = array.AllChunks[entity.ChunkIndex];
            var span = chunk.GetComponentData<T>();
            span[entity.Index] = t;
        }

        // internal unsafe void SetComponentInternal(ComponentType* componentType, Span<uint> entities, void* src, bool incrementSource, bool allowUpdate)
        // {
        //     //Assert.Equals(Has(ref entity, ComponentType<T>.Type.ID), true);
        //     //we are going to put replaces in front of the array and assigns in the back if we are allowed
        //     SpanList<Entity> entityRefs = stackalloc Entity[BATCH_SIZE];

        //     //TODO: come back to this
        //     //Assert.EqualTo(allowUpdate, false);

        //     var specIndex = -1;
        //     for (var i = 0; i < entities.Length; i++)
        //     {
        //         ref var e = ref _entityPool[entities[i]];
        //         var hasComponent = Has(ref e, componentType->ID);
        //         if (!allowUpdate)
        //             Assert.EqualTo(hasComponent, true);

        //         //TODO: I think we need to flush here because moving data can call remove and reorder entities
        //         //if array is changed to work with entityrefs this could be fixed
        //         if (e.SpecIndex != specIndex || !hasComponent || entityRefs.Free == 0)
        //         {
        //             //flush
        //             if (entityRefs.Length > 0)
        //             {
        //                 var array = _entityArrays[specIndex];
        //                 array.Copy(specIndex, componentType, ref src, entityRefs.Slice(), incrementSource);
        //                 //SetComponentInternal(array, entityRefs.Slice(), componentType, ref src, incrementSource);
        //                 entityRefs.Reset();
        //             }
        //             specIndex = e.SpecIndex;
        //         }

        //         if (!hasComponent)
        //             MoveInternal(entities.Slice(i, 1), componentType);

        //         entityRefs.Add(e);
        //     }

        //     if (entityRefs.Length > 0)
        //     {
        //         var array = _entityArrays[specIndex];
        //         array.Copy(specIndex, componentType, ref src, entityRefs.Slice(), incrementSource);
        //     }
        // }


        public unsafe void Move(uint entity, in EntitySpec spec)
        {
            var dstSpecIndex = GetOrCreateSpec(spec);
            ref var e = ref _entityPool[entity];
            Span<uint> entities = stackalloc[] { entity };

            MoveInternal(entities, e.SpecIndex, dstSpecIndex);
        }

        private unsafe void MoveInternal(Span<uint> entities, int srcSpecIndex, int dstSpecIndex)
        {
            var index = 0;
            var src = _entityArrays[srcSpecIndex];
            var dst = _entityArrays[dstSpecIndex];

            //Span<CreatedEntity> createdEntities = stackalloc CreatedEntity[BATCH_SIZE];
            var entityPtr = stackalloc Entity[BATCH_SIZE];
            var entityData = new Span<Entity>(entityPtr, BATCH_SIZE);
            Span<EntityRef> entityRefs = stackalloc EntityRef[BATCH_SIZE];
            for (var i = 0; i < BATCH_SIZE; i++)
                entityRefs[i] = new EntityRef(&entityPtr[i]);

            while (index < entities.Length)
            {
                //var segment = index * createdEntities.Length;
                var remaining = entities.Length - index;
                var count = remaining > BATCH_SIZE ? BATCH_SIZE : remaining;

                //we need to make a copy of the data because Create has side effects
                var workingEntities = entities.Slice(index, count);
                for (var i = 0; i < workingEntities.Length; i++)
                    entityData[i] = _entityPool[workingEntities[i]];

                dst.Create(entityRefs.Slice(0, count));

                for (var i = 0; i < count; i++)
                {
                    ref var createdEntity = ref entityRefs[i];
                    var entity = _entityPool.GetRef(entities[index + i]);

                    var srcChunk = src.AllChunks[entity.ChunkIndex];
                    var dstChunk = dst.AllChunks[createdEntity.ChunkIndex];

                    //TODO: see if we can batch this? it would require both src and dst to be linear
                    //src can be anywhere and unordered
                    //dst will be linear but could span chunks
                    ComponentPackedArray.CopyTo(srcChunk.PackedArray, entity.Index, dstChunk.PackedArray, createdEntity.Index);

                    src.Delete(entity);

                    entity.Replace(new Entity(createdEntity.ID, dstSpecIndex, createdEntity.ChunkIndex, createdEntity.Index));
                }

                index += count;
            }
        }

        private unsafe int MoveInternal(Span<uint> entities, int srcSpecIndex, ComponentType* type)
        {
            var srcSpec = _entityArrays[srcSpecIndex].Specification;

            //we need to move the entity to the new spec
            Span<ComponentType> componentTypes = stackalloc ComponentType[srcSpec.ComponentTypes.Length + 1];
            srcSpec.ComponentTypes.CopyTo(componentTypes);

            componentTypes[srcSpec.ComponentTypes.Length] = *type;

            var specId = ComponentType.CalculateId(componentTypes);
            var dstSpecIndex = GetOrCreateSpec(componentTypes);
            MoveInternal(entities, srcSpecIndex, dstSpecIndex);
            return dstSpecIndex;
        }

        internal unsafe void MoveInternal(Span<uint> entities, ComponentType* type)
        {
            SpanList<uint> batch = stackalloc uint[BATCH_SIZE];

            var srcSpecIndex = -1;
            for (var i = 0; i < entities.Length; i++)
            {
                ref var entityInfo = ref _entityPool[entities[i]];
                Assert.EqualTo(Has(ref entityInfo, type->ID), false);

                //var srcSpecIndex = entityInfo.SpecIndex;
                if (entityInfo.SpecIndex != srcSpecIndex || batch.Free == 0)
                {
                    if (batch.Length > 0)
                        MoveInternal(batch, srcSpecIndex, type);

                    srcSpecIndex = entityInfo.SpecIndex;
                    batch.Reset();
                }

                batch.Add(entities[i]);
            }

            if (batch.Length > 0)
                MoveInternal(batch, srcSpecIndex, type);
        }


        public void Reset(uint entity)
        {
            ref var e = ref _entityPool[entity];
            var array = _entityArrays[e.SpecIndex];
            var chunk = array.AllChunks[e.ChunkIndex];
            chunk.PackedArray.Reset(e.Index);
        }

        public void Reset(Span<uint> entities)
        {
            //TODO: Reset needs optimized to try and batch reset same spec entities using updateinternal
            for (var i = 0; i < entities.Length; i++)
            {
                ref var e = ref _entityPool[entities[i]];
                var array = _entityArrays[e.SpecIndex];
                var chunk = array.AllChunks[e.ChunkIndex];
                chunk.PackedArray.Reset(e.Index);
            }
        }

        public void Reset<T>(uint entity)
            where T : unmanaged
        {
            ref var e = ref _entityPool[entity];
            var array = _entityArrays[e.SpecIndex];
            var chunk = array.AllChunks[e.ChunkIndex];
            chunk.PackedArray.Reset<T>(e.Index);
        }

        public unsafe void Reset<T>(Span<uint> entities)
           where T : unmanaged
        {
            var componentType = stackalloc[] { ComponentType<T>.Type };
            var data = stackalloc[] { default(T) };
            UpdateInternal(componentType, entities, data, false, false);
        }

        public unsafe void Update<T>(uint entity, in T t)
            where T : unmanaged
        {
            Span<uint> entities = stackalloc[] { entity };
            var componentType = stackalloc[] { ComponentType<T>.Type };
            var data = stackalloc[] { t };
            UpdateInternal(componentType, entities, data, false, true);
        }

        internal unsafe void UpdateInternal(ComponentType* componentType, Span<uint> entities, void* src, bool incrementSource, bool allowMove)
        {
            SpanList<EntityRef> entityRefs = stackalloc EntityRef[BATCH_SIZE];

            //since we are operating on a single entity array (src spec)
            //it is safe to track if the current batch needs to move first...
            //i love it when code comes together like this

            var specIndex = -1;
            var hasComponent = false;
            var lastIndex = 0;
            for (var i = 0; i < entities.Length; i++)
            {
                var e = new EntityRef(_entityPool.GetPointer(entities[i]));
                if (e.SpecIndex != specIndex || entityRefs.Free == 0)
                {
                    //flush
                    if (entityRefs.Length > 0)
                    {
                        if (!hasComponent)
                            specIndex = MoveInternal(entities.Slice(lastIndex, entityRefs.Length), specIndex, componentType);

                        var array = _entityArrays[specIndex];
                        array.Copy(specIndex, componentType, ref src, entityRefs.Slice(), incrementSource);
                        entityRefs.Reset();
                    }
                    specIndex = e.SpecIndex;
                    hasComponent = _entityArrays[e.SpecIndex].Specification.Has(componentType->ID);
                    lastIndex = i;

                    Assert.EqualTo(hasComponent | allowMove, true);
                }

                entityRefs.Add(e);
            }

            if (entityRefs.Length > 0)
            {
                if (!hasComponent)
                    specIndex = MoveInternal(entities.Slice(lastIndex, entityRefs.Length), specIndex, componentType);

                var array = _entityArrays[specIndex];
                array.Copy(specIndex, componentType, ref src, entityRefs.Slice(), incrementSource);
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
