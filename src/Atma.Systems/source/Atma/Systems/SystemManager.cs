namespace Atma.Systems
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using Atma.Common;
    using Atma.Entities;
    using Atma.Memory;
    using Microsoft.Extensions.Logging;

    public sealed class SystemManager : UnmanagedDispose
    {
        private ILoggerFactory _logFactory;
        private ILogger _logger;

        private IAllocator _allocator;

        private EntityManager _entityManager;

        private NativeBuffer _variables;

        private Dictionary<string, NativeBufferPtr> _variableLookup = new Dictionary<string, NativeBufferPtr>();

        private SystemGroup[] _systems = new SystemGroup[32];

        private Dictionary<string, int> _stages = new Dictionary<string, int>();

        public string DefaultStage { get; set; }

        public SystemManager(ILoggerFactory logFactory, EntityManager em, IAllocator allocator)
        {
            _logFactory = logFactory;
            _logger = logFactory.CreateLogger<SystemManager>();
            _entityManager = em;
            _allocator = allocator;

            DefaultStage = "Default";
            _variables = new NativeBuffer(allocator);
        }

        public void AddStage(string name)
        {
            Assert.EqualTo(_stages.ContainsKey(name), false);
            Assert.Range(_stages.Count, 0, 32);

            var index = _stages.Count;
            _stages.Add(name, index);
            _systems[index] = new SystemGroup(name, null, 0, null);
        }

        internal NativeBufferPtr GetVariable(string name, Type type)
        {
            if (!_variableLookup.TryGetValue(name, out var ptr))
            {
                ptr = _variables.Take(Marshal.SizeOf(type));
                _variableLookup.Add(name, ptr);
            }

            return ptr;
        }

        public void Init()
        {
            for (var i = 0; i < _systems.Length; i++)
                if (_systems[i] != null)
                    _systems[i].Init();
        }

        public void Tick()
        {
            for (var i = 0; i < _systems.Length; i++)
                if (_systems[i] != null)
                    _systems[i].Tick(this, _entityManager);
        }

        public void Register(SystemProducer system)
        {
            system.Register(this);
        }
        public T Register<T>(T system)
            where T : ISystem
        {
            var stages = system.Stages == null ? new string[] { DefaultStage } : system.Stages;
            for (var i = 0; i < stages.Length; i++)
            {
                if (!_stages.TryGetValue(stages[i], out var index))
                    throw new Exception($"Stage [{stages[i]}] was not registered.");

                _systems[index].Add(system);
            }
            return system;
        }

        public void Tick(params string[] stages)
        {
            if (stages.Length == 0)
            {
                foreach (var stage in _stages.Values)
                    _systems[stage].Tick(this, _entityManager);
            }
            else
            {
                for (var i = 0; i < stages.Length; i++)
                {
                    if (!_stages.TryGetValue(stages[i], out var index))
                        throw new Exception($"Stage [{stages[i]}] was not registered.");

                    _systems[index].Tick(this, _entityManager);
                }
            }
        }

        protected override void OnUnmanagedDispose()
        {
            _variables.Dispose();
        }

        protected override void OnManagedDispose()
        {
            if (_systems != null)
            {
                for (var i = 0; i < _systems.Length; i++)
                {
                    if (_systems[i] != null)
                    {
                        _systems[i].Dispose();
                        _systems[i] = null;
                    }
                }

                _systems = null;
            }
        }
    }
}