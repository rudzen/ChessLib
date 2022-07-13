using BenchmarkDotNet.Attributes;
using Rudz.Chess.Enums;
using Rudz.Chess.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Benchmark
{
    [MemoryDiagnoser]
    public class IterateBenchmark
    {
        [Params(10000, 50000)]
        public int N;


        [Benchmark(Description ="Stackalloc")]
        public void IterateOne()
        {
            Span<Player> players = stackalloc Player[] { Player.White, Player.Black };

            var res = 0;
            for (int i = 0; i < N; ++i)
            {
                foreach (var player in players)
                    res++;
            }

            N = res;
        }


        [Benchmark(Description = "For..Loop")]
        public void IterateTwo()
        {
            Span<Player> players = stackalloc Player[] { Player.White, Player.Black };

            var res = 0;
            for (int i = 0; i < N; ++i)
            {
                for (var player = Players.White; player < Players.PlayerNb; ++player)
                    res++;
            }

            N = res;
        }

    }
}
