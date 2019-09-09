using System;
using System.Collections.Generic;
using Rudz.Chess.Types;

namespace Rudz.Chess
{
    public interface IMoveList : IEnumerable<Move>, IDisposable
    {
        int Count { get; }
        void Add(Move move);
        Move GetMove(int index);

        /// <summary>
        /// Primary use is for polyglot moves
        /// </summary>
        /// <param name="from">The from square</param>
        /// <param name="to">The to square</param>
        /// <returns>The first move that matches from and to squares</returns>
        Move GetMove(Square from, Square to);

        Move this[int index] { get; }
    }
}