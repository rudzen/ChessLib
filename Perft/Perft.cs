namespace Perft
{
    using System;
    using Rudz.Chess;
    using Rudz.Chess.Types;

    
    public class Perf
    {

        public Perf(int depth) => _perftLimit = depth;

        /// <summary>
        /// How deep the test should proceed.
        /// </summary>
        private readonly int _perftLimit;

        private readonly Game game = new Game();

        /// <summary>
        /// To notify about update.
        /// </summary>
        public Action<ulong> CallBack { get; set; }

        public ulong DoPerft()
        {
            ulong total = 0;
            foreach (PerftPositions perftPositions in PerftData.positions) {
                game.SetFen(perftPositions.fen);
                total += game.Perft(_perftLimit);
            }

            return total;
        }
    }
}