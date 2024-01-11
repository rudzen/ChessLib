/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2023 Rudy Alex Kohn

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Runtime.CompilerServices;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Notation.Notations;

public abstract class Notation(ObjectPool<IMoveList> moveLists) : INotation
{
    protected readonly ObjectPool<IMoveList> MoveLists = moveLists;

    public abstract string Convert(IPosition pos, Move move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected char GetCheckChar(IPosition pos)
    {
        var ml = MoveLists.Get();
        ml.Generate(pos);
        var count = ml.Get().Length;
        MoveLists.Return(ml);

        return count switch
        {
            0     => '+',
            var _ => '#'
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static MoveAmbiguities Ambiguity(IPosition pos, Square from, BitBoard similarTypeAttacks)
    {
        var ambiguity = MoveAmbiguities.None;
        var c         = pos.SideToMove;
        var pinned    = pos.PinnedPieces(c);

        while (similarTypeAttacks)
        {
            var square = BitBoards.PopLsb(ref similarTypeAttacks);

            if (pinned & square)
                continue;

            if (pos.GetPieceType(from) != pos.GetPieceType(square))
                continue;

            // ReSharper disable once InvertIf
            if (pos.Pieces(c) & square)
            {
                if (square.File == from.File)
                    ambiguity |= MoveAmbiguities.File;
                else if (square.Rank == from.Rank)
                    ambiguity |= MoveAmbiguities.Rank;

                ambiguity |= MoveAmbiguities.Move;
            }
        }

        return ambiguity;
    }

    /// <summary>
    /// Disambiguation.
    /// <para>If we have more then one piece with destination 'to'.</para>
    /// <para>Note that for pawns is not needed because starting file is explicit.</para>
    /// </summary>
    /// <param name="move">The move to check</param>
    /// <param name="from">The from square</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static IEnumerable<char> Disambiguation(IPosition pos, Move move, Square from)
    {
        var similarAttacks = GetSimilarAttacks(pos, move);
        var ambiguity = Ambiguity(pos, from, similarAttacks);

        if (!ambiguity.HasFlagFast(MoveAmbiguities.Move))
            yield break;

        if (!ambiguity.HasFlagFast(MoveAmbiguities.File))
            yield return from.FileChar;
        else if (!ambiguity.HasFlagFast(MoveAmbiguities.Rank))
            yield return from.RankChar;
        else
        {
            yield return from.FileChar;
            yield return from.RankChar;
        }
    }

    /// <summary>
    /// Get similar attacks based on the move
    /// </summary>
    /// <param name="move">The move to get similar attacks from</param>
    /// <returns>Squares for all similar attacks without the moves from square</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitBoard GetSimilarAttacks(IPosition pos, Move move)
    {
        var from = move.FromSquare();
        var pt = pos.GetPieceType(from);

        return pt switch
        {
            PieceTypes.Pawn or PieceTypes.King => BitBoard.Empty,
            var _ => pos.GetAttacks(move.ToSquare(), pt, pos.Pieces()) ^ from
        };
    }
}