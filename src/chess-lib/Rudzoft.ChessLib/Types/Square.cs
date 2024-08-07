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

using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Extensions;

namespace Rudzoft.ChessLib.Types;

public enum Squares : byte
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    a1 = 0, b1 = 1, c1 = 2, d1 = 3, e1 = 4, f1 = 5, g1 = 6, h1 = 7,
    a2 = 8, b2 = 9, c2 = 10, d2 = 11, e2 = 12, f2 = 13, g2 = 14, h2 = 15,
    a3 = 16, b3 = 17, c3 = 18, d3 = 19, e3 = 20, f3 = 21, g3 = 22, h3 = 23,
    a4 = 24, b4 = 25, c4 = 26, d4 = 27, e4 = 28, f4 = 29, g4 = 30, h4 = 31,
    a5 = 32, b5 = 33, c5 = 34, d5 = 35, e5 = 36, f5 = 37, g5 = 38, h5 = 39,
    a6 = 40, b6 = 41, c6 = 42, d6 = 43, e6 = 44, f6 = 45, g6 = 46, h6 = 47,
    a7 = 48, b7 = 49, c7 = 50, d7 = 51, e7 = 52, f7 = 53, g7 = 54, h7 = 55,
    a8 = 56, b8 = 57, c8 = 58, d8 = 59, e8 = 60, f8 = 61, g8 = 62, h8 = 63,
    none = 65
    // ReSharper restore InconsistentNaming
    // ReSharper restore UnusedMember.Global
}

public static class SquaresExtensions
{
    public static int AsInt(this Squares sq) => (int)sq;
}

