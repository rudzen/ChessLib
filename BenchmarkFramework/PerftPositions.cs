using System.Collections.Generic;

namespace BenchmarkFramework
{
    public struct PerftPositions
    {
        public PerftPositions(string fen, IList<ulong> value)
        {
            this.fen = fen;
            this.value = value;
        }

        public readonly string fen;
        public IList<ulong> value;
    }
}