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
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Test.PiecesTests;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class RegularMobilityFixture
{
    // special case with alpha and beta bands in the corners are taken care of
    public int[] KnightExpected { get; } = { 4, 6, 8, 8 };

    // special case with alpha band is taken care of
    public int[] KingExpected { get; } = { 5, 8, 8, 8 };

    public BitBoard BoardCorners { get; } = Square.A1 | Square.A8 | Square.H1 | Square.H8;

    // pawn = 0 (N/A for now), knight = 1, king = 2
    public Func<Square, BitBoard>[] RegAttacks { get; } =
    {
        static _ => BitBoard.Empty, BitBoards.KnightAttacks, BitBoards.KingAttacks
    };
}