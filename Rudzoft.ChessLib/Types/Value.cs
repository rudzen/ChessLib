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

namespace Rudzoft.ChessLib.Types;

/// <summary>
/// Represents a single value. Used for e.g. Piece value etc
/// </summary>
public readonly struct Value : IEquatable<Value>
{
    public readonly PieceValues Raw;

    public Value(Value value)
        : this(value.Raw) { }

    public Value(int value)
        : this((PieceValues)value) { }

    private Value(PieceValues value) => Raw = value;

    public static Value ValueZero { get; } = new(PieceValues.ValueZero);

    public static Value Infinite { get; } = new(PieceValues.ValueInfinite);

    public static Value MinusInfinite { get; } = new(PieceValues.ValueMinusInfinite);

    public static implicit operator Value(int value)
        => new(value);

    public static implicit operator Value(PieceValues value)
        => new(value);

    public static bool operator ==(Value left, Value right)
        => left.Equals(right);

    public static bool operator !=(Value left, Value right)
        => !left.Equals(right);

    public static Value operator +(Value left, Value right)
        => new((int)left.Raw + (int)right.Raw);

    public static Value operator +(Value left, int right)
        => new((int)left.Raw + right);

    public static Value operator +(Value left, PieceValues right)
        => new((int)left.Raw + (int)right);

    public static Value operator +(int left, Value right)
        => new(left + (int)right.Raw);

    public static Value operator -(Value left, Value right)
        => new(left.Raw - right.Raw);

    public static Value operator -(Value left, int right)
        => new((int)left.Raw - right);

    public static Value operator -(PieceValues left, Value right)
        => new((int)left - right.Raw);

    public static Value operator -(Value left, PieceValues right)
        => new(left.Raw - (int)right);

    public static Value operator -(int left, Value right)
        => new(left - (int)right.Raw);

    public static Value operator ++(Value value)
        => new((int)value.Raw + 1);

    public static Value operator --(Value value)
        => new((int)value.Raw - 1);

    public static Value operator *(Value left, int right)
        => new((int)left.Raw * right);

    public static bool operator >(Value left, Value right)
        => left.Raw > right.Raw;

    public static bool operator <(Value left, Value right)
        => left.Raw < right.Raw;

    public static bool operator <=(Value left, Value right)
        => left.Raw <= right.Raw;

    public static bool operator >=(Value left, Value right)
        => left.Raw >= right.Raw;

    public static bool operator >(Value left, PieceValues right)
        => left.Raw > right;

    public static bool operator <(Value left, PieceValues right)
        => left.Raw < right;

    public static bool operator <=(Value left, PieceValues right)
        => left.Raw <= right;

    public static bool operator >=(Value left, PieceValues right)
        => left.Raw >= right;

    public static bool operator true(Value value)
        => value.Raw > 0;

    public static bool operator false(Value value)
        => value.Raw <= 0;

    public Value ForColor(Player p)
        => p.IsWhite ? this : new Value(-(int)Raw);

    public bool Equals(Value other)
        => Raw == other.Raw;

    public override bool Equals(object obj)
        => obj is Value other && Equals(other);

    public override int GetHashCode()
        => (int)Raw;

    public override string ToString()
        => Raw.ToString();
}