using System;
using System.Linq;

namespace ExtensionGenerator
{
    public class Events : Command
    {
        public override string Name => "Events";

        public override string Description => "Generates the event overloads.";

        protected override int OnRun()
        {
            Console.WriteLine("using System;");
            Console.WriteLine("using System.Collections.Generic;");
            Console.WriteLine("using Atma;");
            Console.WriteLine("using Atma.Entities;");
            Console.WriteLine("using Atma.Memory;");

            Console.WriteLine("namespace Atma.Events{");
            for (var i = 1; i <= 10; i++)
            {
                WriteFunction(i);
            }
            Console.WriteLine("}");

            return 0;
        }

        protected void WriteFunction(int genericCount)
        {
            var generics = genericCount.Range().Select(i => $"T{i}").ToArray();
            var outGenerics = genericCount.Range().Select(i => $"out T{i}");
            var inGenerics = genericCount.Range().Select(i => $"in T{i}");
            var where = genericCount.Range().Select(i => $"where T{i}: unmanaged").ToArray();
            var spanArgs = genericCount.Range().Select(i => $"T{i} t{i}");
            var viewArgs = genericCount.Range().Select(i => $"t{i}");
            var componentType = genericCount.Range().Select(i => $"typeof(T{i}).GetHashCode()").ToArray();
            var componentArrays = genericCount.Range().Select(i => $"  var t{i} = chunk.GetComponentData<T{i}>();").ToArray();
            Console.WriteLine($"  public interface IObservable<{outGenerics.Join()}> {{ IDisposable Subscribe(IObserver<{generics.Join()}> observer);}}");
            Console.WriteLine($"  public interface IObserver<{inGenerics.Join()}> {{ void Fire({spanArgs.Join()});}}");
            Console.WriteLine($"  internal class EventObserver<{generics.Join()}> : EventObserverBase, IObserver<{generics.Join()}>{{");
            Console.WriteLine($"    private Action<{generics.Join()}> _callback;");
            Console.WriteLine($"    internal EventObserver(Action<{generics.Join()}> callback) => _callback = callback;");
            Console.WriteLine($"    public void Fire({spanArgs.Join()}) => _callback?.Invoke({viewArgs.Join()});");
            Console.WriteLine($"  }}");
            Console.WriteLine($"  public partial interface IEventManager {{ EventObservable<{generics.Join()}> GetObservable<{generics.Join()}>(string name); }}");
            Console.WriteLine($"  public partial class EventManager : IEventManager {{");
            Console.WriteLine($"    public EventObservable<{generics.Join()}> GetObservable<{generics.Join()}>(string name) {{");
            Console.WriteLine($"      Span<int> hashCodes = stackalloc[] {{ name.GetHashCode(), {componentType.Join()} }};");
            Console.WriteLine($"      var hashTypeId = HashCode.Hash(hashCodes);");
            Console.WriteLine($"      if (!_observables.TryGetValue(hashTypeId, out var it)) {{ ");
            Console.WriteLine($"        it = new EventObservable<{generics.Join()}>(name);");
            Console.WriteLine($"        _observables.Add(hashTypeId, it);");
            Console.WriteLine($"      }}");
            Console.WriteLine($"      return (EventObservable<{generics.Join()}>)it;");
            Console.WriteLine($"    }}");
            Console.WriteLine($"  }}");
            Console.WriteLine($"  public static partial class EventManagerExtensions {{");
            Console.WriteLine($"    public static IDisposable Subscribe<{generics.Join()}>(this IEventManager events, string name, Action<{generics.Join()}> callback) => events.GetObservable<{generics.Join()}>(name).Subscribe(new EventObserver<{generics.Join()}>(callback));");
            Console.WriteLine($"    public static void Fire<{generics.Join()}>(this IEventManager events, string name, {spanArgs.Join()}) => events.GetObservable<{generics.Join()}>(name).Fire({viewArgs.Join()});");
            Console.WriteLine($"  }}");
            Console.WriteLine($"  public sealed class EventObservable<{generics.Join()}> : EventObservableBase, IObservable<{generics.Join()}> {{");
            Console.WriteLine($"    private List<IObserver<{generics.Join()}>> _observers = new List<IObserver<{generics.Join()}>>();");
            Console.WriteLine($"    public EventObservable(string name) : base(name) {{ }}");
            Console.WriteLine($"    public IDisposable Subscribe(IObserver<{generics.Join()}> observer) {{");
            Console.WriteLine($"      if (!_observers.Contains(observer))");
            Console.WriteLine($"        _observers.Add(observer);");
            Console.WriteLine($"      return new Unsubscriber(_observers, observer);");
            Console.WriteLine($"    }}");
            Console.WriteLine($"    public void Fire({spanArgs.Join()}) {{");
            Console.WriteLine($"      foreach (var it in _observers)");
            Console.WriteLine($"        it.Fire({viewArgs.Join()});");
            Console.WriteLine($"    }}");
            Console.WriteLine($"    private class Unsubscriber : IDisposable {{");
            Console.WriteLine($"      private List<IObserver<{generics.Join()}>> _observers;");
            Console.WriteLine($"      private IObserver<{generics.Join()}> _observer;");
            Console.WriteLine($"      public Unsubscriber(List<IObserver<{generics.Join()}>> observers, IObserver<{generics.Join()}> observer) {{");
            Console.WriteLine($"        this._observers = observers;");
            Console.WriteLine($"        this._observer = observer;");
            Console.WriteLine($"      }}");
            Console.WriteLine($"      public void Dispose() {{ if (_observer != null && _observers.Contains(_observer)) _observers.Remove(_observer); }}");
            Console.WriteLine($"    }}");
            Console.WriteLine($"  }}");



            /*

            */

        }
    }
}