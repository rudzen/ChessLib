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

namespace Rudz.Chess.Types;

using Enums;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

public readonly struct Rank : IEquatable<Rank>
{
    private static readonly char[] RankChars = { '1', '2', '3', '4', '5', '6', '7', '8' };

    private static readonly string[] RankStrings = RankChars.Select(x => x.ToString()).ToArray();

    private readonly Ranks _value;

    public static Rank RANK_1 { get; } = new(Ranks.Rank1);
    public static Rank RANK_2 { get; } = new(Ranks.Rank2);
    public static Rank RANK_3 { get; } = new(Ranks.Rank3);
    public static Rank RANK_4 { get; } = new(Ranks.Rank4);
    public static Rank RANK_5 { get; } = new(Ranks.Rank5);
    public static Rank RANK_6 { get; } = new(Ranks.Rank6);
    public static Rank RANK_7 { get; } = new(Ranks.Rank7);
    public static Rank RANK_8 { get; } = new(Ranks.Rank8);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rank(int file) => _value = (Ranks)file;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rank(Ranks file) => _value = file;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rank(Rank file) => _value = file._value;

    public char Char => RankChars[AsInt()];

    public static int Count => (int)Ranks.RankNb;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Rank(int value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Rank(Ranks value) => new(value);

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
    public static Rank operator ++(Rank rank) => rank._value + 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator --(Rank rank) => rank._value - 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator &(Rank left, ulong right) => left.BitBoardRank().Value & right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator &(ulong left, Rank right) => left & right.BitBoardRank().Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator |(Rank left, Rank right) => left.BitBoardRank() | right.BitBoardRank();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator |(ulong left, Rank right) => left | right.BitBoardRank().Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator |(Rank left, int right) => left.AsInt() | right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator ~(Rank left) => ~left.BitBoardRank();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator >>(Rank left, int right) => left.AsInt() >> right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Rank left, Ranks right) => left._value >= right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Rank left, Rank right) => left.AsInt() > right.AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Rank left, Ranks right) => left._value <= right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Rank left, Rank right) => left.AsInt() < right.AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator true(Rank sq) => sq.IsValid();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator false(Rank sq) => !sq.IsValid();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int AsInt() => (int)_value;

    public bool IsValid() => _value < Ranks.RankNb;

    public override string ToString() => RankStrings[(int)_value + 1];

    public bool Equals(Rank other) => _value == other._value;

    public override bool Equals(object obj) => obj is Rank file && Equals(file);

    public override int GetHashCode() => AsInt();

    public Rank RelativeRank(Player color) => AsInt() ^ (color.Side * 7);
}
