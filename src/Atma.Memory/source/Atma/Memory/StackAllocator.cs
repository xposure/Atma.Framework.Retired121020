
namespace Atma.Memory
{
    using static Atma.Debug;

    using System;

    public unsafe class StackAllocator : IAllocator
    {
        private UnmanagedMemory _memory;
        private IntPtr _dataPtr;
        private uint _allocationIndex = 0;
        private uint _free;
        private bool _thrash;
        private uint _thrashValue = 0x55aa5aa5;
        public uint FreeSize => _free;

        //TODO: Should we really clear to zero by default?
        //the EntityChunk should be managing out of bounds indexing and moving of data
        public StackAllocator(uint size, bool thrash = false, bool clearToZero = true)
        {
            _memory = new UnmanagedMemory(size);
            _free = _memory.Size;
            _dataPtr = _memory.Begin;
            _thrash = thrash;

            if (clearToZero)
                Unsafe.ClearAlign16((void*)_dataPtr, _memory.ActualSize);
        }

        public void Dispose()
        {
            _memory?.Dispose();
            _memory = null;
        }

        public unsafe AllocationHandle Take(int size)
        {
            Assert(size > 0);

            size = Unsafe.Align16(size);
            Assert(_free >= size);

            var index = _allocationIndex++;
            var addr = _dataPtr.ToPointer();
            _dataPtr = IntPtr.Add(_dataPtr, size);
            _free -= (uint)size;

            //safe to do the uint cast since we are Assert > 0
            return new AllocationHandle(_dataPtr, index, (uint)size);
        }

        public unsafe void Free(ref AllocationHandle handle)
        {
            _allocationIndex--;
            Assert(handle.Id == _allocationIndex);
            _dataPtr = IntPtr.Subtract(_dataPtr, (int)handle.Flags);

            if (_thrash)
                Unsafe.ClearAlign16((void*)handle.Address, (int)handle.Flags, _thrashValue);
        }

        public AllocationHandle Transfer(ref AllocationHandle handle)
        {
            var copy = handle;
            handle = new AllocationHandle(IntPtr.Zero, 0, 0);
            return copy;
        }
    }
}