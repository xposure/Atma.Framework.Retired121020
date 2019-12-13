namespace Atma.Memory
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Microsoft.Extensions.Logging;

    //using Atma.Common;
    //using static Atma.Debug;

    [System.Diagnostics.DebuggerStepThrough]
    [StructLayout(LayoutKind.Sequential, Size = 32)]
    public unsafe struct HeapAllocation
    {
        public const int HeapSize = 32;
        public const uint MagicChecksum = 0x55aaa55a;

        public uint MagicSignature;         //4
        public uint Blocks;                 //4
        public HeapAllocation* Previous;   //4/8
        public HeapAllocation* Next;       //4/8
        public uint Flags;                  //4
        public uint Checksum;               //4

        public override string ToString() => $"{{ MSig: {MagicSignature:X8}, Blocks: {Blocks}, Prev: {new IntPtr(Previous)}, Next: {new IntPtr(Next)}, Flags: {Flags:X8}, CRC: {Checksum} }}";

        public HeapAllocation(int size)
        {
            Blocks = (((uint)(size + (HeapSize - 1))) >> 5) - 1;
            Flags = 0;
            Checksum = 0;
            MagicSignature = MagicChecksum;
            Previous = null;
            Next = null;
        }
        public static void ConsumeForward(HeapAllocation* root)
        {
            //System.Diagnostics.Debug.WriteLine($"  ConsumeForward {*root}");
            var ptr = root->Next;
            while (ptr != null)
            {
                //System.Diagnostics.Debug.WriteLine($"  Walked forward {*ptr}");
                Assert.EqualTo(ptr->MagicSignature, MagicChecksum);
                if (!ptr->IsFree)
                    break;

                root->Blocks++;
                root->Blocks += ptr->Blocks;
                var next = ptr->Next;

                *ptr = default;

                ptr = next;
                root->Next = ptr;
            }

            if (root->Next != null)
                root->Next->Previous = root;

            //Assert(ptr->MagicSignature == MagicChecksum);
        }

        public static void Split(HeapAllocation* ptr, uint blocks)
        {
            //var blocks = (size + (HeapSize - 1)) >> 5;
            Assert.GreatherThan(blocks, 0);//make sure we are taking at least one block
            Assert.EqualTo(ptr->MagicSignature, MagicChecksum);

            var remainingBlocks = ptr->Blocks - blocks;
            //Assert.LessThan(remainingBlocks, ptr->Blocks);
            if (remainingBlocks > 1)
            {
                var newBlock = &ptr[blocks + 1];
                newBlock->Blocks = remainingBlocks - 1;
                newBlock->Previous = ptr;
                newBlock->MagicSignature = MagicChecksum;

                if (ptr->Next == null)
                    ptr->Next = newBlock;
                else
                {
                    Assert.EqualTo(ptr->MagicSignature, MagicChecksum);
                    newBlock->Next = ptr->Next;
                    ptr->Next->Previous = newBlock;
                }
                remainingBlocks = 0;
            }
            ptr->Flags |= 1;
            ptr->Blocks = blocks + remainingBlocks;
        }

        public static HeapAllocation* FindFreeBackwards(HeapAllocation* ptr)
        {
            //System.Diagnostics.Debug.WriteLine($"  FindFreeBackwards {*ptr}");
            while (ptr->Previous != null)
            {
                Assert.EqualTo(ptr->MagicSignature, MagicChecksum);
                if (!ptr->Previous->IsFree)
                    break;

                ptr = ptr->Previous;
                //System.Diagnostics.Debug.WriteLine($"    Walked back {*ptr}");
            }

            Assert.EqualTo(ptr->MagicSignature, MagicChecksum);
            return ptr;
        }

        public static void Free(HeapAllocation* root)
        {
            Assert.EqualTo(root->IsFree, false);
            Assert.EqualTo(root->MagicSignature, MagicChecksum);
            root->Flags ^= 1;

            var ptr = FindFreeBackwards(root);
            ConsumeForward(ptr);

        }

        public static uint CountUsedBlocks(HeapAllocation* root, out int allocations)
        {
            allocations = 0;

            var ptr = root;
            var blocks = 0u;
            while (ptr != null)
            {
                blocks++;
                if (!ptr->IsFree)
                {
                    blocks += ptr->Blocks;
                    allocations++;
                }
                ptr = ptr->Next;
            }

            return blocks;
        }

        public static uint CountFreeBlocks(HeapAllocation* root)
        {
            var ptr = root;

            var blocks = 0u;
            while (ptr != null)
            {
                if (ptr->IsFree)
                    blocks += ptr->Blocks;
                ptr = ptr->Next;
            }
            return blocks;
        }

        public uint SizeInBytes => Blocks * HeapSize;

        public bool IsFree => (Flags & 1) == 0;
    }

    public class HeapAllocator : UnmanagedDispose, IAllocator
    {
        public unsafe class HeapPageAllocator : UnmanagedDispose, IAllocator
        {
            private struct HeapPagePointer
            {
                public readonly uint Id;
                public readonly IntPtr Address;
                public readonly int PageIndex;
                public readonly uint Version;

                public HeapPagePointer(uint id, IntPtr address, int pageIndex, uint version)
                {
                    Id = id;
                    Address = address;
                    PageIndex = pageIndex;
                    Version = version;
                }

                public override string ToString() => $"{{ Id: {Id:X8}, Address: {Address}, Page: {PageIndex:X8}, Version: {Version:X8} }}";
            }

            private ILogger _logger;
            private ILoggerFactory _logFactory;
            private IAllocator _allocator;
            private PagedObjectPool<HeapPagePointer> _allocations;// = new PagedObjectPool<HeapPagePointer>();
            private List<HeapPage> _pages = new List<HeapPage>();
            private uint _heapIndex;
            private uint _version;
            private int _desiredSizes;

            public int DesiredSizes => _desiredSizes;

            public HeapPageAllocator(ILoggerFactory logFactory, IAllocator allocator, int heapIndex)
            {
                _logFactory = logFactory;
                _logger = _logFactory.CreateLogger<HeapPageAllocator>();
                Assert.Range(heapIndex, 0, 16);
                _heapIndex = (uint)heapIndex;
                _allocations = new PagedObjectPool<HeapPagePointer>(allocator);
                _allocator = allocator;
                _desiredSizes = ((int)Math.Pow(2, _heapIndex) * 16384);
                _allocations.Take();
            }

            protected override void OnUnmanagedDispose()
            {
                _pages.DisposeAll();
                _allocations.Dispose();
            }

            public void Free(ref AllocationHandle handle)
            {
                ref var ptr = ref _allocations[handle.Id];
                //System.Diagnostics.Debug.WriteLine($"Free {{ Heap: {ptr}, Handle: {handle} }}");
                Assert.EqualTo(ptr.Address, handle.Address);
                Assert.EqualTo(ptr.Version & 0xfffffff, handle.Flags >> 4);

                _pages[ptr.PageIndex].Free(handle.Address);

                //pagedobjectpool will assert on bad version, no need to check
                _allocations.Return(handle.Id);
                handle = AllocationHandle.Null;
            }

            public AllocationHandle Take(int size)
            {
                var id = _allocations.Take();

                ref var heapPagePtr = ref _allocations[id];
                var flags = (++_version << 4) + _heapIndex;

                for (var i = 0; i < _pages.Count; i++)
                {
                    if (_pages[i].TryTake(out var ptr, (uint)size))
                    {
                        heapPagePtr = new HeapPagePointer(id, ptr, i, _version);
                        var handle = new AllocationHandle(ptr, id, flags);
                        //System.Diagnostics.Debug.WriteLine($"Alloc {{ Heap: {heapPagePtr}, Handle: {handle} }}");

                        return handle;
                    }
                }

                {
                    var page = new HeapPage(_allocator, _desiredSizes);
                    _pages.Add(page);

                    Contract.EqualTo(page.TryTake(out var ptr, (uint)size), true);

                    heapPagePtr = new HeapPagePointer(id, ptr, _pages.Count - 1, _version);
                    var handle = new AllocationHandle(ptr, id, flags);
                    //System.Diagnostics.Debug.WriteLine($"Alloc {{ Heap: {heapPagePtr}, Handle: {handle} }}");
                    return handle;
                }
            }

            public AllocationHandle Transfer(ref AllocationHandle handle)
            {
                ref var ptr = ref _allocations[handle.Id];
                Assert.EqualTo(ptr.Version >> 4, (handle.Flags >> 4));

                var heapIndex = handle.Flags & 0xf;
                var version = handle.Flags >> 4;
                var flags = (++_version << 4) + _heapIndex;
                var newHandle = new AllocationHandle(handle.Address, handle.Id, flags);

                ptr = new HeapPagePointer(handle.Id, handle.Address, ptr.PageIndex, version);

                handle = AllocationHandle.Null;
                return newHandle;
            }
        }

        public unsafe class HeapPage : UnmanagedDispose
        {
            private IAllocator _allocator;
            private AllocationHandle _handle;
            public readonly int Size;

            private int _largestFreeBlock = -1;

            //private bool _enableLogging;

            //private string[] _stackTrace;

            private HeapAllocation* _heap;

            public HeapPage(IAllocator allocator, int size)
            {
                _allocator = allocator;
                _handle = allocator.Take((int)size);
                Size = size;
                _heap = (HeapAllocation*)_handle.Address;
                *_heap = new HeapAllocation(size);

                // _enableLogging = enableLogging;
                // if (_enableLogging)
                //     _stackTrace = new string[_heap->Blocks];
            }

            public void Free(IntPtr handle)
            {
                _largestFreeBlock = -1;
                var ptr = (HeapAllocation*)handle;
                HeapAllocation.Free(ptr - 1);
            }

            public bool TryTake(out IntPtr handle, uint size)
            {
                var blocks = (size + (HeapAllocation.HeapSize - 1)) >> 5;
                Assert.GreatherThan(blocks, 0);

                if (_largestFreeBlock > -1 && blocks > _largestFreeBlock)
                {
                    handle = IntPtr.Zero;
                    return false;
                }

                //var id = 0u;
                var ptr = _heap;
                var largestFree = 0u;
                while (ptr != null)
                {
                    //System.Diagnostics.Debug.WriteLine($"Alloc {{ Heap: {*ptr} }}");
                    if (ptr->IsFree)
                    {
                        var freeBlocks = ptr->Blocks;
                        //TODO: we could remember this block and look for a better one if we wanted to
                        if (freeBlocks >= blocks)
                        {
                            HeapAllocation.Split(ptr, blocks);
                            handle = new IntPtr((void*)(ptr + 1));
                            _largestFreeBlock = -1;
                            return true;
                        }

                        if (freeBlocks > largestFree)
                            largestFree = freeBlocks;
                    }

                    //TODO: We should generate an ID for faster look up times
                    //id++;
                    //id += ptr->Blocks;

                    ptr = ptr->Next;
                }

                handle = IntPtr.Zero;
                _largestFreeBlock = (int)largestFree;
                return false;
            }

            protected override void OnUnmanagedDispose()
            {
                _allocator.Free(ref _handle);
            }
        }

        //going to cheat and use a linked list until performance becomes an issue
        private ILogger _logger;
        private ILoggerFactory _logFactory;
        public IAllocator _allocator;
        private HeapPageAllocator[] _pageAllocators = new HeapPageAllocator[16];

        // public HeapAllocator() : this(new DynamicAllocator())
        // {

        // }

        public HeapAllocator(ILoggerFactory logFactory, bool enableLogging = false)
        {
            _logFactory = logFactory;
            _logger = _logFactory.CreateLogger<HeapAllocator>();

            _allocator = new DynamicAllocator(_logFactory, enableLogging);
            for (var i = 0; i < _pageAllocators.Length; i++)
                _pageAllocators[i] = new HeapPageAllocator(_logFactory, _allocator, i);
        }

        public void Free(ref AllocationHandle handle)
        {
            System.Console.WriteLine($"FREE: {handle}");

            lock (_pageAllocators)
            {
                var heapIndex = handle.Flags & 0xf;
                _pageAllocators[heapIndex].Free(ref handle);
            }
        }

        public AllocationHandle Take(int size)
        {
            //var allocatorSize = size 
            lock (_pageAllocators)
            {
                for (var i = 0; i < _pageAllocators.Length - 1; i++)
                {
                    if (size < _pageAllocators[i].DesiredSizes)
                    {
                        var handle = _pageAllocators[i].Take(size);
                        System.Console.WriteLine($"TAKE: {handle}");
                        return handle;
                    }
                }

                var handle2 = _pageAllocators[_pageAllocators.Length - 1].Take(size);
                System.Console.WriteLine($"TAKE: {handle2}");
                return handle2;
            }
        }

        public AllocationHandle Transfer(ref AllocationHandle handle)
        {
            lock (_pageAllocators)
            {
                var heapIndex = handle.Flags & 0xf;
                return _pageAllocators[heapIndex].Transfer(ref handle);
            }
        }

        protected override void OnUnmanagedDispose()
        {
            _pageAllocators.DisposeAll();
            _allocator.Dispose();
        }
    }
}