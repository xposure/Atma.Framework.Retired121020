namespace Atma.Entities
{
    using System;
    using Atma;
    using Atma.Entities;
    using Atma.Memory;
    public readonly partial struct EntitySpec
    {
        public static EntitySpec Create<T0>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type });
        public static EntitySpec Create<T0, T1>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type });
        public static EntitySpec Create<T0, T1, T2>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type });
        public static EntitySpec Create<T0, T1, T2, T3>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type });
        public static EntitySpec Create<T0, T1, T2, T3, T4>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type });
        public static EntitySpec Create<T0, T1, T2, T3, T4, T5>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type });
        public static EntitySpec Create<T0, T1, T2, T3, T4, T5, T6>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type });
        public static EntitySpec Create<T0, T1, T2, T3, T4, T5, T6, T7>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type });
        public static EntitySpec Create<T0, T1, T2, T3, T4, T5, T6, T7, T8>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type });
        public static EntitySpec Create<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type, ComponentType<T9>.Type });
        public static EntitySpec Create<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type, ComponentType<T9>.Type, ComponentType<T10>.Type });
        public static EntitySpec Create<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type, ComponentType<T9>.Type, ComponentType<T10>.Type, ComponentType<T11>.Type });
        public static EntitySpec Create<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type, ComponentType<T9>.Type, ComponentType<T10>.Type, ComponentType<T11>.Type, ComponentType<T12>.Type });
        public static EntitySpec Create<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type, ComponentType<T9>.Type, ComponentType<T10>.Type, ComponentType<T11>.Type, ComponentType<T12>.Type, ComponentType<T13>.Type });
        public static EntitySpec Create<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type, ComponentType<T9>.Type, ComponentType<T10>.Type, ComponentType<T11>.Type, ComponentType<T12>.Type, ComponentType<T13>.Type, ComponentType<T14>.Type });
        public static EntitySpec Create<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged where T15 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type, ComponentType<T9>.Type, ComponentType<T10>.Type, ComponentType<T11>.Type, ComponentType<T12>.Type, ComponentType<T13>.Type, ComponentType<T14>.Type, ComponentType<T15>.Type });
        public static EntitySpec Create<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged where T15 : unmanaged where T16 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type, ComponentType<T9>.Type, ComponentType<T10>.Type, ComponentType<T11>.Type, ComponentType<T12>.Type, ComponentType<T13>.Type, ComponentType<T14>.Type, ComponentType<T15>.Type, ComponentType<T16>.Type });
        public static EntitySpec Create<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged where T15 : unmanaged where T16 : unmanaged where T17 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type, ComponentType<T9>.Type, ComponentType<T10>.Type, ComponentType<T11>.Type, ComponentType<T12>.Type, ComponentType<T13>.Type, ComponentType<T14>.Type, ComponentType<T15>.Type, ComponentType<T16>.Type, ComponentType<T17>.Type });
        public static EntitySpec Create<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged where T15 : unmanaged where T16 : unmanaged where T17 : unmanaged where T18 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type, ComponentType<T9>.Type, ComponentType<T10>.Type, ComponentType<T11>.Type, ComponentType<T12>.Type, ComponentType<T13>.Type, ComponentType<T14>.Type, ComponentType<T15>.Type, ComponentType<T16>.Type, ComponentType<T17>.Type, ComponentType<T18>.Type });
        public static EntitySpec Create<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(params IEntitySpecGroup[] groups)
          where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged where T15 : unmanaged where T16 : unmanaged where T17 : unmanaged where T18 : unmanaged where T19 : unmanaged
          => new EntitySpec(groups, new[] { ComponentType<T0>.Type, ComponentType<T1>.Type, ComponentType<T2>.Type, ComponentType<T3>.Type, ComponentType<T4>.Type, ComponentType<T5>.Type, ComponentType<T6>.Type, ComponentType<T7>.Type, ComponentType<T8>.Type, ComponentType<T9>.Type, ComponentType<T10>.Type, ComponentType<T11>.Type, ComponentType<T12>.Type, ComponentType<T13>.Type, ComponentType<T14>.Type, ComponentType<T15>.Type, ComponentType<T16>.Type, ComponentType<T17>.Type, ComponentType<T18>.Type, ComponentType<T19>.Type });
    }
}
