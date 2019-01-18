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

namespace Rudz.Chess.Enums
{
    using System;

    // TODO : implement Crn/Smith/Descriptive/Coordinate/ICCF notations
    [Flags]
    public enum EMoveNotation
    {
        /// <summary>
        /// Standard algebraic Notation [implemented]
        /// </summary>
        San = 0,

        /// <summary>
        /// Figurine algebraic Notation [implemented]
        /// </summary>
        Fan = 1,

        /// <summary>
        /// The Long algebraic Notaion [almost implemented]
        /// </summary>
        Lan = 2,

        /// <summary>
        /// Reversible algebraic notation
        /// todo: finish implementation
        /// </summary>
        Ran = 4,

        /// <summary>
        /// Concise reversible notation
        /// todo: implement
        /// </summary>
        Crn = 8,

        /// <summary>
        /// The smith notation
        /// todo: implement
        /// </summary>
        Smith = 16,

        /// <summary>
        /// The descriptive notation
        /// </summary>
        Descriptive = 32,

        Coordinate = 64,

        // ReSharper disable once InconsistentNaming
        ICCF = 128,

        /// <summary>
        /// Universal chess interface notation
        /// </summary>
        Uci = 256
    }
}