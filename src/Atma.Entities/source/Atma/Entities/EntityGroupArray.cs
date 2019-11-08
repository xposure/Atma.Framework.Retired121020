namespace Atma.Entities
{
    using System;
    using System.Linq;
    using Atma.Common;
    using Atma.Memory;

    using static Atma.Debug;

    public interface IEntityGroupArray : IDisposable
    {
        //ref readonly Entity Create();
        //int Create();
        //void Delete(int index);

        //int Free { get; }
        //int Count { get; }

        EntitySpec Specification { get; }
        int Length { get; }

        void Move(int src, int dst);

        ComponentDataArrayReadLock ReadComponent<T>(out ReadOnlySpan<T> span) where T : unmanaged;
        ComponentDataArrayWriteLock WriteComponent<T>(out Span<T> span) where T : unmanaged;
    }

    public unsafe sealed class EntityGroupArray : UnmanagedDispose, IEntityGroupArray
    {
        private ComponentDataArray2[] _componentData;
        private IAllocator _allocator;

        public EntitySpec Specification { get; }
        public int Length => Entity.ENTITY_MAX;

        public EntityGroupArray(EntitySpec specification)
        {
            Specification = specification;

            _allocator = new StackAllocator((uint)(Specification.EntitySize * Entity.ENTITY_MAX));

            var _componentTypes = Specification.ComponentTypes;
            _componentData = new ComponentDataArray2[_componentTypes.Length];
            for (var i = 0; i < _componentTypes.Length; i++)
                _componentData[i] = new ComponentDataArray2(_allocator, _componentTypes[i], Entity.ENTITY_MAX);
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

        public ComponentDataArrayReadLock ReadComponent<T>(out ReadOnlySpan<T> span)
            where T : unmanaged
        {
            var index = GetComponentIndex<T>();
            Assert(index > -1);

            var componentDataArray = _componentData[index];
            return componentDataArray.AsReadOnlySpan(out span);
        }

        public ComponentDataArrayWriteLock WriteComponent<T>(out Span<T> span)
            where T : unmanaged
        {
            var index = GetComponentIndex<T>();
            Assert(index > -1);

            var componentDataArray = _componentData[index];
            return componentDataArray.AsSpan(out span);

        }

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
    }
}
