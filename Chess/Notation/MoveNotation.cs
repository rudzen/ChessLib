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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Rudz.Chess.Enums;
using Rudz.Chess.Exceptions;
using Rudz.Chess.Extensions;
using Rudz.Chess.MoveGeneration;
using Rudz.Chess.Types;

namespace Rudz.Chess.Notation;

public sealed class MoveNotation : IMoveNotation
{
    private readonly IDictionary<MoveNotations, Func<Move, string>> _notationFuncs;

    private readonly IPosition _pos;

    private MoveNotation(in IPosition pos)
    {
        _pos = pos;
        _notationFuncs = new Dictionary<MoveNotations, Func<Move, string>>(6)
        {
            { MoveNotations.Fan, ToFan },
            { MoveNotations.San, ToSan },
            { MoveNotations.Lan, ToLan },
            { MoveNotations.Ran, ToRan },
            { MoveNotations.ICCF, ToIccf },
            { MoveNotations.Uci, ToUci }
        };
    }

    public static IMoveNotation Create(in IPosition pos)
        => new MoveNotation(in pos);

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
    private static string ToUci(Move move)
        => move.ToString();

    /// <summary>
    /// <para>Converts a move to FAN notation.</para>
    /// </summary>
    /// <param name="move">The move to convert</param>
    /// <returns>FAN move string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string ToFan(Move move)
    {
        var (from, to) = move.FromTo();

        if (move.IsCastlelingMove())
            return CastlelingExtensions.GetCastlelingString(to, from);

        var pc = _pos.MovedPiece(move);
        var pt = pc.Type();

        Span<char> re = stackalloc char[6];
        var i = 0;

        if (pt != PieceTypes.Pawn)
        {
            re[i++] = pc.GetUnicodeChar();
            foreach (var c in Disambiguation(move, from))
                re[i++] = c;
        }

        if (move.IsEnPassantMove())
        {
            re[i++] = 'e';
            re[i++] = 'p';
            re[i++] = from.FileChar;
        }
        else
        {
            if (_pos.GetPiece(to) != Piece.EmptyPiece)
            {
                if (pt == PieceTypes.Pawn)
                    re[i++] = from.FileChar;
                re[i++] = 'x';
            }
        }

        re[i++] = to.FileChar;
        re[i++] = to.RankChar;

        if (move.IsPromotionMove())
        {
            re[i++] = '=';
            re[i++] = move.PromotedPieceType().MakePiece(_pos.SideToMove).GetUnicodeChar();
        }

        if (_pos.InCheck)
            re[i++] = GetCheckChar();

        return new string(re[..i]);
    }

    /// <summary>
    /// <para>Converts a move to SAN notation.</para>
    /// </summary>
    /// <param name="move">The move to convert</param>
    /// <returns>SAN move string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string ToSan(Move move)
    {
        var (from, to) = move.FromTo();

        if (move.IsCastlelingMove())
            return CastlelingExtensions.GetCastlelingString(to, from);

        Span<char> re = stackalloc char[6];
        var i = 0;

        var pt = _pos.GetPieceType(from);

        if (pt != PieceTypes.Pawn)
        {
            re[i++] = _pos.GetPiece(from).GetPgnChar();
            foreach (var amb in Disambiguation(move, from))
                re[i++] = amb;
        }

        if (move.IsEnPassantMove())
        {
            re[i++] = 'e';
            re[i++] = 'p';
            re[i++] = from.FileChar;
        }
        else
        {
            if (_pos.GetPiece(to) != Piece.EmptyPiece)
            {
                if (pt == PieceTypes.Pawn)
                    re[i++] = from.FileChar;
                re[i++] = 'x';
            }
        }

        re[i++] = to.FileChar;
        re[i++] = to.RankChar;

        if (move.IsPromotionMove())
        {
            re[i++] = '=';
            re[i++] = move.PromotedPieceType().MakePiece(_pos.SideToMove).GetPgnChar();
        }

        if (_pos.InCheck)
            re[i++] = GetCheckChar();

        return new string(re[..i]);
    }

