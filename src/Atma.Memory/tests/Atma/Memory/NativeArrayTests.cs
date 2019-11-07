namespace Atma.Memory
{
    using Shouldly;

    public class NativeArrayTests
    {
        private struct Data
        {
            public int x;
            public int y;
            public byte b;
        }

        public void ShouldAdd()
        {
            using var x = new NativeList<Data>(Allocator.Temp);

            x.Add(new Data() { x = 10, y = 11, b = 12 });
            x.Length.ShouldBe(1);
            x.Add(new Data() { x = 20, y = 21, b = 22 });
            x.Length.ShouldBe(2);
            x.Add(new Data() { x = 30, y = 31, b = 32 });
            x.Length.ShouldBe(3);

            x[0].x.ShouldBe(10);
            x[0].y.ShouldBe(11);
            x[0].b.ShouldBe(12);

            x[1].x.ShouldBe(20);
            x[1].y.ShouldBe(21);
            x[1].b.ShouldBe(22);

            x[2].x.ShouldBe(30);
            x[2].y.ShouldBe(31);
            x[2].b.ShouldBe(32);

            x.RemoveAt(1);
            x.Length.ShouldBe(2);

            x[0].x.ShouldBe(10);
            x[0].y.ShouldBe(11);
            x[0].b.ShouldBe(12);

            x[1].x.ShouldBe(30);
            x[1].y.ShouldBe(31);
            x[1].b.ShouldBe(32);

        }

        public void ShouldResize()
        {
            using var x = new NativeList<Data>(Allocator.Temp);

            var maxLength = x.MaxLength;
            for (var i = 0; i < 1024; i++)
            {
                var shouldGrow = x.Length == x.MaxLength;
                x.Add(new Data() { x = 10 + i, y = 11 + i, b = 0x5a });
                x.Length.ShouldBe(i + 1);
                if (shouldGrow)
                {
                    x.MaxLength.ShouldNotBe(maxLength);
                    maxLength = x.MaxLength;
                }
                else
                {
                    x.MaxLength.ShouldBe(maxLength);
                }
            }

            for (var i = 0; i < x.Length; i++)
            {
                x[i].x.ShouldBe(10 + i);
                x[i].y.ShouldBe(11 + i);
                x[i].b.ShouldBe(0x5a);
            }

        }
    }
}
