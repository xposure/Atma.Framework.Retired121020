namespace Atma.Entities
{
    using Atma.Memory;
    using static Atma.Debug;

    using System;
    using System.Threading;

    public unsafe abstract class ComponentDataArray : IComponentDataArray
    {
        internal volatile int READERS = 0;
        internal volatile int WRITERS = 0;

        public abstract int Length { get; }
        public abstract int ElementSize { get; }
        internal void* _rawData;

        internal ComponentDataArray()
        {
        }

        internal abstract void Initialize(MemoryBlock memory, int length);

        internal abstract void SetComponentData(int entityIndex, void* componetnData);


        internal abstract void SetComponentData<K>(int entityIndex, in K componetnData)
            where K : unmanaged;

        internal abstract K GetComponentData<K>(int entityIndex)
            where K : unmanaged;

        internal abstract void DeleteIndex(int entityIndex, int count, bool clearToZero);

        internal abstract void CopyTo(int srcIndex, ComponentDataArray dst, int dstIndex);

        internal abstract Span<K> GetSpan<K>(int length)
            where K : unmanaged;

        public void Lock(ComponentDataArrayLock lockType)
        {
            if (lockType == ComponentDataArrayLock.Read)
                Interlocked.Increment(ref READERS);
            else
            {
                Assert(Interlocked.Increment(ref READERS) == 1);
                Assert(Interlocked.Increment(ref WRITERS) == 1);
            }
        }

        public void Unlock(ComponentDataArrayLock lockType)
        {
            if (lockType == ComponentDataArrayLock.Read)
                Interlocked.Decrement(ref READERS);
            else
            {
                Assert(Interlocked.Decrement(ref READERS) == 0);
                Assert(Interlocked.Decrement(ref WRITERS) == 0);
            }

        }
    }

    public unsafe sealed class ComponentDataArray<T> : ComponentDataArray
        where T : unmanaged
    {
        public T* _data;
        private int _length;

        public override int Length => _length;

        public override int ElementSize => SizeOf<T>.Size;

        public ComponentDataArray() { }

        public ComponentDataArray(T* data, int length)
        {
            _data = data;
            _rawData = (void*)_data;
            _length = length;
        }

        internal override void Initialize(MemoryBlock memory, int length)
        {
            _data = memory.Take<T>(length);
            _rawData = (void*)_data;
            _length = length;
        }

        internal override void SetComponentData(int entityIndex, void* componentData)
        {
            _data[entityIndex] = *(T*)componentData;
        }

        internal override void SetComponentData<K>(int entityIndex, in K componentData)
        {
            var ptr = (K*)_data;
            ptr[entityIndex] = componentData;
        }

        internal override K GetComponentData<K>(int entityIndex)
        {
            var ptr = (K*)_data;
            return ptr[entityIndex];
        }

        internal override void DeleteIndex(int entityIndex, int count, bool clearToZero)
        {
            _data[entityIndex] = _data[count - 1];
            if (clearToZero)
                _data[count - 1] = default;

        }

        internal override void CopyTo(int srcIndex, ComponentDataArray dst, int dstIndex)
        {
            var dstArray = (ComponentDataArray<T>)dst;

            var dstArr = (T*)dst._rawData;
            dstArr[dstIndex] = _data[srcIndex];
        }

        internal override Span<K> GetSpan<K>(int length)
        {
            return new Span<K>((void*)_data, length);
        }

        public Span<T> Span => new Span<T>(_rawData, _length);
    }
}
