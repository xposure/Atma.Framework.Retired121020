namespace Atma.Entities
{
    using static Atma.Debug;

    public struct Entity //: IEquatable<Entity2>, IEquatable<int>
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

        public uint ID;
        public uint Key;

        public bool IsValid => ID > 0;
        //public uint Version => ID >> 24;

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

        public Entity(uint id, int specIndex, int chunkIndex, int index)
        {
            Assert(specIndex < SPEC_MAX);
            Assert(chunkIndex < CHUNK_MAX);
            Assert(index < ENTITY_MAX);

            ID = id;//(uint)(id & 0xfffff) | (uint)version << 24;

            Key = (uint)(specIndex << SPEC_SHIFT) +
                  (uint)((chunkIndex << CHUNK_SHIFT) & CHUNK_MASK) +
                  (uint)(index & ENTITY_MASK);

            Assert(SpecIndex == specIndex);
            Assert(ChunkIndex == chunkIndex);
            Assert(Index == index);
        }

        // public override int GetHashCode() => ID;

        // public bool Equals(Entity2 other) => ID == other.ID;

        // public bool Equals(int other) => ID == other;

        // public static bool operator ==(Entity2 left, Entity2 right) => left.ID == right.ID && left.Key == right.Key;
        // public static bool operator !=(Entity2 left, Entity2 right) => left.ID != right.ID && left.Key == right.Key;
        // public static bool operator ==(Entity2 left, int right) => left.ID == right;
        // public static bool operator !=(Entity2 left, int right) => left.ID != right;
        // public static bool operator ==(int left, Entity2 right) => left == right.ID;
        // public static bool operator !=(int left, Entity2 right) => left != right.ID;

        public override string ToString() => $"{{ Spec: {SpecIndex}, Chunk: {ChunkIndex}, Index: {Index}, ID: {ID}";
    }
}
