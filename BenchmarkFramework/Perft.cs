namespace BenchmarkFramework
{
    using BenchmarkDotNet.Running;
    using Rudz.Chess;
    using System;

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
            foreach (PerftPositions perftPositions in Positions)
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