namespace Atma.Memory
{
    using System;
    using Divergic.Logging.Xunit;
    using Microsoft.Extensions.Logging;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public unsafe class NativeSliceTests
    {
        private struct Data
        {
            public int x;
            public int y;
            public float b;
        }

        private readonly ILoggerFactory _logFactory;

        public NativeSliceTests(ITestOutputHelper output)
        {
            _logFactory = LogFactory.Create(output);
        }

        [Fact]
        public void ShoudMatchAll()
        {
            //arrange
            var data = stackalloc int[128];
            var span = new Span<int>(data, 128);

            //act
            for (var i = 0; i < span.Length; i++)
                span[i] = i;

            var computedData = span.ToArray();

            //assert
            var slice = new NativeSlice<int>(data, 128);
            var sliceData = slice.ToArray();

            sliceData.ShouldBe(computedData);


        }

        [Fact]
        public void ShoudMatchFromStart()
        {
            //arrange
            var data = stackalloc int[128];
            var span = new Span<int>(data, 128);

            //act
            for (var i = 0; i < span.Length; i++)
                span[i] = i;

            var computedData = span.Slice(16).ToArray();

            //assert
            var slice = new NativeSlice<int>(data, 128);
            var sliceData = slice.Slice(16).ToArray();

            sliceData.ShouldBe(computedData);
        }

        [Fact]
        public void ShoudMatchStartWithLength()
        {
            //arrange
            var data = stackalloc int[128];
            var span = new Span<int>(data, 128);

            //act
            for (var i = 0; i < span.Length; i++)
                span[i] = i;

            var computedData = span.Slice(16, 16).ToArray();

            //assert
            var slice = new NativeSlice<int>(data, 128);
            var sliceData = slice.Slice(16, 16).ToArray();

            sliceData.ShouldBe(computedData);
        }

        [Fact]
        public void ShoudMatchAllReadOnly()
        {
            //arrange
            var data = stackalloc int[128];
            var span = new Span<int>(data, 128);

            //act
            for (var i = 0; i < span.Length; i++)
                span[i] = i;

            var computedData = span.ToArray();

            //assert
            var slice = new NativeReadOnlySlice<int>(data, 128);
            var sliceData = slice.ToArray();

            sliceData.ShouldBe(computedData);


        }

        [Fact]
        public void ShoudMatchFromStartReadOnly()
        {
            //arrange
            var data = stackalloc int[128];
            var span = new Span<int>(data, 128);

            //act
            for (var i = 0; i < span.Length; i++)
                span[i] = i;

            var computedData = span.Slice(16).ToArray();

            //assert
            var slice = new NativeReadOnlySlice<int>(data, 128);
            var sliceData = slice.Slice(16).ToArray();

            sliceData.ShouldBe(computedData);
        }

        [Fact]
        public void ShoudMatchStartWithLengthReadOnly()
        {
            //arrange
            var data = stackalloc int[128];
            var span = new Span<int>(data, 128);

            //act
            for (var i = 0; i < span.Length; i++)
                span[i] = i;

            var computedData = span.Slice(16, 16).ToArray();

            //assert
            var slice = new NativeReadOnlySlice<int>(data, 128);
            var sliceData = slice.Slice(16, 16).ToArray();

            sliceData.ShouldBe(computedData);
        }

    }
}