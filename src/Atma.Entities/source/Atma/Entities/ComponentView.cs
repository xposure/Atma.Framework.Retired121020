
namespace Atma.Entities
{
    using System;
    using System.Collections.Generic;

    internal enum ComponentFieldType
    {
        ComponentType,
        Entity,
        Length
    }

    // internal sealed class ComponentViewList
    // {
    //     private readonly LookupList<ComponentView> _views = new LookupList<ComponentView>();
    //     private readonly ComponentList _componentList;

    //     public ComponentViewList(ComponentList componentList)
    //     {
    //         _componentList = componentList;
    //     }

    //     public ComponentView GetView<T>(EntityManager entityManager)
    //         where T : unmanaged
    //     {
    //         var type = typeof(T);
    //         if (!_views.TryGetValue(type.GetHashCode(), out var view))
    //         {
    //             view = ComponentView.Create<T>(_componentList);
    //             _views.Add(type.GetHashCode(), view);

    //             foreach (var it in entityManager.Archetypes)
    //                 view.EntityArchetypeCreated(it);
    //         }

    //         return view;
    //     }

    //     internal void EntityArchetypeCreated(EntitySpec spec)
    //     {
    //         foreach (var it in _views.AllObjects)
    //             it.EntityArchetypeCreated(spec);
    //     }
    // }

    // internal sealed class ComponentView
    // {
    //     private static readonly Type _entityType = typeof(Entity);

    //     private readonly Type _viewType;
    //     private bool _chunkView;
    //     private ComponentViewField _entityField;
    //     private ComponentViewField _lengthField;
    //     private HashSet<ComponentType> _hasComponents = new HashSet<ComponentType>();
    //     private HashSet<ComponentType> _anyComponents = new HashSet<ComponentType>();
    //     private HashSet<ComponentType> _ignoreComponents = new HashSet<ComponentType>();
    //     private HashSet<ComponentType> _allComponents = new HashSet<ComponentType>();
    //     private List<ComponentViewField> _fields;
    //     private List<EntitySpec> _archetypes = new List<EntitySpec>();

    //     public bool HasEntityField => _entityField != null;
    //     public bool HasLengthField => _lengthField != null;
    //     public bool IsChunkView => _chunkView;
    //     public IReadOnlyCollection<ComponentViewField> Fields => _fields;
    //     public IReadOnlyCollection<EntitySpec> Archetypes => _archetypes;

    //     public int ProjectCount => IsChunkView ? ChunkCount : EntityCount;

    //     public int EntityCount
    //     {
    //         get
    //         {
    //             var count = 0;
    //             foreach (var it in _archetypes)
    //                 count += it.Count;
    //             return count;
    //         }
    //     }

    //     public int ChunkCount
    //     {
    //         get
    //         {
    //             var count = 0;
    //             foreach (var spec in _archetypes)
    //                 foreach (var chunk in spec.ActiveChunks)
    //                     count++;

    //             return count;
    //         }
    //     }

    //     private ComponentView(Type viewType)
    //     {
    //         _viewType = viewType;
    //     }

    //     private void AddComponentTypes(ComponentList componentList, HashSet<ComponentType> components, HashSet<Type> types)
    //     {
    //         foreach (var it in types)
    //         {
    //             if (!componentList.Find(it, out var componentType))
    //                 Assert(false);
    //             components.Add(componentType);
    //             _allComponents.Add(componentType);
    //         }
    //     }

    //     public unsafe void UpdateEntity<T>(EntityManager e, in T t)
    //         where T : unmanaged
    //     {
    //         Assert(_viewType == typeof(T));
    //         Assert(!_chunkView);
    //         Assert(HasEntityField);

    //         var tt = t;
    //         var entity = _entityField.GetEntityFromStruct(&tt);
    //         UpdateEntity(e, &tt, entity);
    //     }

    //     public unsafe void UpdateEntity<T>(EntityManager e, in T t, int entity)
    //       where T : unmanaged
    //     {
    //         Assert(_viewType == typeof(T));
    //         Assert(!_chunkView);

    //         var tt = t;
    //         UpdateEntity(e, &tt, entity);
    //     }

    //     private unsafe void UpdateEntity<T>(EntityManager e, T* t, int entity)
    //         where T : unmanaged
    //     {
    //         e.GetEntityStorage(entity, out var spec, out var chunk, out var index);

