namespace Atma.Entities
{
    using System;
    using System.Collections.Generic;
    using Atma.Memory;

    using static Atma.Debug;


    public unsafe sealed class EntityPackedArray : UnmanagedDispose
    {
        private ComponentDataArray[] _componentData;
        private IAllocator _allocator;

        public EntitySpec Specification { get; }
        public int Length => Entity.ENTITY_MAX;

        public EntityPackedArray(IAllocator allocator, EntitySpec specification)
        {
            Specification = specification;

            _allocator = allocator;

            var _componentTypes = Specification.ComponentTypes;
            _componentData = new ComponentDataArray[_componentTypes.Length];
            for (var i = 0; i < _componentTypes.Length; i++)
                _componentData[i] = new ComponentDataArray(_allocator, _componentTypes[i], Entity.ENTITY_MAX);
        }

        // public ref readonly Entity Create()
        // {
        //     Assert(Free > 0);
        //     _entityCount++;
        //     var id = _entityPool.Take();
        //     return ref _entityPool[id];
        // }

        // public int Create()
        // {
        //     Assert(Free > 0);
        //     return _entityCount++;
        // }

        // public void Delete(int index)
        // {
        //     Assert(index >= 0 && index < _entityCount);
        //     if (index < _entityCount - 1)
        //     {
        //         var src = _entityCount - 1;
        //         var dst = index;
        //         for (var i = 0; i < _componentData.Length; i++)
        //             _componentData[i].Move(src, dst);
        //     }

        //     _entityCount--;
        // }

        private int GetComponentIndex<T>() where T : unmanaged
            => GetComponentIndex(ComponentType<T>.Type);

        private int GetComponentIndex(in ComponentType type)
        {
            var id = type.ID;
            var _componentTypes = Specification.ComponentTypes;
            for (var i = 0; i < _componentTypes.Length; i++)
                if (_componentTypes[i].ID == id)
                    return i;
            return -1;
        }

        //TODO: Add a group lock here and implement internal no lock moves in ComponentDataArray
        public void Move(int src, int dst)
        {
            if (src == dst)
                return;
            Assert(src >= 0 && src < Length && dst >= 0 && dst < Length);

            for (var i = 0; i < _componentData.Length; i++)
                _componentData[i].Move(src, dst);
        }

        public void Reset(int dst)
        {
            Assert(dst >= 0 && dst < Length);

            for (var i = 0; i < _componentData.Length; i++)
                _componentData[i].Reset(dst);
        }

        public void Reset<T>(int dst)
            where T : unmanaged
        {
            Assert(dst >= 0 && dst < Length);
            var index = GetComponentIndex<T>();

            Assert(index > -1);
            _componentData[index].Reset(dst);
        }

        public Span<T> GetComponentSpan<T>()
            where T : unmanaged
        {
            var index = GetComponentIndex<T>();
            Assert(index > -1);

            var componentDataArray = _componentData[index];
            return componentDataArray.AsSpan<T>();
        }

        public unsafe T* GetComponentPointer<T>()
                   where T : unmanaged
        {
            var index = GetComponentIndex<T>();
            Assert(index > -1);

            var componentDataArray = _componentData[index];
            return componentDataArray.AsPointer<T>();
        }

        // public ReadLock ReadComponent<T>(out ReadOnlySpan<T> span)
        //     where T : unmanaged
        // {
        //     var index = GetComponentIndex<T>();
        //     Assert(index > -1);

        //     var componentDataArray = _componentData[index];
        //     return componentDataArray.AsReadOnlySpan(out span);
        // }

        // public WriteLock WriteComponent<T>(out Span<T> span)
        //     where T : unmanaged
        // {
        //     var index = GetComponentIndex<T>();
        //     Assert(index > -1);

        //     var componentDataArray = _componentData[index];
        //     return componentDataArray.AsSpan(out span);
        // }

        protected override void OnManagedDispose()
        {
            _componentData = null;
            _allocator = null;
        }

        protected override void OnUnmanagedDispose()
        {
            //using stackalocator, must do it backwards
            for (int i = _componentData.Length - 1; i >= 0; i--)
                _componentData[i].Dispose();
        }

        internal static void CopyTo(EntityPackedArray srcArray, int srcIndex, EntityPackedArray dstArray, int dstIndex)
        {
            SharedComponentArrays(srcArray, dstArray, (src, dst) => ComponentDataArray.CopyTo(src, srcIndex, dst, dstIndex));
        }

        internal delegate void SharedComponentCallback(ComponentDataArray srcArray, ComponentDataArray dstArray);
        internal static int SharedComponentArrays(EntityPackedArray srcArray, EntityPackedArray dstArray, SharedComponentCallback shareCallback)
        {
            var i0 = 0;
            var i1 = 0;
            var index = 0;

            Span<ComponentType> a = srcArray.Specification.ComponentTypes;
            Span<ComponentType> b = dstArray.Specification.ComponentTypes;
            while (i0 < a.Length && i1 < b.Length)
            {
                var aType = a[i0];
                var bType = b[i1];
                if (aType.ID > bType.ID) i1++;
                else if (bType.ID > aType.ID) i0++;
                else
                {
                    shareCallback(srcArray._componentData[i0], dstArray._componentData[i1]);
                    i0++;
                    i1++;
                }
            }
            return index;
        }
    }
}
