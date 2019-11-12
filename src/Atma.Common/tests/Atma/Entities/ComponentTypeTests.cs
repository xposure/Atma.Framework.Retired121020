namespace Atma.Entities
{
    using Xunit;
    using Shouldly;
    using Atma.Entities;

    public class ComponentTypeTests
    {
        public struct Position
        {
            public float x;
            public float y;
            public Position(float x, float y)
            {
                this.x = x;
                this.y = y;
            }
        }

        public struct Velocity
        {
            public float x;
            public float y;
            public Velocity(float x, float y)
            {
                this.x = x;
                this.y = y;
            }
        }

        public struct float4
        {
            public float x;
            public float y;
            public float z;
            public float w;
            public float4(float x, float y, float z, float w)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.w = w;
            }
        }
        public struct Color
        {
            public float a;
            public float r;
            public float g;
            public float b;
            public Color(float a, float r, float g, float b)
            {
                this.a = a;
                this.r = r;
                this.g = g;
                this.b = b;
            }
        }

        [Fact]
        public void ShouldCalculateSameId()
        {
            //arrange
            var componentTypes = new[] {
                ComponentType<Position>.Type,
                ComponentType<Velocity>.Type,
                ComponentType<float4>.Type,
                ComponentType<Color>.Type
            };

            //act
            var id = ComponentType.CalculateId(componentTypes);
            var ct0 = ComponentType.CalculateId(new[] { componentTypes[0], componentTypes[2], componentTypes[1], componentTypes[3] });
            var ct1 = ComponentType.CalculateId(new[] { componentTypes[2], componentTypes[0], componentTypes[1], componentTypes[3] });
            var ct2 = ComponentType.CalculateId(new[] { componentTypes[2], componentTypes[0], componentTypes[3], componentTypes[1] });
            var ct3 = ComponentType.CalculateId(new[] { componentTypes[2], componentTypes[3], componentTypes[1], componentTypes[0] });
            var ct4 = ComponentType.CalculateId(new[] { componentTypes[1], componentTypes[0], componentTypes[2], componentTypes[3] });

            //assert
            ct0.ShouldBe(id);
            ct1.ShouldBe(id);
            ct2.ShouldBe(id);
            ct3.ShouldBe(id);
            ct4.ShouldBe(id);

        }
    }
}