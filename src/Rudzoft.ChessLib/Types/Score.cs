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
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Rudzoft.ChessLib.Types;

/// <inheritdoc />
/// <summary>
/// Score type with support for Eg/Mg values
/// </summary>
public struct Score : IEquatable<Score>
{
    private Vector2 _data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Score(Vector2 value)
        => _data = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Score(int value)
        => _data = new Vector2(value, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Score(float mg, float eg)
        => _data = new Vector2(mg, eg);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Score(int mg, int eg)
        => _data = new Vector2(mg, eg);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Score(Value mg, Value eg)
        => _data = new Vector2((int)mg.Raw, (int)eg.Raw);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Score(Score s)
        => _data = s._data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Score(int v)
        => new(v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Score(Vector2 v)
        => new(v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Score operator *(Score s, int v)
        => new(Vector2.Multiply(s._data, v));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Score operator /(Score s, int v)
        => new(Vector2.Divide(s._data, v));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Score operator *(Score s, bool b)
        => b ? s : Zero;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Score operator +(Score s1, Score s2)
        => new(Vector2.Add(s1._data, s2._data));

    public static Score operator -(Score s1, Score s2)
        => new(Vector2.Subtract(s1._data, s2._data));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Score operator +(Score s, int v)
        => new(Vector2.Add(s._data, new Vector2(v, v)));

    public static readonly Score Zero = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Deconstruct(out float mg, out float eg)
    {
        mg = _data.X;
        eg = _data.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetMg(int v)
        => _data.X = v;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetEg(int v)
        => _data.Y = v;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float Eg()
        => _data.Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float Mg()
        => _data.X;

    public readonly override string ToString()
        => $"Mg:{_data.X}, Eg:{_data.Y}";

    public bool Equals(Score other)
        => _data.Equals(other._data);

    public override bool Equals(object obj)
        => obj is Score other && Equals(other);

    public override int GetHashCode()
        => _data.GetHashCode();

    public static bool operator ==(Score left, Score right)
        => left._data.Equals(right._data);

    public static bool operator !=(Score left, Score right)
        => !(left == right);
}