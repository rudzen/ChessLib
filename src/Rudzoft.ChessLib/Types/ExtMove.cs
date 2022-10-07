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

namespace Rudzoft.ChessLib.Types;

/// <summary>
/// Extended move structure which combines Move and Score
/// </summary>
public struct ExtMove : IEquatable<ExtMove>, IComparable<ExtMove>
{
    public static readonly ExtMove Empty = new();

    public Move Move { get; set; }

    public int Score { get; set; }

    private ExtMove(Move m, int s)
    {
        Move = m;
        Score = s;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ExtMove(Move m)
        => new(m, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ExtMove(int s)
        => new(Move.EmptyMove, s);

    public static bool operator !=(ExtMove left, ExtMove right)
        => !(left == right);

    public static bool operator ==(ExtMove left, ExtMove right)
        => left.Equals(right);

    public bool Equals(ExtMove other)
        => Move.Equals(other.Move);

    public readonly int CompareTo(ExtMove other)
        => other.Score.CompareTo(Score);

    public override bool Equals(object obj)
        => obj is ExtMove other && Equals(other);

    public readonly override int GetHashCode()
        => Move.GetHashCode();

    public readonly override string ToString()
        => $"{Move}, {Score}";
}