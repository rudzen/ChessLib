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

using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Types;

[assembly: InternalsVisibleTo("Chess.Test")]

namespace Rudzoft.ChessLib.Notation.Notations;

// TODO : implement Cran/Descriptive notations

public enum MoveNotations
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
    /// The Long algebraic Notation [almost implemented]
    /// </summary>
    Lan = 2,

    /// <summary>
    /// Reversible algebraic notation
    /// </summary>
    Ran = 3,

    /// <summary>
    /// Concise reversible algebraic notation
    /// todo: implement
    /// </summary>
    Cran = 4,

    /// <summary>
    /// The smith notation
    /// </summary>
    Smith = 5,

    /// <summary>
    /// The descriptive notation
    /// todo: implement
    /// </summary>
    Descriptive = 6,

    Coordinate = 7,

    // ReSharper disable once InconsistentNaming
    ICCF = 8,

    /// <summary>
    /// Universal chess interface notation
    /// </summary>
    Uci = 9
}

public interface INotation
{
    string Convert(Move move);
}