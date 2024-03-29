namespace Atma.Systems
{
    using Atma.Memory;
    using Atma.Entities;
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using System.Linq;
    using Microsoft.Extensions.Logging;

    public sealed unsafe class SystemMethodExecutor : UnmanagedDispose, ISystem
    {
        private HashSet<ComponentType> _componentTypes = new HashSet<ComponentType>();
        private HashSet<ComponentType> _readComponents = new HashSet<ComponentType>();
        private HashSet<ComponentType> _writeComponents = new HashSet<ComponentType>();

        private ComponentType[] _anyComponent, _ignoreComponent;

        public DependencyList _dependencies;
        public DependencyList Dependencies => _dependencies;

        public int Priority { get; private set; }

        public string Name { get; private set; }

        public string Group { get; private set; }

        public string[] Stages { get; private set; }

        public bool Disabled { get; set; }

        private delegate void Executor(object owner, int length, void** data);

        private object _owner;
        private Type _type;
        private MethodInfo _method;

        private Executor _execute;
        //public Executor Execute => _execute;

        private EntitySpec _spec;
        public EntitySpec Spec => _spec;

        public SystemMethodExecutor(object owner, Type type, MethodInfo method, string name = null, string defaultGroup = null, int? defaultPriority = null, string[] defaultStages = null)
        {
            _owner = owner;
            _type = type;
            _method = method;

            Name = name ?? _method.GetCustomAttribute<NameAttribute>()?.Name ?? method.ToString();
            Group = _method.GetCustomAttribute<GroupAttribute>()?.Name ?? defaultGroup;
            Priority = _method.GetCustomAttribute<PriorityAttribute>()?.Priority ?? defaultPriority ?? 0;
            Stages = _method.GetCustomAttribute<StagesAttribute>()?.Stages ?? defaultStages ?? null;

        }

        public void Init()
        {
            var componentList = new ComponentList();
            var parms = _method.GetParameters();
            var method = new DynamicMethod("Execute", null, new Type[] { typeof(object), typeof(int), typeof(void**) });

            var gen = method.GetILGenerator();
            var i = gen.DeclareLocal(typeof(int));
            var b = gen.DeclareLocal(typeof(bool));

            var arrays = new LocalBuilder[parms.Length];
            for (var k = 0; k < parms.Length; k++)
                arrays[k] = gen.DeclareLocal(parms[k].ParameterType);

            var end = gen.DefineLabel();
            var top = gen.DefineLabel();

            //init component type pointers
            for (var k = 0; k < parms.Length; k++)
            {
                gen.Emit(OpCodes.Ldarg_2);

                if (k > 0)
                {
                    gen.Emit(OpCodes.Sizeof, typeof(void*));
                    gen.Emit(OpCodes.Add);
                }

                gen.Emit(OpCodes.Ldind_I);
                gen.Emit(OpCodes.Stloc, arrays[k].LocalIndex);
            }

            //i = 0
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Stloc, i.LocalIndex);

            gen.Emit(OpCodes.Br_S, end);

            // //for loop
            gen.MarkLabel(top);
            gen.Emit(OpCodes.Ldarg_0); //systemtest

            for (var k = 0; k < parms.Length; k++)
            {
                var pType = parms[k].ParameterType;
                var dataType = pType.GetElementType();

                //load component ptr
                gen.Emit(OpCodes.Ldloc, arrays[k].LocalIndex);

                // gen.Emit(OpCodes.Ldarg_2);
                // if (k > 0)
                // {
                //     gen.Emit(OpCodes.Ldc_I4, k);
                //     gen.Emit(OpCodes.Sizeof, typeof(void*));
                //     gen.Emit(OpCodes.Mul);
                //     gen.Emit(OpCodes.Add);
                // }
                // gen.Emit(OpCodes.Ldind_I);

                //load i
                gen.Emit(OpCodes.Ldloc, i.LocalIndex);
                gen.Emit(OpCodes.Conv_I);

                //increment pointer by index of i
                gen.Emit(OpCodes.Sizeof, dataType);
                gen.Emit(OpCodes.Mul);
                gen.Emit(OpCodes.Add);

                if (!componentList.TryAddComponent(dataType, out var componentType))
                    return; //invalid type

                if (!_componentTypes.Add(componentType))
                    return; //duplicate type

                //check if we are read only (finally a decent way to enforce read only pointers!)
                if (parms[k].GetCustomAttribute<System.Runtime.InteropServices.InAttribute>() != null)
                    _readComponents.Add(componentType);
                else
                    _writeComponents.Add(componentType);
            }

            //call method
            gen.Emit(OpCodes.Call, _method);

            //i++
            gen.Emit(OpCodes.Ldloc, i.LocalIndex);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Add);
            gen.Emit(OpCodes.Stloc, i.LocalIndex);

            //i < length
            gen.MarkLabel(end);
            gen.Emit(OpCodes.Ldloc, i.LocalIndex);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Clt);
            gen.Emit(OpCodes.Stloc, b.LocalIndex);
            gen.Emit(OpCodes.Ldloc, b.LocalIndex);
            gen.Emit(OpCodes.Brtrue_S, top);

            gen.Emit(OpCodes.Ret);

            _execute = (Executor)method.CreateDelegate(typeof(Executor));

            var extraComponents = _method.GetCustomAttribute<HasAttribute>()?.Types.Where(x => componentList.TryAddComponent(x)).Select(x => componentList.Get(x)).ToArray();
            if (extraComponents != null)
                foreach (var it in extraComponents)
                    _componentTypes.Add(it);

            _anyComponent = _method.GetCustomAttribute<AnyAttribute>()?.Types.Where(x => componentList.TryAddComponent(x)).Select(x => componentList.Get(x)).ToArray();
            _ignoreComponent = _method.GetCustomAttribute<IgnoreAttribute>()?.Types.Where(x => componentList.TryAddComponent(x)).Select(x => componentList.Get(x)).ToArray();

            _spec = new EntitySpec(_componentTypes.ToArray());

            _dependencies = new DependencyList(Name, Priority, config =>
            {
                foreach (var it in _readComponents)
                    config.Read(it);
                foreach (var it in _writeComponents)
                    config.Write(it);

                var before = _method.GetCustomAttribute<BeforeAttribute>();
                if (before != null)
                    foreach (var it in before.Names)
                        config.Before(it);

                var after = _method.GetCustomAttribute<AfterAttribute>();
                if (after != null)
                    foreach (var it in after.Names)
                        config.After(it);
            });
        }

        public void Tick(SystemManager systemManager, EntityManager entityManager)
        {
            entityManager.EntityArrays.Filter(_spec, array =>
            {
                Span<ComponentType> componentTypes = _spec.ComponentTypes;
                Span<int> indices = stackalloc int[componentTypes.Length];
                for (var k = 0; k < componentTypes.Length; k++)
                    indices[k] = array.Specification.GetComponentIndex(componentTypes[k]);

                var data = stackalloc void*[componentTypes.Length];
                for (var i = 0; i < array.AllChunks.Count; i++)
                {
                    var chunk = array.AllChunks[i];
                    for (var k = 0; k < componentTypes.Length; k++)
                        data[k] = chunk.PackedArray[indices[k]].Memory;

                    _execute(_owner, chunk.Count, data);
                }
            });
        }

        public override string ToString() => Name;
    }

    public unsafe class SystemProducer : UnmanagedDispose
    {
        private SystemMethodExecutor[] _systems;
        private readonly Type _type;

        public string DefaultGroup { get; private set; }
        public int? DefaultPriority { get; private set; }
        public string[] DefaultStages { get; private set; }

        protected internal SystemProducer(string group = null, int? priority = null, string[] stages = null)
        {
            _type = this.GetType();

            DefaultGroup = group ?? _type.GetCustomAttribute<GroupAttribute>()?.Name ?? null;
            DefaultPriority = priority ?? _type.GetCustomAttribute<PriorityAttribute>()?.Priority;
            DefaultStages = stages ?? _type.GetCustomAttribute<StagesAttribute>()?.Stages;
        }

        internal void Register(SystemManager systemManager)
        {
            var typeMethods = _type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                               .Where(x => string.Compare(x.Name, "Execute", true) == 0).ToArray();

            _systems = new SystemMethodExecutor[typeMethods.Length];
            for (var i = 0; i < _systems.Length; i++)
            {
                _systems[i] = BuildSystem(typeMethods[i]);
                systemManager.Register(_systems[i]);
            }
        }

        private SystemMethodExecutor BuildSystem(MethodInfo method) => new SystemMethodExecutor(this, _type, method, null, DefaultGroup, DefaultPriority, DefaultStages);

        protected override void OnManagedDispose()
        {
            if (_systems != null)
            {
                for (var i = 0; i < _systems.Length; i++)
                {
                    _systems[i].Dispose();
                    _systems[i] = null;
                }

                _systems = null;
            }
        }
    }
}