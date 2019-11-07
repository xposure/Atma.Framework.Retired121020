namespace Atma
{
    using Atma.DI;

    [Injectable]
    public interface IService
    {
    }

    public interface IUpdateService : IService
    {
        void Update();
    }

    public interface IInitService : IService
    {
        void Initialize();
    }
}
