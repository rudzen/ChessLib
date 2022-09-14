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
// ReSharper disable UnusedMember.Global

namespace Rudzoft.ChessLib.Types;

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
    public static Rank RelativeRank(this Ranks r, Player p) => new((Ranks)(r.AsInt() ^ (p.Side * 7)));

    public static int AsInt(this Ranks r) => (int)r;
}

public readonly record struct Rank(Ranks Value) : ISpanFormattable, IValidationType
{
    private static readonly string[] RankStrings = { "1", "2", "3", "4", "5", "6", "7", "8" };

    public static Rank Rank1 { get; } = new(Ranks.Rank1);
    public static Rank Rank2 { get; } = new(Ranks.Rank2);
    public static Rank Rank3 { get; } = new(Ranks.Rank3);
    public static Rank Rank4 { get; } = new(Ranks.Rank4);
    public static Rank Rank5 { get; } = new(Ranks.Rank5);
    public static Rank Rank6 { get; } = new(Ranks.Rank6);
    public static Rank Rank7 { get; } = new(Ranks.Rank7);
    public static Rank Rank8 { get; } = new(Ranks.Rank8);

    public static Rank[] PawnRanks { get; } = { Rank2, Rank3, Rank4, Rank5, Rank6, Rank7 };

    public static Rank[] All { get; } = { Rank1, Rank2, Rank3, Rank4, Rank5, Rank6, Rank7, Rank8 };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rank(int rank)
        : this((Ranks)rank) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rank(Rank r)
        : this(r.Value) { }

    public char Char => (char)('1' + Value.AsInt());

    public bool IsOk
        => Value.AsInt().InBetween(Ranks.Rank1.AsInt(), Ranks.Rank8.AsInt());

    public const int Count = (int)Ranks.RankNb;

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
        => new(left.AsInt() + right.AsInt());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator +(Rank left, int right)
        => new(left.AsInt() + right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator +(Rank left, Ranks right)
        => new(left.AsInt() + (int)right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator -(Rank left, Rank right)
        => new(left.AsInt() - right.AsInt());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator -(Rank left, int right)
        => new(left.AsInt() - right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator -(Rank left, Ranks right)
        => new(left.AsInt() - (int)right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator ++(Rank r)
        => new(r.Value + 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator --(Rank r)
        => new(r.Value - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator &(Rank left, ulong right)
        => new(left.BitBoardRank().Value & right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator &(ulong left, Rank right)
        => new(left & right.BitBoardRank().Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator |(Rank left, Rank right)
        => left.BitBoardRank() | right.BitBoardRank();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator |(ulong left, Rank right)
        => new(left | right.BitBoardRank().Value);

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
    public static bool operator >=(Rank left, Rank right)
        => left.Value >= right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Rank left, Ranks right)
        => left.Value >= right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Rank left, Rank right)
        => left.AsInt() > right.AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Rank left, Rank right)
        => left.Value <= right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Rank left, Ranks right)
        => left.Value <= right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Rank left, Rank right)
        => left.AsInt() < right.AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator true(Rank r)
        => r.IsOk;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator false(Rank r)
        => !r.IsOk;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int AsInt()
        => (int)Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
        => RankStrings[AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(string format, IFormatProvider formatProvider)
        => string.Format(formatProvider, format, ToString());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
    {
        destination[0] = Char;
        charsWritten = 1;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Rank other)
        => Value == other.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rank Relative(Player p) 
        => new(AsInt() ^ (p.Side * 7));

    /// <summary>
    /// Fold rank [12345678] to rank [12344321]
    /// </summary>
    /// <returns>The distance to the edge rank</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EdgeDistance()
        => Math.Min(AsInt() - Rank1.AsInt(), Rank8.AsInt() - AsInt());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rank Clamp(Rank min, Rank max)
        => new(Value.AsInt().Clamp(min.AsInt(), max.AsInt()));
}
