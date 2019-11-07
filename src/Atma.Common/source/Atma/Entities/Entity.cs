namespace Atma.Entities
{
    using System;

    using static Atma.Debug;

    public struct Entity : IEquatable<Entity>, IEquatable<int>
    {

        public const uint MAX_ARCHETYPES = 16384; //16384 - 2^14
        public const uint MAX_CHUNKS_PER_ARCHETYPE = 1024; //256 - 2^10
        public const uint MAX_ENTITIES_PER_CHUNK = 32; //1024 - 2^8

        public int ID;
        public bool IsValid => ID > 0;

        public uint Key;

        public int ArchetypeIndex => (int)((Key >> 18) & (MAX_ARCHETYPES - 1));

        public int ChunkIndex => (int)((Key >> 8) & (MAX_CHUNKS_PER_ARCHETYPE - 1));

        public int Index
        {
            get => (int)( Key & (MAX_ENTITIES_PER_CHUNK - 1));
            set
            {
                Key &= ((MAX_ENTITIES_PER_CHUNK - 1) ^ 0xfffffff);
                Key |= (uint)value & (MAX_ENTITIES_PER_CHUNK - 1);
            }
        }

        public Entity(int id, int archetypeIndex, int chunkIndex, int index)
        {
            Assert(archetypeIndex < MAX_ARCHETYPES);
            Assert(chunkIndex < MAX_CHUNKS_PER_ARCHETYPE);
            Assert(index < MAX_ENTITIES_PER_CHUNK);

            ID = id;
            Key = ((uint)archetypeIndex << 18) + ((uint)chunkIndex << 8) + (uint)index;

            Assert(ArchetypeIndex == archetypeIndex);
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
