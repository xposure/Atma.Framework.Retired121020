namespace Atma.Events
{
    using System;
    using System.Collections.Generic;
    using Atma.Common;

    public interface IEventManager
    {

        void Observe(string name, EventCallback callback);
    }

    public delegate void EventCallback();
    public delegate void EventCallback<T>(T dt);

    internal abstract class Observers
    {
        public readonly int ID;

        protected Observers(int id) => ID = id;


        internal abstract void Remove(int id);


    }

    public class Observer : UnmanagedDispose
    {
        internal static int NextObserverID = 0;
        public readonly int ID;

        internal Observers _observers;

        internal Observer(Observers observers, int id)
        {
            ID = id;
            _observers = observers;
        }

        protected override void OnManagedDispose() => _observers.Remove(ID);

    }

    internal class ObserverRef<T> : Observer
    {
        public readonly EventCallback<T> EventCallback;
        public ObserverRef(Observers observers, int id, EventCallback<T> callback) : base(observers, id) => EventCallback = callback;
        public void Fire(T t) => EventCallback(t);
    }

    internal class ObserverList<T> : Observers
    {
        private LookupList<ObserverRef<T>> _observers = new LookupList<ObserverRef<T>>();

        public ObserverList(int id) : base(id) { }

        public Observer Add(EventCallback<T> callback) => _observers.Add(++Observer.NextObserverID, new ObserverRef<T>(this, Observer.NextObserverID, callback));

        public void Fire(T t)
        {
            foreach (var it in _observers.AllObjects)
                it.Fire(t);
        }

        internal override void Remove(int id) => _observers.Remove(id);
    }

    public unsafe class EventManager : UnmanagedDispose
    {
        private LookupList<Observers> _handlers = new LookupList<Observers>();

        public void Fire<T>(string name, T t)
        {
            Span<int> hashCodes = stackalloc[] { name.GetHashCode(), typeof(T).GetHashCode() };
            var hashTypeId = HashCode.Hash(hashCodes);

            if (_handlers.TryGetValue(hashTypeId, out var handler))
            {
                var observer = (ObserverList<T>)handler;
                observer.Fire(t);
            }
        }

        public Observer Observe<T>(string name, EventCallback<T> callback)
        {
            Span<int> hashCodes = stackalloc[] { name.GetHashCode(), typeof(T).GetHashCode() };
            var hashTypeId = HashCode.Hash(hashCodes);
            if (!_handlers.TryGetValue(hashTypeId, out var observerList))
            {
                var observers = new ObserverList<T>(hashTypeId);
                _handlers.Add(hashTypeId, observers);
                return observers.Add(callback);
            }
            else
            {
                var observers = (ObserverList<T>)observerList;
                return observers.Add(callback);
            }
        }
    }
}