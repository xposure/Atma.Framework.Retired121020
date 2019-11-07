namespace Atma.Profiling
{
    public interface IProfileThread
    {
        IProfileFrame CurrentFrame { get; }
        ProfileScope Begin(string name);
    }
}