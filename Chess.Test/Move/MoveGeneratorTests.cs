using Rudz.Chess;
using Rudz.Chess.Enums;
using Rudz.Chess.Fen;
using Xunit;

namespace Chess.Test.Move
{
    public class MoveGeneratorTests
    {

        [Fact]
        public void InCheckMoveGenerationTest()
        {
            const string fen = "rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6";
            const ulong expectedMoves = 4;
            
            var board = new Board();
            var pos = new Rudz.Chess.Position(board);
            var fd = new FenData(fen);

            pos.SetFen(fd);
            
            // make sure black is in check
            Assert.True(pos.InCheck);
            
            // generate moves for black
            var mg = pos.GenerateMoves();
            var actualMoves = mg.Length;
            
            Assert.Equal(expectedMoves, actualMoves);
        }
    }
}