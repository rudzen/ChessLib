using System;
using BenchmarkDotNet.Attributes;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Benchmark;

[MemoryDiagnoser]
public class IterateBenchmark
{
    [Params(10000, 50000)]
    public int N;

    [Benchmark(Description = "Stackalloc")]
    public void IterateOne()
    {
        Span<Player> players = stackalloc Player[] { Player.White, Player.Black };

        var res = 0;
        for (var i = 0; i < N; ++i)
            foreach (var player in players)
                res += player.Side;

        N = res;
    }

    [Benchmark(Description = "For..Loop")]
    public void IterateTwo()
    {
        var res = 0;
        for (var i = 0; i < N; ++i)
        for (var player = Players.White; player < Players.PlayerNb; ++player)
            res += (int)player;

        N = res;
    }
}