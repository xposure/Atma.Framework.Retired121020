namespace Atma
{
    using System;
    using System.Collections.Generic;

    public static class DisposeExtensions
    {
        public static void DisposeAll<T>(this T[] it)
            where T : IDisposable
        {
            if (it != null)
            {
                for (var i = 0; i < it.Length; i++)
                {
                    it[i].Dispose();
                    it[i] = default;
                }
            }
        }

        public static void DisposeAll<T>(this List<T> it)
           where T : IDisposable
        {
            if (it != null)
            {
                for (var i = 0; i < it.Count; i++)
                {
                    it[i].Dispose();
                    it[i] = default;
                }
            }
        }
    }

    public class UnmanagedDispose : IDisposable
    {
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
#if DEBUG
        private readonly string _stackTrace = Environment.StackTrace;
#endif

        protected virtual void OnManagedDispose() { }
        protected virtual void OnUnmanagedDispose() { }

        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Assert.EqualTo(disposing, true);
                OnUnmanagedDispose();

                if (disposing)
                    OnManagedDispose();

                disposedValue = true;
            }
        }

        ~UnmanagedDispose()
        {
#if DEBUG
            throw new Exception("Object was not disposed.\nCreated At:" + _stackTrace);
#endif
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}