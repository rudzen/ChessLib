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

namespace Rudzoft.ChessLib.Extensions;

public static class SpanExtensions
{
    /// <summary>
    /// Appends an int to a span of char
    /// </summary>
    /// <param name="target">The target span</param>
    /// <param name="v">The value to append</param>
    /// <param name="targetIndex">The current index for target span</param>
    /// <param name="capacity">Max size of "value".ToString(), defaults to 3</param>
    /// <returns>New index after append</returns>
    public static int Append(this Span<char> target, int v, int targetIndex, int capacity = 3)
    {
        Span<char> s = stackalloc char[capacity];
        v.TryFormat(s, out var written);
        for (var i = 0; i < written; ++i)
            target[targetIndex++] = s[i];
        return targetIndex;
    }

    /// <summary>
    /// Appends an ulong to a span of char
    /// </summary>
    /// <param name="target">The target span</param>
    /// <param name="v">The value to append</param>
    /// <param name="targetIndex">The current index for target span</param>
    /// <param name="capacity">Max size of "value".ToString(), defaults to 20</param>
    /// <returns>New index after append</returns>
    public static int Append(this Span<char> target, in ulong v, int targetIndex, int capacity = 20)
    {
        Span<char> s = stackalloc char[capacity];
        v.TryFormat(s, out var written);
        for (var i = 0; i < written; ++i)
            target[targetIndex++] = s[i];
        return targetIndex;
    }
}