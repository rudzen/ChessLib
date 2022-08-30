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

using System.Runtime.CompilerServices;

namespace Rudz.Chess.Types;

public enum Ranks
{
    Rank1 = 0,
    Rank2 = 1,
    Rank3 = 2,
    Rank4 = 3,
    Rank5 = 4,
    Rank6 = 5,
    Rank7 = 6,
    Rank8 = 7,
    RankNb = 8
}

public static class RanksExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank RelativeRank(this Ranks rank, Player color) => (Ranks)(rank.AsInt() ^ (color.Side * 7));

    public static int AsInt(this Ranks r) => (int)r;
}

public readonly record struct Rank(Ranks Value)
{
    private static readonly string[] RankStrings = { "1", "2", "3", "4", "5", "6", "7", "8" };

    public static Rank RANK_1 { get; } = new(Ranks.Rank1);
    public static Rank RANK_2 { get; } = new(Ranks.Rank2);
    public static Rank RANK_3 { get; } = new(Ranks.Rank3);
    public static Rank RANK_4 { get; } = new(Ranks.Rank4);
    public static Rank RANK_5 { get; } = new(Ranks.Rank5);
    public static Rank RANK_6 { get; } = new(Ranks.Rank6);
    public static Rank RANK_7 { get; } = new(Ranks.Rank7);
    public static Rank RANK_8 { get; } = new(Ranks.Rank8);

    public static Rank[] PawnRanks { get; } = { RANK_2, RANK_3, RANK_4, RANK_5, RANK_6, RANK_7 };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rank(int rank) : this((Ranks)rank)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rank(Rank rank) :this(rank.Value)
    {
    }

    public char Char => (char)('1' + Value.AsInt());

    public static int Count => (int)Ranks.RankNb;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Rank(int value)
        => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Rank(Ranks value)
        => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Rank left, Ranks right)
        => left.Value == right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Rank left, Ranks right)
        => left.Value != right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Rank left, int right)
        => left.Value == (Ranks)right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Rank left, int right)
        => left.Value != (Ranks)right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator +(Rank left, Rank right)
        => left.AsInt() + right.AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator +(Rank left, int right)
        => left.AsInt() + right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator +(Rank left, Ranks right)
        => left.AsInt() + (int)right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator -(Rank left, Rank right)
        => left.AsInt() - right.AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator -(Rank left, int right)
        => left.AsInt() - right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator -(Rank left, Ranks right)
        => left.AsInt() - (int)right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator ++(Rank rank)
        => rank.Value + 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator --(Rank rank)
        => rank.Value - 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator &(Rank left, ulong right)
        => left.BitBoardRank().Value & right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator &(ulong left, Rank right)
        => left & right.BitBoardRank().Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator |(Rank left, Rank right)
        => left.BitBoardRank() | right.BitBoardRank();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator |(ulong left, Rank right)
        => left | right.BitBoardRank().Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator |(Rank left, int right)
        => left.AsInt() | right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator ~(Rank left)
        => ~left.BitBoardRank();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator >>(Rank left, int right)
        => left.AsInt() >> right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Rank left, Ranks right)
        => left.Value >= right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Rank left, Rank right)
        => left.AsInt() > right.AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Rank left, Ranks right)
        => left.Value <= right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Rank left, Rank right)
        => left.AsInt() < right.AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator true(Rank sq)
        => sq.IsValid();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator false(Rank sq)
        => !sq.IsValid();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int AsInt()
        => (int)Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsValid()
        => Value < Ranks.RankNb;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
        => RankStrings[AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Rank other)
        => Value == other.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rank Relative(Player color) 
        => AsInt() ^ (color.Side * 7);
}
