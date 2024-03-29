namespace Atma.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public partial interface IEventManager
    {
    }

    public partial class EventManager : UnmanagedDispose, IEventManager
    {
        internal const int GenericCount = 10;
        private Dictionary<int, EventObservableBase> _observables = new Dictionary<int, EventObservableBase>();

    }

    public class EventObservableBase : UnmanagedDispose
    {
        public readonly string Name;
        internal EventObservableBase(string name)
        {
            Name = name;
        }
    }

    internal class EventObserverBase
    {
        protected IDisposable _unsubscriber;

        public void OnCompleted() => _unsubscriber.Dispose();

        public void OnError(Exception error) { }
    }

    public interface IObservable { IDisposable Subscribe(IObserver observer); }
    public interface IObserver { void Fire(); }
    internal class EventObserver : EventObserverBase, IObserver
    {
        private Action _callback;
        internal EventObserver(Action callback) => _callback = callback;
        public void Fire() => _callback?.Invoke();
    }
    public partial interface IEventManager { EventObservable GetObservable(string name); }
    public partial class EventManager : IEventManager
    {
        public EventObservable GetObservable(string name)
        {
            Span<int> hashCodes = stackalloc[] { name.GetHashCode() };
            var hashTypeId = Atma.HashCode.Hash(hashCodes);
            if (!_observables.TryGetValue(hashTypeId, out var it))
            {
                it = new EventObservable(name);
                _observables.Add(hashTypeId, it);
            }
            return (EventObservable)it;
        }
    }
    public static partial class EventManagerExtensions
    {
        public static IDisposable Subscribe(this IEventManager events, string name, Action callback) => events.GetObservable(name).Subscribe(new EventObserver(callback));
        public static void Fire(this IEventManager events, string name) => events.GetObservable(name).Fire();
    }

    public sealed class EventObservable : EventObservableBase, IObservable
    {
        private List<IObserver> _observers = new List<IObserver>();
        public EventObservable(string name) : base(name) { }
        public IDisposable Subscribe(IObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }
        public void Fire()
        {
            foreach (var it in _observers)
                it.Fire();
        }
        private class Unsubscriber : IDisposable
        {
            private List<IObserver> _observers;
            private IObserver _observer;
            public Unsubscriber(List<IObserver> observers, IObserver observer)
            {
                this._observers = observers;
                this._observer = observer;
            }
            public void Dispose() { if (_observer != null && _observers.Contains(_observer)) _observers.Remove(_observer); }
        }
    }
}