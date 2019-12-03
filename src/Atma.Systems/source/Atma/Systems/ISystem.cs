namespace Atma.Systems
{
    using System.Collections.Generic;
    using Atma.Entities;
    using Microsoft.Extensions.Logging;

    public interface ISystem
    {
        void Init();
        void Tick(SystemManager systemManager, EntityManager entityManager);
        IEnumerable<Dependency> Dependencies { get; }
        int Priority { get; }
        //string Group { get; }
        string Name { get; }

        bool Disabled { get; set; }
    }

    // public abstract class AbstractSystem : ISystem
    // {
    //     protected readonly ILoggerFactory _logFactory;
    //     protected readonly ILogger _logger;

    //     protected AbstractSystem(ILoggerFactory logFactory, string name, int priority, string prefix = "System")
    //     {
    //         _logFactory = logFactory;
    //         _logger = _logFactory.CreateLogger($"{prefix}:{name}");
    //     }
    // }
}