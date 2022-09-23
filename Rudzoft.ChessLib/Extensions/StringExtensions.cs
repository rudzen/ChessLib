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
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rudzoft.ChessLib.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Custom tokenizer function which supports different ending trim and separation unit
    /// TODO : Make custom command type
    /// </summary>
    /// <param name="command">The entirety of the command string</param>
    /// <param name="separator">The separator</param>
    /// <param name="tokenizer">The end token char to trim from the pillaged command string</param>
    /// <returns></returns>
    public static Queue<string> Parse(this ReadOnlySpan<char> command, char separator, char tokenizer)
    {
        var q = new Queue<string>();
        var startIndex = 0;
        var inToken = false;

        for (var i = 0; i < command.Length; ++i)
        {
            if (i == command.Length - 1)
            {
                // return last token.
                q.Enqueue(new string(command.Slice(startIndex, i - startIndex + 1).TrimEnd(tokenizer)));
                break;
            }

            var character = command[i];

            if (character == separator)
            {
                // Skip if present.
                if (inToken)
                    continue;

                // return token
                q.Enqueue(new string(command[startIndex..i].TrimEnd(tokenizer)));
                startIndex = i + 1;
            }
            else if (character == tokenizer)
            {
                inToken ^= true;
                startIndex = i + 1;
            }
        }

        return q;
    }

    public static IEnumerable<int> GetLocations(this string @this, char token = ' ')
    {
        var pos = 0;
        do
        {
            var index = @this.IndexOf(token, pos);
            if (index == -1)
                yield break;
            yield return index;
            pos = index + 1;
        } while (true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MemoryStream GenerateStream(this string @this) => new(Encoding.UTF8.GetBytes(@this ?? string.Empty));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty(this string @this) => string.IsNullOrEmpty(@this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrWhiteSpace(this string @this) => string.IsNullOrWhiteSpace(@this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char FlipCase(this char c)
    {
#pragma warning disable S3358 // Ternary operators should not be nested
        return char.IsLetter(c) ? char.IsLower(c) ? char.ToUpper(c) : char.ToLower(c) : c;
#pragma warning restore S3358 // Ternary operators should not be nested
    }
}
