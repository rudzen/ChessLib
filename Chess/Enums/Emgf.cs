/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2020 Rudy Alex Kohn

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

namespace Rudz.Chess.Enums
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Move generation flag
    /// </summary>
    [Flags]
    public enum Emgf
    {
        /// <summary>
        /// Generate capture moves
        /// </summary>
        Capture = 1,

        /// <summary>
        /// Generate quiet moves
        /// </summary>
        Quiet = 2,

        /// <summary>
        /// Generate quiet moves giving check
        /// </summary>
        QuietChecks = 4,

        /// <summary>
        /// Generate all moves while not in check
        /// </summary>
        NonEvasion = 8,

        /// <summary>
        /// Generate all moves while in check
        /// </summary>
        Evasion = 16,

        /// <summary>
        /// General generate all legal move
        /// </summary>
        Legalmoves = 32,

        /// <summary>
        /// Generate capture while in check
        /// </summary>
        EvasionCapture = 64,

        /// <summary>
        /// Generate quiet moves while in check
        /// </summary>
        EvasionQuiet = 128,

        /// <summary>
        /// Only include queen promotions
        /// </summary>
        Queenpromotion = 256
    }

    public static class EmgfExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagFast(this Emgf value, Emgf flag) => (value & flag) != 0;
    }
}