using System;
using System.Runtime.CompilerServices;
using Rudz.Chess.Extensions;

namespace Rudz.Chess.Types;

public enum Depths
{
    ZERO = 0,
    QS_CHECK = 0,
    QS_NO_CHECK = -1,
    QS_RECAP = -5,
    NONE = -6,
    OFFSET = NONE - 1,
    MAX_PLY = 256 + OFFSET - 4, // Used only for TT entry occupancy check
}

public static class DepthsExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AsInt(this Depths @this) => (int)@this;
}

public readonly struct Depth : IEquatable<Depth>
{
    public Depth(int depth) => Value = depth;

    public Depth(Depths depth) => Value = (int)depth;

    public int Value { get; }

    public static Depth Zero => new(Depths.ZERO);

    public static Depth QsCheck => new(Depths.QS_CHECK);

    public static Depth QsNoCheck => new(Depths.QS_NO_CHECK);

    public static Depth QsRecap => new(Depths.QS_RECAP);

    public static Depth None => new(Depths.NONE);

    public static Depth MaxPly => new(Depths.MAX_PLY);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Depth(string value)
        => new(Maths.ToIntegral(value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Depth(Depths value)
        => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Depth(int value)
        => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Depth left, Depth right)
        => left.Value == right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Depth left, Depth right)
        => left.Value != right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Depth left, Depth right) => left.Value <= right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Depth left, Depth right) => left.Value >= right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Depth left, Depth right) => left.Value < right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Depth left, Depth right) => left.Value > right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Depth operator ++(Depth left) => new(left.Value + 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Depth operator --(Depth left) => new(left.Value - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Depth left, int right)
        => left.Value == right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Depth left, int right)
        => left.Value != right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Depth left, Depths right)
        => left.Value == right.AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Depth left, Depths right)
        => left.Value != right.AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Depth other)
        => Value == other.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj)
        => obj is Depth other && Equals(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => Value;
}