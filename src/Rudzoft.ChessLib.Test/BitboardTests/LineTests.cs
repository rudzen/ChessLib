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

using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Test.BitboardTests;

public sealed class LineTests
{
    [Theory]
    [InlineData(Squares.a1, Squares.a1, 0)]
    [InlineData(Squares.a1, Squares.a2, 8)]
    [InlineData(Squares.a1, Squares.a3, 8)]
    [InlineData(Squares.a1, Squares.a4, 8)]
    [InlineData(Squares.a1, Squares.a5, 8)]
    [InlineData(Squares.a1, Squares.a6, 8)]
    [InlineData(Squares.a1, Squares.a7, 8)]
    [InlineData(Squares.a1, Squares.a8, 8)]
    [InlineData(Squares.a1, Squares.b1, 8)]
    [InlineData(Squares.a1, Squares.b2, 8)]
    [InlineData(Squares.a1, Squares.b3, 0)]
    [InlineData(Squares.a1, Squares.b4, 0)]
    [InlineData(Squares.a1, Squares.b5, 0)]
    [InlineData(Squares.a1, Squares.b6, 0)]
    [InlineData(Squares.a1, Squares.b7, 0)]
    [InlineData(Squares.a1, Squares.b8, 0)]
    [InlineData(Squares.a1, Squares.c1, 8)]
    [InlineData(Squares.a1, Squares.c2, 0)]
    [InlineData(Squares.a1, Squares.c3, 8)]
    [InlineData(Squares.a1, Squares.c4, 0)]
    [InlineData(Squares.a1, Squares.c5, 0)]
    [InlineData(Squares.a1, Squares.c6, 0)]
    [InlineData(Squares.a1, Squares.c7, 0)]
    [InlineData(Squares.a1, Squares.c8, 0)]
    [InlineData(Squares.a1, Squares.d1, 8)]
    [InlineData(Squares.a1, Squares.d2, 0)]
    [InlineData(Squares.a1, Squares.d3, 0)]
    [InlineData(Squares.a1, Squares.d4, 8)]
    [InlineData(Squares.a1, Squares.d5, 0)]
    [InlineData(Squares.a1, Squares.d6, 0)]
    [InlineData(Squares.a1, Squares.d7, 0)]
    [InlineData(Squares.a1, Squares.d8, 0)]
    [InlineData(Squares.a1, Squares.e1, 8)]
    [InlineData(Squares.a1, Squares.e2, 0)]
    [InlineData(Squares.a1, Squares.e3, 0)]
    [InlineData(Squares.a1, Squares.e4, 0)]
    [InlineData(Squares.a1, Squares.e5, 8)]
    [InlineData(Squares.a1, Squares.e6, 0)]
    [InlineData(Squares.a1, Squares.e7, 0)]
    [InlineData(Squares.a1, Squares.e8, 0)]
    [InlineData(Squares.a1, Squares.f1, 8)]
    [InlineData(Squares.a1, Squares.f2, 0)]
    [InlineData(Squares.a1, Squares.f3, 0)]
    [InlineData(Squares.a1, Squares.f4, 0)]
    [InlineData(Squares.a1, Squares.f5, 0)]
    [InlineData(Squares.a1, Squares.f6, 8)]
    [InlineData(Squares.a1, Squares.f7, 0)]
    [InlineData(Squares.a1, Squares.f8, 0)]
    [InlineData(Squares.a1, Squares.g1, 8)]
    [InlineData(Squares.a1, Squares.g2, 0)]
    [InlineData(Squares.a1, Squares.g3, 0)]
    [InlineData(Squares.a1, Squares.g4, 0)]
    [InlineData(Squares.a1, Squares.g5, 0)]
    [InlineData(Squares.a1, Squares.g6, 0)]
    [InlineData(Squares.a1, Squares.g7, 8)]
    [InlineData(Squares.a1, Squares.g8, 0)]
    [InlineData(Squares.a1, Squares.h1, 8)]
    [InlineData(Squares.a1, Squares.h2, 0)]
    [InlineData(Squares.a1, Squares.h3, 0)]
    [InlineData(Squares.a1, Squares.h4, 0)]
    [InlineData(Squares.a1, Squares.h5, 0)]
    [InlineData(Squares.a1, Squares.h6, 0)]
    [InlineData(Squares.a1, Squares.h7, 0)]
    [InlineData(Squares.a1, Squares.h8, 8)]
    public void LineCount(Squares s1, Squares s2, int expected)
    {
        var sq1 = new Square(s1);
        var sq2 = new Square(s2);

        var line = sq1.Line(sq2);

        var actual = line.Count;

        Assert.Equal(expected, actual);
    }
}