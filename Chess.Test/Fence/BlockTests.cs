using Rudz.Chess;
using Rudz.Chess.Factories;
using Xunit;

namespace Chess.Test.Fence
{
    public class BlockTests
    {
        [Fact]
        public void BasicBlockageFail()
        {
            // a white pawn cannot be blocked by the black king
            const string fen = "8/8/k7/p1p1pPp1/P1P1P1P1/8/8/4K3 w - - 0 1";

            const bool expected = false;

            var g = GameFactory.Create();
            g.NewGame(fen);
            var pos = g.Pos;

            var blockage = new Blockage(pos);

            var actual = blockage.IsBlocked();

            Assert.Equal(expected, actual);
        }
    }
}