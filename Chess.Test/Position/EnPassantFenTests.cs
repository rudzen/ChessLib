using FluentAssertions;
using Rudz.Chess.Factories;
using Xunit;

namespace Chess.Test.Position
{
    public sealed class EnPassantFenTests
    {
        [Theory]
        [ClassData(typeof(EnPassantTestData))]
        public void ValidateEnPassantSquareFen(string fen, Rudz.Chess.Types.Square expected)
        {
            var game = GameFactory.Create(fen);
            var actual = game.Pos.EnPassantSquare;
            actual.Should().Be(expected);
        }
    }
}
