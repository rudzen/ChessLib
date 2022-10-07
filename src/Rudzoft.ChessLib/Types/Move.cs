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
using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Extensions;

namespace Rudzoft.ChessLib.Types;

public enum MoveTypes : ushort
{
    Normal = 0,
    Promotion = 1 << 14,
    Enpassant = 2 << 14,
    Castling = 3 << 14
}

public static class MoveTypesExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AsInt(this MoveTypes @this) => (int)@this;
}

/// <summary>
/// Move struct. Contains a single ushort for move related information. Also includes set and
/// get functions for the relevant data stored in the bits.
/// </summary>
public record struct Move(ushort Data) : ISpanFormattable
{
    private const int MaxMoveStringSize = 5;

    public Move(Square from, Square to) : this((ushort)(to | (from.AsInt() << 6)))
    {
    }

    public Move(Square from, Square to, MoveTypes moveType, PieceTypes promoPt = PieceTypes.Knight)
        : this((ushort)(to | (from.AsInt() << 6) | moveType.AsInt() | ((promoPt - PieceTypes.Knight) << 12)))
    {
    }

    public static readonly Move EmptyMove = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out Square from, out Square to)
    {
        from = FromSquare();
        to = ToSquare();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out Square from, out Square to, out MoveTypes type)
    {
        from = FromSquare();
        to = ToSquare();
        type = MoveType();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Move(string value)
        => new(new Square(value[1] - '1', value[0] - 'a'), new Square(value[3] - '1', value[2] - 'a'));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Move(ExtMove extMove)
        => extMove.Move;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Move(ushort value)
        => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Move Create(Square from, Square to)
        => new(from, to);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Create(Span<ExtMove> moves, int index, Square from, ref BitBoard to)
    {
        while (to)
            moves[index++].Move = Create(from, BitBoards.PopLsb(ref to));
        return index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Move Create(Square from, Square to, MoveTypes moveType, PieceTypes promoPt = PieceTypes.Knight)
        => new(from, to, moveType, promoPt);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Square FromSquare()
        => new((Data >> 6) & 0x3F);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Square ToSquare()
        => new(Data & 0x3F);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PieceTypes PromotedPieceType()
        => (PieceTypes)(((Data >> 12) & 3) + 2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsQueenPromotion()
        => PromotedPieceType() == PieceTypes.Queen;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveTypes MoveType()
        => (MoveTypes)(Data & (3 << 14));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsType(MoveTypes moveType)
        => MoveType() == moveType;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEnPassantMove()
        => MoveType() == MoveTypes.Enpassant;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsCastleMove()
        => MoveType() == MoveTypes.Castling;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsPromotionMove()
        => MoveType() == MoveTypes.Promotion;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNullMove()
        => Data == 0;

    /// <summary>
    /// Makes no assumption of the legality of the move,
    /// only if it is different squares
    /// </summary>
    /// <returns>true if not the same from and to square</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsValidMove()
        => FromSquare() != ToSquare();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Move other)
        => Data == other.Data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => Data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        if (IsNullMove())
            return "(null)";

        Span<char> s = stackalloc char[MaxMoveStringSize];
        return TryFormat(s, out var size)
            ? new string(s[..size])
            : "(error)";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(string format, IFormatProvider formatProvider = null)
        => string.Format(formatProvider, format, ToString());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format = default,
        IFormatProvider provider = null)
    {
        var (from, to) = this;
        switch (MoveType())
        {
            case MoveTypes.Normal or MoveTypes.Enpassant:
                destination[0] = from.FileChar;
                destination[1] = from.RankChar;
                destination[2] = to.FileChar;
                destination[3] = to.RankChar;
                charsWritten = 4;
                return true;
            case MoveTypes.Castling:
                destination[0] = 'O';
                destination[1] = '-';
                destination[2] = 'O';

                if (to >= from)
                {
                    charsWritten = 3;
                    return true;
                }

                destination[3] = '-';
                destination[4] = 'O';
                charsWritten = MaxMoveStringSize;

                return true;
            case MoveTypes.Promotion:
                destination[0] = from.FileChar;
                destination[1] = from.RankChar;
                destination[2] = to.FileChar;
                destination[3] = to.RankChar;
                destination[4] = PromotedPieceType().GetPromotionChar();
                charsWritten = MaxMoveStringSize;
                return true;
            default:
                charsWritten = 0;
                return false;
        }
    }
}