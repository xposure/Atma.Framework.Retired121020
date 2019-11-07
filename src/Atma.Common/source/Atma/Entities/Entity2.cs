//namespace Atma.Entities
//{
//    using static Atma.Debug;

//    public struct Entity2// : IEquatable<Entity2>
//    {
//        public const int MAX_ARCHETYPES = 16384; //2^14
//        public const int MAX_ENTITIES_PER_CHUNK = 256; // 2^10
//        public const int MAX_CHUNKS_PER_ARCHETYPE = 1024; //2^8

//        public bool IsValid => ArchetypeIndex > 0;

//        public int Key;

//        public int ArchetypeIndex => Key >> 18;

//        public int ChunkIndex => (Key >> 10) & 0xff;

//        public int Index
//        {
//            get { return Key & 0x3ff; }
//            set { Key = (Key & 0x7ffffc00) + (value & 0x7ff); }
//        }

//        public Entity2(int archetypeIndex, int chunkIndex, int index)
//        {
//            Assert(archetypeIndex < MAX_ARCHETYPES);
//            Assert(chunkIndex < MAX_CHUNKS_PER_ARCHETYPE);
//            Assert(index < MAX_ENTITIES_PER_CHUNK);

//            Key = (archetypeIndex << 18) + (chunkIndex << 8) + index;

//            //Assert(ArchetypeIndex == archetypeIndex);
//            //Assert(ChunkIndex == chunkIndex);
//            //Assert(Index == index);
//        }

//        //public static implicit operator int(Entity v) => v.ID;

//        //public static implicit operator Entity(int id) => new Entity(id);

//        public override int GetHashCode() => Key;

//        public override bool Equals(object obj) => (obj is Entity2 entity) && Key == entity.Key;

//        public bool Equals(Entity2 other) => Key == other.Key;

//        public bool Equals(int other) => Key == other;

//        public override string ToString() => Key.ToString();
//    }
//}
