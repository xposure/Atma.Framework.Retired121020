namespace Atma.Memory
{
    using System;
    using System.Collections.Generic;

    using static Atma.Debug;

    public static class MemoryManager
    {
        public const int ALLOCATION_UNIT = 1024 * 1024;

        private static List<HeapMemory> _persistent = new List<HeapMemory>();
        private static List<HeapMemory> _temporary = new List<HeapMemory>();
        private static List<HeapMemory> _stack = new List<HeapMemory>();
        private static object _syncObject = new object();

        static MemoryManager()
        {
            _persistent.Add(new HeapMemory(ALLOCATION_UNIT, 0, Allocator.Persistent));
            _temporary.Add(new HeapMemory(ALLOCATION_UNIT, 0, Allocator.Temp));
            _stack.Add(new HeapMemory(ALLOCATION_UNIT, 0, Allocator.Stack));
        }

        internal static AllocationHandleOld Take(Allocator allocator, int sizeInBytes)
        {
            //AssertMainThread();
            lock (_syncObject)
            {
                Assert(allocator != Allocator.None);
                //var sizeInBytes = SizeOf<T>.Size * length;
                var list = GetList(allocator);

                if (allocator != Allocator.Stack)
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        var heap = list[i];
                        if (heap.TryTake(sizeInBytes, out var handle))
                            return handle;
                    }
                }
                else
                {
                    var heap = list[list.Count - 1];
                    if (heap.TryTake(sizeInBytes, out var handle))
                        return handle;
                }

                //we are adding +1 because most array resizers use current * 3 / 2
                //this will give us a chance that this resize will fit inside
                //the deallocated array
                var size = Math.Max(ALLOCATION_UNIT, sizeInBytes * 3 / 2 + 1);
                var newHeap = new HeapMemory(size, (uint)list.Count, allocator);
                list.Add(newHeap);
                var allocation = newHeap.Take(sizeInBytes);
                Console.WriteLine($"Allocated: {allocation.Allocator}, {sizeInBytes}");
                return allocation;
            }
        }

        internal static void Free(ref AllocationHandleOld handle)
        {
            //AssertMainThread();
            lock (_syncObject)
            {
                Assert(handle.Allocator != Allocator.None);

                var allocator = handle.Allocator;
                var list = GetList(allocator);

                if (allocator != Allocator.Stack)
                    list[(int)handle.HeapIndex].Free(ref handle);
                else
                {

                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        var heap = list[i];
                        if (heap.FreeSize == heap.Size)
                            continue;

                        heap.Free(ref handle);
                        return;
                    }

                    Assert(false);
                }
            }
        }

        internal static List<HeapMemory> GetList(Allocator allocator)
        {
            switch (allocator)
            {
                case Allocator.Persistent: return _persistent;
                case Allocator.Temp: return _temporary;
                case Allocator.Stack: return _stack;
                default:
                    throw new Exception($"Allocator {allocator} not supported.");
            }
        }
    }
}
