namespace Atma.Memory
{
    using System.Runtime.InteropServices;
    using static Atma.Debug;


    [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 16)]
    public unsafe struct AllocationHandleOld
    {
        public void* Address;
        internal ulong Flags;

        internal Allocator Allocator => (Allocator)(Flags & 0xff);

        internal uint HeapIndex => (uint)((Flags >> 8) & 0xffffff);

        internal uint Version => (uint)(Flags >> 24);

        internal uint Checksum => (uint)(Flags >> 32);

        public bool IsValid => Address != null;

        public override string ToString()
        {
            return $"Address: {((long)Address).ToString("X16")}, Allocator: {Allocator}, HeapIndex: {HeapIndex.ToString("X8")}, Version: {Version.ToString("X8")}: Checksum: {Checksum.ToString("X8")}";
        }

        internal AllocationHandleOld(void* address, Allocator allocator, uint heapIndex, uint checksum)
        {
            Address = address;
            Flags = (byte)allocator;
            Flags += (heapIndex & 0xffffff) << 8;
            //Flags += version << 24;
            Flags += (ulong)checksum << 32;
            Assert(Allocator == allocator);
            Assert(HeapIndex == heapIndex);
            Assert(Checksum == checksum);
        }

        internal AllocationHandleOld Clone()
        {
            var handle = new AllocationHandleOld();
            handle.Address = Address;
            handle.Flags = Flags;

            return handle;
        }

        //public static explicit operator (void* value)
    }
}
