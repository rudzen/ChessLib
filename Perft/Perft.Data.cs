namespace Perft
{
    using System.Collections.Generic;

    public partial class Perft
    {
        private static readonly IList<PerftPositions> Positions;

        static Perft()
        {
            Positions = new List<PerftPositions>();
            IList<ulong> vals = new List<ulong>(6);
            vals.Add(20);
            vals.Add(400);
            vals.Add(8902);
            vals.Add(197281);
            vals.Add(4865609);
            vals.Add(119060324);
            Positions.Add(new PerftPositions("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", vals));
        }

    }
}