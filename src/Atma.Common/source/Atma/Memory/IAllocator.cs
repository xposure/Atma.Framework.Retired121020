namespace Atma.Memory
{
    using System;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    public enum AllocatorBounds
    {
        Front,
        Back
    }

    public interface IAllocator : IDisposable
    {
        AllocationHandle Take(int size);
        void Free(ref AllocationHandle handle);
        AllocationHandle Transfer(ref AllocationHandle handle);

        //public void Log(string message) { }
    }

    public ref struct DisposableAllocHandle
    {
        private ILoggerFactory _logFactory;
        private ILogger _logger;
        private AllocationHandle _handle;
        public readonly IAllocator _allocator;

        public DisposableAllocHandle(ILoggerFactory logFactory, IAllocator allocator, in AllocationHandle handle)
        {
            _logFactory = logFactory;
            _logger = logFactory?.CreateLogger("DisposableAllocHandle");
            _allocator = allocator;
            _handle = handle;
        }

        public IntPtr Address => _handle.Address;
        public uint Id => _handle.Id;
        public uint Flags => _handle.Flags;

        public AllocationHandle Handle => _handle;

        public void Free()
        {
            if (_handle.IsValid)
            {
                _logger?.LogDebug($"(DisposableAllochandle) Freeing {_handle}");
                _allocator.Free(ref _handle);
            }
        }

        public void Dispose()
        {
            if (_handle.IsValid)
            {
                _logger?.LogDebug($"(DisposableAllochandle) Auto disposing {_handle}");
                Free();
            }
        }
    }

    public unsafe readonly struct AllocationHandle //: IDisposable
    {
        public static AllocationHandle Null => new AllocationHandle(IntPtr.Zero, 0, 0);
        public readonly IntPtr Address;
        public readonly uint Id;
        public readonly uint Flags;

        public bool IsValid => Address != IntPtr.Zero;

        // #if DEBUG
        //         public readonly string StackTrace;
        // #endif

        public AllocationHandle(IntPtr address, uint id, uint flags)
        {
            // #if DEBUG
            //StackTrace = Environment.StackTrace;
            // #endif

            Address = address;
            Id = id;
            Flags = flags;
        }

        public override string ToString() => $"{{ Address: {Address}, Id: {Id:X8}, Flags: {Flags:X8} }}";
    }
}