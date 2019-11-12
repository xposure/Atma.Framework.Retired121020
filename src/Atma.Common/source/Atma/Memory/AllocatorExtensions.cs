using Microsoft.Extensions.Logging;

namespace Atma.Memory
{
    public static class AllocatorExtensions
    {
        public static AllocationHandle Take<T>(this IAllocator it, int count)
            where T : unmanaged
        {

            var size = SizeOf<T>.Size;
            return it.Take(size * count);
        }

        public static DisposableAllocHandle TakeScoped<T>(this IAllocator it, int count, ILoggerFactory logFactory = null)
            where T : unmanaged
            => new DisposableAllocHandle(logFactory, it, it.Take<T>(count));

        public static DisposableAllocHandle TakeScoped(this IAllocator it, int bytes, ILoggerFactory logFactory = null)
           => new DisposableAllocHandle(logFactory, it, it.Take<byte>(bytes));
    }
}