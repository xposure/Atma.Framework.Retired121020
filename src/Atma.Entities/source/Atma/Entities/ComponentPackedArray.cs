namespace Atma.Entities
{
    using System;
    using Atma.Memory;
    using Microsoft.Extensions.Logging;

    public unsafe sealed class ComponentPackedArray : UnmanagedDispose
    {
        private ComponentDataArray[] _componentData;
        private ILogger _logger;
        private ILoggerFactory _logFactory;
        private IAllocator _allocator;

        public readonly EntitySpec Specification;
        public int Length => Entity.ENTITY_MAX;

        internal ComponentPackedArray(ILoggerFactory logFactory, IAllocator allocator, EntitySpec specification)
        {
            _logFactory = logFactory;
            _logger = _logFactory.CreateLogger<ComponentPackedArray>();
            _allocator = allocator;

            Specification = specification;

            var _componentTypes = Specification.ComponentTypes;
            _componentData = new ComponentDataArray[_componentTypes.Length];
            for (var i = 0; i < _componentTypes.Length; i++)
                _componentData[i] = new ComponentDataArray(_logFactory, _allocator, _componentTypes[i], Entity.ENTITY_MAX);
        }

        internal void Copy(int componentIndex, ref void* src, int dstIndex, int length, bool incrementSrc)
        {
            _componentData[componentIndex].Copy(ref src, dstIndex, length, incrementSrc);
        }

        //TODO: Add a group lock here and implement internal no lock moves in ComponentDataArray
        internal void Move(int src, int dst)
        {
            if (src == dst)
                return;

            Assert.Range(src, 0, Length);
            Assert.Range(dst, 0, Length);

            for (var i = 0; i < _componentData.Length; i++)
                _componentData[i].Move(src, dst);
        }

        internal void Reset(int dst)
        {
            Assert.Range(dst, 0, Length);

            for (var i = 0; i < _componentData.Length; i++)
                _componentData[i].Reset(dst);
        }

        internal void Reset<T>(int dst)
            where T : unmanaged
        {
            Assert.Range(dst, 0, Length);
            var index = GetComponentIndex<T>();

            Assert.GreatherThan(index, -1);
            _componentData[index].Reset(dst);
        }

        internal int GetComponentIndex<T>() where T : unmanaged
         => Specification.GetComponentIndex(ComponentType<T>.Type);

        internal Span<T> GetComponentData<T>(int index = -1, ComponentType componentType = default)
            where T : unmanaged
        {
            if (index == -1)
                index = GetComponentIndex<T>();

            if (componentType.ID == 0)
                componentType = ComponentType<T>.Type;

            Assert.Range(index, 0, _componentData.Length);

            var componentDataArray = _componentData[index];
            return componentDataArray.AsSpan<T>(componentType);
        }

        protected override void OnManagedDispose()
        {
            _componentData = null;
            _allocator = null;
        }

        protected override void OnUnmanagedDispose()
        {
            _componentData.DisposeAll();
        }

        internal static void CopyTo(ComponentPackedArray srcArray, int srcIndex, ComponentPackedArray dstArray, int dstIndex)
        {
            SharedComponentArrays(srcArray, dstArray, (src, dst) => ComponentDataArray.CopyTo(src, srcIndex, dst, dstIndex));
        }

        internal delegate void SharedComponentCallback(ComponentDataArray srcArray, ComponentDataArray dstArray);
        internal static int SharedComponentArrays(ComponentPackedArray srcArray, ComponentPackedArray dstArray, SharedComponentCallback shareCallback)
        {
            var i0 = 0;
            var i1 = 0;
            var index = 0;

            System.Span<ComponentType> a = srcArray.Specification.ComponentTypes;
            System.Span<ComponentType> b = dstArray.Specification.ComponentTypes;
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
