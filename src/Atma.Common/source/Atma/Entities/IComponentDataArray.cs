namespace Atma.Entities
{
    public enum ComponentDataArrayLock
    {
        Read,
        Write
    }
    public interface IComponentDataArray
    {
        int ElementSize { get; }
        int Length { get; }

        void Lock(ComponentDataArrayLock lockType);
        void Unlock(ComponentDataArrayLock lockType);
    }
}