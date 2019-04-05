namespace BenchmarkCore
{
    using System.Collections.Generic;

    public partial class Perft
    {
        internal static readonly PerftPosition[] Positions;

        static Perft()
        {
            Positions = new PerftPosition[1];
            var vals = new List<ulong>(6)
            {
                20,
                400,
                8902,
                197281,
                4865609,
                119060324
            };
            Positions[0] = new PerftPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", vals);
        }
    }
}