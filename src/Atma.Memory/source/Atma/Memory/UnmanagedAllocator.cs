namespace Atma.Memory
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Atma.Common;
    using static Atma.Debug;

    public unsafe sealed class DynamicAllocator : UnmanagedDispose, IAllocator
    {
        private int _blocks;
        public int Blocks => _blocks;

        private uint _size;
        public uint Size => _size;

        private ObjectPoolInt _dynamicMemoryTracker = new ObjectPoolInt(1024);
        private AllocationHandle[] _handles = new AllocationHandle[1024];

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
                    _handles[i] = new AllocationHandle(IntPtr.Zero, 0, 0);
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
                var newHandles = new AllocationHandle[_handles.Length * 3 / 2];
                Array.Copy(_handles, newHandles, _handles.Length);
                _handles = newHandles;
            }

            if (!Unsafe.IsAligned16(size))
                size = Unsafe.Align16(size + 15); // some room for alignment

            var intPtr = Marshal.AllocHGlobal(size);
            ref var handle = ref _handles[id];

            handle = new AllocationHandle(intPtr, id, 0);
            return handle;
        }

        public void Free(ref AllocationHandle handle)
        {
            Assert(handle.Id >= 0 && handle.Id < _handles.Length);

            ref var h = ref _handles[handle.Id];
            Assert(handle.Address == h.Address);

            Marshal.FreeHGlobal(h.Address);
            h = new AllocationHandle(IntPtr.Zero, 0, 0);
        }
    }
}