/// <summary>
/// Square data struct. Contains a single enum value which represents a square on the board.
/// </summary>
public readonly record struct Square(Squares Value) : ISpanFormattable, IComparable<Square>
{
    private static readonly string[] SquareStrings = Enum
        .GetValues(typeof(Squares))
        .Cast<Squares>()
        .Take(64)
        .Select(static x => x.ToString())
        .ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Square(byte square) : this((Squares)square) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Square(int square) : this((Squares)square) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Square(Square sq) : this(sq.Value) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Square(int rank, int file) : this((Squares)(rank << 3) + (byte)file) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Square(Ranks r, Files f) : this(new(r), new File(f)) { }

    public Square(Rank r, File f) : this(r.AsInt(), f.AsInt()) { }

    public Square((Rank, File) rankFile) : this(rankFile.Item1, rankFile.Item2) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Square(string value) => new(new Square(value[1] - '1', value[0] - 'a'));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out Rank r, out File f)
    {
        r = Rank;
        f = File;
    }

    public Rank Rank => new(AsInt() >> 3);

    public char RankChar => Rank.Char;

    public File File => new(AsInt() & 7);

    public char FileChar => File.Char;

    public bool IsOk => ((int)Value).IsBetween((int)Squares.a1, (int)Squares.h8);

    public bool IsPromotionRank => (BitBoards.PromotionRanksBB & this).IsNotEmpty;

    public bool IsDark => (Types.Color.Black.ColorBB() & this).IsNotEmpty;

    public static Square None { get; } = new(Squares.none);

    public static Square A1 { get; } = new(Squares.a1);
    public static Square B1 { get; } = new(Squares.b1);
    public static Square C1 { get; } = new(Squares.c1);
    public static Square D1 { get; } = new(Squares.d1);
    public static Square E1 { get; } = new(Squares.e1);
    public static Square F1 { get; } = new(Squares.f1);
    public static Square G1 { get; } = new(Squares.g1);
    public static Square H1 { get; } = new(Squares.h1);

    public static Square A2 { get; } = new(Squares.a2);
    public static Square B2 { get; } = new(Squares.b2);
    public static Square C2 { get; } = new(Squares.c2);
    public static Square D2 { get; } = new(Squares.d2);
    public static Square E2 { get; } = new(Squares.e2);
    public static Square F2 { get; } = new(Squares.f2);
    public static Square G2 { get; } = new(Squares.g2);
    public static Square H2 { get; } = new(Squares.h2);

    public static Square A3 { get; } = new(Squares.a3);
    public static Square B3 { get; } = new(Squares.b3);
    public static Square C3 { get; } = new(Squares.c3);
    public static Square D3 { get; } = new(Squares.d3);
    public static Square E3 { get; } = new(Squares.e3);
    public static Square F3 { get; } = new(Squares.f3);
    public static Square G3 { get; } = new(Squares.g3);
    public static Square H3 { get; } = new(Squares.h3);

    public static Square A4 { get; } = new(Squares.a4);
    public static Square B4 { get; } = new(Squares.b4);
    public static Square C4 { get; } = new(Squares.c4);
    public static Square D4 { get; } = new(Squares.d4);
    public static Square E4 { get; } = new(Squares.e4);
    public static Square F4 { get; } = new(Squares.f4);
    public static Square G4 { get; } = new(Squares.g4);
    public static Square H4 { get; } = new(Squares.h4);

    public static Square A5 { get; } = new(Squares.a5);
    public static Square B5 { get; } = new(Squares.b5);
    public static Square C5 { get; } = new(Squares.c5);
    public static Square D5 { get; } = new(Squares.d5);
    public static Square E5 { get; } = new(Squares.e5);
    public static Square F5 { get; } = new(Squares.f5);
    public static Square G5 { get; } = new(Squares.g5);
    public static Square H5 { get; } = new(Squares.h5);

    public static Square A6 { get; } = new(Squares.a6);
    public static Square B6 { get; } = new(Squares.b6);
    public static Square C6 { get; } = new(Squares.c6);
    public static Square D6 { get; } = new(Squares.d6);
    public static Square E6 { get; } = new(Squares.e6);
    public static Square F6 { get; } = new(Squares.f6);
    public static Square G6 { get; } = new(Squares.g6);
    public static Square H6 { get; } = new(Squares.h6);

    public static Square A7 { get; } = new(Squares.a7);
    public static Square B7 { get; } = new(Squares.b7);
    public static Square C7 { get; } = new(Squares.c7);
    public static Square D7 { get; } = new(Squares.d7);
    public static Square E7 { get; } = new(Squares.e7);
    public static Square F7 { get; } = new(Squares.f7);
    public static Square G7 { get; } = new(Squares.g7);
    public static Square H7 { get; } = new(Squares.h7);

    public static Square A8 { get; } = new(Squares.a8);
    public static Square B8 { get; } = new(Squares.b8);
    public static Square C8 { get; } = new(Squares.c8);
    public static Square D8 { get; } = new(Squares.d8);
    public static Square E8 { get; } = new(Squares.e8);
    public static Square F8 { get; } = new(Squares.f8);
    public static Square G8 { get; } = new(Squares.g8);
    public static Square H8 { get; } = new(Squares.h8);

    public const int Count = 64;

    public static Square[] All =>
    [
        A1, A2, A3, A4, A5, A6, A7, A8,
        B1, B2, B3, B4, B5, B6, B7, B8,
        C1, C2, C3, C4, C5, C6, C7, C8,
        D1, D2, D3, D4, D5, D6, D7, D8,
        E1, E2, E3, E4, E5, E6, E7, E8,
        F1, F2, F3, F4, F5, F6, F7, F8,
        G1, G2, G3, G4, G5, G6, G7, G8,
        H1, H2, H3, H4, H5, H6, H7, H8
    ];

    public static readonly Range WhiteSide = new(0, 32);

    public static readonly Range BlackSide = new(32, 64);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square Create(Rank r, File f) => new(r, f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square Create(byte b) => new((Squares)b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Square(int value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Square(Squares sq) => new(sq);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Square((Rank, File) value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Square left, Squares right) => left.Value == right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Square left, Squares right) => left.Value != right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator +(Square left, Square right) => new(left.Value + (byte)right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator +(Square left, int right) => new(left.Value + (byte)right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator +(Square left, Direction right) => new(left.Value + (byte)right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator +(Square left, Directions right) => new(left.Value + (byte)right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator -(Square left, Square right) => new(left.Value - right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator -(Square left, int right) => new(left.Value - (byte)right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator -(Square left, Direction right) => new(left.Value - (byte)right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator -(Square left, Directions right) => new(left.Value - (byte)right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator ++(Square sq) => new(sq.Value + 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator --(Square sq) => new(sq.Value - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator &(Square left, ulong right) => new(left.AsBb().Value & right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator &(ulong left, Square right) => left & right.AsBb();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator |(Square left, Square right) => new(left.AsBb().Value | right.AsBb().Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator |(ulong left, Square right) => new(left | right.AsBb().Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator |(Square left, int right) => left.AsInt() | right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator ~(Square left) => ~left.AsBb();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator >> (Square left, int right) => left.AsInt() >> right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Square left, Square right) => left.Value > right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Square left, Square right) => left.Value < right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Square left, Square right) => left.Value <= right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Square left, Square right) => left.Value >= right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator true(Square sq) => sq.IsOk;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator false(Square sq) => !sq.IsOk;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator byte(Square sq) => (byte)sq.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Square Relative(Color c) => (int)Value ^ (c.Side * 56);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Square Max(Square other) => Value > other.Value ? this : other;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Square Min(Square other) => Value <= other.Value ? this : other;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => SquareStrings[AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(string format, IFormatProvider formatProvider)
        => string.Format(formatProvider, format, ToString());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format = default, IFormatProvider provider = null)
    {
        destination[0] = FileChar;
        destination[1] = RankChar;
        charsWritten = 2;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Square other) => Value == other.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int AsInt() => (int)Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard AsBb() => BitBoards.BbSquares[AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rank RelativeRank(Color c) => Rank.Relative(c);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOppositeColor(Square other)
        => ((Value.AsInt() + Rank.AsInt() + (int)other.Value + other.Rank.AsInt()) & 1) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Square other)
    {
        if (Value < other.Value)
            return -1;
        return (Value > other.Value).AsByte();
    }

    /// <summary>
    /// Swap A1 &lt;-&gt; H1
    /// </summary>
    /// <returns>Flipped square by File</returns>
    public Square FlipFile() => AsInt() ^ Squares.h1.AsInt();

    /// <summary>
    /// Swap A1 &lt;-&gt; A8
    /// </summary>
    /// <returns>Flipped square by Rank</returns>
    public Square FlipRank() => AsInt() ^ Squares.a8.AsInt();

    public Color Color() => ((AsInt() + Rank) ^ 1) & 1;
}