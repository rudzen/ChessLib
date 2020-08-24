/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2020 Rudy Alex Kohn

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace Chess.Test.Fence
{
    using Rudz.Chess;
    using Rudz.Chess.Factories;
    using Xunit;

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