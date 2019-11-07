namespace Atma
{
    using System;

    public class UnmanagedDispose : IDisposable
    {
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void OnManagedDispose() { }
        protected virtual void OnUnmanagedDispose() { }

        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                OnUnmanagedDispose();

                if (disposing)
                    OnManagedDispose();

                disposedValue = true;
            }
        }

        ~UnmanagedDispose()
        {
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