
namespace Atma.Memory
{
    using static Atma.Debug;

    using System;

    public unsafe class StackAllocator : IAllocator
    {
        private UnmanagedMemory _memory;
        private IntPtr _front, _back;
        private uint _frontIndex = 0, _backIndex = uint.MaxValue;
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

        public unsafe AllocationHandle Take(int size, AllocatorBounds bounds)
        //    where T : unmanaged
        {
            //var size = SizeOf<T>.Size;
            Assert(size > 0);

            size = Unsafe.Align16(size);
            Assert(_free >= size);

            uint index; //using 0x80000000 to determine front or back
            void* addr;
            if (bounds == AllocatorBounds.Front)
            {
                index = _frontIndex++;
                Assert((index & 0x80000000) == 0);
                addr = _front.ToPointer();
                _front = IntPtr.Add(_front, size);
            }
            else
            {
                index = _backIndex--;
                Assert((index & 0x80000000) == 0x80000000);
                _back = IntPtr.Subtract(_back, size);
                addr = _back.ToPointer();
            }

            _free -= (uint)size;

            //safe to do the uint cast since we are Assert > 0
            return new AllocationHandle(addr, index, (uint)size);
        }

        private AllocatorBounds GetBounds(ref AllocationHandle handle)
        {
            return (handle.Id & 0x80000000) == 0 ?
                                       AllocatorBounds.Front : AllocatorBounds.Back;
        }

        public unsafe void Free(ref AllocationHandle handle)
        {
            var bounds = GetBounds(ref handle);

            if (bounds == AllocatorBounds.Front)
            {
                _frontIndex--;
                Assert(handle.Id == _frontIndex);
                _front = IntPtr.Subtract(_front, (int)handle.Length);
            }
            else
            {
                _backIndex++;
                Assert(handle.Id == _backIndex);
                _back = IntPtr.Add(_back, (int)handle.Length);
            }

            if (_thrash)
                Unsafe.ClearAlign16(handle.Address, (int)handle.Length, _thrashValue);
        }
    }
}