namespace Atma.Memory
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Atma.Common;
    using static Atma.Debug;

    [StructLayout(LayoutKind.Sequential, Size = 32)]
    public unsafe struct HeapAllocation2
    {
        public const int HeapSize = 32;

        public uint MagicSignature;         //4
        public uint Blocks;                 //4
        public HeapAllocation2* Previous;   //4/8
        public HeapAllocation2* Next;       //4/8
        public uint Flags;                  //4
        public uint Checksum;               //4

        public HeapAllocation2(int size)
        {
            Blocks = ((uint)(size + (HeapSize - 1))) >> 5;
            Flags = 0;
            Checksum = 0;
            MagicSignature = 0x5aa555aa;
            Previous = null;
            Next = null;
        }
        public static void ConsumeForward(HeapAllocation2* root)
        {
            var ptr = root->Next;
            while (ptr != null)
            {
                if (!ptr->IsFree)
                    break;

                root->Blocks++;
                root->Blocks += ptr->Blocks;
                var next = ptr->Next;

#if DEBUG
                *ptr = default;
#endif

                ptr = next;
                root->Next = ptr;
            }

            if (root->Next != null)
                root->Next->Previous = root;
        }

        public static void Split(HeapAllocation2* ptr, uint blocks)
        {
            //var blocks = (size + (HeapSize - 1)) >> 5;
            Assert(blocks > 0); //make sure we are taking at least one block

            var remainingBlocks = ptr->Blocks - blocks - 1;
            if (remainingBlocks > 0)
            {
                var newBlock = &ptr[blocks + 1];
                newBlock->Blocks = remainingBlocks;
                newBlock->Previous = ptr;

                if (ptr->Next == null)
                    ptr->Next = newBlock;
                else
                {
                    newBlock->Next = ptr->Next;
                    ptr->Next->Previous = newBlock;
                }
            }
            ptr->Flags |= 1;
            ptr->Blocks = blocks;
        }

        public static HeapAllocation2* FindFreeBackwards(HeapAllocation2* ptr)
        {
            while (ptr->Previous != null)
            {
                if (!ptr->Previous->IsFree)
                    break;

                ptr = ptr->Previous;
            }

            return ptr;
        }

        public static void Free(HeapAllocation2* root)
        {
            Assert(!root->IsFree);
            root->Flags ^= 1;

            var ptr = FindFreeBackwards(root);
            ConsumeForward(ptr);

        }

        public static uint CountUsedBlocks(HeapAllocation2* root, out int allocations)
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

        public static uint CountFreeBlocks(HeapAllocation2* root)
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
            }

            private IAllocator _allocator;
            private PagedObjectPool<HeapPagePointer> _allocations = new PagedObjectPool<HeapPagePointer>();
            private List<HeapPage> _pages = new List<HeapPage>();
            private uint _heapIndex;
            private uint _version;
            private int _desiredSizes;

            public int DesiredSizes => _desiredSizes;

            public HeapPageAllocator(IAllocator allocator, int heapIndex)
            {
                Assert(heapIndex >= 0 && heapIndex < 16);

                _allocator = allocator;
                _desiredSizes = ((int)Math.Pow(2, _heapIndex) * 16384);
                _heapIndex = (uint)heapIndex;
            }

            protected override void OnUnmanagedDispose()
            {
                _pages.DisposeAll();
            }

            public void Free(ref AllocationHandle handle)
            {
                ref var ptr = ref _allocations[handle.Id];

                Assert((ptr.Version >> 4) == (handle.Flags >> 4));

                _pages[ptr.PageIndex].Free(handle.Address);

                //pagedobjectpool will assert on bad version, no need to check
                _allocations.Return(handle.Id);
                handle = AllocationHandle.Null;
            }

            public AllocationHandle Take(int size)
            {
                var id = _allocations.Take();
                ref var heapPagePtr = ref _allocations[id];

                for (var i = 0; i < _pages.Count; i++)
                {
                    if (_pages[i].TryTake(out var ptr, (uint)size))
                    {
                        heapPagePtr = new HeapPagePointer(id, ptr, i, _version++);
                        return new AllocationHandle(ptr, id, _heapIndex);
                    }
                }

                {
                    var page = new HeapPage(_allocator, _desiredSizes);
                    _pages.Add(page);

                    Contract.EqualTo(page.TryTake(out var ptr, (uint)size), true);

                    heapPagePtr = new HeapPagePointer(id, ptr, 0, _version++);
                    return new AllocationHandle(ptr, id, _heapIndex);
                }
            }

            public AllocationHandle Transfer(ref AllocationHandle handle)
            {
                ref var ptr = ref _allocations[handle.Id];
                Assert((ptr.Version >> 4) == (handle.Flags >> 4));

                var heapIndex = handle.Flags & 0xf;
                var version = handle.Flags >> 4;
                version++;

                var flags = ((version & 0xffffffff) >> 4) + heapIndex;
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

            private HeapAllocation2* _heap;

            public HeapPage(IAllocator allocator, int size)
            {
                _allocator = allocator;
                _handle = allocator.Take((int)size);
                Size = size;

                *(_heap = (HeapAllocation2*)_handle.Address) = new HeapAllocation2(size);
            }

            public void Free(IntPtr handle)
            {
                _largestFreeBlock = -1;
                var ptr = (HeapAllocation2*)handle;
                HeapAllocation2.Free(ptr);
            }

            public bool TryTake(out IntPtr handle, uint size)
            {
                var blocks = (size + (HeapAllocation2.HeapSize - 1)) >> 5;
                Assert(blocks > 0);

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
                    if (ptr->IsFree)
                    {
                        var freeBlocks = ptr->Blocks;
                        //TODO: we could remember this block and look for a better one if we wanted to
                        if (freeBlocks >= blocks)
                        {
                            HeapAllocation2.Split(ptr, blocks);
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
        public IAllocator _allocator;
        private HeapPageAllocator[] _pageAllocators = new HeapPageAllocator[16];

        public HeapAllocator(IAllocator allocator)
        {
            _allocator = allocator;
            for (var i = 0; i < _pageAllocators.Length; i++)
                _pageAllocators[i] = new HeapPageAllocator(_allocator, i);
        }

        public void Free(ref AllocationHandle handle)
        {
            var heapIndex = handle.Flags & 0xf;
            _pageAllocators[heapIndex].Free(ref handle);
        }

        public AllocationHandle Take(int size)
        {
            //var allocatorSize = size 
            for (var i = 0; i < _pageAllocators.Length - 1; i++)
                if (size < _pageAllocators[i].DesiredSizes)
                    return _pageAllocators[i].Take(size);

            return _pageAllocators[_pageAllocators.Length - 1].Take(size);
        }

        public AllocationHandle Transfer(ref AllocationHandle handle)
        {
            var heapIndex = handle.Flags & 0xf;
            return _pageAllocators[heapIndex].Transfer(ref handle);
        }

        protected override void OnUnmanagedDispose()
        {
            _pageAllocators.DisposeAll();
        }
    }
}