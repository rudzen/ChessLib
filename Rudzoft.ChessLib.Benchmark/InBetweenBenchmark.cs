using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace Rudzoft.ChessLib.Benchmark;

[MemoryDiagnoser]
public class InBetweenBenchmark
{
    [Params(10000, 50000)] public int N;

    [Benchmark(Description = "Boolean AND [int]")]
    public void SignedOrTrad()
    {
        var half = N / 2;
        for (var i = 0; i < N; ++i)
        {
            var inBetween = InBetweenTrad(i, 0, half);
        }
    }

    [Benchmark(Description = "Signed OR [int]")]
    public void SignedOrInt()
    {
        var half = N / 2;
        for (var i = 0; i < N; ++i)
        {
            var inBetween = InBetweenOr(i, 0, half);
        }
    }

    [Benchmark(Baseline = true, Description = "Unsigned Compare [int]")]
    public void UnsignedCompareInt()
    {
        var half = N / 2;
        for (var i = 0; i < N; ++i)
        {
            var inBetween = InBetween(i, 0, half);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool InBetweenOr(int v, int min, int max) => ((v - min) | (max - v)) >= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool InBetweenOr(byte v, byte min, byte max) => (((int)v - (int)min) | ((int)max - (int)v)) >= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool InBetweenOr(char v, char min, char max) => (((int)v - (int)min) | ((int)max - (int)v)) >= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool InBetween(int v, int min, int max) => (uint)v - (uint)min <= (uint)max - (uint)min;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool InBetween(byte v, byte min, byte max) => v - (uint)min <= max - (uint)min;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool InBetween(char v, char min, char max) => v - (uint)min <= max - min;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool InBetweenTrad(int v, int min, int max) => v >= min && v <= max;

}