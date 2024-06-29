/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2023 Rudy Alex Kohn

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

namespace Rudzoft.ChessLib.Test.PerftTests;

public sealed class PerftOneTest : PerftVerify
{
    private static readonly string[] Fens = Enumerable.Repeat(Fen.Fen.StartPositionFen, 6).ToArray();

    private static readonly int[] Depths =
    [
        1, 2, 3, 4, 5, 6
    ];

    private static readonly ulong[] Results =
    [
        20UL, 400UL, 8_902UL, 197_281UL, 4_865_609UL, 119_060_324UL
    ];

    public static readonly PerftTheoryData PerftTheoryData = new(Fens, Depths, Results);

    [Theory]
    [MemberData(nameof(PerftTheoryData))]
    public void Perft(string fen, int depth, ulong expected)
        => AssertPerft(fen, depth, in expected);
}

public sealed class PerftTwoTest : PerftVerify
{
    private const string Fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";

    private static readonly string[] Fens = Enumerable.Repeat(Fen, 5).ToArray();

    private static readonly int[] Depths =
    [
        1, 2, 3, 4, 5
    ];

    private static readonly ulong[] Results =
    [
        48UL, 2_039UL, 97_862UL, 4_085_603UL, 193_690_690UL
    ];

    public static readonly PerftTheoryData PerftTheoryData = new(Fens, Depths, Results);

    [Theory]
    [MemberData(nameof(PerftTheoryData))]
    public void Perft(string fen, int depth, ulong expected)
        => AssertPerft(fen, depth, in expected);
}

public sealed class PerftThreeTest : PerftVerify
{
    private const string Fen = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1";

    private static readonly string[] Fens = Enumerable.Repeat(Fen, 7).ToArray();

    private static readonly int[] Depths =
    [
        1, 2, 3, 4, 5, 6, 7
    ];

    private static readonly ulong[] Results =
    [
        14UL, 191UL, 2_812UL, 43_238UL, 674_624UL, 11_030_083UL, 178_633_661UL
    ];

    public static readonly PerftTheoryData PerftTheoryData = new(Fens, Depths, Results);

    [Theory]
    [MemberData(nameof(PerftTheoryData))]
    public void Perft(string fen, int depth, ulong expected)
        => AssertPerft(fen, depth, in expected);
}

public sealed class PerftFourTest : PerftVerify
{
    private const string Fen = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1";

    private static readonly string[] Fens = Enumerable.Repeat(Fen, 5).ToArray();

    private static readonly int[] Depths =
    [
        1, 2, 3, 4, 5
    ];

    private static readonly ulong[] Results =
    [
        6UL, 264UL, 9_467UL, 422_333UL, 15_833_292UL
    ];

    public static readonly PerftTheoryData PerftTheoryData = new(Fens, Depths, Results);

    [Theory]
    [MemberData(nameof(PerftTheoryData))]
    public void Perft(string fen, int depth, ulong expected)
        => AssertPerft(fen, depth, in expected);
}

public sealed class PerftFiveTest : PerftVerify
{
    private const string Fen = "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10";

    private static readonly string[] Fens = Enumerable.Repeat(Fen, 5).ToArray();

    private static readonly int[] Depths =
    [
        1, 2, 3, 4, 5
    ];

    private static readonly ulong[] Results =
    [
        46UL, 2_079UL, 89_890UL, 3_894_594UL, 164_075_551UL
    ];

    public static readonly PerftTheoryData PerftTheoryData = new(Fens, Depths, Results);

    [Theory]
    [MemberData(nameof(PerftTheoryData))]
    public void Perft(string fen, int depth, ulong expected)
        => AssertPerft(fen, depth, in expected);
}

public sealed class TalkChessPerftTests : PerftVerify
{
    private static readonly string[] Fens =
    [
        "3k4/3p4/8/K1P4r/8/8/8/8 b - - 0 1",         //--Illegal ep move #1
        "8/8/4k3/8/2p5/8/B2P2K1/8 w - - 0 1",        //--Illegal ep move #2
        "8/8/1k6/2b5/2pP4/8/5K2/8 b - d3 0 1",       //--EP Capture Checks Opponent
        "5k2/8/8/8/8/8/8/4K2R w K - 0 1",            //--Short Castling Gives Check
        "3k4/8/8/8/8/8/8/R3K3 w Q - 0 1",            //--Long Castling Gives Check
        "r3k2r/1b4bq/8/8/8/8/7B/R3K2R w KQkq - 0 1", //--Castle Rights
        "r3k2r/8/3Q4/8/8/5q2/8/R3K2R b KQkq - 0 1",  //--Castling Prevented
        "2K2r2/4P3/8/8/8/8/8/3k4 w - - 0 1",         //--Promote out of Check
        "8/8/1P2K3/8/2n5/1q6/8/5k2 b - - 0 1",       //--Discovered Check
        "4k3/1P6/8/8/8/8/K7/8 w - - 0 1",            //--Promote to give check
        "8/P1k5/K7/8/8/8/8/8 w - - 0 1",             //--Under Promote to give check
        "K1k5/8/P7/8/8/8/8/8 w - - 0 1",             //--Self Stalemate
        "8/k1P5/8/1K6/8/8/8/8 w - - 0 1",            //--Stalemate & Checkmate
        "8/8/2k5/5q2/5n2/8/5K2/8 b - - 0 1"          //--Stalemate & Checkmate
    ];