    //         foreach (var it in _fields)
    //             if (it.IsWriteable)
    //                 it.UpdateFromStruct(t, spec, chunk, index);
    //     }

    //     public IEnumerable<T> Project<T>()
    //         where T : unmanaged
    //     {
    //         Assert(_viewType == typeof(T));
    //         if (_chunkView)
    //         {
    //             foreach (var spec in _archetypes)
    //                 foreach (var chunk in spec.ActiveChunks)
    //                     yield return GetChunkView<T>(spec, chunk);
    //         }
    //         else
    //         {
    //             foreach (var spec in _archetypes)
    //                 foreach (var chunk in spec.ActiveChunks)
    //                     for (var i = 0; i < chunk.Count; i++)
    //                         yield return GetEntityView<T>(spec, chunk, i);
    //         }
    //     }

    //     public unsafe void ProjectTo<T>(in NativeSlice<T> slice)
    //         where T : unmanaged
    //     {
    //         Assert(_viewType == typeof(T));
    //         Assert(ProjectCount == slice.Length);

    //         var p = slice.RawPointer;
    //         if (_chunkView)
    //         {
    //             foreach (var spec in _archetypes)
    //                 foreach (var chunk in spec.ActiveChunks)
    //                     MapChunkView(p++, spec, chunk);
    //         }
    //         else
    //         {
    //             foreach (var spec in _archetypes)
    //                 foreach (var chunk in spec.ActiveChunks)
    //                     for (var i = 0; i < chunk.Count; i++)
    //                         MapEntityView(p++, spec, chunk, i);
    //         }
    //     }

    //     private unsafe void MapEntityView<T>(T* p, EntitySpec spec, EntityChunk chunk, int index)
    //         where T : unmanaged
    //     {
    //         foreach (var it in _fields)
    //             it.CopyToStruct(p, spec, chunk, index);
    //     }

    //     private unsafe void MapChunkView<T>(T* p, EntitySpec spec, EntityChunk chunk)
    //         where T : unmanaged
    //     {
    //         foreach (var it in _fields)
    //             it.CopyPtrsToStruct(p, spec, chunk);
    //     }

    //     private unsafe T GetEntityView<T>(EntitySpec spec, EntityChunk chunk, int index)
    //         where T : unmanaged
    //     {
    //         var t = new T();
    //         MapEntityView(&t, spec, chunk, index);
    //         return t;
    //     }

    //     private unsafe T GetChunkView<T>(EntitySpec spec, EntityChunk chunk)
    //         where T : unmanaged
    //     {
    //         var t = new T();
    //         MapChunkView(&t, spec, chunk);
    //         return t;
    //     }

    //     internal void EntityArchetypeCreated(EntitySpec spec)
    //     {
    //         foreach (var it in _ignoreComponents)
    //             if (spec.HasComponentType(it))
    //                 return;

    //         if (_anyComponents.Count > 0 && _anyComponents.Count(x => spec.HasComponentType(x)) == 0)
    //             return;

    //         if (_hasComponents.Count != _hasComponents.Count(x => spec.HasComponentType(x)))
    //             return;

    //         _archetypes.Add(spec);
    //     }

    //     internal static ComponentView Create<T>(ComponentList componentList)
    //     {
    //         var view = new ComponentView(typeof(T));

    //         var type = view._viewType;
    //         var attrs = view._viewType.GetCustomAttributes();
    //         var hasComponents = new HashSet<Type>(type.GetCustomAttribute<HasComponent>()?.Type ?? new Type[0]);
    //         var anyComponents = new HashSet<Type>(type.GetCustomAttribute<AnyComponent>()?.Type ?? new Type[0]);
    //         var ignoreComponents = new HashSet<Type>(type.GetCustomAttribute<IgnoreComponent>()?.Type ?? new Type[0]);

    //         view.AddComponentTypes(componentList, view._hasComponents, hasComponents);
    //         view.AddComponentTypes(componentList, view._anyComponents, anyComponents);
    //         view.AddComponentTypes(componentList, view._ignoreComponents, ignoreComponents);

    //         //lets validate that entity type hasn't been attributed
    //         Assert(!view._allComponents.Any(x => x.ID == _entityType.GetHashCode()));