    /// <summary>
    /// <para>Converts a move to LAN notation.</para>
    /// </summary>
    /// <param name="move">The move to convert</param>
    /// <returns>LAN move string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string ToLan(Move move)
    {
        var (from, to) = move.FromTo();

        if (move.IsCastlelingMove())
            return CastlelingExtensions.GetCastlelingString(to, from);

        Span<char> re = stackalloc char[6];
        var i = 0;

        var pt = _pos.GetPieceType(from);

        if (pt != PieceTypes.Pawn)
            re[i++] = pt.GetPieceChar();

        re[i++] = from.FileChar;
        re[i++] = from.RankChar;

        if (move.IsEnPassantMove())
        {
            re[i++] = 'e';
            re[i++] = 'p';
            re[i++] = from.FileChar;
        }
        else
        {
            var capturedPiece = _pos.GetPiece(to);
            if (capturedPiece != Piece.EmptyPiece)
            {
                if (pt == PieceTypes.Pawn)
                    re[i++] = from.FileChar;

                re[i++] = 'x';
            }
            else
                re[i++] = '-';
        }

        re[i++] = to.FileChar;
        re[i++] = to.RankChar;

        if (move.IsPromotionMove())
        {
            re[i++] = '=';
            re[i++] = move.PromotedPieceType().MakePiece(_pos.SideToMove).GetUnicodeChar();
        }

        if (_pos.InCheck)
            re[i++] = GetCheckChar();

        return new string(re[..i]);
    }

    /// <summary>
    /// <para>Converts a move to RAN notation.</para>
    /// </summary>
    /// <param name="move">The move to convert</param>
    /// <returns>RAN move string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string ToRan(Move move)
    {
        var (from, to) = move.FromTo();

        if (move.IsCastlelingMove())
            return CastlelingExtensions.GetCastlelingString(to, from);

        Span<char> re = stackalloc char[6];
        var i = 0;

        var pt = _pos.GetPieceType(from);

        if (pt != PieceTypes.Pawn)
            re[i++] = pt.GetPieceChar();

        re[i++] = from.FileChar;
        re[i++] = from.RankChar;

        if (move.IsEnPassantMove())
        {
            re[i++] = 'e';
            re[i++] = 'p';
            re[i++] = from.FileChar;
        }
        else
        {
            var capturedPiece = _pos.GetPiece(to);
            if (capturedPiece != Piece.EmptyPiece)
            {
                if (pt == PieceTypes.Pawn)
                    re[i++] = from.FileChar;

                re[i++] = 'x';
                re[i++] = capturedPiece.Type().GetPieceChar();
            }
            else
                re[i++] = '-';
        }

        re[i++] = to.FileChar;
        re[i++] = to.RankChar;

        if (move.IsPromotionMove())
        {
            re[i++] = '=';
            re[i++] = move.PromotedPieceType().MakePiece(_pos.SideToMove).GetUnicodeChar();
        }

        if (_pos.InCheck)
            re[i++] = GetCheckChar();

        return new string(re[..i]);
    }

    private static string ToIccf(Move move)
    {
        var (from, to) = move.FromTo();

        Span<char> re = stackalloc char[5];
        var i = 0;

        re[i++] = (char)('1' + from.File.AsInt());
        re[i++] = (char)('1' + from.Rank.AsInt());
        re[i++] = (char)('1' + to.File.AsInt());
        re[i++] = (char)('1' + to.Rank.AsInt());

        if (move.IsPromotionMove())
        {
            var c = move.PromotedPieceType() switch
            {
                PieceTypes.Queen => 1,
                PieceTypes.Rook => 2,
                PieceTypes.Bishop => 3,
                PieceTypes.Knight => 4,
                _ => throw new NotImplementedException()
            };

            re[i++] = (char)('0' + c);
        }

        return new string(re[..i]);
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
        var pinned = _pos.PinnedPieces(c);

        while (similarTypeAttacks)
        {
            var square = BitBoards.PopLsb(ref similarTypeAttacks);

            if (pinned & square)
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
    /// Disambiguation.
    /// <para>If we have more then one piece with destination 'to'.</para>
    /// <para>Note that for pawns is not needed because starting file is explicit.</para>
    /// </summary>
    /// <param name="move">The move to check</param>
    /// <param name="from">The from square</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReadOnlySpan<char> Disambiguation(Move move, Square from)
    {
        var similarAttacks = GetSimilarAttacks(move);
        var ambiguity = Ambiguity(move, similarAttacks);

        if (!ambiguity.HasFlagFast(MoveAmbiguities.Move))
            return Array.Empty<char>();

        if (!ambiguity.HasFlagFast(MoveAmbiguities.File))
            return new[] { from.FileChar };

        return !ambiguity.HasFlagFast(MoveAmbiguities.Rank)
            ? new[] { from.RankChar }
            : new[] { from.FileChar, from.RankChar };
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

        return pt is PieceTypes.Pawn or PieceTypes.King
            ? BitBoard.Empty
            : _pos.GetAttacks(move.ToSquare(), pt, _pos.Pieces()) ^ from;
    }
}