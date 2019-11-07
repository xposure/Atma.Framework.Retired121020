using Atma.Entities;
using SimpleInjector;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using static Atma.Debug;

namespace Atma.DI
{
    public class ServiceContainer// : IServiceContainer
    {
        private bool _isInited = false;
        private Container _container = new Container();

        private HashSet<Assembly> _assemblies = new HashSet<Assembly>();
        private HashSet<Type> _componentTypes = new HashSet<Type>();
        private Dictionary<Type, Type> _defaults = new Dictionary<Type, Type>();
        private Dictionary<Type, Type> _providers = new Dictionary<Type, Type>();
        private Dictionary<Type, HashSet<Type>> _collections = new Dictionary<Type, HashSet<Type>>();

        public ServiceContainer()
        {
            //.net is only loading assemblies being touched
            //hence why I had to get a ref to job manager earlier
            //so that the dll would load

            foreach (var it in Assembly.GetEntryAssembly().GetReferencedAssemblies())
                if (it.FullName.StartsWith("Intrinsic"))
                    _assemblies.Add(Assembly.Load(it.ToString()));
        }

        public void AddAssembly(Assembly asm)
        {
            _assemblies.Add(asm);
        }

        public void AddSingleton<T>()
            where T : class, new()
        {
            Assert(!_isInited);
            _container.RegisterInstance<T>(new T());
        }

        public void AddSingleton<T>(T instance)
            where T : class
        {
            Assert(!_isInited);
            _container.RegisterInstance(instance);
        }

        public void AddSingleton<T, K>(K instance)
            where T : class
            where K : T
        {
            Assert(!_isInited);
            _container.RegisterInstance<T>(instance);
        }

        public void RegisterAll<T>() => RegisterAll<T>(_assemblies);

        public void RegisterAll<T>(IEnumerable<Assembly> assemblies)
        {
            foreach (var it in assemblies)
                RegisterAll<T>(it);
        }

        public void RegisterAll<T>(Assembly asm)
        {
            Assert(!_isInited);
            var serviceType = typeof(T);
            var servicesQuery =
                from type in asm.GetExportedTypes()
                where serviceType.IsAssignableFrom(type) &&
                      type.IsClass && !type.IsAbstract
                select type;


            if (!_collections.TryGetValue(serviceType, out var types))
            {
                types = new HashSet<Type>();
                _collections.Add(serviceType, types);
            }

            foreach (var it in servicesQuery)
            {
                if (!DefaultService.IsDefaultService(it))
                {
                    var providedService = ServiceProvider.GetServiceProvided(it);
                    if (providedService != null)
                        _providers[providedService] = it;
                    _container.Register(it, it, Lifestyle.Singleton);
                    types.Add(it);
                }
                else
                {
                    var defaultService = ServiceProvider.GetServiceProvided(it);
                    Assert(defaultService != null);
                    _defaults[defaultService] = it;
                }
            }
        }

        private void AddComponentsFromNamespace(Assembly assembly, string name)
        {
            foreach (var it in assembly.GetTypes().Where(t => t.IsValueType && t.Namespace == name))
                _componentTypes.Add(it);
        }

        public void AddComponentsFromNamespace<T>()
            where T : unmanaged
        {
            Assert(!_isInited);
            var type = typeof(T);
            var asm = type.Assembly;
            var ns = type.Namespace;
            AddComponentsFromNamespace(asm, ns);
        }

        public T Get<T>() where T : class
        {
            Assert(_isInited);
            return _container.GetInstance<T>();
        }

        public IEnumerable<T> GetAll<T>() where T : class
        {
            Assert(_isInited);
            return _container.GetAllInstances<T>();
        }

        public IEnumerable<IService> GetServices()
        {
            Assert(_isInited);
            return _container.GetAllInstances<IService>();
        }

        public IEnumerable<T> GetServices<T>()
            where T : class
        {
            foreach (var it in GetServices())
                if (it is T t)
                    yield return t;
        }

        public void Initialize()
        {
            Assert(!_isInited);
            _isInited = true;
            var entityViewType = typeof(EntityView);
            var serviceType = typeof(IService);

            var componentList = new ComponentList();
            foreach (var it in _componentTypes)
                componentList.AddComponent(it);

            _container.RegisterInstance(componentList);

            _container.RegisterConditional(
                typeof(EntityView<>),
                typeof(EntityView<>),
                Lifestyle.Singleton,
                c => !c.Handled
            );

            _container.ResolveUnregisteredType += _container_ResolveUnregisteredType;

            foreach (var kvp in _collections)
            {
                _container.Collection.Register(kvp.Key, kvp.Value);
            }

            _container.Verify();

            var systemManager = Get<SystemManager>();
            foreach (var it in _container.GetAllInstances<ComponentSystem>())
            {
                var type = it.GetType();
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (var field in fields)
                {
                    var fieldType = field.FieldType;
                    if (Injectable.IsInjectable(fieldType))
                    {
                        if (fieldType.IsGenericType && entityViewType.IsAssignableFrom(fieldType))
                        {
                            var entityView = _container.GetInstance(fieldType);
                            field.SetValue(it, entityView);
                            it.AddView(entityView as EntityView);
                        }
                        else if (serviceType.IsAssignableFrom(fieldType))
                        {
                            var service = _container.GetInstance(fieldType);
                            field.SetValue(it, service);
                        }
                    }
                }

                systemManager.AddSystem(it);
            }

            foreach (var it in GetServices<IInitService>())
                it.Initialize();
        }

        private void _container_ResolveUnregisteredType(object sender, UnregisteredTypeEventArgs e)
        {
            var hasProvider = _providers.TryGetValue(e.UnregisteredServiceType, out var type);
            if (!hasProvider)
            {
                var hasDefault = _defaults.TryGetValue(e.UnregisteredServiceType, out type);
                Assert(hasDefault);
            }
            e.Register(Lifestyle.Singleton.CreateRegistration(type, _container));
            //e.Handled = true;
        }
    }
}
