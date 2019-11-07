using Atma.DI;

namespace Atma.Profiling
{

    [ServiceProvider]
    public interface IProfileService : IService
    {
        IProfileThread AddProfileThread();
        IProfileThread GetProfileThread();

        IProfileThread Current { get; }

        void BeginFrame();
        void EndFrame();
    }
}