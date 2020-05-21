using Rudz.Chess;
using Rudz.Chess.Factories;
using Xunit;

namespace Chess.Test.Fence
{
    public class FenceTests
    {
        [Fact]
        public void BlockedOne()
        {
            const string fen = "8/8/k7/p1p1p1p1/P1P1P1P1/8/8/4K3 w - - 0 1";

            const bool expected = true;

            var g = GameFactory.Create();
            g.NewGame(fen);
            var pos = g.Pos;

            var blockage = new Blockage(pos);

            var actual = blockage.IsBlocked();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BlockedTwo()
        {
            const string fen = "4k3/5p2/1p1p1P1p/1P1P1P1P/3P4/8/4K3/8 w - - 0 1";

            const bool expected = true;

            var g = GameFactory.Create();
            g.NewGame(fen);
            var pos = g.Pos;

            var blockage = new Blockage(pos);

            var actual = blockage.IsBlocked();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BlockedThree()
        {
            const string fen = "5k2/8/8/p1p1pPp1/P1P1P1P1/8/8/4K3 w - - 0 1";

            const bool expected = true;

            var g = GameFactory.Create();
            g.NewGame(fen);
            var pos = g.Pos;

            var blockage = new Blockage(pos);

            var actual = blockage.IsBlocked();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void OpenOne()
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

        [Fact]
        public void OpenTwo()
        {
            const string fen = "5k2/8/8/pPp1pPp1/P1P1P1P1/8/8/4K3 w - - 0 1";

            const bool expected = false;

            var g = GameFactory.Create();
            g.NewGame(fen);
            var pos = g.Pos;

            var blockage = new Blockage(pos);

            var actual = blockage.IsBlocked();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void OpenThree()
        {
            const string fen = "8/2p5/kp2p1p1/p1p1P1P1/P1P2P2/1P4K1/8/8 w - - 0 1";

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