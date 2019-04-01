namespace BenchmarkCore
{
    using System;
    using BenchmarkDotNet.Running;
    using Rudz.Chess;

    /*

// * Summary *

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17763.379 (1809/October2018Update/Redstone5)
Intel Core i7-8086K CPU 4.00GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=2.2.101
  [Host]     : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT  [AttachedDebugger]
  DefaultJob : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT

Method	N	Mean	Error	StdDev
Result	1	296.4 us	1.496 us	1.326 us
Result	2	300.2 us	1.678 us	1.570 us
Result	3	379.0 us	1.897 us	1.584 us
Result	4	2,408.1 us	12.816 us	11.988 us
Result	5	64,921.2 us	1,310.703 us	1,402.437 us
Result	6	1,912,300.6 us	3,551.167 us	3,148.017 us
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
            var game = new Game();
            ulong total = 0;
            foreach (var perftPositions in global::BenchmarkCore.Perft.Positions)
            {
                game.SetFen(perftPositions.fen);
                var res = game.Perft(_perftLimit);
                total += res;
                CallBack?.Invoke(perftPositions.fen, res);
            }

            return total;
        }
    }
}