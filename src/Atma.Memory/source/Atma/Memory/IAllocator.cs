namespace Atma.Memory
{
    using System;
    using static Atma.Debug;

    public enum AllocatorBounds
    {
        Front,
        Back
    }

    public interface IAllocator : IDisposable
    {
        AllocationHandle2 Take<T>(int count, AllocatorBounds bounds = AllocatorBounds.Front)
            where T : unmanaged;
        void Free(AllocationHandle2 handle2);
    }

    public unsafe struct AllocationHandle2 : IDisposable
    {
        public readonly IAllocator Allocator;
        public readonly void* Address;
        public readonly uint Id;
        public readonly uint Length;

        public AllocationHandle2(IAllocator allocator, void* address, uint id, uint length)
        {
            Allocator = allocator;
            Address = address;
            Id = id;
            Length = length;
        }

        public void Dispose()
        {
            Allocator.Free(this);
        }
    }

    public class StackAllocator : IAllocator
    {
        private UnmanagedMemory _memory;
        private IntPtr _front, _back;
        private uint _frontIndex = 0, _backIndex = uint.MaxValue;
        private uint _free;
        private bool _thrash;
        private uint _thrashValue = 0x55aa5aa5;
        public uint FreeSize => _free;

        public StackAllocator(uint size, bool thrash = false)
        {
            _memory = new UnmanagedMemory(size);
            _free = _memory.Size;
            _front = _memory.Begin;
            _back = _memory.End;
            _thrash = thrash;
        }

        public void Dispose()
        {
            _memory?.Dispose();
            _memory = null;
        }

        public unsafe AllocationHandle2 Take<T>(int count, AllocatorBounds bounds)
            where T : unmanaged
        {
            var size = SizeOf<T>.Size;
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
            return new AllocationHandle2(this, addr, index, (uint)size);
        }

        public unsafe void Free(AllocationHandle2 handle2)
        {
            var bounds = (handle2.Id & 0x80000000) == 0 ?
                            AllocatorBounds.Front : AllocatorBounds.Back;

            if (bounds == AllocatorBounds.Front)
            {
                _frontIndex--;
                Assert(handle2.Id == _frontIndex);
                _front = IntPtr.Subtract(_front, (int)handle2.Length);
            }
            else
            {
                _backIndex++;
                Assert(handle2.Id == _backIndex);
                _back = IntPtr.Add(_back, (int)handle2.Length);
            }

            if (_thrash)
                Unsafe.ClearAlign16(handle2.Address, (int)handle2.Length, _thrashValue);
        }
    }
}