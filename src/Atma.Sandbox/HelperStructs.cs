namespace Atma.Sandbox
{
    using System;
    using Atma.Memory;

    [System.Diagnostics.DebuggerStepThrough]
    public struct Position : IEquatable<Position>
    {
        public float X;
        public float Y;

        public Position(float v)
        {
            X = v;
            Y = v;
        }

        public Position(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override int GetHashCode() => HashCode.Combine(X.GetHashCode(), Y.GetHashCode());
        public override bool Equals(object obj)
        {
            if (obj != null && obj is Position b)
                return Equals(b);

            return false;
        }

        public static bool operator ==(Position a, Position b) => a.Equals(b);
        public static bool operator !=(Position a, Position b) => !a.Equals(b);

        public bool Equals(Position other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        public override string ToString() => $"X: {X}, Y: {Y}";
    }

    [System.Diagnostics.DebuggerStepThrough]
    public struct Velocity : IEquatable<Velocity>
    {
        public float X;
        public float Y;

        public Velocity(float v)
        {
            X = v;
            Y = v;
        }

        public Velocity(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override int GetHashCode() => HashCode.Combine(X.GetHashCode(), Y.GetHashCode());
        public override bool Equals(object obj)
        {
            if (obj != null && obj is Velocity b)
                return Equals(b);

            return false;
        }

        public static bool operator ==(Velocity a, Velocity b) => a.Equals(b);
        public static bool operator !=(Velocity a, Velocity b) => !a.Equals(b);

        public bool Equals(Velocity other)
        {
            return this.X == other.X && this.Y == other.Y;
        }
        public override string ToString() => $"X: {X}, Y: {Y}";
    }

    public unsafe ref struct SystemSpanWithLength
    {
        public float dt;
        public float xyz;
        public IAllocator test;
        public Span<Position> position;
        public ReadOnlySpan<Velocity> velocity;

        public void Execute(int length)
        {
            for (var i = 0; i < length; i++)
            {
                ref var p = ref position[i];
                ref readonly var v = ref velocity[i];
                p.X += v.X * dt;
                p.Y += v.Y * dt;
            }
        }
    }
}