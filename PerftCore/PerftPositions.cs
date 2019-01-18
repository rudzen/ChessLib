namespace Perft
{
    using System.Collections.Generic;

    public struct PerftPositions
    {
        public PerftPositions(string fen, List<ulong> value)
        {
            this.fen = fen;
            this.value = value;
        }

        public readonly string fen;
        public List<ulong> value;
    }
}