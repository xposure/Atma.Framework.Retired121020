namespace Atma.Memory
{
    using System;

    using static Atma.Debug;

    public sealed class MemoryBlock : IDisposable
    {
        private AllocationHandle _handle;
        private int _length;
        private int _offset = 0;

        public MemoryBlock(Allocator allocator, int sizeInBytes)
        {
            _handle = MemoryManager.Take(allocator, sizeInBytes);
            _length = sizeInBytes;
        }

        public unsafe T* Take<T>(int length)
            where T : unmanaged
        {
            var size = SizeOf<T>.Size * length;
            Assert(size + _offset <= _length);

            var p = (byte*)_handle.Address + _offset;
            _offset += size;

            return (T*)p; ;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                //if (disposing)
                //{
                //}

                MemoryManager.Free(ref _handle);

                disposedValue = true;
            }
        }

        ~MemoryBlock()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
             GC.SuppressFinalize(this);
        }
        #endregion

    }
}
