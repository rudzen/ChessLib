/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2022 Rudy Alex Kohn

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

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Notation.Notations;

public abstract class Notation : INotation
{
    protected readonly IPosition Pos;

    protected Notation(IPosition pos) => Pos = pos;

    public abstract string Convert(Move move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected char GetCheckChar() =>
        Pos.GenerateMoves().Length switch
        {
            0 => '+',
            _ => '#'
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool GivesCheck(Move move) => Pos.GivesCheck(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MoveAmbiguities Ambiguity(Square from, BitBoard similarTypeAttacks)
    {
        var ambiguity = MoveAmbiguities.None;
        var c = Pos.SideToMove;
        var pinned = Pos.PinnedPieces(c);

        while (similarTypeAttacks)
        {
            var square = BitBoards.PopLsb(ref similarTypeAttacks);

            if (pinned & square)
                continue;

            if (Pos.GetPieceType(from) != Pos.GetPieceType(square))
                continue;

            // ReSharper disable once InvertIf
            if (Pos.Pieces(c) & square)
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
    protected IEnumerable<char> Disambiguation(Move move, Square from)
    {
        var similarAttacks = GetSimilarAttacks(move);
        var ambiguity = Ambiguity(from, similarAttacks);

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
    private BitBoard GetSimilarAttacks(Move move)
    {
        var from = move.FromSquare();
        var pt = Pos.GetPieceType(from);

        return pt switch
        {
            PieceTypes.Pawn or PieceTypes.King => BitBoard.Empty,
            _ => Pos.GetAttacks(move.ToSquare(), pt, Pos.Pieces()) ^ from
        };
    }
}