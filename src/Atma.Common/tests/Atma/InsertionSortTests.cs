namespace Atma
{
    using Xunit;
    using Shouldly;
    using System;

    public unsafe class InsertionSortTests
    {

        [Fact]
        public void ShouldInsertionSort()
        {
            //act
            Span<int> data = stackalloc int[] { -10, 333, 1000, -20, 444 };

            //arrange
            data.InsertionSort();

            //assert
            var arr = data.ToArray();
            arr.ShouldBe(new int[] { -20, -10, 333, 444, 1000 });
        }

        [Fact]
        public void ShouldSort()
        {
            //act
            Span<int> data = stackalloc int[] { -10, 333, 1000, -20, 444 };

            //arrange
            data.InsertionSort();

            //assert
            var arr = data.ToArray();
            arr.ShouldBe(new int[] { -20, -10, 333, 444, 1000 });
        }


    }
}