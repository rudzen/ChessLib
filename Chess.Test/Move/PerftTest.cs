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

namespace Chess.Test.Move
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Rudz.Chess;
    using Rudz.Chess.Fen;
    using Xunit;

    public sealed class PerftTest
    {
        private const int ShortCount = 2;

        private const int MediumCount = 4;

        private static readonly KeyValuePair<string, ulong[]>[] Positions;

        static PerftTest()
        {
            Positions = new KeyValuePair<string, ulong[]>[5];
            Positions[0] = new KeyValuePair<string, ulong[]>(Fen.StartPositionFen, new ulong[] { 20, 400, 8902, 197281, 4865609, 119060324 });
            Positions[1] = new KeyValuePair<string, ulong[]>("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", new ulong[] { 48, 2039, 97862, 4085603, 193690690, 8031647685 });
            Positions[2] = new KeyValuePair<string, ulong[]>("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", new ulong[] { 14, 191, 2812, 43238, 674624, 11030083, 178633661 });
            Positions[3] = new KeyValuePair<string, ulong[]>("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", new ulong[] { 6, 264, 9467, 422333, 15833292, 706045033 });
            Positions[4] = new KeyValuePair<string, ulong[]>("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", new ulong[] { 46, 2079, 89890, 3894594, 164075551, 6923051137 });
        }

        [Fact]
        public void PerftShort()
        {
            var game = new Game();
            foreach (var (item1, item2) in Positions)
            {
                for (var i = 0; i < ShortCount; ++i)
                {
                    var res = DoPerft(game, item1, i + 1);
                    Assert.Equal(item2[i], res);
                }
            }
        }

        [Fact]
        public void PerftMedium()
        {
            var game = new Game();
            foreach (var position in Positions)
            {
                for (var i = 0; i < MediumCount; ++i)
                {
                    var res = DoPerft(game, position.Key, i + 1);
                    Assert.Equal(position.Value[i], res);
                }
            }
        }

        //[Fact]
        //public void PerftFull()
        //{
        //    var game = new Game();
        //    foreach (var (key, value) in Positions)
        //    {
        //        for (var i = 0; i < value.Length; ++i)
        //        {
        //            var res = DoPerft(game, key, i + 1);
        //            Assert.Equal(value[i], res);
        //        }
        //    }
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong DoPerft(Game game, string fen, int depth)
        {
            game.SetFen(fen);
            return game.Perft(depth);
        }
    }
}