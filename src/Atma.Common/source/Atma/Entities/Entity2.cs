// namespace Atma.Entities
// {
//     using System;

//     using static Atma.Debug;

//     public ref struct Entity2 //: IEquatable<Entity2>, IEquatable<int>
//     {
//         //we have 32 bits to play with here
//         public const int ARCHETYPE_BITS = 12;
//         public const int CHUNK_BITS = 10;
//         public const int ENTITY_BITS = 10;

//         public const int ARCHETYPE_MAX = 1 << ARCHETYPE_BITS;
//         public const int CHUNK_MAX = 1 << CHUNK_BITS;
//         public const int ENTITY_MAX = 1 << ENTITY_BITS;

//         public const int ARCHETYPE_SHIFT = ENTITY_BITS + CHUNK_BITS;
//         public const int CHUNK_SHIFT = CHUNK_BITS;

//         public const uint ENTITY_MASK = (1 << ENTITY_BITS) - 1;
//         public const uint CHUNK_MASK = ((1 << CHUNK_SHIFT) - 1) << ENTITY_BITS;
//         public const uint ARCHETYPE_MASK = ((1 << ARCHETYPE_SHIFT) - 1) ^ ENTITY_MASK ^ CHUNK_MASK;
//         public const uint ARCHETYPECHUNK_MASK = ARCHETYPE_MASK + CHUNK_MASK;

//         public int ID;
//         public bool IsValid => ID > 0;

//         public uint Key;

//         //TODO: should be uint
//         public int ArchetypeIndex => (int)(Key >> ARCHETYPE_SHIFT); //no need to mask

//         public int ChunkIndex => (int)((Key & CHUNK_MASK) >> CHUNK_SHIFT);

//         public int Index
//         {
//             get => (int)(Key & ENTITY_MASK);
//             set
//             {
//                 var index = (uint)(value & ENTITY_MASK);
//                 Key = (Key & ARCHETYPECHUNK_MASK) | index;
//             }
//         }

//         public Entity2(int id, int archetypeIndex, int chunkIndex, int index)
//         {
//             Assert(archetypeIndex < ARCHETYPE_MAX);
//             Assert(chunkIndex < CHUNK_MAX);
//             Assert(index < ENTITY_MAX);

//             ID = id;
//             Key = (uint)(archetypeIndex << ARCHETYPE_SHIFT) +
//                   (uint)((chunkIndex << CHUNK_SHIFT) & CHUNK_MASK) +
//                   (uint)(index & ENTITY_MASK);

//             Assert(ArchetypeIndex == archetypeIndex);
//             Assert(ChunkIndex == chunkIndex);
//             Assert(Index == index);
//         }

//         public override int GetHashCode() => ID;

//         public bool Equals(Entity2 other) => ID == other.ID;

//         public bool Equals(int other) => ID == other;

//         public static bool operator ==(Entity2 left, Entity2 right) => left.ID == right.ID && left.Key == right.Key;
//         public static bool operator !=(Entity2 left, Entity2 right) => left.ID != right.ID && left.Key == right.Key;
//         public static bool operator ==(Entity2 left, int right) => left.ID == right;
//         public static bool operator !=(Entity2 left, int right) => left.ID != right;
//         public static bool operator ==(int left, Entity2 right) => left == right.ID;
//         public static bool operator !=(int left, Entity2 right) => left != right.ID;

//         public override string ToString() => $"{{ Spec: {ArchetypeIndex}, Chunk: {ChunkIndex}, Index: {Index}, ID: {ID}";
//     }
// }
