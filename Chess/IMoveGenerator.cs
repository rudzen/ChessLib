namespace Rudz.Chess
{
    using Enums;
    using System.Collections.Generic;
    using Types;

    public interface IMoveGenerator
    {
        bool InCheck { get; set; }
        BitBoard Pinned { get; set; }
        Emgf Flags { get; set; }
        List<Move> Moves { get; }

        void GenerateMoves(State state, bool force = false);

        /// <summary>
        /// Determine if a move is legal or not, by performing the move and checking if the king is under attack afterwards.
        /// </summary>
        /// <param name="move">The move to check</param>
        /// <param name="piece">The moving piece</param>
        /// <param name="from">The from square</param>
        /// <param name="type">The move type</param>
        /// <returns>true if legal, otherwise false</returns>
        bool IsLegal(Move move, Piece piece, Square from, EMoveType type);

        bool IsLegal(Move move);

        /// <summary>
        /// <para>"Validates" a move basic on simple logic. For example if the piece being moved actually exists etc.</para>
        /// <para>This is basically only useful while developing and/or debugging</para>
        /// </summary>
        /// <param name="move">The move to check for logical errors</param>
        /// <returns>True if move "appears" to be legal, otherwise false</returns>
        bool IsPseudoLegal(Move move);
    }
}