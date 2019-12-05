namespace Atma.Entities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Atma.Common;
    using Atma.Memory;
    using Microsoft.Extensions.Logging;

    public sealed class EntityArrayList : UnmanagedDispose, IEnumerable<EntityChunkList>
    {
        private ILogger _logger;
        private ILoggerFactory _logFactory;
        private IAllocator _allocator;

        private LookupList<List<EntityChunkList>> _specMapping = new LookupList<List<EntityChunkList>>();
        private LookupList<EntitySpec> _knownSpecs = new LookupList<EntitySpec>();
        private List<EntityChunkList> _entityArrays = new List<EntityChunkList>();
        public IReadOnlyList<EntityChunkList> EntityArrays => _entityArrays;

        public int Count => _entityArrays.Count;

        public EntityArrayList(ILoggerFactory logFactory, IAllocator allocator)
        {
            _logger = logFactory.CreateLogger<EntityArrayList>();
            _logFactory = logFactory;
            _allocator = allocator;
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

                var array = new EntityChunkList(_logFactory, _allocator, spec, specIndex);
                _entityArrays.Add(array);
                for (var i = 0; i < spec.ComponentTypes.Length; i++)
                {
                    if (!_specMapping.TryGetValue(spec.ComponentTypes[i].ID, out var lists))
                    {
                        lists = new List<EntityChunkList>();
                        _specMapping.Add(spec.ComponentTypes[i].ID, lists);
                    }

                    lists.Add(array);
                }
            }
            return specIndex;
        }

        internal int EntityCount(in ComponentType type)
        {
            if (!_specMapping.TryGetValue(type.ID, out var lists))
                return 0;

            var count = 0;
            for (var i = 0; i < lists.Count; i++)
                count += lists[i].EntityCount;

            return count;
        }

        internal int EntityCount()
        {
            var count = 0;
            for (var i = 0; i < _entityArrays.Count; i++)
                count += _entityArrays[i].EntityCount;

            return count;
        }

        public int EntityCount<T>() where T : unmanaged => EntityCount(ComponentType<T>.Type);

        internal List<EntityChunkList> FindSmallest(in EntitySpec spec) => FindSmallest(spec.ComponentTypes);
        internal List<EntityChunkList> FindSmallest(Span<ComponentType> componentTypes)
        {
            var current = _specMapping[componentTypes[0].ID];
            var count = EntityCount(componentTypes[0]);

            for (var i = 1; i < componentTypes.Length; i++)
            {
                var nextCount = EntityCount(componentTypes[i]);
                if (nextCount < count)
                    current = _specMapping[componentTypes[i].ID];
            }

            return current;
        }

        // public IEnumerable<EntityChunkList> Filter(EntitySpec spec)
        // {
        //     var smallest = FindSmallest(spec);
        //     for (var i = 0; i < smallest.Count; i++)
        //     {
        //         var array = smallest[i];
        //         if (array.Specification.HasAll(spec.ComponentTypes)
        //             //&& (excludedComponents.IsEmpty || array.Specification.HasNone(excludedComponents))
        //             )
        //             yield return array;
        //     }
        // }


        internal int GetOrCreateSpec(Span<ComponentType> componentTypes)
        {
            var specId = ComponentType.CalculateId(componentTypes);
            var specIndex = _knownSpecs.IndexOf(specId);
            if (specIndex == -1)
                return GetOrCreateSpec(new EntitySpec(specId, componentTypes));

            return specIndex;
        }

        public EntityChunkList this[int index]
        {
            get
            {
                return _entityArrays[index];
            }
        }

        public EntityChunkList this[in EntitySpec spec]
        {
            get
            {
                var index = GetOrCreateSpec(spec);
                return _entityArrays[index];
            }
        }
        public EntityChunkList this[Span<ComponentType> componentTypes]
        {
            get
            {
                var index = GetOrCreateSpec(componentTypes);
                return _entityArrays[index];
            }
        }

        protected override void OnUnmanagedDispose()
        {
            _entityArrays.DisposeAll();
            _entityArrays.Clear();
        }

        public IEnumerator<EntityChunkList> GetEnumerator()
        {
            foreach (var it in _entityArrays)
                yield return it;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var it in _entityArrays)
                yield return it;
        }
    }

    public static class EntityArrayListExtensions
    {
        public static void Filter(this EntityArrayList it, Span<ComponentType> componentTypes, Action<EntityChunkList> result) => it.Filter(componentTypes, Span<ComponentType>.Empty, result);
        public static void Filter(this EntityArrayList it, Span<ComponentType> componentTypes, Span<ComponentType> excludedComponents, Action<EntityChunkList> result)
        {
            var smallest = it.FindSmallest(componentTypes);
            for (var i = 0; i < smallest.Count; i++)
            {
                var array = smallest[i];
                if (array.Specification.HasAll(componentTypes)
                    && (excludedComponents.IsEmpty || array.Specification.HasNone(excludedComponents)))
                    result(array);
            }
        }
    }

}