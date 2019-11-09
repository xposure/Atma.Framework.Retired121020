
namespace Atma.Memory
{
    using static Atma.Debug;

    using System;

    public unsafe class StackAllocator : IAllocator
    {
        private UnmanagedMemory _memory;
        private IntPtr _front, _back;
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
            _front = _memory.Begin;
            _back = _memory.End;
            _thrash = thrash;

            if (clearToZero)
                Unsafe.ClearAlign16((void*)_front, _memory.ActualSize);
        }

        public void Dispose()
        {
            _memory?.Dispose();
            _memory = null;
        }

        public unsafe AllocationHandle Take(int size)
        //    where T : unmanaged
        {
            //var size = SizeOf<T>.Size;
            Assert(size > 0);

            size = Unsafe.Align16(size);
            Assert(_free >= size);

            var index = _allocationIndex++;
            var addr = _front.ToPointer();
            _front = IntPtr.Add(_front, size);
            _free -= (uint)size;

            //safe to do the uint cast since we are Assert > 0
            return new AllocationHandle(addr, index, (uint)size);
        }

        public unsafe void Free(ref AllocationHandle handle)
        {
            _allocationIndex--;
            Assert(handle.Id == _allocationIndex);
            _front = IntPtr.Subtract(_front, (int)handle.Length);

            if (_thrash)
                Unsafe.ClearAlign16(handle.Address, (int)handle.Length, _thrashValue);
        }
    }
}