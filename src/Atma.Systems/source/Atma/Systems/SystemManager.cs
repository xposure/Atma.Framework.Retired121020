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

    public sealed class SystemManager
    {
        private ILoggerFactory _logFactory;
        private ILogger _logger;

        private IAllocator _allocator;

        private EntityManager _entityManager;

        private SystemGroup _systems;

        private NativeBuffer _variables;

        private Dictionary<string, NativeBufferPtr> _variableLookup = new Dictionary<string, NativeBufferPtr>();

        public SystemManager(ILoggerFactory logFactory, EntityManager em, IAllocator allocator)
        {
            _logFactory = logFactory;
            _logger = logFactory.CreateLogger<SystemManager>();
            _entityManager = em;
            _systems = new SystemGroup("Root", 0);
            _allocator = allocator;

            _variables = new NativeBuffer(allocator);
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

        public T Add<T>(T system)
            where T : ISystem
        {
            _systems.Add(system);
            return system;
        }

        public void Init()
        {
            _systems.Init();
        }

        public void Tick()
        {
            if (!_systems.Disabled)
                _systems.Tick(this, _entityManager);
        }
    }


}