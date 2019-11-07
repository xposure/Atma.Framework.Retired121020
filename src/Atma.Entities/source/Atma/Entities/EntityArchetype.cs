namespace Atma.Entities
{
    using Atma.Common;
    using Atma.Memory;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using static Atma.Debug;

    public sealed class EntityArchetype //: IEquatable<EntityArchetype>
    {

        private readonly int _id;
        private readonly int _archetypeIndex;
        internal int _entityCount = 0;
        private EntityManager _entityManager;
        //private int _length;


        public int EntitySize { get; }

        public int ID => _id;

        public bool IsValid => _id != 0 && EntitySize != 0;

        public int Index => _archetypeIndex;

        public int Count => _entityCount;

        public int Capacity => _chunks.Count * (int)Entity.ENTITY_MAX;

        public int Free => Capacity - Count;

        public int ChunkCount => _chunks.Count;

        private List<ArchetypeChunk> _chunks = new List<ArchetypeChunk>();
        public IReadOnlyList<ArchetypeChunk> Chunks => _chunks;

        public IEnumerable<ArchetypeChunk> ActiveChunks
        {
            get
            {
                foreach (var chunk in _chunks)
                    if (chunk.Count > 0)
                        yield return chunk;
            }
        }

        public ComponentType[] ComponentTypes { get; }
        internal ComponentTypeInfo[] ComponentTypeInfo { get; }

        internal EntityArchetype(EntityManager entityManager, int archetypeIndex, int id, ComponentTypeInfo[] types)
        {
            _entityManager = entityManager;
            _id = id;
            _archetypeIndex = archetypeIndex;

            ComponentTypes = new ComponentType[types.Length];
            ComponentTypeInfo = types;
            for (var i = 0; i < types.Length; i++)
            {
                var typeInfo = types[i];
                ComponentTypes[i] = typeInfo.ComponentType;
                //make sure the createarray has been initialized
                //this is a bit of a hack since the type is defined in Common
                //and this is needed in Entities
                if (typeInfo.CreateArray == null)
                {
                    var componentDataArrayType = typeof(ComponentDataArray<>).MakeGenericType(typeInfo.Type);
                    typeInfo.CreateArray = Expression.Lambda<Func<IComponentDataArray>>(
                       Expression.New(componentDataArrayType)
                   ).Compile();
                }
            }


            EntitySize = types.Sum(x => x.ComponentType.Size);
        }

        internal EntityArchetype(EntityManager entityManager, int archetypeIndex, int id)
        {
            _entityManager = entityManager;
            _id = id;
            _archetypeIndex = archetypeIndex;

            ComponentTypes = new ComponentType[0];
            ComponentTypeInfo = new ComponentTypeInfo[0];
            EntitySize = 0;
        }

        internal Entity CreateEntityInternal(int id)
        {
            if (Free == 0)
                _chunks.Add(new ArchetypeChunk(this, _chunks.Count, (int)Entity.ENTITY_MAX));

            for (var i = 0; i < _chunks.Count; i++)
            {
                var chunk = _chunks[i];
                if (chunk.Free > 0)
                {
                    var index = chunk.CreateEntityInternal(id);
                    _entityCount++;
                    //break;
                    return new Entity(id, this.Index, chunk.Index, index);
                }
            }

            //should never get here
            Assert(false);
            return default;
        }

        internal void CreateEntity(EntityPool entityPool, in NativeSlice<int> entities)
        {
            while (Free < entities.Length)
                _chunks.Add(new ArchetypeChunk(this, _chunks.Count, (int)Entity.ENTITY_MAX));

            var index = 0;
            for (var i = 0; i < _chunks.Count; i++)
            {
                var chunk = _chunks[i];
                if (chunk.Free > 0)
                    chunk.CreateEntity(entityPool, entities, ref index);

                if (index >= entities.Length)
                    break;
            }

            _entityCount += entities.Length;
        }

        internal void DestroyEntity(EntityPool pool, int entity, ArchetypeChunk chunk, int index)
        {
            chunk.DeleteIndex(pool, index, true);
            _entityCount--;
        }

        //internal void DestroyEntity(EntityPool pool, in Entity entity, bool clearToZero = true)
        //{
        //    var chunk = _chunks[entity.ChunkIndex];
        //    chunk.DeleteIndex(pool, entity.Index, clearToZero);
        //    _entityCount--;
        //}


        public bool HasComponentType(ComponentType type)
        {
            for (var i = 0; i < ComponentTypes.Length; i++)
                if (ComponentTypes[i].ID == type.ID)
                    return true;

            return false;
        }

        public bool HasComponentType(Type type)
        {
            var id = type.GetHashCode();
            for (var i = 0; i < ComponentTypes.Length; i++)
                if (ComponentTypes[i].ID == id)
                    return true;

            return false;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EntityArchetype other)) return false;
            return this._id == other._id;
        }

        public override int GetHashCode() => _id;

        public bool Equals(EntityArchetype other)
        {
            return _id == other._id;
        }

        internal static int GetId(Span<ComponentType> types)
        {
            Assert(types.Length > 0);

            types.Sort();

            var hashCode = new HashCode();
            for (var i = 0; i < types.Length; i++)
                hashCode.Add(types[i]);

            return hashCode.ToHashCode();
        }

        public static IEnumerable<ComponentType> FindMatches(EntityArchetype a, EntityArchetype b)
        {
            var i0 = 0;
            var i1 = 0;

            while (i0 < a.ComponentTypes.Length && i1 < b.ComponentTypes.Length)
            {
                var aType = a.ComponentTypes[i0];
                var bType = b.ComponentTypes[i1];
                if (aType.ID > bType.ID) i1++;
                else if (bType.ID > aType.ID) i0++;
                else
                {
                    yield return aType;
                    i0++;
                    i1++;
                }
            }
        }

        public static bool HasAll(EntityArchetype a, EntityArchetype b/*, bool debug = false*/)
        {
            //all the debug code is left over for future need
            //there was an issue where Entity type was always list in the array
            //when running without the debugger attached, talk about a fun thing to
            //debug.......
            //oh and it only happened when I had my sample PlayerSystem enabled

            var entity = typeof(Entity).GetHashCode();
            var i0 = 0;
            var i1 = 0;

            while (i0 < a.ComponentTypes.Length && i1 < b.ComponentTypes.Length)
            {
                var aType = a.ComponentTypes[i0];
                var bType = b.ComponentTypes[i1];
                if (aType.ID == entity)
                {
                    throw new Exception("You can not create an archetype with Entity, this is assumed.");
                    //i0++;
                    //if (debug) Console.WriteLine($"aType was entity ... {aType.ClrType.Name}:{bType.ClrType.Name}");
                }
                else if (bType.ID == entity)
                {
                    i1++;
                    //if (debug) Console.WriteLine($"bType was entity ... {aType.ClrType.Name}:{bType.ClrType.Name}");
                }
                else if (aType.ID > bType.ID)
                {
                    //if (debug) Console.WriteLine($"aType was > bType, exiting ... {aType.ClrType.Name}:{bType.ClrType.Name}");
                    return false; // i1++;
                }
                else if (bType.ID > aType.ID)
                {
                    //if (debug) Console.WriteLine($"bType was > aType, advancing ... {aType.ClrType.Name}:{bType.ClrType.Name}");
                    i0++;
                }
                else
                {
                    i0++;
                    i1++;
                    //if (debug) Console.WriteLine($"aType == bType ... {aType.ClrType.Name}:{bType.ClrType.Name}");
                }
            }

            //entity should never be in aType, but if its the last element of bType
            //we need to check and advance i1 pointer to move past it since its assumed
            //to always exist
            if (i1 < b.ComponentTypes.Length && b.ComponentTypes[i1].ID == entity)
                i1++;

            //if(debug) Console.WriteLine($"bSeek {i1}, len: {b.ComponentTypes.Length}");

            return i1 == b.ComponentTypes.Length;
        }

        public static bool HasAny(EntityArchetype a, EntityArchetype b)
        {
            var i0 = 0;
            var i1 = 0;

            while (i0 < a.ComponentTypes.Length && i1 < b.ComponentTypes.Length)
            {
                var aType = a.ComponentTypes[i0];
                var bType = b.ComponentTypes[i1];
                if (aType.ID > bType.ID) i1++;
                else if (bType.ID > aType.ID) i0++;
                else
                {
                    return true;
                }
            }

            return false;
        }

        internal int GetComponentIndex(in ComponentType type)
        {
            for (var i = 0; i < ComponentTypes.Length; i++)
                if (ComponentTypes[i].ID == type.ID)
                    return i;

            return -1;
        }

        internal int GetComponentIndex(Type type)
        {
            for (var i = 0; i < ComponentTypes.Length; i++)
            {
                if (ComponentTypes[i].ID == type.GetHashCode())
                    return i;
            }

            return -1;
        }

        public int GetComponentIndex<T>()
            where T : unmanaged
        {
            return GetComponentIndex(typeof(T));
        }

        public static void MoveEntity(EntityPool pool, int entity, ArchetypeChunk chunkSrc, int indexSrc, EntityArchetype archetype, EntityArchetype newArchetype)
        {
            var i0 = 0;
            var i1 = 0;

            var dstEntity = newArchetype.CreateEntityInternal(entity);
            var chunkDst = newArchetype.Chunks[dstEntity.ChunkIndex];
            var indexDst = dstEntity.Index;

            while (i0 < archetype.ComponentTypes.Length && i1 < newArchetype.ComponentTypes.Length)
            {
                var aType = archetype.ComponentTypes[i0];
                var bType = newArchetype.ComponentTypes[i1];
                if (aType.ID > bType.ID) i1++;
                else if (bType.ID > aType.ID) i0++;
                else
                {
                    using var arrSrc = chunkSrc.GetReadComponentArray(i0);
                    using var arrDst = chunkDst.GetWriteComponentArray(i1);

                    arrSrc.Array.CopyTo(indexSrc, arrDst.Array, indexDst);

                    i0++;
                    i1++;
                }
            }

            chunkSrc.DeleteIndex(pool, indexSrc, true);
            archetype._entityCount--; //need to do house keeping
            pool[entity] = new Entity(entity, newArchetype.Index, chunkDst.Index, indexDst);
        }
    }
}
