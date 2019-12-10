
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Atma.Entities;

public unsafe class SystemMethodExecutor
{
    private HashSet<ComponentType> _componentTypes = new HashSet<ComponentType>();
    private HashSet<ComponentType> _readComponents = new HashSet<ComponentType>();
    private HashSet<ComponentType> _writeComponents = new HashSet<ComponentType>();

    // public DependencyList _dependencies;
    // public DependencyList Dependencies => _dependencies;
    public delegate void Executor(object owner, int length, void** data);
    private readonly Type _type;
    public readonly Executor Execute;

    public readonly EntitySpec Spec;

    public SystemMethodExecutor(Type type, MethodInfo m)
    {
        _type = type;
        var componentList = new ComponentList();
        var parms = m.GetParameters();
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
            if (pType.GetCustomAttribute<System.Runtime.InteropServices.InAttribute>() != null)
                _readComponents.Add(componentType);
            else
                _writeComponents.Add(componentType);
        }

        //call method
        gen.Emit(OpCodes.Call, m);

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

        Execute = (Executor)method.CreateDelegate(typeof(Executor));
        Spec = new EntitySpec(_componentTypes.ToArray());
    }
}

public unsafe class SystemMethodList
{

    private readonly Type _type;
    private readonly SystemMethodExecutor[] _methods;

    public SystemMethodList(Type type)
    {
        _type = type;

        var typeMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                .Where(x => string.Compare(x.Name, "Execute", true) == 0).ToArray();

        _methods = new SystemMethodExecutor[typeMethods.Length];
        for (var i = 0; i < _methods.Length; i++)
            _methods[i] = BuildMethod(typeMethods[i]);
    }

    private SystemMethodExecutor BuildMethod(MethodInfo method) => new SystemMethodExecutor(_type, method);

    public void Execute<T>(EntityManager entityManager, T system)
    {
        //dependency graph
        for (var i = 0; i < _methods.Length; i++)
        {
            var method = _methods[i];
            entityManager.EntityArrays.Filter(_methods[i].Spec, array =>
            {
                Span<ComponentType> componentTypes = method.Spec.ComponentTypes;
                Span<int> indices = stackalloc int[componentTypes.Length];
                for (var k = 0; k < componentTypes.Length; k++)
                    indices[k] = array.Specification.GetComponentIndex(componentTypes[k]);

                var data = stackalloc void*[componentTypes.Length];
                for (var i = 0; i < array.AllChunks.Count; i++)
                {
                    var chunk = array.AllChunks[i];
                    for (var k = 0; k < componentTypes.Length; k++)
                        data[k] = chunk.PackedArray[indices[k]].Memory;

                    method.Execute(system, chunk.Count, data);
                }
            });
        }
    }
}


public unsafe class SystemTest
{
    public float dt = 0;
    public float maxx = 0;
    public float maxy = 0;

    private SystemMethodList _systemMethodList;

    public SystemTest()
    {
        _systemMethodList = new SystemMethodList(this.GetType());
    }

    public void Process(EntityManager entityManager)
    {
        _systemMethodList.Execute(entityManager, this);
    }

    public void Execute(ref Position position, ref Velocity velocity)
    {
        position.X += velocity.X * dt;
        position.Y += velocity.Y * dt;

        velocity.X -= velocity.X * dt;
        velocity.Y -= velocity.Y * dt;

        if ((position.X > maxx && velocity.X > 0) || (position.X < 0 && velocity.X < 0))
            velocity.X = -velocity.X;

        if ((position.Y > maxy && velocity.Y > 0) || (position.Y < 0 && velocity.Y < 0))
            velocity.Y = -velocity.Y;
    }
}