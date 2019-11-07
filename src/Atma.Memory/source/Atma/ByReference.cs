namespace Atma
{
    using System;

    public unsafe struct Read<T>
        where T : unmanaged
    {
        private T* _value;

        public Read(IntPtr value)
        {
            _value = (T*)value.ToPointer();
        }

        public Read(void* value)
        {
            _value = (T*)value;
        }

        public readonly ref T this[int index]
        {
            get => ref _value[index];
        }
    }

    public unsafe struct Write<T>
        where T : unmanaged
    {
        private T* _value;

        public Write(IntPtr value)
        {
            _value = (T*)value.ToPointer();
        }

        public Write(void* value)
        {
            _value = (T*)value;
        }

        public ref T this[int index]
        {
            get => ref _value[index];
        }
    }
}