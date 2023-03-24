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
    [Theory]
    [InlineData(Fen.Fen.StartPositionFen, 1, 20UL)]
    [InlineData(Fen.Fen.StartPositionFen, 2, 400UL)]
    [InlineData(Fen.Fen.StartPositionFen, 3, 8_902UL)]
    [InlineData(Fen.Fen.StartPositionFen, 4, 197_281UL)]
    [InlineData(Fen.Fen.StartPositionFen, 5, 4_865_609UL)]
#if !DEBUG
    [InlineData(Fen.Fen.StartPositionFen, 6, 119_060_324UL)]
#endif
    public void Perft(string fen, int depth, ulong expected)
        => AssertPerft(fen, depth, in expected);
}

public sealed class PerftTwoTest : PerftVerify
{
    [Theory]
    [InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", 1, 48UL)]
    [InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", 2, 2_039UL)]
    [InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", 3, 97_862UL)]
    [InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", 4, 4_085_603UL)]
#if !DEBUG
    [InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", 5, 193_690_690UL)]
#endif
    public void Perft(string fen, int depth, ulong expected)
        => AssertPerft(fen, depth, in expected);
}

public sealed class PerftThreeTest : PerftVerify
{
    [Theory]
    [InlineData("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", 1, 14UL)]
    [InlineData("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", 2, 191UL)]
    [InlineData("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", 3, 2_812UL)]
    [InlineData("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", 4, 43_238UL)]
    [InlineData("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", 5, 674_624UL)]
#if !DEBUG
    [InlineData("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", 6, 11_030_083UL)]
    [InlineData("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", 7, 178_633_661UL)]
#endif
    public void Perft(string fen, int depth, ulong expected)
        => AssertPerft(fen, depth, in expected);
}

public sealed class PerftFourTest : PerftVerify
{
    [Theory]
    [InlineData("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", 1, 6UL)]
    [InlineData("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", 2, 264UL)]
    [InlineData("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", 3, 9_467UL)]
    [InlineData("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", 4, 422_333UL)]
    [InlineData("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", 5, 15_833_292UL)]
    public void Perft(string fen, int depth, ulong expected)
        => AssertPerft(fen, depth, in expected);
}

public sealed class PerftFiveTest : PerftVerify
{
    [Theory]
    [InlineData("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", 1, 46UL)]
    [InlineData("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", 2, 2_079UL)]
    [InlineData("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", 3, 89_890UL)]
    [InlineData("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", 4, 3_894_594UL)]
#if !DEBUG
    [InlineData("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", 5, 164_075_551UL)]
#endif
    public void Perft(string fen, int depth, ulong expected)
        => AssertPerft(fen, depth, in expected);
}

public sealed class TalkChessPerftTests : PerftVerify
{
    [Theory]
    [InlineData("3k4/3p4/8/K1P4r/8/8/8/8 b - - 0 1", 6, 1_134_888UL)] //--Illegal ep move #1
    [InlineData("8/8/4k3/8/2p5/8/B2P2K1/8 w - - 0 1", 6, 1_015_133UL)] //--Illegal ep move #2
    [InlineData("8/8/1k6/2b5/2pP4/8/5K2/8 b - d3 0 1", 6, 1_440_467UL)] //--EP Capture Checks Opponent
    [InlineData("5k2/8/8/8/8/8/8/4K2R w K - 0 1", 6, 661_072UL)] //--Short Castling Gives Check
    [InlineData("3k4/8/8/8/8/8/8/R3K3 w Q - 0 1", 6, 803_711UL)] //--Long Castling Gives Check
    [InlineData("r3k2r/1b4bq/8/8/8/8/7B/R3K2R w KQkq - 0 1", 4, 1_274_206UL)] //--Castle Rights
    [InlineData("r3k2r/8/3Q4/8/8/5q2/8/R3K2R b KQkq - 0 1", 4, 1_720_476UL)] //--Castling Prevented
    [InlineData("2K2r2/4P3/8/8/8/8/8/3k4 w - - 0 1", 6, 3_821_001UL)] //--Promote out of Check
    [InlineData("8/8/1P2K3/8/2n5/1q6/8/5k2 b - - 0 1", 5, 1_004_658UL)] //--Discovered Check
    [InlineData("4k3/1P6/8/8/8/8/K7/8 w - - 0 1", 6, 217_342UL)] //--Promote to give check
    [InlineData("8/P1k5/K7/8/8/8/8/8 w - - 0 1", 6, 92_683UL)] //--Under Promote to give check
    [InlineData("K1k5/8/P7/8/8/8/8/8 w - - 0 1", 6, 2_217UL)] //--Self Stalemate
    [InlineData("8/k1P5/8/1K6/8/8/8/8 w - - 0 1", 7, 567_584UL)] //--Stalemate & Checkmate
    [InlineData("8/8/2k5/5q2/5n2/8/5K2/8 b - - 0 1", 4, 23_527UL)] //--Stalemate & Checkmate
    public void Perft(string fen, int depth, ulong expected)
        => AssertPerft(fen, depth, in expected);
}