namespace Atma.Profiling
{
    public interface IProfileFrame
    {
        double End { get; }
        double Start { get; }
        double Timer { get; }


        ProfileScope Begin(string name);
        void Pop(in ProfileScope scope);
    }
}
