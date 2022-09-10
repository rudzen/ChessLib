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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Extensions;

namespace Rudzoft.ChessLib.Types;

public enum Pieces : byte
{
    NoPiece = 0,
    WhitePawn = 1,
    WhiteKnight = 2,
    WhiteBishop = 3,
    WhiteRook = 4,
    WhiteQueen = 5,
    WhiteKing = 6,
    BlackPawn = 9,
    BlackKnight = 10,
    BlackBishop = 11,
    BlackRook = 12,
    BlackQueen = 13,
    BlackKing = 14,
    PieceNb = 15
}

public enum PieceTypes
{
    NoPieceType = 0,
    Pawn = 1,
    Knight = 2,
    Bishop = 3,
    Rook = 4,
    Queen = 5,
    King = 6,
    PieceTypeNb = 7,
    AllPieces = 0
}

public static class PieceTypesExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AsInt(this PieceTypes p) => (int)p;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece MakePiece(this PieceTypes @this, Player side) => (int)@this | (side.Side << 3);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSlider(this PieceTypes @this) => @this.InBetween(PieceTypes.Bishop, PieceTypes.Queen);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InBetween(this PieceTypes v, PieceTypes min, PieceTypes max) =>
        (uint)v - (uint)min <= (uint)max - (uint)min;
}

/// <inheritdoc />
/// <summary>
/// Piece. Contains the piece type which indicate what type and color the piece is
/// </summary>
public readonly record struct Piece(Pieces Value)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Piece(int pc)
        : this((Pieces)pc) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Piece(Piece pc)
        : this(pc.Value) { }

    public const int Count = (int)Pieces.PieceNb + 1;

    public bool IsWhite => ColorOf().IsWhite;

    public bool IsBlack => ColorOf().IsBlack;

    public static Piece EmptyPiece { get; } = new(Pieces.NoPiece);
    public static Piece WhitePawn { get; } = new(Pieces.WhitePawn);
    public static Piece WhiteKnight { get; } = new(Pieces.WhiteKnight);
    public static Piece WhiteBishop { get; } = new(Pieces.WhiteBishop);
    public static Piece WhiteRook { get; } = new(Pieces.WhiteRook);
    public static Piece WhiteQueen { get; } = new(Pieces.WhiteQueen);
    public static Piece WhiteKing { get; } = new(Pieces.WhiteKing);
    public static Piece BlackPawn { get; } = new(Pieces.BlackPawn);
    public static Piece BlackKnight { get; } = new(Pieces.BlackKnight);
    public static Piece BlackBishop { get; } = new(Pieces.BlackBishop);
    public static Piece BlackRook { get; } = new(Pieces.BlackRook);
    public static Piece BlackQueen { get; } = new(Pieces.BlackQueen);
    public static Piece BlackKing { get; } = new(Pieces.BlackKing);


    public static Piece[] AllPieces { get; } =
    {
        Pieces.WhitePawn,
        Pieces.WhiteKnight,
        Pieces.WhiteBishop,
        Pieces.WhiteRook,
        Pieces.WhiteQueen,
        Pieces.WhiteKing,
        Pieces.BlackPawn,
        Pieces.BlackKnight,
        Pieces.BlackBishop,
        Pieces.BlackRook,
        Pieces.BlackQueen,
        Pieces.BlackKing
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Piece(char value)
        => new(GetPiece(value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Piece(int value)
        => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Piece(Pieces pc)
        => new(pc);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator ~(Piece pc)
        => new(pc.AsInt() ^ 8);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator +(Piece left, Player right)
        => new(left.Value + (byte)(right << 3));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator >> (Piece left, int right)
        => new((int)left.Value >> right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator <<(Piece left, int right)
        => new((int)left.Value << right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Piece left, Pieces right)
        => left.Value == right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Piece left, Pieces right)
        => left.Value != right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Piece left, Pieces right)
        => left.Value <= right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Piece left, Pieces right)
        => left.Value >= right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Piece left, Pieces right)
        => left.Value < right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Piece left, Pieces right)
        => left.Value > right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator ++(Piece left)
        => new(left.Value + 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator --(Piece left)
        => new(left.Value - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator true(Piece pc)
        => pc.Value.AsInt() != EmptyPiece.AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator false(Piece pc)
        => pc.Value.AsInt() == EmptyPiece.AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Player ColorOf()
        => new((int)Value >> 3);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Piece other)
        => Value == other.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => (int)Value << 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
        => this.GetPieceString();

    private static Piece GetPiece(char character)
    {
        return character switch
        {
            'P' => Pieces.WhitePawn,
            'p' => Pieces.BlackPawn,
            'N' => Pieces.WhiteKnight,
            'B' => Pieces.WhiteBishop,
            'R' => Pieces.WhiteRook,
            'Q' => Pieces.WhiteQueen,
            'K' => Pieces.WhiteKing,
            'n' => Pieces.BlackKnight,
            'b' => Pieces.BlackBishop,
            'r' => Pieces.BlackRook,
            'q' => Pieces.BlackQueen,
            'k' => Pieces.BlackKing,
            _ => Pieces.NoPiece
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out PieceTypes pt, out Player p)
    {
        pt = Type();
        p = ColorOf();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int AsInt()
        => (int)Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PieceTypes Type()
        => (PieceTypes)(AsInt() & 0x7);
}