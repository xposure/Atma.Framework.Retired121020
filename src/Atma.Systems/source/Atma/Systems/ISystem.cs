namespace Atma.Systems
{
    using System.Collections.Generic;
    using Atma.Entities;

    public interface ISystem
    {
        void Init();
        void Tick(SystemManager systemManager, EntityManager entityManager);
        IEnumerable<Dependency> Dependencies { get; }
        int Priority { get; }
        //string Group { get; }
        string Name { get; }

    }
}