namespace Atma.Entities
{
    using System;

    [System.Diagnostics.DebuggerStepThrough]
    public struct Position : IEquatable<Position>
    {
        public int X;
        public int Y;

        public Position(int v)
        {
            X = v;
            Y = v;
        }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override int GetHashCode() => HashCode.Combine(X, Y);
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
        public int X;
        public int Y;

        public Velocity(int v)
        {
            X = v;
            Y = v;
        }

        public Velocity(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override int GetHashCode() => HashCode.Combine(X, Y);
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

    public struct GroupA : IEntitySpecGroup
    {
        public int HashCode;

        public override int GetHashCode() => HashCode;
    }

    public struct GroupB : IEntitySpecGroup
    {
        public int HashCode;

        public override int GetHashCode() => HashCode;
    }

}