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

namespace Rudz.Chess;

using Enums;
using Exceptions;
using Microsoft.Extensions.ObjectPool;
using MoveGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Types;

public sealed class MoveAmbiguity : IMoveAmbiguity
{
    private readonly ObjectPool<StringBuilder> _sbPool = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy(), 128);

    private readonly IDictionary<MoveNotations, Func<Move, string>> _notationFuncs;

    private readonly IPosition _pos;

    private MoveAmbiguity(in IPosition pos)
    {
        _pos = pos;
        _notationFuncs = new Dictionary<MoveNotations, Func<Move, string>>
        {
            {MoveNotations.Fan, ToFan},
            {MoveNotations.San, ToSan},
            {MoveNotations.Lan, ToLan},
            {MoveNotations.Ran, ToRan},
            {MoveNotations.Uci, ToUci}
        };
    }

    public static IMoveAmbiguity Create(in IPosition pos)
    {
        return new MoveAmbiguity(in pos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToNotation(Move move, MoveNotations notation = MoveNotations.Fan)
    {
        if (move.IsNullMove())
            return "(none)";

        if (!_notationFuncs.TryGetValue(notation, out var func))
            throw new InvalidMove("Invalid move notation detected.");

        return func(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ToUci(Move move) => move.ToString();

    /// <summary>
    /// <para>Converts a move to FAN notation.</para>
    /// </summary>
    /// <param name="move">The move to convert</param>
    /// <returns>FAN move string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string ToFan(Move move)
    {
        var from = move.FromSquare();
        var to = move.ToSquare();

        var notation = _sbPool.Get();

        if (move.IsCastlelingMove())
            notation.Append(CastlelingExtensions.GetCastlelingString(to, from));
        else
        {
            var pc = _pos.MovedPiece(move);
            var pt = pc.Type();

            if (pt != PieceTypes.Pawn)
            {
                notation.Append(pc.GetUnicodeChar());
                Disambiguation(move, from, notation);
            }

            if (move.IsEnPassantMove())
                notation.Append("ep").Append(from.FileChar);
            else
            {
                var capturedPiece = _pos.GetPiece(to);
                if (capturedPiece != Piece.EmptyPiece)
                {
                    if (pt == PieceTypes.Pawn)
                        notation.Append(from.FileChar);
                    notation.Append('x');
                }
            }

            notation.Append(to.ToString());

            if (move.IsPromotionMove())
                notation.Append('=').Append(move.PromotedPieceType().MakePiece(_pos.SideToMove).GetUnicodeChar());

            if (_pos.InCheck)
                notation.Append(GetCheckChar());
        }

        var result = notation.ToString();

        _sbPool.Return(notation);

        return result;
    }

    /// <summary>
    /// <para>Converts a move to SAN notation.</para>
    /// </summary>
    /// <param name="move">The move to convert</param>
    /// <returns>SAN move string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string ToSan(Move move)
    {
        var from = move.FromSquare();
        var to = move.ToSquare();

        var notation = _sbPool.Get();

        if (move.IsCastlelingMove())
            notation.Append(CastlelingExtensions.GetCastlelingString(to, from));
        else
        {
            var pt = _pos.GetPieceType(from);

            if (pt != PieceTypes.Pawn)
            {
                notation.Append(_pos.GetPiece(from).GetPgnChar());
                Disambiguation(move, from, notation);
            }

            if (move.IsEnPassantMove())
                notation.Append("ep").Append(from.FileChar);
            else
            {
                var capturedPiece = _pos.GetPiece(to);
                if (capturedPiece != Piece.EmptyPiece)
                {
                    if (pt == PieceTypes.Pawn)
                        notation.Append(from.FileChar);
                    notation.Append('x');
                }
            }

            notation.Append(to.ToString());

            if (move.IsPromotionMove())
                notation.Append('=').Append(move.PromotedPieceType().MakePiece(_pos.SideToMove).GetPgnChar());

            if (_pos.InCheck)
                notation.Append(GetCheckChar());
        }

        var result = notation.ToString();

        _sbPool.Return(notation);

        return result;
    }

    /// <summary>
    /// <para>Converts a move to LAN notation.</para>
    /// </summary>
    /// <param name="move">The move to convert</param>
    /// <returns>LAN move string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string ToLan(Move move)
    {
        var from = move.FromSquare();
        var to = move.ToSquare();

        var notation = _sbPool.Get();

        if (move.IsCastlelingMove())
            notation.Append(CastlelingExtensions.GetCastlelingString(to, from));
        else
        {
            var pt = _pos.GetPieceType(from);

            if (pt != PieceTypes.Pawn)
                notation.Append(pt.GetPieceChar());

            notation.Append(from.ToString());

            if (move.IsEnPassantMove())
                notation.Append("ep").Append(from.FileChar);
            else
            {
                var capturedPiece = _pos.GetPiece(to);
                if (capturedPiece != Piece.EmptyPiece)
                {
                    if (pt == PieceTypes.Pawn)
                        notation.Append(from.FileChar);

                    notation.Append('x');
                }
                else
                    notation.Append('-');
            }

            notation.Append(to.ToString());

            if (move.IsPromotionMove())
                notation.Append('=').Append(move.PromotedPieceType().MakePiece(_pos.SideToMove).GetUnicodeChar());

            if (_pos.InCheck)
                notation.Append(GetCheckChar());
        }

        var result = notation.ToString();

        _sbPool.Return(notation);

        return result;
    }

    /// <summary>
    /// <para>Converts a move to RAN notation.</para>
    /// </summary>
    /// <param name="move">The move to convert</param>
    /// <returns>RAN move string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string ToRan(Move move)
    {
        var from = move.FromSquare();
        var to = move.ToSquare();

        var notation = _sbPool.Get();

        if (move.IsCastlelingMove())
            notation.Append(CastlelingExtensions.GetCastlelingString(to, from));
        else
        {
            var pt = _pos.GetPieceType(from);

            if (pt != PieceTypes.Pawn)
                notation.Append(pt.GetPieceChar());

            notation.Append(from.ToString());

            if (move.IsEnPassantMove())
                notation.Append("ep").Append(from.FileChar);
            else
            {
                var capturedPiece = _pos.GetPiece(to);
                if (capturedPiece != Piece.EmptyPiece)
                {
                    if (pt == PieceTypes.Pawn)
                        notation.Append(from.FileChar);

                    notation.Append('x').Append(capturedPiece.Type().GetPieceChar());
                }
                else
                    notation.Append('-');
            }

            notation.Append(to.ToString());

            if (move.IsPromotionMove())
                notation.Append('=').Append(move.PromotedPieceType().MakePiece(_pos.SideToMove).GetUnicodeChar());

            if (_pos.InCheck)
                notation.Append(GetCheckChar());
        }

        var result = notation.ToString();

        _sbPool.Return(notation);

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private char GetCheckChar()
        => _pos.GenerateMoves().Any() ? '+' : '#';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MoveAmbiguities Ambiguity(Move move, BitBoard similarTypeAttacks)
    {
        var ambiguity = MoveAmbiguities.None;
        var c = _pos.SideToMove;
        var from = move.FromSquare();

        foreach (var square in similarTypeAttacks)
        {
            var pinned = _pos.PinnedPieces(c);

            if (square & pinned)
                continue;

            if (_pos.GetPieceType(from) != _pos.GetPieceType(square))
                continue;

            // ReSharper disable once InvertIf
            if (_pos.Pieces(c) & square)
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
    /// Get similar attacks based on the move
    /// </summary>
    /// <param name="move">The move to get similar attacks from</param>
    /// <returns>Squares for all similar attacks without the moves from square</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BitBoard GetSimilarAttacks(Move move)
    {
        var from = move.FromSquare();
        var pt = _pos.GetPieceType(from);

        return pt == PieceTypes.Pawn || pt == PieceTypes.King
            ? BitBoard.Empty
            : _pos.GetAttacks(move.ToSquare(), pt, _pos.Pieces()) ^ from;
    }

    /// <summary>
    /// Disambiguation.
    /// <para>If we have more then one piece with destination 'to'.</para>
    /// <para>Note that for pawns is not needed because starting file is explicit.</para>
    /// </summary>
    /// <param name="move">The move to check</param>
    /// <param name="from">The from square</param>
    /// <param name="sb">The StringBuilder to append to if needed</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Disambiguation(Move move, Square from, StringBuilder sb)
    {
        var similarAttacks = GetSimilarAttacks(move);
        var ambiguity = Ambiguity(move, similarAttacks);

        if (!ambiguity.HasFlagFast(MoveAmbiguities.Move))
            return;

        if (!ambiguity.HasFlagFast(MoveAmbiguities.File))
            sb.Append(from.FileChar);
        else if (!ambiguity.HasFlagFast(MoveAmbiguities.Rank))
            sb.Append(from.RankChar);
        else
            sb.Append(from.ToString());
    }
}
