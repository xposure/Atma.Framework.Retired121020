namespace Atma.Entities
{
    using System;
    public interface IComponentDataArray2 : IDisposable
    {
        int ElementSize { get; }
        int Length { get; }

        Span<T> AsSpan<T>() where T : unmanaged;
        //ComponentDataArrayReadLock AsReadOnlySpan<T>(out ReadOnlySpan<T> span) where T : unmanaged;
        void Move(int src, int dst);
    }
}