namespace Atma
{
    using System;
    using System.Collections.Generic;
    using Atma.Common;

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

        public static void DisposeAll<T>(this LookupList<T> it)
           where T : IDisposable
        {
            if (it != null)
            {
                foreach (var item in it.AllObjects)
                    item.Dispose();

                it.Clear();
            }
        }
    }

    public class UnmanagedDispose : IDisposable
    {
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private List<IDisposable> _trackedDisposables;

#if DEBUG
        private readonly string _stackTrace = Environment.StackTrace;
#endif
        protected UnmanagedDispose()
        {
            //System.Console.WriteLine($"CREATED: {this.GetType().Name} [{this.GetHashCode()}] -> {this.ToString()}");
        }
        protected virtual void OnManagedDispose() { }
        protected virtual void OnUnmanagedDispose() { }

        protected void Dispose(bool disposing)
        {
            //System.Console.WriteLine($"DISPOSING: {this.GetType().Name} [{this.GetHashCode()}] -> {this.ToString()} ");
            if (!disposedValue)
            {
                Assert.EqualTo(disposing, true);
                OnUnmanagedDispose();

                if (disposing)
                {
                    if (_trackedDisposables != null)
                    {
                        for (var i = 0; i < _trackedDisposables.Count; i++)
                            _trackedDisposables[i].Dispose();

                        _trackedDisposables.Clear();
                        _trackedDisposables = null;
                    }

                    OnManagedDispose();
                }

                disposedValue = true;
            }
            //System.Console.WriteLine($"DISPOSED: {this.GetType().Name} [{this.GetHashCode()}] -> {this.ToString()} ");
        }

        protected void Track(IDisposable disposable)
        {
            if (_trackedDisposables == null)
                _trackedDisposables = new List<IDisposable>();

            _trackedDisposables.Add(disposable);
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