using System;
using System.Collections.Generic;
using Atma;
using Atma.Entities;
using Atma.Memory;
namespace Atma.Events
{
    public interface IObservable<out T0> { IDisposable Subscribe(IObserver<T0> observer); }
    public interface IObserver<in T0> { void Fire(T0 t0); }
    internal class EventObserver<T0> : EventObserverBase, IObserver<T0>
    {
        private Action<T0> _callback;
        internal EventObserver(Action<T0> callback) => _callback = callback;
        public void Fire(T0 t0) => _callback?.Invoke(t0);
    }
    public partial interface IEventManager { EventObservable<T0> GetObservable<T0>(string name); }
    public partial class EventManager : IEventManager
    {
        public EventObservable<T0> GetObservable<T0>(string name)
        {
            Span<int> hashCodes = stackalloc[] { name.GetHashCode(), typeof(T0).GetHashCode() };
            var hashTypeId = HashCode.Hash(hashCodes);
            if (!_observables.TryGetValue(hashTypeId, out var it))
            {
                it = new EventObservable<T0>(name);
                _observables.Add(hashTypeId, it);
            }
            return (EventObservable<T0>)it;
        }
    }
    public static partial class EventManagerExtensions
    {
        public static IDisposable Subscribe<T0>(this IEventManager events, string name, Action<T0> callback) => events.GetObservable<T0>(name).Subscribe(new EventObserver<T0>(callback));
        public static void Fire<T0>(this IEventManager events, string name, T0 t0) => events.GetObservable<T0>(name).Fire(t0);
    }
    public sealed class EventObservable<T0> : EventObservableBase, IObservable<T0>
    {
        private List<IObserver<T0>> _observers = new List<IObserver<T0>>();
        public EventObservable(string name) : base(name) { }
        public IDisposable Subscribe(IObserver<T0> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }
        public void Fire(T0 t0)
        {
            foreach (var it in _observers)
                it.Fire(t0);
        }
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<T0>> _observers;
            private IObserver<T0> _observer;
            public Unsubscriber(List<IObserver<T0>> observers, IObserver<T0> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }
            public void Dispose() { if (_observer != null && _observers.Contains(_observer)) _observers.Remove(_observer); }
        }
    }
    public interface IObservable<out T0, out T1> { IDisposable Subscribe(IObserver<T0, T1> observer); }
    public interface IObserver<in T0, in T1> { void Fire(T0 t0, T1 t1); }
    internal class EventObserver<T0, T1> : EventObserverBase, IObserver<T0, T1>
    {
        private Action<T0, T1> _callback;
        internal EventObserver(Action<T0, T1> callback) => _callback = callback;
        public void Fire(T0 t0, T1 t1) => _callback?.Invoke(t0, t1);
    }
    public partial interface IEventManager { EventObservable<T0, T1> GetObservable<T0, T1>(string name); }
    public partial class EventManager : IEventManager
    {
        public EventObservable<T0, T1> GetObservable<T0, T1>(string name)
        {
            Span<int> hashCodes = stackalloc[] { name.GetHashCode(), typeof(T0).GetHashCode(), typeof(T1).GetHashCode() };
            var hashTypeId = HashCode.Hash(hashCodes);
            if (!_observables.TryGetValue(hashTypeId, out var it))
            {
                it = new EventObservable<T0, T1>(name);
                _observables.Add(hashTypeId, it);
            }
            return (EventObservable<T0, T1>)it;
        }
    }
    public static partial class EventManagerExtensions
    {
        public static IDisposable Subscribe<T0, T1>(this IEventManager events, string name, Action<T0, T1> callback) => events.GetObservable<T0, T1>(name).Subscribe(new EventObserver<T0, T1>(callback));
        public static void Fire<T0, T1>(this IEventManager events, string name, T0 t0, T1 t1) => events.GetObservable<T0, T1>(name).Fire(t0, t1);
    }
    public sealed class EventObservable<T0, T1> : EventObservableBase, IObservable<T0, T1>
    {
        private List<IObserver<T0, T1>> _observers = new List<IObserver<T0, T1>>();
        public EventObservable(string name) : base(name) { }
        public IDisposable Subscribe(IObserver<T0, T1> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }
        public void Fire(T0 t0, T1 t1)
        {
            foreach (var it in _observers)
                it.Fire(t0, t1);
        }
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<T0, T1>> _observers;
            private IObserver<T0, T1> _observer;
            public Unsubscriber(List<IObserver<T0, T1>> observers, IObserver<T0, T1> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }
            public void Dispose() { if (_observer != null && _observers.Contains(_observer)) _observers.Remove(_observer); }
        }
    }
    public interface IObservable<out T0, out T1, out T2> { IDisposable Subscribe(IObserver<T0, T1, T2> observer); }
    public interface IObserver<in T0, in T1, in T2> { void Fire(T0 t0, T1 t1, T2 t2); }
    internal class EventObserver<T0, T1, T2> : EventObserverBase, IObserver<T0, T1, T2>
    {
        private Action<T0, T1, T2> _callback;
        internal EventObserver(Action<T0, T1, T2> callback) => _callback = callback;
        public void Fire(T0 t0, T1 t1, T2 t2) => _callback?.Invoke(t0, t1, t2);
    }
    public partial interface IEventManager { EventObservable<T0, T1, T2> GetObservable<T0, T1, T2>(string name); }
    public partial class EventManager : IEventManager
    {
        public EventObservable<T0, T1, T2> GetObservable<T0, T1, T2>(string name)
        {
            Span<int> hashCodes = stackalloc[] { name.GetHashCode(), typeof(T0).GetHashCode(), typeof(T1).GetHashCode(), typeof(T2).GetHashCode() };
            var hashTypeId = HashCode.Hash(hashCodes);
            if (!_observables.TryGetValue(hashTypeId, out var it))
            {
                it = new EventObservable<T0, T1, T2>(name);
                _observables.Add(hashTypeId, it);
            }
            return (EventObservable<T0, T1, T2>)it;
        }
    }
    public static partial class EventManagerExtensions
    {
        public static IDisposable Subscribe<T0, T1, T2>(this IEventManager events, string name, Action<T0, T1, T2> callback) => events.GetObservable<T0, T1, T2>(name).Subscribe(new EventObserver<T0, T1, T2>(callback));
        public static void Fire<T0, T1, T2>(this IEventManager events, string name, T0 t0, T1 t1, T2 t2) => events.GetObservable<T0, T1, T2>(name).Fire(t0, t1, t2);
    }
    public sealed class EventObservable<T0, T1, T2> : EventObservableBase, IObservable<T0, T1, T2>
    {
        private List<IObserver<T0, T1, T2>> _observers = new List<IObserver<T0, T1, T2>>();
        public EventObservable(string name) : base(name) { }
        public IDisposable Subscribe(IObserver<T0, T1, T2> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }
        public void Fire(T0 t0, T1 t1, T2 t2)
        {
            foreach (var it in _observers)
                it.Fire(t0, t1, t2);
        }
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<T0, T1, T2>> _observers;
            private IObserver<T0, T1, T2> _observer;
            public Unsubscriber(List<IObserver<T0, T1, T2>> observers, IObserver<T0, T1, T2> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }
            public void Dispose() { if (_observer != null && _observers.Contains(_observer)) _observers.Remove(_observer); }
        }
    }
    public interface IObservable<out T0, out T1, out T2, out T3> { IDisposable Subscribe(IObserver<T0, T1, T2, T3> observer); }
    public interface IObserver<in T0, in T1, in T2, in T3> { void Fire(T0 t0, T1 t1, T2 t2, T3 t3); }
    internal class EventObserver<T0, T1, T2, T3> : EventObserverBase, IObserver<T0, T1, T2, T3>
    {
        private Action<T0, T1, T2, T3> _callback;
        internal EventObserver(Action<T0, T1, T2, T3> callback) => _callback = callback;
        public void Fire(T0 t0, T1 t1, T2 t2, T3 t3) => _callback?.Invoke(t0, t1, t2, t3);
    }
    public partial interface IEventManager { EventObservable<T0, T1, T2, T3> GetObservable<T0, T1, T2, T3>(string name); }
    public partial class EventManager : IEventManager
    {
        public EventObservable<T0, T1, T2, T3> GetObservable<T0, T1, T2, T3>(string name)
        {
            Span<int> hashCodes = stackalloc[] { name.GetHashCode(), typeof(T0).GetHashCode(), typeof(T1).GetHashCode(), typeof(T2).GetHashCode(), typeof(T3).GetHashCode() };
            var hashTypeId = HashCode.Hash(hashCodes);
            if (!_observables.TryGetValue(hashTypeId, out var it))
            {
                it = new EventObservable<T0, T1, T2, T3>(name);
                _observables.Add(hashTypeId, it);
            }
            return (EventObservable<T0, T1, T2, T3>)it;
        }
    }
    public static partial class EventManagerExtensions
    {
        public static IDisposable Subscribe<T0, T1, T2, T3>(this IEventManager events, string name, Action<T0, T1, T2, T3> callback) => events.GetObservable<T0, T1, T2, T3>(name).Subscribe(new EventObserver<T0, T1, T2, T3>(callback));
        public static void Fire<T0, T1, T2, T3>(this IEventManager events, string name, T0 t0, T1 t1, T2 t2, T3 t3) => events.GetObservable<T0, T1, T2, T3>(name).Fire(t0, t1, t2, t3);
    }
    public sealed class EventObservable<T0, T1, T2, T3> : EventObservableBase, IObservable<T0, T1, T2, T3>
    {
        private List<IObserver<T0, T1, T2, T3>> _observers = new List<IObserver<T0, T1, T2, T3>>();
        public EventObservable(string name) : base(name) { }
        public IDisposable Subscribe(IObserver<T0, T1, T2, T3> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }
        public void Fire(T0 t0, T1 t1, T2 t2, T3 t3)
        {
            foreach (var it in _observers)
                it.Fire(t0, t1, t2, t3);
        }
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<T0, T1, T2, T3>> _observers;
            private IObserver<T0, T1, T2, T3> _observer;
            public Unsubscriber(List<IObserver<T0, T1, T2, T3>> observers, IObserver<T0, T1, T2, T3> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }
            public void Dispose() { if (_observer != null && _observers.Contains(_observer)) _observers.Remove(_observer); }
        }
    }
    public interface IObservable<out T0, out T1, out T2, out T3, out T4> { IDisposable Subscribe(IObserver<T0, T1, T2, T3, T4> observer); }
    public interface IObserver<in T0, in T1, in T2, in T3, in T4> { void Fire(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4); }
    internal class EventObserver<T0, T1, T2, T3, T4> : EventObserverBase, IObserver<T0, T1, T2, T3, T4>
    {
        private Action<T0, T1, T2, T3, T4> _callback;
        internal EventObserver(Action<T0, T1, T2, T3, T4> callback) => _callback = callback;
        public void Fire(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4) => _callback?.Invoke(t0, t1, t2, t3, t4);
    }
    public partial interface IEventManager { EventObservable<T0, T1, T2, T3, T4> GetObservable<T0, T1, T2, T3, T4>(string name); }
    public partial class EventManager : IEventManager
    {
        public EventObservable<T0, T1, T2, T3, T4> GetObservable<T0, T1, T2, T3, T4>(string name)
        {
            Span<int> hashCodes = stackalloc[] { name.GetHashCode(), typeof(T0).GetHashCode(), typeof(T1).GetHashCode(), typeof(T2).GetHashCode(), typeof(T3).GetHashCode(), typeof(T4).GetHashCode() };
            var hashTypeId = HashCode.Hash(hashCodes);
            if (!_observables.TryGetValue(hashTypeId, out var it))
            {
                it = new EventObservable<T0, T1, T2, T3, T4>(name);
                _observables.Add(hashTypeId, it);
            }
            return (EventObservable<T0, T1, T2, T3, T4>)it;
        }
    }
    public static partial class EventManagerExtensions
    {
        public static IDisposable Subscribe<T0, T1, T2, T3, T4>(this IEventManager events, string name, Action<T0, T1, T2, T3, T4> callback) => events.GetObservable<T0, T1, T2, T3, T4>(name).Subscribe(new EventObserver<T0, T1, T2, T3, T4>(callback));
        public static void Fire<T0, T1, T2, T3, T4>(this IEventManager events, string name, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4) => events.GetObservable<T0, T1, T2, T3, T4>(name).Fire(t0, t1, t2, t3, t4);
    }
    public sealed class EventObservable<T0, T1, T2, T3, T4> : EventObservableBase, IObservable<T0, T1, T2, T3, T4>
    {
        private List<IObserver<T0, T1, T2, T3, T4>> _observers = new List<IObserver<T0, T1, T2, T3, T4>>();
        public EventObservable(string name) : base(name) { }
        public IDisposable Subscribe(IObserver<T0, T1, T2, T3, T4> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }
        public void Fire(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            foreach (var it in _observers)
                it.Fire(t0, t1, t2, t3, t4);
        }
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<T0, T1, T2, T3, T4>> _observers;
            private IObserver<T0, T1, T2, T3, T4> _observer;
            public Unsubscriber(List<IObserver<T0, T1, T2, T3, T4>> observers, IObserver<T0, T1, T2, T3, T4> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }
            public void Dispose() { if (_observer != null && _observers.Contains(_observer)) _observers.Remove(_observer); }
        }
    }
    public interface IObservable<out T0, out T1, out T2, out T3, out T4, out T5> { IDisposable Subscribe(IObserver<T0, T1, T2, T3, T4, T5> observer); }
    public interface IObserver<in T0, in T1, in T2, in T3, in T4, in T5> { void Fire(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5); }
    internal class EventObserver<T0, T1, T2, T3, T4, T5> : EventObserverBase, IObserver<T0, T1, T2, T3, T4, T5>
    {
        private Action<T0, T1, T2, T3, T4, T5> _callback;
        internal EventObserver(Action<T0, T1, T2, T3, T4, T5> callback) => _callback = callback;
        public void Fire(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) => _callback?.Invoke(t0, t1, t2, t3, t4, t5);
    }
    public partial interface IEventManager { EventObservable<T0, T1, T2, T3, T4, T5> GetObservable<T0, T1, T2, T3, T4, T5>(string name); }
    public partial class EventManager : IEventManager
    {
        public EventObservable<T0, T1, T2, T3, T4, T5> GetObservable<T0, T1, T2, T3, T4, T5>(string name)
        {
            Span<int> hashCodes = stackalloc[] { name.GetHashCode(), typeof(T0).GetHashCode(), typeof(T1).GetHashCode(), typeof(T2).GetHashCode(), typeof(T3).GetHashCode(), typeof(T4).GetHashCode(), typeof(T5).GetHashCode() };
            var hashTypeId = HashCode.Hash(hashCodes);
            if (!_observables.TryGetValue(hashTypeId, out var it))
            {
                it = new EventObservable<T0, T1, T2, T3, T4, T5>(name);
                _observables.Add(hashTypeId, it);
            }
            return (EventObservable<T0, T1, T2, T3, T4, T5>)it;
        }
    }
    public static partial class EventManagerExtensions
    {
        public static IDisposable Subscribe<T0, T1, T2, T3, T4, T5>(this IEventManager events, string name, Action<T0, T1, T2, T3, T4, T5> callback) => events.GetObservable<T0, T1, T2, T3, T4, T5>(name).Subscribe(new EventObserver<T0, T1, T2, T3, T4, T5>(callback));
        public static void Fire<T0, T1, T2, T3, T4, T5>(this IEventManager events, string name, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) => events.GetObservable<T0, T1, T2, T3, T4, T5>(name).Fire(t0, t1, t2, t3, t4, t5);
    }
    public sealed class EventObservable<T0, T1, T2, T3, T4, T5> : EventObservableBase, IObservable<T0, T1, T2, T3, T4, T5>
    {
        private List<IObserver<T0, T1, T2, T3, T4, T5>> _observers = new List<IObserver<T0, T1, T2, T3, T4, T5>>();
        public EventObservable(string name) : base(name) { }
        public IDisposable Subscribe(IObserver<T0, T1, T2, T3, T4, T5> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }
        public void Fire(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        {
            foreach (var it in _observers)
                it.Fire(t0, t1, t2, t3, t4, t5);
        }
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<T0, T1, T2, T3, T4, T5>> _observers;
            private IObserver<T0, T1, T2, T3, T4, T5> _observer;
            public Unsubscriber(List<IObserver<T0, T1, T2, T3, T4, T5>> observers, IObserver<T0, T1, T2, T3, T4, T5> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }
            public void Dispose() { if (_observer != null && _observers.Contains(_observer)) _observers.Remove(_observer); }
        }
    }
    public interface IObservable<out T0, out T1, out T2, out T3, out T4, out T5, out T6> { IDisposable Subscribe(IObserver<T0, T1, T2, T3, T4, T5, T6> observer); }
    public interface IObserver<in T0, in T1, in T2, in T3, in T4, in T5, in T6> { void Fire(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6); }
    internal class EventObserver<T0, T1, T2, T3, T4, T5, T6> : EventObserverBase, IObserver<T0, T1, T2, T3, T4, T5, T6>
    {
        private Action<T0, T1, T2, T3, T4, T5, T6> _callback;
        internal EventObserver(Action<T0, T1, T2, T3, T4, T5, T6> callback) => _callback = callback;
        public void Fire(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) => _callback?.Invoke(t0, t1, t2, t3, t4, t5, t6);
    }
    public partial interface IEventManager { EventObservable<T0, T1, T2, T3, T4, T5, T6> GetObservable<T0, T1, T2, T3, T4, T5, T6>(string name); }
    public partial class EventManager : IEventManager
    {
        public EventObservable<T0, T1, T2, T3, T4, T5, T6> GetObservable<T0, T1, T2, T3, T4, T5, T6>(string name)
        {
            Span<int> hashCodes = stackalloc[] { name.GetHashCode(), typeof(T0).GetHashCode(), typeof(T1).GetHashCode(), typeof(T2).GetHashCode(), typeof(T3).GetHashCode(), typeof(T4).GetHashCode(), typeof(T5).GetHashCode(), typeof(T6).GetHashCode() };
            var hashTypeId = HashCode.Hash(hashCodes);
            if (!_observables.TryGetValue(hashTypeId, out var it))
            {
                it = new EventObservable<T0, T1, T2, T3, T4, T5, T6>(name);
                _observables.Add(hashTypeId, it);
            }
            return (EventObservable<T0, T1, T2, T3, T4, T5, T6>)it;
        }
    }
    public static partial class EventManagerExtensions
    {
        public static IDisposable Subscribe<T0, T1, T2, T3, T4, T5, T6>(this IEventManager events, string name, Action<T0, T1, T2, T3, T4, T5, T6> callback) => events.GetObservable<T0, T1, T2, T3, T4, T5, T6>(name).Subscribe(new EventObserver<T0, T1, T2, T3, T4, T5, T6>(callback));
        public static void Fire<T0, T1, T2, T3, T4, T5, T6>(this IEventManager events, string name, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) => events.GetObservable<T0, T1, T2, T3, T4, T5, T6>(name).Fire(t0, t1, t2, t3, t4, t5, t6);
    }
    public sealed class EventObservable<T0, T1, T2, T3, T4, T5, T6> : EventObservableBase, IObservable<T0, T1, T2, T3, T4, T5, T6>
    {
        private List<IObserver<T0, T1, T2, T3, T4, T5, T6>> _observers = new List<IObserver<T0, T1, T2, T3, T4, T5, T6>>();
        public EventObservable(string name) : base(name) { }
        public IDisposable Subscribe(IObserver<T0, T1, T2, T3, T4, T5, T6> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }
        public void Fire(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
        {
            foreach (var it in _observers)
                it.Fire(t0, t1, t2, t3, t4, t5, t6);
        }
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<T0, T1, T2, T3, T4, T5, T6>> _observers;
            private IObserver<T0, T1, T2, T3, T4, T5, T6> _observer;
            public Unsubscriber(List<IObserver<T0, T1, T2, T3, T4, T5, T6>> observers, IObserver<T0, T1, T2, T3, T4, T5, T6> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }
            public void Dispose() { if (_observer != null && _observers.Contains(_observer)) _observers.Remove(_observer); }
        }
    }
    public interface IObservable<out T0, out T1, out T2, out T3, out T4, out T5, out T6, out T7> { IDisposable Subscribe(IObserver<T0, T1, T2, T3, T4, T5, T6, T7> observer); }
    public interface IObserver<in T0, in T1, in T2, in T3, in T4, in T5, in T6, in T7> { void Fire(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7); }
    internal class EventObserver<T0, T1, T2, T3, T4, T5, T6, T7> : EventObserverBase, IObserver<T0, T1, T2, T3, T4, T5, T6, T7>
    {
        private Action<T0, T1, T2, T3, T4, T5, T6, T7> _callback;
        internal EventObserver(Action<T0, T1, T2, T3, T4, T5, T6, T7> callback) => _callback = callback;
        public void Fire(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) => _callback?.Invoke(t0, t1, t2, t3, t4, t5, t6, t7);
    }
    public partial interface IEventManager { EventObservable<T0, T1, T2, T3, T4, T5, T6, T7> GetObservable<T0, T1, T2, T3, T4, T5, T6, T7>(string name); }
    public partial class EventManager : IEventManager
    {
        public EventObservable<T0, T1, T2, T3, T4, T5, T6, T7> GetObservable<T0, T1, T2, T3, T4, T5, T6, T7>(string name)
        {
            Span<int> hashCodes = stackalloc[] { name.GetHashCode(), typeof(T0).GetHashCode(), typeof(T1).GetHashCode(), typeof(T2).GetHashCode(), typeof(T3).GetHashCode(), typeof(T4).GetHashCode(), typeof(T5).GetHashCode(), typeof(T6).GetHashCode(), typeof(T7).GetHashCode() };
            var hashTypeId = HashCode.Hash(hashCodes);
            if (!_observables.TryGetValue(hashTypeId, out var it))
            {
                it = new EventObservable<T0, T1, T2, T3, T4, T5, T6, T7>(name);
                _observables.Add(hashTypeId, it);
            }
            return (EventObservable<T0, T1, T2, T3, T4, T5, T6, T7>)it;
        }
    }
    public static partial class EventManagerExtensions
    {
        public static IDisposable Subscribe<T0, T1, T2, T3, T4, T5, T6, T7>(this IEventManager events, string name, Action<T0, T1, T2, T3, T4, T5, T6, T7> callback) => events.GetObservable<T0, T1, T2, T3, T4, T5, T6, T7>(name).Subscribe(new EventObserver<T0, T1, T2, T3, T4, T5, T6, T7>(callback));
        public static void Fire<T0, T1, T2, T3, T4, T5, T6, T7>(this IEventManager events, string name, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) => events.GetObservable<T0, T1, T2, T3, T4, T5, T6, T7>(name).Fire(t0, t1, t2, t3, t4, t5, t6, t7);
    }
    public sealed class EventObservable<T0, T1, T2, T3, T4, T5, T6, T7> : EventObservableBase, IObservable<T0, T1, T2, T3, T4, T5, T6, T7>
    {
        private List<IObserver<T0, T1, T2, T3, T4, T5, T6, T7>> _observers = new List<IObserver<T0, T1, T2, T3, T4, T5, T6, T7>>();
        public EventObservable(string name) : base(name) { }
        public IDisposable Subscribe(IObserver<T0, T1, T2, T3, T4, T5, T6, T7> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }
        public void Fire(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
        {
            foreach (var it in _observers)
                it.Fire(t0, t1, t2, t3, t4, t5, t6, t7);
        }
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<T0, T1, T2, T3, T4, T5, T6, T7>> _observers;
            private IObserver<T0, T1, T2, T3, T4, T5, T6, T7> _observer;
            public Unsubscriber(List<IObserver<T0, T1, T2, T3, T4, T5, T6, T7>> observers, IObserver<T0, T1, T2, T3, T4, T5, T6, T7> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }
            public void Dispose() { if (_observer != null && _observers.Contains(_observer)) _observers.Remove(_observer); }
        }
    }
    public interface IObservable<out T0, out T1, out T2, out T3, out T4, out T5, out T6, out T7, out T8> { IDisposable Subscribe(IObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8> observer); }
    public interface IObserver<in T0, in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8> { void Fire(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8); }
    internal class EventObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8> : EventObserverBase, IObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8>
    {
        private Action<T0, T1, T2, T3, T4, T5, T6, T7, T8> _callback;
        internal EventObserver(Action<T0, T1, T2, T3, T4, T5, T6, T7, T8> callback) => _callback = callback;
        public void Fire(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) => _callback?.Invoke(t0, t1, t2, t3, t4, t5, t6, t7, t8);
    }
    public partial interface IEventManager { EventObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8> GetObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8>(string name); }
    public partial class EventManager : IEventManager
    {
        public EventObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8> GetObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8>(string name)
        {
            Span<int> hashCodes = stackalloc[] { name.GetHashCode(), typeof(T0).GetHashCode(), typeof(T1).GetHashCode(), typeof(T2).GetHashCode(), typeof(T3).GetHashCode(), typeof(T4).GetHashCode(), typeof(T5).GetHashCode(), typeof(T6).GetHashCode(), typeof(T7).GetHashCode(), typeof(T8).GetHashCode() };
            var hashTypeId = HashCode.Hash(hashCodes);
            if (!_observables.TryGetValue(hashTypeId, out var it))
            {
                it = new EventObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8>(name);
                _observables.Add(hashTypeId, it);
            }
            return (EventObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8>)it;
        }
    }
    public static partial class EventManagerExtensions
    {
        public static IDisposable Subscribe<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this IEventManager events, string name, Action<T0, T1, T2, T3, T4, T5, T6, T7, T8> callback) => events.GetObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8>(name).Subscribe(new EventObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8>(callback));
        public static void Fire<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this IEventManager events, string name, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) => events.GetObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8>(name).Fire(t0, t1, t2, t3, t4, t5, t6, t7, t8);
    }
    public sealed class EventObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8> : EventObservableBase, IObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8>
    {
        private List<IObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8>> _observers = new List<IObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8>>();
        public EventObservable(string name) : base(name) { }
        public IDisposable Subscribe(IObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }
        public void Fire(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)
        {
            foreach (var it in _observers)
                it.Fire(t0, t1, t2, t3, t4, t5, t6, t7, t8);
        }
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8>> _observers;
            private IObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8> _observer;
            public Unsubscriber(List<IObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8>> observers, IObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }
            public void Dispose() { if (_observer != null && _observers.Contains(_observer)) _observers.Remove(_observer); }
        }
    }
    public interface IObservable<out T0, out T1, out T2, out T3, out T4, out T5, out T6, out T7, out T8, out T9> { IDisposable Subscribe(IObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> observer); }
    public interface IObserver<in T0, in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9> { void Fire(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9); }
    internal class EventObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> : EventObserverBase, IObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
        private Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> _callback;
        internal EventObserver(Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> callback) => _callback = callback;
        public void Fire(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) => _callback?.Invoke(t0, t1, t2, t3, t4, t5, t6, t7, t8, t9);
    }
    public partial interface IEventManager { EventObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> GetObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string name); }
    public partial class EventManager : IEventManager
    {
        public EventObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> GetObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string name)
        {
            Span<int> hashCodes = stackalloc[] { name.GetHashCode(), typeof(T0).GetHashCode(), typeof(T1).GetHashCode(), typeof(T2).GetHashCode(), typeof(T3).GetHashCode(), typeof(T4).GetHashCode(), typeof(T5).GetHashCode(), typeof(T6).GetHashCode(), typeof(T7).GetHashCode(), typeof(T8).GetHashCode(), typeof(T9).GetHashCode() };
            var hashTypeId = HashCode.Hash(hashCodes);
            if (!_observables.TryGetValue(hashTypeId, out var it))
            {
                it = new EventObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(name);
                _observables.Add(hashTypeId, it);
            }
            return (EventObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>)it;
        }
    }
    public static partial class EventManagerExtensions
    {
        public static IDisposable Subscribe<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IEventManager events, string name, Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> callback) => events.GetObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(name).Subscribe(new EventObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(callback));
        public static void Fire<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IEventManager events, string name, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) => events.GetObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(name).Fire(t0, t1, t2, t3, t4, t5, t6, t7, t8, t9);
    }
    public sealed class EventObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> : EventObservableBase, IObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
        private List<IObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>> _observers = new List<IObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>>();
        public EventObservable(string name) : base(name) { }
        public IDisposable Subscribe(IObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }
        public void Fire(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9)
        {
            foreach (var it in _observers)
                it.Fire(t0, t1, t2, t3, t4, t5, t6, t7, t8, t9);
        }
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>> _observers;
            private IObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> _observer;
            public Unsubscriber(List<IObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>> observers, IObserver<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }
            public void Dispose() { if (_observer != null && _observers.Contains(_observer)) _observers.Remove(_observer); }
        }
    }
}
