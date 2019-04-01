namespace Perft
{
    using System.Collections.Generic;

    public partial class Perft
    {
        internal static readonly List<PerftPositions> Positions;

        static Perft()
        {
            Positions = new List<PerftPositions>();
            var vals = new List<ulong>(6)
            {
                20,
                400,
                8902,
                197281,
                4865609,
                119060324
            };
            Positions.Add(new PerftPositions("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", vals));
        }
    }
}