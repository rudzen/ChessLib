namespace Chess.Test.Bitboard
{
    using Rudz.Chess.Enums;
    using Rudz.Chess.Types;
    using System.Linq;
    using Xunit;

    public sealed class BitboardDataTests
    {
        [Fact]
        public void AlignedSimplePositiveTest()
        {
            const bool expected = true;

            const ESquare sq1 = ESquare.a1;
            const ESquare sq2 = ESquare.a2;
            const ESquare sq3 = ESquare.a3;

            var actual = BitBoards.Aligned(sq1, sq2, sq3);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AlignedSimpleTotalTest()
        {
            const int expected = 10640;

            var actual = 0;

            foreach (var s1 in BitBoards.AllSquares)
                actual += BitBoards.AllSquares
                    .Sum(s2 => BitBoards.AllSquares
                        .Count(s3 => s1.Aligned(s2, s3)));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AlignedSimpleTotalNoIdenticalsTest()
        {
            const int expected = 9184;

            var actual = 0;

            foreach (var s1 in BitBoards.AllSquares)
                actual += BitBoards.AllSquares
                    .Where(x2 => x2 != s1)
                    .Sum(s2 => BitBoards.AllSquares
                        .Where(x3 => x3 != s2)
                        .Count(s3 => s1.Aligned(s2, s3)));

            Assert.Equal(expected, actual);
        }
    }
}