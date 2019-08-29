using Chess.Perft.Interfaces;

namespace Chess.Perft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class PerftFactory
    {
        public static IPerft Create(Action<string> boardPrintCallback = null, IEnumerable<IPerftPosition> positions = null)
        {
            if (positions == null)
                positions = Enumerable.Empty<IPerftPosition>();
            return new Perft(boardPrintCallback, positions);
        }

    }
}