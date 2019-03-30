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

// ReSharper disable IdentifierTypo

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
        NoMgf = 0,
        Legalmoves = 1,
        Stages = 2,
        Queenpromotion = 4
    }

    public static class EmgfExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagFast(this Emgf value, Emgf flag) => (value & flag) != 0;
    }
}