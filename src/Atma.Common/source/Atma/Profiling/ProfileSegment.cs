namespace Atma.Profiling
{
    public struct ProfileSegment
    {
        public int ID;
        public int Depth;
        public double Start, End;

        public double Duration => End - Start;
    }
}
