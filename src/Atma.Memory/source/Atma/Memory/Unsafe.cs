namespace Atma.Memory
{
    using System;

    using static Atma.Debug;

    public unsafe static class Unsafe
    {
        public const int THRASH = 0;

        public static bool IsAligned16(uint value) => ((long)value % 16) == 0;

        public static bool IsAligned16(IntPtr value) => ((long)value % 16) == 0;

        public static bool IsAligned16(int value) => (value % 16) == 0;

        public static bool IsAligned16(void* rawPointer) => ((long)rawPointer % 16) == 0;

        public static uint Align16(uint value) => 16 * ((value + 15) / 16);

        public static int Align16(int value) => 16 * ((value + 15) / 16);

        public static long Align16(long value) => 16 * ((value + 15) / 16);

        public static IntPtr Align16(IntPtr value)
        {
            if (IntPtr.Size == 8)
                return new IntPtr(Align16((long)value));

            return new IntPtr(Align16((int)value));
        }

        public static HeapMemory Allocate(int size)
        {
            return null;
        }

        public static HeapMemory Allocate<T>(int length)
            where T : unmanaged
        {
            return Allocate(SizeOf<T>.Size * length);
        }

        public static void ClearAlign16(void* rawPointer, int sizeInBytes, int value = THRASH)
        {
            Assert(IsAligned16(rawPointer));
            Assert(IsAligned16(sizeInBytes));

            var begin = (int*)rawPointer;
            var end = begin + sizeInBytes / 4;
            while (begin < end)
                *begin++ = value;
        }

        public static void ClearAlign16(void* rawPointer, int sizeInBytes, uint value)
        {
            Assert(IsAligned16(rawPointer));
            Assert(IsAligned16(sizeInBytes));

            var begin = (uint*)rawPointer;
            var end = begin + sizeInBytes / 4;
            while (begin < end)
                *begin++ = value;
        }

        public static void CopyAlign16(void* src, void* dst, int sizeInBytes)
        {
            Assert(IsAligned16(src));
            Assert(IsAligned16(dst));
            Assert(IsAligned16(sizeInBytes));

            var beginSrc = (int*)src;
            var beginDst = (int*)dst;
            var endSrc = beginSrc + sizeInBytes / 4;
            while (beginSrc < endSrc)
                *beginDst++ = *beginSrc++;
        }


        //public static void Set(void* rawPointer, int sizeInBytes, int value = THRASH)
        //{
        //    var len = sizeInBytes;
        //    var unaligned = (long)rawPointer % 4;
        //    var byteptr = (byte*)rawPointer;
        //    if (unaligned > 0)
        //    {
        //        unaligned = 4 - unaligned;
        //        while (unaligned-- > 0)
        //        {
        //            *byteptr = (byte)(value >> 24);
        //            byteptr++;
        //            len--;
        //            //wrap thrash
        //            value = (value << 8) + (value >> 24);
        //        }
        //    }

        //    var ptr = (int*)byteptr;
        //    while (len >= 4)
        //    {
        //        *ptr = value;
        //        ptr++;
        //        len -= 4;
        //    }

        //    if (len > 0)
        //    {
        //        byteptr = (byte*)ptr;
        //        while (len-- > 0)
        //        {
        //            *byteptr = (byte)(value >> 24);
        //            byteptr++;
        //            //wrap thrash
        //            value = (value << 8) + (value >> 24);
        //        }
        //    }
        //}
    }
}
