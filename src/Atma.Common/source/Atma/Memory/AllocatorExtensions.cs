namespace Atma.Memory
{
    public static class AllocatorExtensions
    {
        public static AllocationHandle Take<T>(this IAllocator it, int count, AllocatorBounds bounds = AllocatorBounds.Front)
            where T : unmanaged
        {
            var size = SizeOf<T>.Size;
            return it.Take<T>(size * count, bounds);
        }
    }
}