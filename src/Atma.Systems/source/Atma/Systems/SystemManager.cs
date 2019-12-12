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

        public uint DefaultStages { get; private set; }

        public SystemManager(ILoggerFactory logFactory, EntityManager em, IAllocator allocator, uint defaultStage = 1)
        {
            _logFactory = logFactory;
            _logger = logFactory.CreateLogger<SystemManager>();
            _entityManager = em;
            _allocator = allocator;
            DefaultStages = defaultStage;

            _variables = new NativeBuffer(allocator);

            // for (var i = 0; i < _systems.Length; i++)
            //     _systems[i] = new SystemGroup($"Stage {i}", null, 0);
        }

        public uint AddStage(uint stage, string name)
        {
            Assert.NotEqualTo(stage, 0);
            var index = 0;
            var bits = 0;

            for (; index < _systems.Length; index++)
            {
                if ((stage & 1) == 1)
                    bits++;

                stage >>= 1;

                if (stage == 0)
                    break;
            }

            Assert.EqualTo(bits, 1);
            Assert.EqualTo(_systems[index], null);
            _systems[index] = new SystemGroup(name, null, 0, stage);
            return stage;
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

        public void Add(SystemProducer system)
        {
            system.Register(this);
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

        protected override void OnUnmanagedDispose()
        {
            _variables.Dispose();
        }

        public T Register<T>(T system)
            where T : ISystem
        {
            var stages = system.Stages == 0 ? DefaultStages : system.Stages;
            var index = 0;
            while (stages > 0)
            {
                if ((stages & 1) == 1)
                    _systems[index].Add(system);

                index++;
                stages >>= 1;
            }
            return system;
        }

        public void Tick(uint stages)
        {
            Assert.NotEqualTo(stages, 0);
            var index = 0;
            while (stages > 0)
            {
                if ((stages & 1) == 1)
                {
                    if (_systems[index] == null)
                        throw new Exception("Make sure you call add stage first to init and set its name.");

                    _systems[index].Tick(this, _entityManager);
                }

                index++;
                stages >>= 1;
            }
        }
    }


}