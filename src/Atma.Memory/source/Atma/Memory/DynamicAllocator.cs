namespace Atma.Memory
{
    using System;
    using System.Runtime.InteropServices;
    using Atma.Common;
    using Microsoft.Extensions.Logging;

    public unsafe sealed class DynamicAllocator : UnmanagedDispose, IAllocator
    {
        private struct DynamicMemoryHandle
        {
            public readonly IntPtr Address;
            public readonly uint Id;
            public uint Version;
            public readonly uint Size;

            public override string ToString() => $"{{ Address: {Address}, Id: {Id:X8}, Version: {Version:X8}, Size: {Size} }}";

            public DynamicMemoryHandle(IntPtr address, uint id, uint version, uint size)
            {
                Address = address;
                Id = id;
                Version = version;
                Size = size;
            }

            public bool IsValid => Address != IntPtr.Zero;

        }
        private int _blocks = 0;
        public int Blocks => _blocks;

        private uint _size;
        public uint Size => _size;

        private ILogger _logger;
        private ILoggerFactory _logFactory;
        private ObjectPoolInt _dynamicMemoryTracker = new ObjectPoolInt(1024);
        private DynamicMemoryHandle[] _handles = new DynamicMemoryHandle[1024];
        private bool _enableStackTrace = false;

        public DynamicAllocator(ILoggerFactory logFactory, bool enableStackTrace = false)
        {
            _logFactory = logFactory;
            _logger = _logFactory.CreateLogger<DynamicAllocator>();
            _enableStackTrace = enableStackTrace;

            //take the first to enforce id = 0 as invalid
            _dynamicMemoryTracker.Take();
        }

        protected override void OnUnmanagedDispose()
        {
            for (var i = 0; i < _handles.Length; i++)
            {
                Assert.EqualTo(_handles[i].IsValid, false);
                if (_handles[i].IsValid)
                {
                    _logger.LogWarning($"{_handles[i]}\nAllocation was not released, consider enabling stack tracing.");
                    Marshal.FreeHGlobal(_handles[i].Address);
                    _handles[i] = new DynamicMemoryHandle(IntPtr.Zero, 0, 0, 0);
                    //_dynamicMemoryTracker.Return(i);
                }
            }
        }

        public AllocationHandle Take(int size)
        {
            Assert.GreatherThan(size, 0);

            var id = (uint)_dynamicMemoryTracker.Take();
            if (id > _handles.Length)
            {
                var newHandles = new DynamicMemoryHandle[_handles.Length * 3 / 2];
                Array.Copy(_handles, newHandles, _handles.Length);
                _handles = newHandles;
            }

            if (!Unsafe.IsAligned16(size))
                size = Unsafe.Align16(size + 15); // some room for alignment

            _size += (uint)size;

            var intPtr = Marshal.AllocHGlobal(size);
            Unsafe.ClearAlign16((void*)intPtr, size);
            _handles[id] = new DynamicMemoryHandle(intPtr, id, 0, (uint)size);

            ++_blocks;
            //_logger.LogDebug($"DynamicAlloc allocated { _handles[id]}");

            return new AllocationHandle(intPtr, id, 0);
        }

        public void Free(ref AllocationHandle handle)
        {
            AssertValid(ref handle);
            //_logger.LogDebug($"DynamicAlloc freeing {handle}");
            Marshal.FreeHGlobal(handle.Address);
            _size -= _handles[handle.Id].Size;
            _handles[handle.Id] = new DynamicMemoryHandle(IntPtr.Zero, 0, 0, 0);
            handle = AllocationHandle.Null;
            --_blocks;
        }

        private void AssertValid(ref AllocationHandle handle)
        {
            Assert.Range(handle.Id, 0, (uint)_handles.Length);
            ref var h = ref _handles[handle.Id];
            Assert.EqualTo(handle.Address, h.Address);
            Assert.EqualTo(handle.Flags, h.Version);
        }

        public AllocationHandle Transfer(ref AllocationHandle handle)
        {
            AssertValid(ref handle);
            ref var h = ref _handles[handle.Id];
            h.Version++;
            handle = AllocationHandle.Null;
            return new AllocationHandle(h.Address, h.Id, h.Version);
        }
    }
}
