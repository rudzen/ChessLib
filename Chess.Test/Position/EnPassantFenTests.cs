using Rudz.Chess.Enums;
using Rudz.Chess.Factories;
using Xunit;

namespace Chess.Test.Position
{
    public sealed class EnPassantFenTests
    {
        [Theory]
        [InlineData("rnbkqbnr/pp1pp1pp/5p2/2pP4/8/8/PPP1PPPP/RNBKQBNR w KQkq c6 0 1", Squares.c6)]   // valid
        [InlineData("rnbkqbnr/pp1pp1pp/5p2/2pP4/8/8/PPP1PPPP/RNBKQBNR w KQkq c7 0 1", Squares.none)] // invalid rank
        [InlineData("rnbkqbnr/pp1pp1pp/5p2/2pP4/8/8/PPP1PPPP/RNBKQBNR w KQkq - 0 1" , Squares.none)] // no square set
        [InlineData("rnbkqbnr/pp1pp1pp/5p2/2pP4/8/8/PPP1PPPP/RNBKQBNR w KQkq c 0 1" , Squares.none)] // only file set
        [InlineData("rnbqkbnr/pppppp2/7p/8/3PP1pP/5P2/PPP3P1/RNBQKBNR b KQkq h3 0 1", Squares.h3)]   // valid
        [InlineData("rnbqkbnr/pppppp2/7p/8/3PP1pP/5P2/PPP3P1/RNBQKBNR b KQkq h4 0 1", Squares.none)] // invalid rank
        [InlineData("rnbqkbnr/pppppp2/7p/8/3PP1pP/5P2/PPP3P1/RNBQKBNR b KQkq - 0 1" , Squares.none)] // no square set
        [InlineData("rnbqkbnr/pppppp2/7p/8/3PP1pP/5P2/PPP3P1/RNBQKBNR b KQkq -- 0 1", Squares.none)] // invalid format
        [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq c6 0 1"     , Squares.none)] // start pos with ep square
        [InlineData("rnbqkbnr/2pppp2/p6p/1p2PB2/3P2pP/5P2/PPP3P1/RNBQK1NR b KQkq h3 0 1", Squares.h3)] // valid
        public void EnPassantSquare(string fen, Squares expected)
        {
            var game = GameFactory.Create(fen);
            var actual = game.Pos.EnPassantSquare;
            Assert.Equal(expected, actual.Value);
        }
    }
}
