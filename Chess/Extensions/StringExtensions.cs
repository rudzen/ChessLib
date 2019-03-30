/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2019 Rudy Alex Kohn

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

namespace Rudz.Chess.Extensions
{
    using System.Collections.Generic;

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
        public static IEnumerable<string> Parse(this string command, char separator, char tokenizer)
        {
            var startIndex = 0;
            var inToken = false;
            for (var index = 0; index < command.Length; index++)
            {
                var character = command[index];
                if (index == command.Length - 1)
                {
                    // return last token.
                    yield return command.Substring(startIndex, index - startIndex + 1).TrimEnd(tokenizer);
                    break;
                }

                if (character == separator)
                {
                    // Skip if present.
                    if (inToken)
                        continue;

                    // return token
                    yield return command.Substring(startIndex, index - startIndex).TrimEnd(tokenizer);
                    startIndex = index + 1;
                }
                else if (character == tokenizer)
                {
                    inToken ^= true;
                    startIndex = index + 1;
                }
            }
        }
    }
}