    //         var componentFields = new List<ComponentViewField>();
    //         var typeInfo = type.GetTypeInfo();
    //         foreach (var it in typeInfo.DeclaredFields)
    //         {
    //             var fieldType = it.FieldType;
    //             var pointer = fieldType.IsPointer;
    //             if (pointer && !fieldType.IsGenericType)
    //                 fieldType = fieldType.GetElementType();

    //             if (fieldType.IsValueType)
    //             {
    //                 if (fieldType.IsGenericType)
    //                 {
    //                     var fieldTypeInfo = fieldType.GetTypeInfo();
    //                     var generic = fieldType.GetGenericTypeDefinition().GetTypeInfo();
    //                     if (generic == typeof(Read<>) || generic == typeof(Write<>))
    //                     {
    //                         var writeable = false;
    //                         Assert(!pointer);

    //                         fieldType = fieldTypeInfo.GenericTypeArguments[0];
    //                         var offset = Marshal.OffsetOf(type, it.Name);
    //                         var isEntityField = fieldType == _entityType;
    //                         if (!componentList.Find(fieldType, out var componentType))
    //                             Assert(false);

    //                         if (!isEntityField)
    //                         {
    //                             Assert(!ignoreComponents.Contains(fieldType));
    //                             if (!anyComponents.Contains(fieldType))
    //                                 if (!view._hasComponents.Add(componentType))
    //                                     Assert(false);
    //                             writeable = generic == typeof(Write<>);
    //                             componentFields.Add(new ComponentViewField(true, writeable, false, ComponentFieldType.ComponentType, componentType, offset.ToInt32()));
    //                         }
    //                         else
    //                         {
    //                             Assert(!view.HasEntityField);
    //                             componentFields.Add(view._entityField = new ComponentViewField(true, writeable, false, ComponentFieldType.Entity, componentType, offset.ToInt32()));
    //                         }
    //                     }
    //                 }
    //                 else
    //                 {
    //                     var offset = Marshal.OffsetOf(type, it.Name);
    //                     if (fieldType == typeof(int) && string.Compare(it.Name, "length", true) == 0)
    //                     {
    //                         Assert(!fieldType.IsPointer);
    //                         componentFields.Add(view._lengthField = new ComponentViewField(false, false, false, ComponentFieldType.Length, default, offset.ToInt32()));
    //                     }
    //                     else
    //                     {
    //                         if (!componentList.Find(fieldType, out var componentType))
    //                             continue;

    //                         var writeable = false;
    //                         var isEntityField = fieldType == _entityType;
    //                         if (!isEntityField)
    //                         {
    //                             writeable = !it.IsInitOnly || pointer;
    //                             Assert(!ignoreComponents.Contains(fieldType));
    //                             if (!anyComponents.Contains(fieldType))
    //                                 Assert(view._hasComponents.Add(componentType));
    //                             componentFields.Add(new ComponentViewField(false, writeable, pointer, ComponentFieldType.ComponentType, componentType, offset.ToInt32()));
    //                         }
    //                         else
    //                         {
    //                             Assert(!pointer);
    //                             Assert(!view.HasEntityField);
    //                             componentFields.Add(view._entityField = new ComponentViewField(false, writeable, pointer, ComponentFieldType.Entity, componentType, offset.ToInt32()));
    //                         }
    //                     }
    //                 }
    //             }
    //         }

    //         //lets make sure we aren't mixing and matching field types
    //         var totalArrays = componentFields
    //             .Where(x => x.FieldType != ComponentFieldType.Length)
    //             .Count(x => x.IsArray);

    //         var totalInstances = componentFields
    //             .Where(x => x.FieldType != ComponentFieldType.Length)
    //             .Count(x => !x.IsArray);

    //         Assert((totalArrays == 0 && totalInstances != 0) || (totalArrays != 0 && totalInstances == 0));

    //         view._fields = componentFields;
    //         view._chunkView = totalArrays > 0;

    //         return view;
    //     }
    // }

    // internal sealed class ComponentViewField
    // {
    //     public readonly bool IsArray;
    //     public readonly bool IsWriteable;
    //     public readonly bool IsPointer;
    //     public readonly ComponentFieldType FieldType;
    //     public readonly ComponentType ComponentType;
    //     public readonly int OffsetInStruct;

