/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2019 Rudy Alex Kohn

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

using System.Threading.Tasks;

namespace Chess.Test.Move
{
    using System.Collections.Generic;
    using Rudz.Chess.Fen;
    using Rudz.Chess.Perft;
    using Xunit;

    public sealed class PerftTest
    {
        private const int ShortCount = 2;

        private const int MediumCount = 4;

        private static readonly PerftPosition[] Positions;

        static PerftTest()
        {
            Positions = new PerftPosition[5];
            Positions[0] = new PerftPosition(Fen.StartPositionFen, new List<ulong>(6) { 20, 400, 8902, 197281, 4865609, 119060324 });
            Positions[1] = new PerftPosition("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", new List<ulong>(6) { 48, 2039, 97862, 4085603, 193690690, 8031647685 });
            Positions[2] = new PerftPosition("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", new List<ulong>(6) { 14, 191, 2812, 43238, 674624, 11030083, 178633661 });
            Positions[3] = new PerftPosition("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", new List<ulong>(6) { 6, 264, 9467, 422333, 15833292, 706045033 });
            Positions[4] = new PerftPosition("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", new List<ulong>(6) { 46, 2079, 89890, 3894594, 164075551, 6923051137 });
        }

        [Fact]
        public async Task PerftShort()
        {
            foreach (var perftPosition in Positions)
            {
                var perft = new Perft(ShortCount);
                perft.AddPosition(perftPosition);
                var expected = perft.GetPositionCount(0, ShortCount);
                var actual = await perft.DoPerft();
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public async Task PerftMedium()
        {
            foreach (var perftPosition in Positions)
            {
                var perft = new Perft(MediumCount);
                perft.AddPosition(perftPosition);
                var expected = perft.GetPositionCount(0, MediumCount);
                var actual = await perft.DoPerft();
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public async Task PerftFull()
        {
            foreach (var perftPosition in Positions)
            {
                var perft = new Perft(Positions.Length);
                perft.AddPosition(perftPosition);
                var expected = perft.GetPositionCount(0, Positions.Length);
                var actual = await perft.DoPerft();
                Assert.Equal(expected, actual);
            }
        }
    }
}