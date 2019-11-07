namespace Atma
{
    using System;
    using System.Diagnostics;

    public static class Debug
    {
        [ThreadStatic]
        public static readonly bool IsMainThread = true;

        [Conditional("DEBUG")]
        public static void AssertMainThread()
        {
            Assert(IsMainThread);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition)
        {
            if (!condition) throw new Exception();
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new Exception(message);
        }


        [Conditional("DEBUG")]
        public static void Assert<T>(bool condition)
            where T : Exception, new()
        {
            if (!condition) throw new T();
        }

    }
}
