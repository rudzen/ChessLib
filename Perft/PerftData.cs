namespace Perft
{
    using System.Collections.Generic;

    public static class PerftData
    {

        public static readonly IList<PerftPositions> positions;

        static PerftData()
        {

            positions = new List<PerftPositions>();

            IList<ulong> vals = new List<ulong>(6);
            vals.Add(20);
            vals.Add(400);
            vals.Add(8902);
            vals.Add(197281);
            vals.Add(4865609);
            vals.Add(119060324);
            
            positions.Add(new PerftPositions("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", vals));
            
//                {"r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1",     {48ull, 2039ull, 97862ull, 4085603ull, 193690690ull, 8031647685ull}},
//                {"8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1",                                {14ull, 191ull,  2812ull,  43238ull,   674624ull,    11030083ull, 178633661ull}},
//                {"r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1",         {6ull,  264ull,  9467ull,  422333ull,  15833292ull,  706045033ull}},
//                {"r2q1rk1/pP1p2pp/Q4n2/bbp1p3/Np6/1B3NBn/pPPP1PPP/R3K2R b KQ - 0 1",         {6ull,  264ull,  9467ull,  422333ull,  15833292ull,  706045033ull}},
//                {"rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8",                {44ull, 1486ull, 62379ull, 2103487ull, 89941194ull}},
//                {"r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", {46ull, 2079ull, 89890ull, 3894594ull, 164075551ull, 6923051137ull}}

            
            
        }

    }
}