/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2020 Rudy Alex Kohn

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

namespace Rudz.Chess.Types
{
    using Enums;
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Square data struct. Contains a single enum value which represents a square on the board.
    /// </summary>
    public readonly struct Square : IComparable<Square>
    {
        private static readonly string[] SquareStrings;

        static Square()
        {
            SquareStrings = Enum.GetValues(typeof(Squares)).Cast<Squares>().Take(64).Select(x => x.ToString()).ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square(int square) => Value = (Squares)square;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square(Square square) => Value = square.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square(int rank, int file)
            : this((Squares)(rank << 3) + file) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square(Ranks rank, Files file)
            : this((int)rank, (int)file) { }

        public Square(Rank rank, File file)
            : this(rank.AsInt(), file.AsInt()) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Square(Squares square) => Value = square;

        public readonly Squares Value;

        public Rank Rank => (Ranks)(AsInt() >> 3);

        public char RankChar => Rank.Char;

        public File File => AsInt() & 7;

        public char FileChar => File.FileChar();

        public bool IsOk => Value >= Squares.a1 && Value <= Squares.h8;

        public bool IsPromotionRank => !(BitBoards.PromotionRanksBB & this).IsEmpty;

        public bool IsDark => !(BitBoards.DarkSquares & this).IsEmpty;

        public static readonly Square None = new Square(Squares.none);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Square(int value)
            => new Square(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Square(Squares value)
            => new Square(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Square left, Square right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Square left, Square right)
            => !left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Square left, Squares right)
            => left.Value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Square left, Squares right)
            => left.Value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator +(Square left, Square right)
            => left.Value + right.AsInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator +(Square left, int right)
            => left.Value + right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator +(Square left, Direction right)
            => left.Value + (int)right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator +(Square left, Directions right)
            => left.Value + (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator -(Square left, Square right)
            => left.Value - right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator -(Square left, int right)
            => left.Value - right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator -(Square left, Direction right)
            => left.Value - (int)right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator -(Square left, Directions right)
            => left.Value - (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator ++(Square square)
            => new Square(square.Value + 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator --(Square square)
            => new Square(square.Value - 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator &(Square left, ulong right)
            => left.AsBb().Value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator &(ulong left, Square right)
            => left & right.AsBb();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator |(Square left, Square right)
            => left.AsBb().Value | right.AsBb().Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator |(ulong left, Square right)
            => left | right.AsBb().Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Square left, int right)
            => left.AsInt() | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator ~(Square left)
            => ~left.AsBb();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(Square left, int right)
            => left.AsInt() >> right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Square left, Square right)
            => left.Value > right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Square left, Square right)
            => left.Value < right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Square left, Square right)
            => left.Value <= right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Square left, Square right)
            => left.Value >= right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator true(Square sq)
            => sq.IsOk;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator false(Square sq)
            => !sq.IsOk;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square Relative(Player p)
            => (int)Value ^ (p.Side * 56);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square Max(Square other)
            => Value > other.Value ? this : other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square Min(Square other)
            => Value <= other.Value ? this : other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => SquareStrings[AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Square other)
            => Value == other.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is Square square && Equals(square);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => AsInt();

        public int AsInt()
            => (int)Value;

        public BitBoard AsBb()
            => BitBoards.BbSquares[AsInt()];

        public Rank RelativeRank(Player color)
            => Rank.RelativeRank(color);

        public bool IsOppositeColor(Square other)
            => (((int)Value + Rank.AsInt() + (int)other.Value + other.Rank.AsInt()) & 1) != 0;

        public int CompareTo(Square other)
            => Value.CompareTo(other.Value);
    }
}