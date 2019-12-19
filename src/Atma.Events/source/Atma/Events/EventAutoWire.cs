namespace Atma.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    public class EventAttribute : Attribute
    {
        public readonly string Name;
        public EventAttribute() { Name = null; }
        public EventAttribute(string name) { Name = name; }
    }

    public interface IAutoEventManager : IDisposable
    {
        void Subscribe(UnmanagedDispose disposable);
    }

    public class AutoEventManager : UnmanagedDispose, IAutoEventManager
    {
        private delegate IDisposable WireUp(IEventManager events, string name, Delegate dg);
        private readonly IEventManager _events;
        private Dictionary<int, EventAutoWire> _eventWireUpCache = new Dictionary<int, EventAutoWire>();

        public AutoEventManager(IEventManager events)
        {
            _events = events;
        }

        public void Subscribe(UnmanagedDispose disposable)
        {
            var type = disposable.GetType();
            var methods = from method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                          select new { method, attr = method.GetCustomAttribute<EventAttribute>() };

            foreach (var m in methods)
            {
                if (m.attr != null)
                {
                    var name = m.attr?.Name ?? m.method.Name;
                    Subscribe(disposable, m.method, name);
                }
            }
        }

        private void Subscribe(UnmanagedDispose disposable, MethodInfo methodInfo, string name)
        {
            var parms = methodInfo.GetParameters();
            var types = parms.Select(x => x.ParameterType).ToArray();
            var hash = HashCode.Combine(types);

            if (!_eventWireUpCache.TryGetValue(hash, out var wire))
            {
                wire = new EventAutoWire(hash, types);
                _eventWireUpCache.Add(hash, wire);
            }

            disposable.Track(wire.Subscribe(_events, disposable, methodInfo, name));
        }

    }

    public class EventAutoWire
    {
        public delegate IDisposable WireUp(IEventManager events, string name, Delegate o);

        private static MethodInfo[] _eventSubscribeMethods = new MethodInfo[EventManager.GenericCount];
        static EventAutoWire()
        {
            var autoType = typeof(EventManagerExtensions);
            var methods = autoType.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(x => x.Name == "Subscribe").ToArray();
            for (var i = 0; i < _eventSubscribeMethods.Length; i++)
                _eventSubscribeMethods[i] = methods.First(x => x.GetGenericArguments().Length == i);
        }

        public readonly int ID;
        public readonly Type[] Types;

        private WireUp _wireUp;

        private MethodInfo _genericSubscribe;

        public EventAutoWire(int id, Type[] types)
        {
            ID = id;
            Types = types;
            _genericSubscribe = _eventSubscribeMethods[types.Length];
            if (types.Length > 0)
                _genericSubscribe = _genericSubscribe.MakeGenericMethod(Types);

            _wireUp = Build();
        }

        public IDisposable Subscribe(IEventManager events, UnmanagedDispose disposable, MethodInfo mi, string name)
        {
            if (Types.Length > 0)
            {
                var action = typeof(Action<>);
                var genericAction = action.MakeGenericType(Types);
                var dg = mi.CreateDelegate(genericAction, disposable);
                return _wireUp(events, name, dg);
            }
            else
            {
                var dg = mi.CreateDelegate(typeof(Action), disposable);
                return _wireUp(events, name, dg);
            }
        }

        public WireUp Build()
        {
            var method = new DynamicMethod(_genericSubscribe.Name.ToString(), typeof(IDisposable), new Type[] { typeof(IEventManager), typeof(string), typeof(Delegate) });

            var gen = method.GetILGenerator();

            //call method
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldarg_2);
            gen.Emit(OpCodes.Call, _genericSubscribe);

            gen.Emit(OpCodes.Ret);

            return (WireUp)method.CreateDelegate(typeof(WireUp));
        }
    }
}