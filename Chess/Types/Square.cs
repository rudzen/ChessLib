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
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Square data struct.
    /// Contains a single enum value which represents a square on the board.
    /// </summary>
    public struct Square
    {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Square(Squares square) => Value = square;

        public Squares Value { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Square(int value) => new Square(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Square(Squares value) => new Square(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Square left, Square right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Square left, Square right) => !left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Square left, Squares right) => left.Value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Square left, Squares right) => left.Value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator +(Square left, Square right) => left.AsInt() + right.AsInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator +(Square left, int right) => left.AsInt() + right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator +(Square left, Direction right) => left.AsInt() + (int)right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator +(Square left, Directions right) => left.AsInt() + (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator -(Square left, Square right) => left.AsInt() - right.AsInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator -(Square left, int right) => left.AsInt() - right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator -(Square left, Direction right) => left.AsInt() - (int)right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator -(Square left, Directions right) => left.AsInt() - (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator ++(Square square) => ++square.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator &(Square left, ulong right) => left.BitBoardSquare() & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator &(ulong left, Square right) => left & right.BitBoardSquare();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator |(Square left, Square right) => left.BitBoardSquare() | right.BitBoardSquare();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator |(ulong left, Square right) => left | right.BitBoardSquare();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Square left, int right) => left.AsInt() | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator ~(Square left) => ~left.BitBoardSquare();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(Square left, int right) => left.AsInt() >> right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Square left, Square right) => left.AsInt() > right.AsInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Square left, Square right) => left.AsInt() < right.AsInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator true(Square sq) => sq.IsValid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator false(Square sq) => !sq.IsValid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square Relative(Player p) => (int) Value ^ (p.Side * 56);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => this.GetSquareString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Square other) => Value == other.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is Square square && Equals(square);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => AsInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int AsInt() => (int)Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public File File() => AsInt() & 7;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rank Rank() => (Ranks)(AsInt() >> 3);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rank RelativeRank(Player color) => Rank().RelativeRank(color);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOppositeColor(Square other) => (((int)Value + Rank().AsInt() + (int)other.Value + other.Rank().AsInt()) & 1) != 0;
    }
}