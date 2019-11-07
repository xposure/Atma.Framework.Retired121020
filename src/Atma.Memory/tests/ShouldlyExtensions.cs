namespace Atma
{
    using Shouldly;

    public static class ShouldlyExtensions
    {
        public static void ShouldBe(this byte b, byte value)
        {
            var r = (int)b;
            r.ShouldBe(value);
        }
    }
}
