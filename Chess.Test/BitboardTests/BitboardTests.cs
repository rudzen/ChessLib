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

using Rudz.Chess.Types;
using System;
using Xunit;

namespace Chess.Test.BitboardTests;

public sealed class BitboardTests
{
    [Fact]
    public void MakeBitBoard()
    {
        // a few squares
        var b1 = BitBoards.MakeBitboard(Squares.a1, Squares.b1, Squares.a2, Squares.b2);
        var b2 = Squares.a1.BitBoardSquare() | Squares.b1.BitBoardSquare() | Squares.a2.BitBoardSquare() | Squares.b2.BitBoardSquare();
        Assert.Equal(b1, b2);

        // a single square (not needed, but still has to work in case of list of squares etc)
        var b3 = BitBoards.MakeBitboard(Squares.h3);
        var b4 = Squares.h3.BitBoardSquare();
        Assert.Equal(b3, b4);
    }

    [Fact]
    public void BitBoardOrAll()
    {
        var baseSquares = new Square[] { Squares.a1, Squares.a2, Squares.a3, Squares.a4, Squares.a5, Squares.a6, Squares.a7, Squares.a8 };
        var bb = BitBoard.Empty.OrAll(baseSquares);
        while (!bb.IsEmpty)
        {
            var s = BitBoards.PopLsb(ref bb);
            var valid = Array.BinarySearch(baseSquares, s) > -1;
            Assert.True(valid);
        }
    }
}
