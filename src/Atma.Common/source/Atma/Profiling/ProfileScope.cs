namespace Atma.Profiling
{
    public ref struct ProfileScope
    {
        public IProfileFrame Frame;
        public double Start, End;
        public int ID;
        public int Parent;
        public int Depth;

        public ProfileScope(IProfileFrame frame, int id, int parent, int depth)
        {
            Frame = frame;
            End = Start = Frame.Timer;
            ID = id;
            Parent = parent;
            Depth = depth;
        }

        public void Dispose()
        {
            End = Frame.Timer;
            Frame.Pop(this);
        }
    }
}
