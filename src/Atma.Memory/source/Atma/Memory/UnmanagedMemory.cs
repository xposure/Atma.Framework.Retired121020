namespace Atma.Memory
{
    using System;
    using System.Runtime.InteropServices;

    using static Atma.Debug;

    public unsafe sealed class UnmanagedMemory : IDisposable
    {
        private readonly string _stackTrace;

        private readonly IntPtr _heapPtr;
        public readonly IntPtr Begin;
        public readonly IntPtr End;
        public readonly uint Size;
        public readonly uint ActualSize;

        public UnmanagedMemory(uint size)
        {
            _stackTrace = Environment.StackTrace;

            if (!Unsafe.IsAligned16(size))
                ActualSize = Unsafe.Align16(size + 15); // some room for alignment
            else
                ActualSize = size;

            Size = size;

            _heapPtr = Marshal.AllocHGlobal(new IntPtr(ActualSize));

            Begin = Unsafe.Align16(_heapPtr);

            var addr = (byte*)Begin;
            addr += ActualSize;
            End = new IntPtr(addr);

            Assert(Unsafe.IsAligned16(Begin));
            Assert(Unsafe.IsAligned16(End));
            Assert(Unsafe.IsAligned16(Size));
        }

        public void Clear(int value = 0)
        {
            var begin = (int*)this.Begin;
            var end = (int*)End;
            while (begin < end)
                *begin++ = value;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                Marshal.FreeHGlobal(_heapPtr);

                disposedValue = true;
            }
        }

        ~UnmanagedMemory()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
