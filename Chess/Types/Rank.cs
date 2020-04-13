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

    public struct Rank
    {
        private Ranks _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rank(int file) => _value = (Ranks)file;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rank(Ranks file) => _value = file;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rank(Rank file) => _value = file._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Rank(int value) => new Rank(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Rank(Ranks value) => new Rank(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Rank left, Rank right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Rank left, Rank right) => !left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Rank left, Ranks right) => left._value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Rank left, Ranks right) => left._value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Rank left, int right) => left._value == (Ranks)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Rank left, int right) => left._value != (Ranks)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank operator +(Rank left, Rank right) => left.AsInt() + right.AsInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank operator +(Rank left, int right) => left.AsInt() + right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank operator +(Rank left, Ranks right) => left.AsInt() + (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank operator -(Rank left, Rank right) => left.AsInt() - right.AsInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank operator -(Rank left, int right) => left.AsInt() - right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank operator -(Rank left, Ranks right) => left.AsInt() - (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank operator ++(Rank rank) => ++rank._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator &(Rank left, ulong right) => left.BitBoardRank() & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator &(ulong left, Rank right) => left & right.BitBoardRank();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator |(Rank left, Rank right) => left.BitBoardRank() | right.BitBoardRank();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator |(ulong left, Rank right) => left | right.BitBoardRank();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Rank left, int right) => left.AsInt() | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator ~(Rank left) => ~left.BitBoardRank();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(Rank left, int right) => left.AsInt() >> right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Rank left, Rank right) => left.AsInt() > right.AsInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Rank left, Rank right) => left.AsInt() < right.AsInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator true(Rank sq) => sq.IsValid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator false(Rank sq) => !sq.IsValid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int AsInt() => (int)_value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsValid() => _value < Ranks.RankNb;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => ((int) _value + 1).ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Rank other) => _value == other._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is Rank file && Equals(file);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => AsInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Rank RelativeRank(Player color) => AsInt() ^ (color.Side * 7);
    }
}