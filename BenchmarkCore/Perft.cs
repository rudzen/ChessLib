using System.Threading.Tasks;

namespace BenchmarkCore
{
    using System;
    using BenchmarkDotNet.Running;
    using Rudz.Chess;

    /*

// * Summary *

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17763.253 (1809/October2018Update/Redstone5)
Intel Core i7-8086K CPU 4.00GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=2.2.101
  [Host]     : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT
  DefaultJob : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT


 Method | N |           Mean |         Error |        StdDev |
------- |-- |---------------:|--------------:|--------------:|
 Result | 1 |       301.3 us |      2.417 us |      2.261 us |
 Result | 2 |       305.7 us |      1.604 us |      1.501 us |
 Result | 3 |       396.4 us |      1.338 us |      1.251 us |
 Result | 4 |     2,670.5 us |      6.113 us |      5.105 us |
 Result | 5 |    71,036.1 us |  1,479.684 us |  1,235.603 us |
 Result | 6 | 2,124,442.2 us | 42,237.581 us | 53,417.016 us |
     */
    public partial class Perft
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<PerftBench>();
        }

        public Perft(int depth) => _perftLimit = depth;

        /// <summary>
        /// How deep the test should proceed.
        /// </summary>
        private readonly int _perftLimit;

        /// <summary>
        /// To notify about update.
        /// </summary>
        private Action<string, ulong> CallBack { get; set; }

        public ulong DoPerft()
        {
            Game game = new Game();
            ulong total = 0;
            foreach (PerftPositions perftPositions in global::BenchmarkCore.Perft.Positions)
            {
                game.SetFen(perftPositions.fen);
                var res = game.Perft(_perftLimit);
                total += res.Result;
                CallBack?.Invoke(perftPositions.fen, res.Result);
            }

            return total;
        }
    }
}