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
using Rudz.Chess.Types;

namespace Chess.Test.BitboardTests;

public sealed class BitboardTests
{
    [Fact]
    public void MakeBitBoard()
    {
        // a few squares
        var b1 = BitBoards.MakeBitboard(Square.A1, Square.B1, Square.A2, Square.B2);
        var b2 = Square.A1.AsBb() | Square.B1.AsBb() | Square.A2.AsBb() | Square.B2.AsBb();
        Assert.Equal(b1, b2);

        // a single square (not needed, but still has to work in case of list of squares etc)
        var b3 = BitBoards.MakeBitboard(Square.H3);
        var b4 = Square.H3.AsBb();
        Assert.Equal(b3, b4);
    }

    [Fact]
    public void BitBoardOrAll()
    {
        Span<Square> baseSquares = stackalloc Square[8] { Square.A1, Square.A2, Square.A3, Square.A4, Square.A5, Square.A6, Square.A7, Square.A8 };
        var bb = BitBoard.Empty.OrAll(baseSquares);
        while (bb)
        {
            var s = BitBoards.PopLsb(ref bb);
            var valid = baseSquares.BinarySearch(s) > -1;
            Assert.True(valid);
        }
    }
}