    //     public ComponentViewField(bool isArray, bool isWriteable, bool isPointer, ComponentFieldType fieldType, in ComponentType componentType, int offsetInStruct)
    //     {
    //         IsArray = isArray;
    //         FieldType = fieldType;
    //         IsWriteable = isWriteable;
    //         IsPointer = isPointer;
    //         ComponentType = componentType;
    //         OffsetInStruct = offsetInStruct;
    //     }

    //     internal unsafe int GetEntityFromStruct<T>(T* t)
    //         where T : unmanaged
    //     {
    //         Assert.EqualTo(IsArray, false);
    //         Assert.EqualTo(FieldType, ComponentFieldType.Entity);

    //         var ptr = (byte*)t;
    //         ptr += OffsetInStruct;
    //         return *(int*)ptr;
    //     }

    //     internal unsafe void UpdateFromStruct<T>(T* t, EntitySpec spec, EntityChunk chunk, int index)
    //         where T : unmanaged
    //     {
    //         Assert.EqualTo(IsArray, false);
    //         Assert.EqualTo(IsWriteable, true);
    //         Assert.EqualTo(FieldType, ComponentFieldType.ComponentType);

    //         var arrIndex = spec.GetComponentIndex(ComponentType);
    //         if (arrIndex == -1)
    //             return;

    //         using var arrRef = chunk.GetReadComponentArray(arrIndex);
    //         var arr = arrRef.Array;

    //         var ptr = (byte*)t;
    //         ptr += OffsetInStruct;

    //         var rawData = (byte*)arr._rawData;
    //         var elementSize = arr.ElementSize;
    //         rawData += index * elementSize;

    //         for (var k = 0; k < elementSize; k++)
    //             rawData[k] = ptr[k];
    //     }

    //     internal unsafe void CopyToStruct<T>(T* t, EntitySpec spec, EntityChunk chunk, int index)
    //         where T : unmanaged
    //     {
    //         Assert.EqualTo(IsArray, false);


    //         var ptr = (byte*)t;
    //         ptr += OffsetInStruct;

    //         ComponentDataArray arr;
    //         if (FieldType == ComponentFieldType.Entity)
    //         {
    //             using var arrRef = chunk.GetReadComponent<Entity>();
    //             arr = arrRef.Array;
    //         }
    //         else if (FieldType == ComponentFieldType.Length)
    //         {
    //             return;
    //         }
    //         else
    //         {
    //             var arrIndex = spec.GetComponentIndex(ComponentType);
    //             if (arrIndex == -1)
    //                 return;

    //             using var arrRef = chunk.GetReadComponentArray(arrIndex);
    //             arr = arrRef.Array;
    //         }

    //         var rawData = (byte*)arr._rawData;
    //         var elementSize = arr.ElementSize;
    //         rawData += index * elementSize;

    //         if (IsPointer)
    //         {
    //             var p = (void**)ptr;
    //             *p = rawData;
    //         }
    //         else
    //         {
    //             for (var k = 0; k < elementSize; k++)
    //                 ptr[k] = rawData[k];
    //         }
    //     }

    //     internal unsafe void CopyPtrsToStruct<T>(T* t, EntitySpec spec, EntityChunk chunk)
    //         where T : unmanaged
    //     {
    //         Assert.EqualTo(IsArray || FieldType == ComponentFieldType.Length, true);

    //         var ptr = (byte*)t;
    //         ptr += OffsetInStruct;

    //         ComponentDataArray arr;
    //         if (FieldType == ComponentFieldType.Entity)
    //         {
    //             using var arrRef = chunk.GetReadComponent<Entity>();
    //             arr = arrRef.Array;
    //         }
    //         else if (FieldType == ComponentFieldType.Length)
    //         {
    //             var chunkLengthPtr = (int*)ptr;
    //             *chunkLengthPtr = chunk.Count;
    //             return;
    //         }
    //         else
    //         {
    //             var arrIndex = spec.GetComponentIndex(ComponentType);
    //             if (arrIndex == -1)
    //                 return;

    //             using var arrRef = chunk.GetReadComponentArray(arrIndex);
    //             arr = arrRef.Array;
    //         }

    //         var p = (void**)ptr;
    //         *p = arr._rawData;
    //     }
    // }
}