using System.Collections.Generic;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Benchmark;

[MemoryDiagnoser]
public class BitOpBench
{
    private static readonly int[] Lsb64Table =
    {
            63, 30,  3, 32, 59, 14, 11, 33,
            60, 24, 50,  9, 55, 19, 21, 34,
            61, 29,  2, 53, 51, 23, 41, 18,
            56, 28,  1, 43, 46, 27,  0, 35,
            62, 31, 58,  4,  5, 49, 54,  6,
            15, 52, 12, 40,  7, 42, 45, 16,
            25, 57, 48, 13, 10, 39,  8, 44,
            20, 47, 38, 22, 17, 37, 36, 26
        };

    private static readonly int[] Msb64Table =
    {
            0, 47,  1, 56, 48, 27,  2, 60,
            57, 49, 41, 37, 28, 16,  3, 61,
            54, 58, 35, 52, 50, 42, 21, 44,
            38, 32, 29, 23, 17, 11,  4, 62,
            46, 55, 26, 59, 40, 36, 15, 53,
            34, 51, 20, 43, 31, 22, 10, 45,
            25, 39, 14, 33, 19, 30,  9, 24,
            13, 18,  8, 12,  7,  6,  5, 63
        };

    [Benchmark]
    [ArgumentsSource(nameof(BitboardParams))]
    public int LsbCore(BitBoard bb)
    {
        return BitOperations.TrailingZeroCount(bb.Value);
    }

    [Benchmark]
    [ArgumentsSource(nameof(BitboardParams))]
    public int MsbCore(BitBoard bb)
    {
        return 63 - BitOperations.LeadingZeroCount(bb.Value);
    }

    [Benchmark]
    [ArgumentsSource(nameof(BitboardParams))]
    public int CountCore(BitBoard bb)
    {
        return BitOperations.PopCount(bb.Value);
    }

    [Benchmark]
    [ArgumentsSource(nameof(BitboardParams))]
    public int LsbTable(BitBoard bb)
    {
        // @ C author Matt Taylor (2003)
        bb ^= bb - 1;
        var folded = (uint)(bb.Value ^ (bb.Value >> 32));
        return Lsb64Table[folded * 0x78291ACF >> 26];
    }

    [Benchmark]
    [ArgumentsSource(nameof(BitboardParams))]
    public int MsbTable(BitBoard bb)
    {
        const ulong debruijn64 = 0x03f79d71b4cb0a89UL;
        bb |= bb >> 1;
        bb |= bb >> 2;
        bb |= bb >> 4;
        bb |= bb >> 8;
        bb |= bb >> 16;
        bb |= bb >> 32;
        return Msb64Table[(bb.Value * debruijn64) >> 58];
    }

    [Benchmark]
    [ArgumentsSource(nameof(BitboardParams))]
    public int Count(BitBoard bb)
    {
        var y = 0;
        while (bb)
        {
            y++;
            bb &= bb - 1;
        }

        return y;
    }

    public static IEnumerable<object> BitboardParams()
    {
        yield return BitBoards.AllSquares;
        yield return BitBoards.Center;
        yield return BitBoards.BlackArea;
        yield return BitBoards.CenterFiles;
        yield return Player.Black.ColorBB();
        yield return BitBoards.CornerA1;
        yield return BitBoards.CornerA8;
        yield return BitBoards.CornerH1;
        yield return BitBoards.CornerH8;
        yield return BitBoard.Empty;
    }
}
