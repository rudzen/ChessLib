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

namespace Rudzoft.ChessLib.Types;

public enum Directions
{
    NoDirection = 0,
    North = 8,
    East = 1,
    South = -North, // -8
    West = -East, // -1
    NorthEast = North + East, //  9
    SouthEast = South + East, // -7
    SouthWest = South + West, // -9
    NorthWest = North + West, //  7

    NorthDouble = North + North,
    SouthDouble = South + South,

    NorthFill = NorthDouble << 1,
    SouthFill = -NorthFill
}

public readonly record struct Direction(Directions Value)
{
    public static Direction North { get; } = new(Directions.North);

    public static Direction South { get; } = new(Directions.South);

    public static Direction East { get; } = new(Directions.East);

    public static Direction West { get; } = new(Directions.West);

    public static Direction NorthEast { get; } = new(Directions.NorthEast);

    public static Direction NorthWest { get; } = new(Directions.NorthWest);

    public static Direction SouthEast { get; } = new(Directions.SouthEast);

    public static Direction SouthWest { get; } = new(Directions.SouthWest);

    public static Direction NorthDouble { get; } = new(Directions.NorthDouble);

    public static Direction SouthDouble { get; } = new(Directions.SouthDouble);

    public static Direction SouthFill { get; } = new(Directions.SouthFill);

    public static Direction NorthFill { get; } = new(Directions.NorthFill);

    public static Direction None { get; } = new(Directions.NoDirection);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Direction(int d)
        : this((Directions)d) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Direction(int value)
        => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Direction(Directions value)
        => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Direction operator +(Direction left, Direction right)
        => new(left.Value + (int)right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Direction operator +(Direction left, Directions right)
        => new(left.Value + (int)right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Direction operator +(Direction left, int right)
        => new(left.Value + right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Direction operator -(Direction left, Direction right)
        => new(left.Value - (int)right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Direction operator -(Direction left, Directions right)
        => new(left.Value - (int)right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Direction operator -(Direction left, int right)
        => new(left.Value - right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Direction operator *(Direction left, Direction right)
        => new((int)left.Value * (int)right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Direction operator *(Direction left, Directions right)
        => new((int)left.Value * (int)right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Direction operator *(Direction left, int right)
        => new((int)left.Value * right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Direction other)
        => Value == other.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => AsInt();

    private int AsInt()
        => (int)Value;
}