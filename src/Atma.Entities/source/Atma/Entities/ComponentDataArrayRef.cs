namespace Atma.Entities
{
    using System;

    public readonly ref struct ReadComponentDataSpanRef<T>
        where T : unmanaged
    {

        private readonly Span<T> Span;
        internal readonly ComponentDataArray Array;
        public int Length => Span.Length;
        public ReadComponentDataSpanRef(ComponentDataArray array, int count)
        {
            Array = array;
            Span = array.GetSpan<T>(count);

            Array.Lock(ComponentDataArrayLock.Read);
        }

        //I wish I knew why the compiler doesn't enforce readonly ref
        //the value is still allowed to be modified
        //public  T this[int index] =>  Span[index];
        public readonly ref T this[int index] => ref Span[index];

        public void Dispose()
        {
            Array.Unlock(ComponentDataArrayLock.Read);
        }
    }

    public readonly ref struct WriteComponentDataSpanRef<T>
        where T : unmanaged
    {

        public readonly Span<T> Span;
        internal readonly ComponentDataArray _array;
        public int Length => Span.Length;

        public WriteComponentDataSpanRef(ComponentDataArray array, int count)
        {
            _array = array;
            Span = array.GetSpan<T>(count);

            _array.Lock(ComponentDataArrayLock.Write);
        }

        public ref T this[int index] => ref Span[index];

        public void Dispose()
        {
            _array.Unlock(ComponentDataArrayLock.Write);
        }
    }

    public readonly ref struct ReadComponentDataArrayRef
    {
        public readonly ComponentDataArray Array;

        public ReadComponentDataArrayRef(ComponentDataArray array)
        {
            Array = array;
            Array.Lock(ComponentDataArrayLock.Read);

        }

        public void Dispose()
        {
            Array.Unlock(ComponentDataArrayLock.Read);

        }
    }

    public readonly ref struct WriteComponentDataArrayRef
    {

        public readonly ComponentDataArray Array;

        public WriteComponentDataArrayRef(ComponentDataArray array)
        {
            Array = array;
            Array.Lock(ComponentDataArrayLock.Write);
        }

        public void Dispose()
        {
            Array.Unlock(ComponentDataArrayLock.Write);
        }
    }
}
