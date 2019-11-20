namespace Atma.Entities
{


    public struct Position
    {
        public float X;
        public float Y;

        public Position(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    public struct Velocity
    {
        public float X;
        public float Y;
        public Velocity(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
    }

}