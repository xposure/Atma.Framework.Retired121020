namespace Atma
{
    using System.Threading;

    public enum LockType
    {
        Read,
        Write
    }

    public ref struct RWLock
    {
        private readonly ReaderWriterLockSlim _rwLock;
        private readonly LockType _lockType;

        public RWLock(ReaderWriterLockSlim rwLock, LockType lockType)
        {
            _rwLock = rwLock;
            _lockType = lockType;
        }

        public void Dispose()
        {
            if (_lockType == LockType.Read)
                _rwLock.ExitReadLock();
            else
                _rwLock.ExitReadLock();
        }
    }

    public ref struct ReadLock
    {
        private readonly ReaderWriterLockSlim _rwLock;
        public ReadLock(ReaderWriterLockSlim rwLock)
        {
            _rwLock = rwLock;
        }

        public void Dispose()
        {
            _rwLock.ExitReadLock();
        }
    }

    public ref struct WriteLock
    {
        private readonly ReaderWriterLockSlim _rwLock;
        public WriteLock(ReaderWriterLockSlim rwLock)
        {
            _rwLock = rwLock;
        }

        public void Dispose()
        {
            _rwLock.ExitWriteLock();
        }
    }
}