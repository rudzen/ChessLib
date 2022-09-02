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

namespace Rudzoft.ChessLib.Types;

public enum Depths
{
    Zero = 0,
#pragma warning disable CA1069
    QsCheck = 0,
#pragma warning restore CA1069
    QsNoCheck = -1,
    QsRecap = -5,
    None = -6,
    Offset = None - 1,
    MaxPly = 256 + Offset - 4, // Used only for TT entry occupancy check
}

public static class DepthsExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AsInt(this Depths @this) => (int)@this;
}

public struct Depth : IEquatable<Depth>
{
    private Depth(int depth) => Value = depth;

    private Depth(Depths depth) => Value = (int)depth;

    public int Value { get; set; }

    public static Depth Zero => new(Depths.Zero);

    public static Depth QsCheck => new(Depths.QsCheck);

    public static Depth QsNoCheck => new(Depths.QsNoCheck);

    public static Depth QsRecap => new(Depths.QsRecap);

    public static Depth None => new(Depths.None);

    public static Depth MaxPly => new(Depths.MaxPly);

    public static Depth Offset => new(Depths.Offset);

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
    public static implicit operator Depth(byte value)
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
    public static Depth operator -(Depth left, Depth right) => new(left.Value - right.Value);

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