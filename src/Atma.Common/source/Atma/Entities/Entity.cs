namespace Atma.Entities
{
    using System;

    using static Atma.Debug;

    public struct Entity : IEquatable<Entity>, IEquatable<int>
    {
        //we have 32 bits to play with here
        public const int SPEC_BITS = 12;
        public const int CHUNK_BITS = 10;
        public const int ENTITY_BITS = 10;

        public const int SPEC_MAX = 1 << SPEC_BITS;
        public const int CHUNK_MAX = 1 << CHUNK_BITS;
        public const int ENTITY_MAX = 1 << ENTITY_BITS;

        public const int SPEC_SHIFT = ENTITY_BITS + CHUNK_BITS;
        public const int CHUNK_SHIFT = CHUNK_BITS;

        public const uint ENTITY_MASK = (1 << ENTITY_BITS) - 1;
        public const uint CHUNK_MASK = ((1 << CHUNK_SHIFT) - 1) << ENTITY_BITS;
        public const uint SPEC_MASK = ((1 << SPEC_SHIFT) - 1) ^ ENTITY_MASK ^ CHUNK_MASK;
        public const uint SPECCHUNK_MASK = SPEC_MASK + CHUNK_MASK;

        public int ID;
        public bool IsValid => ID > 0;

        public uint Key;

        //TODO: should be uint
        public int SpecIndex => (int)(Key >> SPEC_SHIFT); //no need to mask

        public int ChunkIndex => (int)((Key & CHUNK_MASK) >> CHUNK_SHIFT);

        public int Index
        {
            get => (int)(Key & ENTITY_MASK);
            set
            {
                var index = (uint)(value & ENTITY_MASK);
                Key = (Key & SPECCHUNK_MASK) | index;
            }
        }

        public Entity(int id, int archetypeIndex, int chunkIndex, int index)
        {
            Assert(archetypeIndex < SPEC_MAX);
            Assert(chunkIndex < CHUNK_MAX);
            Assert(index < ENTITY_MAX);

            ID = id;
            Key = (uint)(archetypeIndex << SPEC_SHIFT) +
                  (uint)((chunkIndex << CHUNK_SHIFT) & CHUNK_MASK) +
                  (uint)(index & ENTITY_MASK);

            Assert(SpecIndex == archetypeIndex);
            Assert(ChunkIndex == chunkIndex);
            Assert(Index == index);
        }

        //public static implicit operator int(Entity v) => v.ID;

        //public static implicit operator Entity(int id) => new Entity(id);

        public override int GetHashCode() => ID;

        public override bool Equals(object obj) => (obj is Entity entity) && ID == entity.ID;

        public bool Equals(Entity other) => ID == other.ID;

        public bool Equals(int other) => ID == other;

        public override string ToString() => ID.ToString();
    }
}
