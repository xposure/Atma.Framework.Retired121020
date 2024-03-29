﻿namespace Atma.Entities
{
    using Atma.Common;
    using Atma.Memory;
    using Microsoft.Extensions.Logging;
    using System;

    public sealed partial class EntityManager : UnmanagedDispose
    {
        private const int BATCH_SIZE = 256;

        public int EntityCount { get => EntityArrays.EntityCount(); }


        private EntityArrayList _entityArrays;

        public EntityArrayList EntityArrays => _entityArrays;

        private ILogger _logger;
        private ILoggerFactory _logFactory;
        private IAllocator _allocator;

        private EntityPool _entityPool;// = new EntityPool2();

        public EntityManager(ILoggerFactory logFactory, IAllocator allocator)
        {
            _logFactory = logFactory;
            _logger = _logFactory.CreateLogger<EntityManager>();
            _allocator = allocator;

            _entityPool = new EntityPool(_logFactory, _allocator);
            _entityArrays = new EntityArrayList(logFactory, allocator);
        }


        public uint Create(in EntitySpec spec)
        {
            //var specIndex = GetOrCreateSpec(spec);
            var array = EntityArrays[spec];
            Span<uint> createdEntities = stackalloc uint[1];
            CreateInternal(array, createdEntities);
            return createdEntities[0];
        }

        public uint Create(Span<ComponentType> componentTypes)
        {
            //var specIndex = GetOrCreateSpec(componentTypes);
            var array = EntityArrays[componentTypes];
            Span<uint> createdEntities = stackalloc uint[1];
            CreateInternal(array, createdEntities);
            return createdEntities[0];
        }

        public unsafe void Create(in EntitySpec spec, Span<uint> createdEntities)
        {
            //var specIndex = GetOrCreateSpec(spec);
            var array = EntityArrays[spec];
            CreateInternal(array, createdEntities);
        }

        public unsafe void Create(Span<ComponentType> componentTypes, Span<uint> createdEntities)
        {
            //var specIndex = GetOrCreateSpec(componentTypes);
            var array = EntityArrays[componentTypes];
            CreateInternal(array, createdEntities);
        }

        internal unsafe void CreateInternal(EntityChunkList array, Span<uint> entities)
        {
            //var array = EntityArrays2[specIndex];
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
            Span<uint> entities = stackalloc[] { entity };
            var componentType = stackalloc[] { ComponentType<T>.Type };
            var data = stackalloc[] { t };
            AssignInternal(entities, componentType, data, false);
        }

        public unsafe void Assign<T>(Span<uint> entities, in T t)
           where T : unmanaged
        {
            var componentType = stackalloc[] { ComponentType<T>.Type };
            var data = stackalloc[] { t };
            AssignInternal(entities, componentType, data, false);
        }

        public unsafe void Assign<T>(Span<uint> entities, NativeArray<T> array)
            where T : unmanaged
        {
            Assert.EqualTo(entities.Length, array.Length);
            var componentType = stackalloc[] { ComponentType<T>.Type };

            AssignInternal(entities, componentType, array.RawPointer, true);
        }

        internal unsafe void AssignInternal(Span<uint> entities, ComponentType* componentType, void* src, bool incrementSource)
        {
            //we want to do move first so that it throws an exception
            //if the entity already has the component (update won't)
            MoveInternal(entities, new Span<ComponentType>(componentType, 1), true);

            SetComponentData(componentType, entities, src, incrementSource, false);
        }

        public ref T Get<T>(uint entity)
            where T : unmanaged
        {
            Assert.EqualTo(Has<T>(entity), true);

            ref readonly var e = ref _entityPool[entity];

            var array = EntityArrays[e.SpecIndex];
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
            var spec = EntityArrays[entityInfo.SpecIndex].Specification;
            return spec.Has(componentId);
        }

        public unsafe void Remove<T>(uint entity)
            where T : unmanaged
        {
            Span<uint> entities = stackalloc[] { entity };
            Remove<T>(entities);
        }

        public unsafe void Remove(uint entity, Span<ComponentType> componentTypes)
        {
            Span<uint> entities = stackalloc[] { entity };
            MoveInternal(entities, componentTypes, false);
        }

        public unsafe void Remove<T>(Span<uint> entities)
          where T : unmanaged
        {
            Span<ComponentType> componentTypes = stackalloc[] { ComponentType<T>.Type };
            MoveInternal(entities, componentTypes, false);
        }

        public unsafe void Remove(Span<uint> entities, Span<ComponentType> componentTypes)
        {
            MoveInternal(entities, componentTypes, false);
        }

        internal unsafe void RemoveInternal<T>(Span<uint> entities)
            where T : unmanaged
        {
            Span<ComponentType> componentTypes = stackalloc[] { ComponentType<T>.Type };
            MoveInternal(entities, componentTypes, false);
        }

        public unsafe void Delete(uint entity)
        {
            Span<uint> entities = stackalloc[] { entity };
            Delete(entities);
        }

        public unsafe void Delete(Span<uint> entities) => DeleteInternal(entities, true);

        internal unsafe void DeleteInternal(Span<uint> entities, bool returnId)
        {
            Span<EntityRef> batch = stackalloc EntityRef[BATCH_SIZE];
            var index = 0;
            while (index < entities.Length)
            {
                var remaining = entities.Length - index;
                var count = remaining > batch.Length ? batch.Length : remaining;

                for (var i = 0; i < count; i++)
                    batch[i] = _entityPool.GetRef(entities[index + i]);

                DeleteInternal(batch.Slice(0, count), returnId);
                index += batch.Length;
            }
        }

        internal unsafe void DeleteInternal(Span<EntityRef> entities, bool returnId)
        {
            if (entities.Length == 0)
                return;

            var specIndex = entities[0].SpecIndex;
            var chunkIndex = entities[0].ChunkIndex;
            var lastIndex = 0;
            for (var i = 1; i < entities.Length; i++)
            {
                ref var e = ref entities[i];
                if (e.SpecIndex != specIndex || e.ChunkIndex != chunkIndex)
                {
                    //flush
                    var amountToRemove = i - lastIndex - 1;
                    if (amountToRemove > 0)
                    {
                        var array = EntityArrays[specIndex];
                        var workingEntities = entities.Slice(lastIndex, amountToRemove);
                        array.Delete(workingEntities);

                        if (returnId)
                            _entityPool.Return(workingEntities);
                    }

                    specIndex = e.SpecIndex;
                    chunkIndex = e.ChunkIndex;
                    lastIndex = i + 1;
                }

            }

            if (entities.Length - lastIndex > 0)
            {
                var array = EntityArrays[specIndex];
                var workingEntities = entities.Slice(lastIndex, entities.Length - lastIndex);
                array.Delete(workingEntities);
                if (returnId)
                    _entityPool.Return(workingEntities);
            }
        }

        public void Replace<T>(uint entity, in T t)
            where T : unmanaged
        {
            Span<uint> entities = stackalloc[] { entity };
            Replace<T>(entities, t);
        }

        public unsafe void Replace<T>(Span<uint> entities, in T t)
            where T : unmanaged
        {
            var data = stackalloc[] { t };
            var componentType = stackalloc[] { ComponentType<T>.Type };
            SetComponentData(componentType, entities, data, false, false);
        }

        public unsafe void Replace<T>(Span<uint> entities, in NativeArray<T> t)
            where T : unmanaged
        {
            Assert.EqualTo(entities.Length, t.Length);

            var componentType = stackalloc[] { ComponentType<T>.Type };
            SetComponentData(componentType, entities, t.RawPointer, true, false);
        }

        public unsafe void Move(uint entity, in EntitySpec spec)
        {
            var dstSpecIndex = EntityArrays.GetOrCreateSpec(spec);
            Span<EntityRef> entities = stackalloc[] { _entityPool.GetRef(entity) };

            MoveInternal(entities, entities[0].SpecIndex, dstSpecIndex);
        }

        internal unsafe void Move(Span<EntityRef> entities, in EntitySpec spec)
        {
            if (entities.Length == 0)
                return;
            var dstSpecIndex = EntityArrays.GetOrCreateSpec(spec);

            var srcSpecIndex = entities[0].SpecIndex;
            var lastIndex = 0;
            for (var i = 1; i < entities.Length; i++)
            {
                var remaining = entities.Length - i;
                var count = remaining > entities.Length ? entities.Length : remaining;

                ref var entity = ref entities[i];
                if (entity.SpecIndex != srcSpecIndex)
                {
                    var amountToMove = i - lastIndex - 1;
                    MoveInternal(entities.Slice(lastIndex, amountToMove), srcSpecIndex, dstSpecIndex);
                    lastIndex = i + 1;
                }

                srcSpecIndex = entity.SpecIndex;
            }

            if (entities.Length - lastIndex > 0)
                MoveInternal(entities.Slice(lastIndex, entities.Length - lastIndex), srcSpecIndex, dstSpecIndex);
        }

        public unsafe void Move(Span<uint> entities, in EntitySpec spec)
        {
            if (entities.Length == 0)
                return;

            Span<EntityRef> entityRefs = stackalloc EntityRef[BATCH_SIZE];
            for (var i = 0; i < entities.Length;)
            {
                var remaining = entities.Length - i;
                var count = remaining > entityRefs.Length ? entityRefs.Length : remaining;

                for (var j = 0; j < count; j++)
                    entityRefs[j] = _entityPool.GetRef(entities[i + j]);

                Move(entityRefs.Slice(0, count), spec);
                i += count;
            }
        }

        // private unsafe void MoveInternal(Span<uint> entities, int srcSpecIndex, int dstSpecIndex)
        // {
        //     var index = 0;
        //     var src = EntityArrays2[srcSpecIndex];
        //     var dst = EntityArrays2[dstSpecIndex];

        //     var entityPtr = stackalloc Entity[BATCH_SIZE];
        //     var entityData = new Span<Entity>(entityPtr, BATCH_SIZE);
        //     Span<EntityRef> entityRefs = stackalloc EntityRef[BATCH_SIZE];
        //     for (var i = 0; i < BATCH_SIZE; i++)
        //         entityRefs[i] = new EntityRef(&entityPtr[i]);

        //     while (index < entities.Length)
        //     {
        //         var remaining = entities.Length - index;
        //         var count = remaining > BATCH_SIZE ? BATCH_SIZE : remaining;

        //         //we need to make a copy of the data because Create has side effects
        //         var workingEntities = entities.Slice(index, count);
        //         for (var i = 0; i < workingEntities.Length; i++)
        //             entityData[i] = _entityPool[workingEntities[i]];

        //         dst.Create(entityRefs.Slice(0, count));

        //         for (var i = 0; i < count; i++)
        //         {
        //             ref var createdEntity = ref entityRefs[i];
        //             var entity = _entityPool.GetRef(entities[index + i]);

        //             var srcChunk = src.AllChunks[entity.ChunkIndex];
        //             var dstChunk = dst.AllChunks[createdEntity.ChunkIndex];

        //             //TODO: see if we can batch this? it would require both src and dst to be linear
        //             //src can be anywhere and unordered
        //             //dst will be linear but could span chunks
        //             //ComponentPackedArray.CopyTo(srcChunk.PackedArray, entity.Index, dstChunk.PackedArray, createdEntity.Index);

        //             src.Delete(entity);

        //             entity.Replace(new Entity(createdEntity.ID, dstSpecIndex, createdEntity.ChunkIndex, createdEntity.Index));
        //         }

        //         index += count;
        //     }
        // }

        private unsafe void MoveInternal(Span<EntityRef> entities, int srcSpecIndex, int dstSpecIndex)
        {
            var index = 0;
            var src = EntityArrays[srcSpecIndex];
            var dst = EntityArrays[dstSpecIndex];

            var tempEntitiesPtr = stackalloc Entity[BATCH_SIZE];
            var tempEntities = new Span<Entity>(tempEntitiesPtr, BATCH_SIZE);
            Span<EntityRef> tempEntityRefs = stackalloc EntityRef[BATCH_SIZE];
            for (var i = 0; i < BATCH_SIZE; i++)
                tempEntityRefs[i] = new EntityRef(&tempEntitiesPtr[i]);

            while (index < entities.Length)
            {
                var remaining = entities.Length - index;
                var count = remaining > BATCH_SIZE ? BATCH_SIZE : remaining;

                //we need to make a copy of the data because Create has side effects
                var workingEntities = entities.Slice(index, count);
                for (var i = 0; i < workingEntities.Length; i++)
                {
                    ref var entity = ref entities[i];
                    tempEntities[i] = new Entity(entity.ID, srcSpecIndex, entity.ChunkIndex, entity.Index);
                }

                var oldEntities = tempEntityRefs.Slice(0, count);
                dst.Create(workingEntities);
                EntityChunkList.CopyTo(src, oldEntities, dst, workingEntities);
                src.Delete(oldEntities);

                index += count;
            }
        }

        private unsafe int MoveInternal(Span<uint> entities, int srcSpecIndex, Span<ComponentType> types, bool addComponent)
        {
            var srcSpec = EntityArrays[srcSpecIndex].Specification;

            //we need to move the entity to the new spec
            var srcComponents = srcSpec.ComponentTypes.AsSpan();
            Span<ComponentType> componentTypes = stackalloc ComponentType[srcSpec.ComponentTypes.Length + (addComponent ? types.Length : -types.Length)];
            if (addComponent)
            {
                srcComponents.CopyTo(componentTypes);
                for (var i = 0; i < types.Length; i++)
                    componentTypes[srcComponents.Length + i] = types[i];
            }
            else
            {
                var index = 0;
                for (var i = 0; i < srcSpec.ComponentTypes.Length; i++)
                    if (!ComponentType.HasAny(types, srcComponents.Slice(i, 1)))
                        componentTypes[index++] = srcSpec.ComponentTypes[i];

                Assert.EqualTo(index, componentTypes.Length);
            }

            if (componentTypes.Length == 0)
            {
                Delete(entities);
                return -1;
            }
            else
            {
                //var specId = ComponentType.CalculateId(componentTypes);                
                var dstSpecIndex = EntityArrays.GetOrCreateSpec(componentTypes);

                //TODO: needs rewrote, just making the tests pass
                Span<EntityRef> entityRefs = stackalloc EntityRef[entities.Length];
                for (var i = 0; i < entityRefs.Length; i++)
                    entityRefs[i] = _entityPool.GetRef(entities[i]);

                MoveInternal(entityRefs, srcSpecIndex, dstSpecIndex);
                return dstSpecIndex;
            }
        }

        internal unsafe void MoveInternal(Span<uint> entities, Span<ComponentType> componentTypes, bool addComponent)
        {
            SpanList<uint> batch = stackalloc uint[BATCH_SIZE];

            var srcSpecIndex = -1;
            for (var i = 0; i < entities.Length; i++)
            {
                ref var entityInfo = ref _entityPool[entities[i]];
                if (entityInfo.SpecIndex != srcSpecIndex || batch.Free == 0)
                {
                    if (batch.Length > 0)
                    {
                        if (addComponent)
                            Assert.EqualTo(EntityArrays[srcSpecIndex].Specification.HasAny(componentTypes), false);
                        else
                            Assert.EqualTo(EntityArrays[srcSpecIndex].Specification.HasAll(componentTypes), true);

                        MoveInternal(batch, srcSpecIndex, componentTypes, addComponent);
                    }

                    srcSpecIndex = entityInfo.SpecIndex;
                    batch.Reset();
                }

                batch.Add(entities[i]);
            }

            if (batch.Length > 0)
            {
                if (addComponent)
                    Assert.EqualTo(EntityArrays[srcSpecIndex].Specification.HasAny(componentTypes), false);
                else
                    Assert.EqualTo(EntityArrays[srcSpecIndex].Specification.HasAll(componentTypes), true);

                MoveInternal(batch, srcSpecIndex, componentTypes, addComponent);
            }
        }


        public void Reset(uint entity)
        {
            ref var e = ref _entityPool[entity];
            var array = EntityArrays[e.SpecIndex];
            var chunk = array.AllChunks[e.ChunkIndex];
            chunk.PackedArray.Reset(e.Index);
        }

        public void Reset(Span<uint> entities)
        {
            //TODO: Reset needs optimized to try and batch reset same spec entities using updateinternal
            for (var i = 0; i < entities.Length; i++)
            {
                ref var e = ref _entityPool[entities[i]];
                var array = EntityArrays[e.SpecIndex];
                var chunk = array.AllChunks[e.ChunkIndex];
                chunk.PackedArray.Reset(e.Index);
            }
        }

        public void Reset<T>(uint entity)
            where T : unmanaged
        {
            ref var e = ref _entityPool[entity];
            var array = EntityArrays[e.SpecIndex];
            var chunk = array.AllChunks[e.ChunkIndex];
            chunk.PackedArray.Reset<T>(e.Index);
        }

        public unsafe void Reset<T>(Span<uint> entities)
           where T : unmanaged
        {
            var componentType = stackalloc[] { ComponentType<T>.Type };
            var data = stackalloc[] { default(T) };
            SetComponentData(componentType, entities, data, false, false);
        }

        public unsafe void Update<T>(uint entity, in T t)
            where T : unmanaged
        {
            Span<uint> entities = stackalloc[] { entity };
            Update<T>(entities, t);
        }

        public unsafe void Update<T>(Span<uint> entities, in T t)
            where T : unmanaged
        {
            var componentType = stackalloc[] { ComponentType<T>.Type };
            var data = stackalloc[] { t };
            SetComponentData(componentType, entities, data, false, true);
        }

        public unsafe void Update<T>(Span<uint> entities, NativeArray<T> data)
            where T : unmanaged
        {
            var componentType = stackalloc[] { ComponentType<T>.Type };
            SetComponentData(componentType, entities, data.RawPointer, true, true);
        }

        internal unsafe void SetComponentData(ComponentType* componentType, Span<uint> entities, void* src, bool incrementSource, bool allowMove)
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
                            specIndex = MoveInternal(entities.Slice(lastIndex, entityRefs.Length), specIndex, new Span<ComponentType>(componentType, 1), true);

                        var array = EntityArrays[specIndex];
                        array.Copy(specIndex, componentType, ref src, entityRefs.Slice(), incrementSource);
                        entityRefs.Reset();
                    }
                    specIndex = e.SpecIndex;
                    hasComponent = EntityArrays[e.SpecIndex].Specification.Has(componentType->ID);
                    lastIndex = i;

                    Assert.EqualTo(hasComponent | allowMove, true);
                }

                entityRefs.Add(e);
            }

            if (entityRefs.Length > 0)
            {
                if (!hasComponent)
                {
                    specIndex = MoveInternal(entities.Slice(lastIndex, entityRefs.Length), specIndex, new Span<ComponentType>(componentType, 1), true);
                }

                var array = EntityArrays[specIndex];
                array.Copy(specIndex, componentType, ref src, entityRefs.Slice(), incrementSource);
            }
        }

        public EntityCommandBuffer CreateCommandBuffer(int lengthInBytes = -1)
        {
            if (lengthInBytes > 0)
                return new EntityCommandBuffer(this, _allocator, lengthInBytes);

            return new EntityCommandBuffer(this, _allocator);
        }

        public unsafe EntitySpec SetGroupData<T>(uint entity, T data)
            where T : IEntitySpecGroup
        {
            var e = _entityPool.GetRef(entity);
            var array = EntityArrays[e.SpecIndex];
            var srcSpec = array.Specification;

            var componentTypes = srcSpec.ComponentTypes.AsSpan();
            IEntitySpecGroup[] groups = null;
            if (srcSpec.Has<T>(out var index))
            {
                //need to replace
                groups = srcSpec.Grouping;
                groups[index] = data;
            }
            else
            {
                var count = 1;
                if (srcSpec.Grouping != null)
                    count += srcSpec.Grouping.Length;

                groups = new IEntitySpecGroup[count];
                for (var i = 0; i < groups.Length; i++)
                {
                    if (i < groups.Length - 1)
                        groups[i] = srcSpec.Grouping[i];
                    else
                        groups[i] = data;
                }
            }

            var dstSpec = new EntitySpec(groups, componentTypes);
            var dstSpecIndex = EntityArrays.GetOrCreateSpec(dstSpec);
            Span<EntityRef> entities = stackalloc[] { e };
            MoveInternal(entities, e.SpecIndex, dstSpecIndex);
            return dstSpec;
        }

        public unsafe T GetGroupData<T>(uint entity)
            where T : IEntitySpecGroup
        {
            var e = _entityPool.GetRef(entity);
            var array = EntityArrays[e.SpecIndex];
            var srcSpec = array.Specification;
            return srcSpec.GetGroupData<T>();
        }

        public void ClearAll()
        {
            _entityArrays.Dispose();
            _entityArrays = new EntityArrayList(_logFactory, _allocator);

            _entityPool.Dispose();
            _entityPool = new EntityPool(_logFactory, _allocator);
        }

        protected override void OnUnmanagedDispose()
        {
            EntityArrays.Dispose();
            _entityPool.Dispose();
        }
    }
}
