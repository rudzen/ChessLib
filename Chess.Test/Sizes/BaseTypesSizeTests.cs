using FluentAssertions;
using Xunit;

namespace Chess.Test.Sizes
{
    public class BaseTypesSizeTests
    {
        [Fact]
        public unsafe void MoveSizeTest()
        {
            const int expected = 2;
            var actual = sizeof(Rudz.Chess.Types.Move);
            actual.Should().Be(expected);
        }

        [Fact]
        public unsafe void PieceSizeTest()
        {
            const int expected = 1;
            var actual = sizeof(Rudz.Chess.Types.Piece);
            actual.Should().Be(expected);
        }
        
        [Fact]
        public unsafe void SquareSizeTest()
        {
            const int expected = 4;
            var actual = sizeof(Rudz.Chess.Types.Square);
            actual.Should().Be(expected);
        }
        
        [Fact]
        public unsafe void RankSizeTest()
        {
            const int expected = 4;
            var actual = sizeof(Rudz.Chess.Types.Rank);
            actual.Should().Be(expected);
        }
    }
}