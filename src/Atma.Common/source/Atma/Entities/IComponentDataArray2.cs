using System;
using System.Threading;

namespace Atma.Entities
{

    public enum LockType
    {
        Read,
        Write
    }

    public ref struct ComponentDataArrayReadLock
    {
        private readonly ReaderWriterLockSlim _rwLock;
        public ComponentDataArrayReadLock(ReaderWriterLockSlim rwLock)
        {
            _rwLock = rwLock;
        }

        public void Dispose()
        {
            _rwLock.ExitReadLock();
        }
    }

    public ref struct ComponentDataArrayWriteLock
    {
        private readonly ReaderWriterLockSlim _rwLock;
        public ComponentDataArrayWriteLock(ReaderWriterLockSlim rwLock)
        {
            _rwLock = rwLock;
        }

        public void Dispose()
        {
            _rwLock.ExitWriteLock();
        }
    }

    public interface IComponentDataArray2 : IDisposable
    {
        int ElementSize { get; }
        int Length { get; }

        ComponentDataArrayWriteLock AsSpan<T>(out Span<T> span) where T : unmanaged;
        ComponentDataArrayReadLock AsReadOnlySpan<T>(out ReadOnlySpan<T> span) where T : unmanaged;
        void Move<T>(int src, int dst) where T : unmanaged;
    }
}