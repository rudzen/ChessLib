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

namespace Rudzoft.ChessLib.Extensions;

public static class Maths
{
    /// <summary>
    /// Converts a string to an int.
    /// Approx. 17 times faster than int.Parse.
    /// </summary>
    /// <param name="str">The string to convert</param>
    /// <returns>The resulting number</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToIntegral(ReadOnlySpan<char> str)
    {
        if (str.IsEmpty || str.IsWhiteSpace())
            return default;

        var x = 0;
        var neg = false;
        var pos = 0;
        var max = str.Length - 1;
        if (str[pos] == '-')
        {
            neg = true;
            pos++;
        }

        while (pos <= max && str[pos].InBetween('0', '9'))
        {
            x = x * 10 + (str[pos] - '0');
            pos++;
        }

        return neg ? -x : x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ToIntegral(this ReadOnlySpan<char> str, out ulong result)
    {
        if (str.IsEmpty || str.IsWhiteSpace())
        {
            result = 0;
            return false;
        }

        var x = ulong.MinValue;
        var pos = 0;
        var max = str.Length - 1;
        while (pos <= max && str[pos].InBetween('0', '9'))
        {
            x = x * 10 + (ulong)(str[pos] - '0');
            pos++;
        }

        result = x;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ToIntegral(ReadOnlySpan<char> str, out int result)
    {
        if (str.IsEmpty)
        {
            result = 0;
            return false;
        }

        var x = 0;
        var pos = 0;
        var max = str.Length - 1;
        while (pos <= max && str[pos].InBetween('0', '9'))
        {
            x = x * 10 + (str[pos] - '0');
            pos++;
        }

        result = x;
        return true;
    }

}