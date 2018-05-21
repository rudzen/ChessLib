namespace Perft
{
    using System;
    using Rudz.Chess;
    using Rudz.Chess.Types;

    
    public partial class Perft
    {

        public Perft(int depth) => _perftLimit = depth;

        /// <summary>
        /// How deep the test should proceed.
        /// </summary>
        private readonly int _perftLimit;

        /// <summary>
        /// To notify about update.
        /// </summary>
        public Action<ulong> CallBack { get; set; }

        public ulong DoPerft()
        {
            Game game = new Game();
            ulong total = 0;
            foreach (PerftPositions perftPositions in Positions) {
                game.SetFen(perftPositions.fen);
                total += game.Perft(_perftLimit);
            }

            return total;
        }
    }
}