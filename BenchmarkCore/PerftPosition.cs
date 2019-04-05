namespace BenchmarkCore
{
    using System.Collections.Generic;

    public struct PerftPosition
    {
        public PerftPosition(string fen, List<ulong> value)
        {
            this.fen = fen;
            this.value = value;
        }

        public readonly string fen;
        public List<ulong> value;
    }
}