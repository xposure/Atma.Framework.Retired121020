namespace Atma.Memory
{
    using System;
    using System.Diagnostics;

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