    private static readonly int[] Depths =
    [
        6, 6, 6, 6, 6,
        4, 4, 6, 5, 6,
        6, 6, 7, 4
    ];

    private static readonly ulong[] Results =
    [
        1_134_888UL, 1_015_133UL, 1_440_467UL, 661_072UL, 803_711UL,
        1_274_206UL, 1_720_476UL, 3_821_001UL, 1_004_658UL, 217_342UL,
        92_683UL, 2_217UL, 567_584UL, 23_527UL
    ];

    public static readonly PerftTheoryData PerftTheoryData = new(Fens, Depths, Results);

    [Theory]
    [MemberData(nameof(PerftTheoryData))]
    public void Perft(string fen, int depth, ulong expected)
        => AssertPerft(fen, depth, in expected);
}

public sealed class WeirdErrorFenTest : PerftVerify
{
    private static readonly string[] Fens =
    [
        "rnkq1bnr/p3ppp1/1ppp3p/3B4/6b1/2PQ3P/PP1PPP2/RNB1K1NR w KQkq - 0 1"
    ];

    private static readonly int[] Depths =
    [
        6
    ];

    private static readonly ulong[] Results =
    [
        840484313UL
    ];

    public static readonly PerftTheoryData PerftTheoryData = new(Fens, Depths, Results);

    [Theory]
    [MemberData(nameof(PerftTheoryData))]
    public void Perft(string fen, int depth, ulong expected)
        => AssertPerft(fen, depth, in expected);
}

/// <summary>
/// https://gist.github.com/peterellisjones/8c46c28141c162d1d8a0f0badbc9cff9
/// </summary>
public sealed class PeterJonesPerftTests : PerftVerify
{
    private static readonly string[] Fens =
    [
        "r6r/1b2k1bq/8/8/7B/8/8/R3K2R b KQ - 3 2",
        "8/8/8/2k5/2pP4/8/B7/4K3 b - d3 0 3",
        "r1bqkbnr/pppppppp/n7/8/8/P7/1PPPPPPP/RNBQKBNR w KQkq - 2 2",
        "r3k2r/p1pp1pb1/bn2Qnp1/2qPN3/1p2P3/2N5/PPPBBPPP/R3K2R b KQkq - 3 2",
        "2kr3r/p1ppqpb1/bn2Qnp1/3PN3/1p2P3/2N5/PPPBBPPP/R3K2R b KQ - 3 2",
        "rnb2k1r/pp1Pbppp/2p5/q7/2B5/8/PPPQNnPP/RNB1K2R w KQ - 3 9",
        "2r5/3pk3/8/2P5/8/2K5/8/8 w - - 5 4",
        "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8",
        "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10",
        "3k4/3p4/8/K1P4r/8/8/8/8 b - - 0 1",
        "8/8/4k3/8/2p5/8/B2P2K1/8 w - - 0 1",
        "8/8/1k6/2b5/2pP4/8/5K2/8 b - d3 0 1",
        "5k2/8/8/8/8/8/8/4K2R w K - 0 1",
        "3k4/8/8/8/8/8/8/R3K3 w Q - 0 1",
        "r3k2r/1b4bq/8/8/8/8/7B/R3K2R w KQkq - 0 1",
        "r3k2r/8/3Q4/8/8/5q2/8/R3K2R b KQkq - 0 1",
        "2K2r2/4P3/8/8/8/8/8/3k4 w - - 0 1",
        "8/8/1P2K3/8/2n5/1q6/8/5k2 b - - 0 1",
        "4k3/1P6/8/8/8/8/K7/8 w - - 0 1",
        "8/P1k5/K7/8/8/8/8/8 w - - 0 1",
        "K1k5/8/P7/8/8/8/8/8 w - - 0 1",
        "8/k1P5/8/1K6/8/8/8/8 w - - 0 1",
        "8/8/2k5/5q2/5n2/8/5K2/8 b - - 0 1"
    ];

    private static readonly int[] Depths =
    [
        1, 1, 1, 1, 1, 1, 1, 3, 3, 6, 6, 6, 6, 6, 4, 4, 6,
        5, 6, 6, 6, 7, 4
    ];

    private static readonly ulong[] Results =
    [
        8, 8, 19, 5, 44, 39, 9, 62379, 89890, 1134888, 1015133, 1440467, 661072, 803711, 1274206, 1720476, 3821001,
        1004658, 217342, 92683, 2217, 567584, 23527
    ];

    public static readonly PerftTheoryData PerftTheoryData = new(Fens, Depths, Results);

    [Theory]
    [MemberData(nameof(PerftTheoryData))]
    public void Perft(string fen, int depth, ulong expected)
        => AssertPerft(fen, depth, in expected);
}