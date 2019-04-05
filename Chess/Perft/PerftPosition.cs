namespace Rudz.Chess.Perft
{
    using System.Collections.Generic;

    public struct PerftPosition
    {
        public PerftPosition(string fen, List<ulong> value)
        {
            Fen = fen;
            Value = value;
        }

        public readonly string Fen;
        public List<ulong> Value;
    }
}