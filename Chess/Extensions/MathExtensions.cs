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
using System.Runtime.InteropServices;
using Rudz.Chess.Types;

namespace Rudz.Chess.Extensions;

public static class MathExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InBetween(this int v, int min, int max) => ((v - min) | (max - v)) >= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InBetween(this byte v, byte min, byte max) => (((int)v - (int)min) | ((int)max - (int)v)) >= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InBetween(this char v, char min, char max) => (((int)v - (int)min) | ((int)max - (int)v)) >= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InBetween(this uint v, int min, int max) => v - (uint)min <= (uint)max - (uint)min;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Clamp(this int v, int min, int max) => v < min ? min : v > max ? max : v;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Clamp(this double v, double min, double max) => v < min ? min : v > max ? max : v;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Abs(this int @this) => Math.Abs(@this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Max(this int @this, int value) => Math.Max(@this, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Round(this double @this, int digits) => Math.Round(@this, digits);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe byte AsByte(this bool @this) => *(byte*)&@this;

    /// <summary>
    /// Modulo for pow^2 values...
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ModPow2(int input, int ceil) => input & (ceil - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Pow2(this int value) => 1 << BitBoards.Msb(value).AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEven(this int value) => (value & 1) == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOdd(this int value) => !value.IsEven();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt(this bool @this)
    {
        var myBoolSpan = MemoryMarshal.CreateReadOnlySpan(ref @this, 1);
        return MemoryMarshal.AsBytes(myBoolSpan)[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long MidPoint(this long @this, long that)
    {
        return (@this + that) >> 1;
    }
}
