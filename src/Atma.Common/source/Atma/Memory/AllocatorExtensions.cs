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
    }
}