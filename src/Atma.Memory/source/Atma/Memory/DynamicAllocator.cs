namespace Atma.Memory
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Atma.Common;
    using static Atma.Debug;

    public unsafe sealed class DynamicAllocator : UnmanagedDispose, IAllocator
    {
        private struct DynamicMemoryHandle
        {
            public readonly IntPtr Address;
            public readonly uint Id;
            public uint Version;
            public readonly uint Size;

            public DynamicMemoryHandle(IntPtr address, uint id, uint version, uint size)
            {
                Address = address;
                Id = id;
                Version = version;
                Size = size;
            }

            public bool IsValid => Address != IntPtr.Zero;

        }
        private int _blocks;
        public int Blocks => _blocks;

        private uint _size;
        public uint Size => _size;

        private ObjectPoolInt _dynamicMemoryTracker = new ObjectPoolInt(1024);
        private DynamicMemoryHandle[] _handles = new DynamicMemoryHandle[1024];

#if DEBUG
        private bool _enableStackTrace = false;
#endif

        public DynamicAllocator(bool enableStackTrace = false)
        {
            _enableStackTrace = enableStackTrace;

            //take the first to enforce id = 0 as invalid
            _dynamicMemoryTracker.Take();
        }

        protected override void OnUnmanagedDispose()
        {
            for (var i = 0; i < _handles.Length; i++)
            {
                if (_handles[i].IsValid)
                {
                    Console.WriteLine("Allocation was not released, consider enabling stack tracing.");
                    Marshal.FreeHGlobal(_handles[i].Address);
                    _handles[i] = new DynamicMemoryHandle(IntPtr.Zero, 0, 0, 0);
                    //_dynamicMemoryTracker.Return(i);
                }
            }
        }

        public AllocationHandle Take(int size)
        {
            Assert(size > 0);

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
            _handles[id] = new DynamicMemoryHandle(intPtr, id, 0, (uint)size);

            return new AllocationHandle(intPtr, id, 0);
        }

        public void Free(ref AllocationHandle handle)
        {
            AssertValid(ref handle);
            Marshal.FreeHGlobal(handle.Address);
            _size -= _handles[handle.Id].Size;
            _handles[handle.Id] = new DynamicMemoryHandle(IntPtr.Zero, 0, 0, 0);
            handle = new AllocationHandle(IntPtr.Zero, 0, 0);
        }

        private void AssertValid(ref AllocationHandle handle)
        {
            Assert(handle.Id >= 0 && handle.Id < _handles.Length);
            ref var h = ref _handles[handle.Id];
            Assert(handle.Address == h.Address);
            Assert(handle.Flags == h.Version);
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
