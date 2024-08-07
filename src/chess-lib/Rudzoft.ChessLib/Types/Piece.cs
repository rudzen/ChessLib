﻿/*
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

using System.Numerics;
using System.Runtime.CompilerServices;
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

/// <summary>
/// Piece. Contains the piece type which indicate what type and color the piece is
/// </summary>
public readonly record struct Piece(Pieces Value) : IMinMaxValue<Piece>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Piece(int pc) : this((Pieces)pc) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Piece(Piece pc) : this(pc.Value) { }

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

    public static Piece[] All { get; } =
    [
        WhitePawn, WhiteKnight, WhiteBishop, WhiteRook, WhiteQueen, WhiteKing,
        BlackPawn, BlackKnight, BlackBishop, BlackRook, BlackQueen, BlackKing
    ];

    public static Piece MaxValue => WhitePawn;

    public static Piece MinValue => BlackKing;

    public static Range WhitePieces => new(0, 5);

    public static Range BlackPieces => new(6, 11);

    public static PieceType[] AllTypes { get; } =
    [
        PieceType.Pawn,
        PieceType.Knight,
        PieceType.Bishop,
        PieceType.Rook,
        PieceType.Queen,
        PieceType.King
    ];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Piece(char value) => new(GetPiece(value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Piece(int value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Piece(Pieces pc) => new(pc);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator ~(Piece pc) => new(pc ^ 8);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator +(Piece left, Color right) => new(left.Value + (byte)(right << 3));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator >> (Piece left, int right) => new((int)left.Value >> right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator <<(Piece left, int right) => new((int)left.Value << right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Piece left, Pieces right) => left.Value == right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Piece left, Pieces right) => left.Value != right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Piece left, Pieces right) => left.Value <= right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Piece left, Pieces right) => left.Value >= right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Piece left, Pieces right) => left.Value < right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Piece left, Pieces right) => left.Value > right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator ++(Piece left) => new(left.Value + 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator --(Piece left) => new(left.Value - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator true(Piece pc) => pc != EmptyPiece;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator false(Piece pc) => pc == EmptyPiece;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator byte(Piece pc) => (byte)pc.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color ColorOf() => new((int)Value >> 3);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Piece other) => Value == other.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => (int)Value << 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => this.GetPieceString();

    public static Piece GetPiece(char t)
    {
        var pcIndex = PieceExtensions.PieceChars.IndexOf(t);
        if (pcIndex == -1)
            return EmptyPiece;

        Color c = new(char.IsLower(PieceExtensions.PieceChars[pcIndex]));
        return new PieceType(pcIndex).MakePiece(c);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out PieceType pt, out Color c)
    {
        pt = Type();
        c = ColorOf();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int AsInt() => (int)Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PieceType Type() => (PieceTypes)(AsInt() & 0x7);
}