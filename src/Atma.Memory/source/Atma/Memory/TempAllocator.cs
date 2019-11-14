// using Microsoft.Extensions.Logging;

// namespace Atma.Memory
// {

//     public class TempAllocator : UnmanagedDispose, IAllocator
//     {
//         private HeapAllocator _heap;
//         private ILoggerFactory _logFactory;
//         private NativeList<AllocationHandle> _handles;

//         public TempAllocator(ILoggerFactory logFactory)
//         {
//             _heap = new HeapAllocator(logFactory);
//             _handles = new NativeList<AllocationHandle>(_heap, 1024);
//         }

//         public void FreeAll()
//         {

//         }

//         protected override void OnUnmanagedDispose()
//         {
//             _heap.Dispose();
//         }

//         public void Free(ref AllocationHandle handle)
//         {
//             throw new System.NotImplementedException();
//         }

//         public AllocationHandle Take(int size)
//         {
//             throw new System.NotImplementedException();
//         }

//         public AllocationHandle Transfer(ref AllocationHandle handle)
//         {
//             throw new System.NotImplementedException();
//         }
//     }
// }