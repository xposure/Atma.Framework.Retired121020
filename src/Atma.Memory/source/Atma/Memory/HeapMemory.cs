namespace Atma.Memory
{
    //#define LOG
    using System;
    using System.Runtime.InteropServices;

    using static Atma.Debug;

    [Flags]
    public enum HeapAllocationFlags : int
    {
        Used = 1
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 32)]
    public unsafe struct HeapAllocationHeader
    {
        public uint Checksum;
        public uint MagicSignature;
        public HeapAllocation* Previous; //4-8
        public HeapAllocation* Next;    //4-8
        public int Size; //4
        public HeapAllocationFlags Flags; //4

        //public void* Data; //4-8 - probably not needed

        //public int TotalSize => Size + 32;

        public bool HasNext => Next != null;
        public bool HasPrev => Previous != null;

        public bool IsSet(HeapAllocationFlags flags) => (Flags & flags) == flags;
        public bool IsClear(HeapAllocationFlags flags) => (Flags & flags) == 0;
        public void Set(HeapAllocationFlags flags) => Flags |= flags;
        public void Clear(HeapAllocationFlags flags) => Flags &= (HeapAllocationFlags)((uint)flags ^ 0xffffff);
    }

    //public unsafe struct HeapPointer
    //{
    //    internal readonly HeapAllocation* _heapAllocation;

    //    public void* RawAddress => (byte*)_heapAllocation + HeapAllocation.HEADERSIZE;

    //    internal HeapPointer(HeapAllocation* heapAllocation)
    //    {
    //        _heapAllocation = heapAllocation;
    //    }
    //}

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct HeapAllocation
    {
        public const int HEADERSIZE = 32;
        public const uint MAGICSIG = 0x5aa5aa55;
        public HeapAllocationHeader Header;

        public static void* GetData(HeapAllocation* alloc)
        {
            return (byte*)alloc + HEADERSIZE;
        }

        public static void Initialize(HeapAllocation* alloc, int size)
        {
            Assert(size >= 256); //We want a *sane* start
            alloc->Header.Previous = null;
            alloc->Header.Next = null;
            alloc->Header.Size = size - HEADERSIZE;
            alloc->Header.Flags = 0;
            alloc->Header.Checksum = 0;
            alloc->Header.MagicSignature = MAGICSIG;
        }

        public static void Split(HeapAllocation* alloc, int size, uint checksum = 0)
        {
            //we need to add the header to the size we need
            //var totalSize = size + HEADERSIZE;
            Assert(size > 0);
            Assert(alloc->Header.Size >= size);
            Assert(Unsafe.IsAligned16(size));
            Assert(!alloc->Header.IsSet(HeapAllocationFlags.Used));
            Assert(alloc->Header.MagicSignature == MAGICSIG);

            var remaining = alloc->Header.Size - size;
            if (remaining <= HEADERSIZE) //lets take everything if there isn't enoughs pace to add a header
            {
                alloc->Header.Set(HeapAllocationFlags.Used);
                alloc->Header.Checksum = checksum;
                return;
            }

            var nextAlloc = (HeapAllocation*)((byte*)alloc + HEADERSIZE + size);
            nextAlloc->Header.Previous = alloc;
            nextAlloc->Header.Next = alloc->Header.Next;
            nextAlloc->Header.Size = remaining - HEADERSIZE;
            nextAlloc->Header.Flags = 0;
            nextAlloc->Header.MagicSignature = MAGICSIG;
            //nextAlloc->Header.Data = (void*)((byte*)nextAlloc + HEADERSIZE);

            alloc->Header.Next = nextAlloc;
            alloc->Header.Size = size;
            alloc->Header.Set(HeapAllocationFlags.Used);
            alloc->Header.Checksum = checksum;
        }

        public static void Free(HeapAllocation* alloc, uint checksum = 0)
        {
            Assert(alloc->Header.IsSet(HeapAllocationFlags.Used));
            Assert(alloc->Header.Checksum == checksum);
            Assert(alloc->Header.MagicSignature == MAGICSIG);


            //reclaim next first, so that reclaim previous takes next inheritly
            if (alloc->Header.HasNext &&
                alloc->Header.Next->Header.IsClear(HeapAllocationFlags.Used))
            {
                //we need to consume next
                var next = alloc->Header.Next;
                alloc->Header.Next = next->Header.Next;
                alloc->Header.Size += next->Header.Size + HEADERSIZE;

                //we need to point next + 1's previous to us
                if (next->Header.HasNext)
                    next->Header.Next->Header.Previous = alloc;

                Unsafe.ClearAlign16((void*)next, HEADERSIZE);
            }

            alloc->Header.Clear(HeapAllocationFlags.Used);

            //reclaim previous
            if (alloc->Header.HasPrev &&
                alloc->Header.Previous->Header.IsClear(HeapAllocationFlags.Used))
            {
                var prev = alloc->Header.Previous;
                if (alloc->Header.HasNext)
                {
                    var next = alloc->Header.Next;
                    next->Header.Previous = alloc->Header.Previous;
                    prev->Header.Next = next;
                }
                else
                    prev->Header.Next = null;

                prev->Header.Size += alloc->Header.Size + HEADERSIZE;

                alloc->Header.MagicSignature = 0;
                Unsafe.ClearAlign16((void*)alloc, HEADERSIZE);
            }
        }

        public static HeapAllocation* FindUnused(HeapAllocation* alloc)
        {
            if (alloc->Header.IsClear(HeapAllocationFlags.Used))
                return alloc;

            while (alloc->Header.HasNext)
            {
                alloc = alloc->Header.Next;
                if (alloc->Header.IsClear(HeapAllocationFlags.Used))
                    return alloc;
            }

            return null;
        }

        public delegate bool HeapAllocationFilter(HeapAllocation* x);
        public static int Count(HeapAllocation* alloc, HeapAllocationFilter filter = null)
        {
            var count = 0;
            if (filter?.Invoke(alloc) ?? true)
                count++;

            while (alloc->Header.HasNext)
            {
                alloc = alloc->Header.Next;
                if (filter?.Invoke(alloc) ?? true)
                    count++;
            }

            return count;
        }

        public static int CountReverse(HeapAllocation* alloc, HeapAllocationFilter filter = null)
        {
            var count = 0;
            if (filter?.Invoke(alloc) ?? true)
                count++;

            while (alloc->Header.HasPrev)
            {
                alloc = alloc->Header.Previous;
                if (filter?.Invoke(alloc) ?? true)
                    count++;
            }

            return count;
        }

        public override string ToString()
        {
            return $"Checksum: {Header.Checksum.ToString("X8")},Size: {Header.Size.ToString("X8")}, Prev: {((long)Header.Previous).ToString("X16")}, Next: {((long)Header.Next).ToString("X16")}, {(Header.IsSet(HeapAllocationFlags.Used) ? "Used" : "Free")}";
        }
    }

    public unsafe sealed class HeapMemory : IDisposable
    {
        private readonly IntPtr _heapPtr;
        private readonly string _stackTrace;
        private readonly HeapAllocation* _root;
        private int _freeSize;
        private uint _version;
        private uint _heapIndex;
        private Allocator _allocator;

        public readonly IntPtr AlignedPointer;
        public readonly IntPtr EndPointer;
        public readonly int Size;
        public int FreeSize => _freeSize;

        public int UsedSize => Size - FreeSize;

        public HeapMemory(int size, uint heapIndex = 0, Allocator allocator = Allocator.None)
        {
            size += HeapAllocation.HEADERSIZE;
            if (!Unsafe.IsAligned16(size))
                Size = Unsafe.Align16(size + 15); // some room for alignment
            else
                Size = size;

            _heapPtr = Marshal.AllocHGlobal(size);
            AlignedPointer = Unsafe.Align16(_heapPtr);
            EndPointer = IntPtr.Add(AlignedPointer, Size);
            _stackTrace = Environment.StackTrace;
            _root = (HeapAllocation*)AlignedPointer;

            Size -= HeapAllocation.HEADERSIZE;
            _freeSize = Size;

            HeapAllocation.Initialize(_root, Size);
            _heapIndex = heapIndex;
            _allocator = allocator;

            Assert(Unsafe.IsAligned16(AlignedPointer));
            Assert(Unsafe.IsAligned16(EndPointer));
            Assert(Unsafe.IsAligned16(Size));
        }

        public void Clear(int value = 0)
        {
            var begin = (int*)AlignedPointer;
            var end = (int*)EndPointer;
            while (begin < end)
                *begin++ = value;
        }

        public AllocationHandle Take(int sizeInBytes)
        {
            if (!TryTake(sizeInBytes, out var handle))
                throw new OutOfMemoryException();

            return handle;
        }

        private HeapAllocation* MoveNext(HeapAllocation* alloc)
        {
            //were always passing in ROOT which means the pointer 
            //should never be aligned pointer on next
            if (alloc->Header.Next == null)
                return null;

            Assert(alloc->Header.Next > (HeapAllocation*)AlignedPointer);
            Assert(alloc->Header.Next < (HeapAllocation*)EndPointer);
            return alloc->Header.Next;
        }

        private void Walk(HeapAllocation* alloc)
        {
            while (alloc != null)
            {
#if LOG
                Console.WriteLine(*alloc);
#endif
                alloc = MoveNext(alloc);// alloc->Header.Next;
            }
        }

        public bool TryTake(int size, out AllocationHandle handle)
        {
            size = Unsafe.Align16(size);
            var alloc = _root;

            var prev = alloc;
            try
            {
                while (alloc != null &&
                    (alloc->Header.Size < size ||
                    alloc->Header.IsSet(HeapAllocationFlags.Used)))
                {
                    prev = alloc;
                    alloc = MoveNext(alloc);// alloc->Header.Next;
                }
            }
            catch //(Exception ex)
            {
#if LOG
                Console.WriteLine("Memory access violation*");
                Walk(_root);
#endif
                //Console.WriteLine(prev->Header.Previous->ToString());
                //Console.WriteLine(prev->ToString());
            }

            if (alloc == null)
            {
                handle = default;
                return false;
            }

            var version = unchecked(++_version);
            var checksum = _allocator == Allocator.Stack ? version : (uint)NextChecksum();
            HeapAllocation.Split(alloc, size, checksum);

            _freeSize -= size + HeapAllocation.HEADERSIZE;

            handle = new AllocationHandle(
                (byte*)alloc + HeapAllocation.HEADERSIZE,
                _allocator, _heapIndex, checksum);
            //Assert(handle.Version == checksum);
#if LOG
            Console.WriteLine($"Take: {size.ToString("X8")}");
            Walk(_root);
#endif
            return true;
        }

        public void Free(ref AllocationHandle handle)
        {
            Assert(handle.IsValid);

            var rawAddress = handle.Address;
            var alloc = (HeapAllocation*)((byte*)rawAddress - HeapAllocation.HEADERSIZE);
            _freeSize += alloc->Header.Size + HeapAllocation.HEADERSIZE;

            var checksum = handle.Checksum;
            if (_allocator == Allocator.Stack)
                checksum = _version--;

#if LOG
            Console.WriteLine($"Free: {handle}");
            HeapAllocation.Free(alloc, checksum);
#endif
            handle.Address = null;
            handle.Flags = 0;

#if LOG
            Walk(_root);
#endif
        }

        private static ulong x = 123456789, y = 362436069, z = 521288629;

        private static ulong NextChecksum()
        {          //period 2^96-1
            ulong t;
            x ^= x << 16;
            x ^= x >> 5;
            x ^= x << 1;

            t = x;
            x = y;
            y = z;
            z = t ^ x ^ y;

            return z;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                Marshal.FreeHGlobal(_heapPtr);

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~HeapMemory()
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
