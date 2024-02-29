/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2023 Rudy Alex Kohn

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

/// <summary>
/// Value based move structure which combines move and a simple move ordering score
/// </summary>
public struct ValMove : IEquatable<ValMove>, IComparable<ValMove>
{
    public static readonly ValMove Empty = new();

    public int Score;

    public Move Move;

    private ValMove(Move m, int s)
    {
        Move = m;
        Score = s;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ValMove(Move m) => new(m, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ValMove(int s) => new(Move.EmptyMove, s);

    public static bool operator !=(ValMove left, ValMove right) => !(left.Move == right.Move);

    public static bool operator ==(ValMove left, ValMove right) => left.Move == right.Move;

    public override bool Equals(object obj) => obj is ValMove other && Equals(other);

    public bool Equals(ValMove other) => Move.Equals(other.Move);

    public readonly int CompareTo(ValMove other) => other.Score.CompareTo(Score);

    public readonly override int GetHashCode() => Move.GetHashCode();

    public readonly override string ToString() => $"{Move}, {Score}";
}