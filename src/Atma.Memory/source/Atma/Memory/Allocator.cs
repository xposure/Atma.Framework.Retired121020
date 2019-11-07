namespace Atma.Memory
{
    public enum Allocator
    {
        None,
        Stack,
        Temp,
        Persistent
    }

    public static class Allocators
    {
        public readonly static IAllocator Persistent = new StackAllocator(1024 * 1024 * 64);
    }
}
