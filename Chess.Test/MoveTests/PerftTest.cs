/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2022 Rudy Alex Kohn

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

namespace Chess.Test.MoveTests;

using Perft;
using Perft.Interfaces;
using Rudz.Chess.Fen;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public sealed class PerftTest
{
    private const int ShortCount = 2;

    private const int MediumCount = 4;

    private static readonly List<IPerftPosition> Positions = new(5)
    {
        PerftPositionFactory.Create(
            Guid.NewGuid().ToString(),
            Fen.StartPositionFen,
            new List<(int, ulong)>(6)
            {
                    (1, 20),
                    (2, 400),
                    (3, 8902),
                    (4, 197281),
                    (5, 4865609),
                    (6, 119060324)
            }),
        PerftPositionFactory.Create(
            Guid.NewGuid().ToString(),
            "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1",
            new List<(int, ulong)>(6)
            {
                    (1, 48),
                    (2, 2039),
                    (3, 97862),
                    (4, 4085603),
                    (5, 193690690),
                    (6, 8031647685)
            }),
        PerftPositionFactory.Create(
            Guid.NewGuid().ToString(),
            "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1",
            new List<(int, ulong)>(6)
            {
                    (1, 14),
                    (2, 191),
                    (3, 2812),
                    (4, 43238),
                    (5, 674624),
                    (6, 11030083),
                    (7, 178633661)
            }),
        PerftPositionFactory.Create(
            Guid.NewGuid().ToString(),
            "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1",
            new List<(int, ulong)>(6)
            {
                    (1, 6),
                    (2, 264),
                    (3, 9467),
                    (4, 422333),
                    (5, 15833292),
                    (6, 706045033)
            }),
        PerftPositionFactory.Create(
            Guid.NewGuid().ToString(),
            "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10",
            new List<(int, ulong)>(6)
            {
                    (1, 46),
                    (2, 2079),
                    (3, 89890),
                    (4, 3894594),
                    (5, 164075551),
                    (6, 6923051137)
            })
    };

    [Fact]
    public async Task PerftShort()
    {
        foreach (var perftPosition in Positions)
        {
            var perft = PerftFactory.Create();
            perft.AddPosition(perftPosition);
            var expected = perft.GetPositionCount(0, ShortCount);
            var perftResults = perft.DoPerft(ShortCount);

            var actual = ulong.MinValue;
            await foreach (var result in perftResults.ConfigureAwait(false))
                actual += result;

            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public async Task PerftMedium()
    {
        foreach (var perftPosition in Positions)
        {
            var perft = PerftFactory.Create();
            perft.AddPosition(perftPosition);
            var expected = perft.GetPositionCount(0, MediumCount);
            var perftResults = perft.DoPerft(MediumCount);

            var actual = ulong.MinValue;
            await foreach (var result in perftResults.ConfigureAwait(false))
                actual += result;

            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public async Task PerftFull()
    {
        foreach (var perftPosition in Positions)
        {
            var perft = PerftFactory.Create();
            perft.AddPosition(perftPosition);
            var expected = perft.GetPositionCount(0, Positions.Count);
            var perftResults = perft.DoPerft(Positions.Count);

            var actual = ulong.MinValue;
            await foreach (var result in perftResults.ConfigureAwait(false))
                actual += result;

            Assert.Equal(expected, actual);
        }
    }
}
