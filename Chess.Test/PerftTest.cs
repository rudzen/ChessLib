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
using Rudz.Chess;
using Rudz.Chess.Fen;

namespace Chess.Tests
{
    using System;
    using System.Runtime.CompilerServices;
    using Xunit;

    public class PerftTest
    {
        private const int ShortCount = 2;

        private const int MediumCount = 4;

        private static readonly Tuple<string, ulong[]>[] positions;

        static PerftTest()
        {
            positions = new Tuple<string, ulong[]>[5];
            positions[0] = new Tuple<string, ulong[]>(Fen.StartPositionFen, new ulong[] { 20, 400, 8902, 197281, 4865609, 119060324 });
            positions[1] = new Tuple<string, ulong[]>("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", new ulong[] { 48, 2039, 97862, 4085603, 193690690, 8031647685 });
            positions[2] = new Tuple<string, ulong[]>("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", new ulong[] { 14, 191, 2812, 43238, 674624, 11030083, 178633661 });
            positions[3] = new Tuple<string, ulong[]>("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", new ulong[] { 6, 264, 9467, 422333, 15833292, 706045033 });
            positions[4] = new Tuple<string, ulong[]>("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", new ulong[] { 46, 2079, 89890, 3894594, 164075551, 6923051137 });
        }

        [Fact]
        public async void PerftShort()
        {
            Game game = new Game();
            foreach (var (item1, item2) in positions)
            {
                for (int i = 0; i < ShortCount; ++i)
                {
                    var res = await DoPerft(game, item1, i + 1).ConfigureAwait(false);
                    Assert.Equal(item2[i], res);
                }
            }
        }

        [Fact]
        public async void PerftMedium()
        {
            Game game = new Game();
            foreach (Tuple<string, ulong[]> position in positions)
            {
                for (int i = 0; i < MediumCount; ++i)
                {
                    var res = await DoPerft(game, position.Item1, i + 1).ConfigureAwait(false);
                    Assert.Equal(position.Item2[i], res);
                }
            }
        }

        //[Fact]
        //public void PerftFull()
        //{
        //    Game game = new Game();
        //    foreach (Tuple<string, ulong[]> tuple in positions)
        //    {
        //        for (int i = 0; i < tuple.Item2.Length; ++i)
        //        {
        //            ulong res = DoPerft(ref game, tuple.Item1, i + 1);
        //            Assert.Equal(tuple.Item2[i], res);
        //        }
        //    }
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Task<ulong> DoPerft(Game game, string fen, int depth)
        {
            game.SetFen(fen);
            return game.Perft(depth);
        }
    }
}