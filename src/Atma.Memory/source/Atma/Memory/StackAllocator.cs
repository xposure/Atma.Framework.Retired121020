
namespace Atma.Memory
{
    using System;

    [System.Diagnostics.DebuggerStepThrough]
    public unsafe class StackAllocator : IAllocator
    {
        private IAllocator _allocator;
        private AllocationHandle _handle;

        private IntPtr _dataPtr;
        private uint _allocationIndex = 0;
        private uint _free;
        private bool _thrash;
        private uint _thrashValue = 0x55aa5aa5;
        public uint FreeSize => _free;

        //TODO: Should we really clear to zero by default?
        //the EntityChunk should be managing out of bounds indexing and moving of data
        public StackAllocator(IAllocator allocator, int size, bool thrash = false)
        {
            _allocator = allocator;
            _handle = allocator.Take(size);
            _free = (uint)size;
            _dataPtr = _handle.Address;
            _thrash = thrash;
        }

        public void Dispose()
        {
            _allocator.Free(ref _handle);
            _allocator = null;
        }

        public unsafe AllocationHandle Take(int size)
        {
            Assert.GreatherThan(size, 0);

            size = Unsafe.Align16(size);
            Assert.GreatherThanEqualTo(_free, size);

            var index = _allocationIndex++;
            var addr = _dataPtr.ToPointer();
            _dataPtr = IntPtr.Add(_dataPtr, size);
            _free -= (uint)size;

            //safe to do the uint cast since we are Assert > 0
            return new AllocationHandle(_dataPtr, index, (uint)size);
        }

        public unsafe void Free(ref AllocationHandle handle)
        {
            Contract.EqualTo(handle.Id, _allocationIndex - 1);
            _allocationIndex--;
            //Contract.Assert(handle.Id == _allocationIndex);
            //Contract.Requires(handle.Id == _allocationIndex);
            //handle.Id.AssertShouldBe(_allocationIndex);
            _dataPtr = IntPtr.Subtract(_dataPtr, (int)handle.Flags);

            if (_thrash)
                Unsafe.ClearAlign16((void*)handle.Address, (int)handle.Flags, _thrashValue);

            handle = AllocationHandle.Null;
        }

        public AllocationHandle Transfer(ref AllocationHandle handle)
        {
            var copy = handle;
            handle = new AllocationHandle(IntPtr.Zero, 0, 0);
            return copy;
        }
    }
}