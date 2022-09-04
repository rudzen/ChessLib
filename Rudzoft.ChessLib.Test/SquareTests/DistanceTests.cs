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

namespace Rudzoft.ChessLib.Test.SquareTests;

public sealed class DistanceTests
{
    [Theory]
    [InlineData(Squares.a1, Squares.a2, 1)]
    [InlineData(Squares.a1, Squares.a3, 2)]
    [InlineData(Squares.a1, Squares.a4, 3)]
    [InlineData(Squares.a1, Squares.a5, 4)]
    [InlineData(Squares.a1, Squares.a6, 5)]
    [InlineData(Squares.a1, Squares.a7, 6)]
    [InlineData(Squares.a1, Squares.a8, 7)]
    [InlineData(Squares.a1, Squares.b1, 1)]
    [InlineData(Squares.a1, Squares.b2, 1)]
    [InlineData(Squares.a1, Squares.b3, 2)]
    [InlineData(Squares.a1, Squares.b4, 3)]
    [InlineData(Squares.a1, Squares.b5, 4)]
    [InlineData(Squares.a1, Squares.b6, 5)]
    [InlineData(Squares.a1, Squares.b7, 6)]
    [InlineData(Squares.a1, Squares.b8, 7)]
    [InlineData(Squares.a1, Squares.c1, 2)]
    [InlineData(Squares.a1, Squares.c2, 2)]
    [InlineData(Squares.a1, Squares.c3, 2)]
    [InlineData(Squares.a1, Squares.c4, 3)]
    [InlineData(Squares.a1, Squares.c5, 4)]
    [InlineData(Squares.a1, Squares.c6, 5)]
    [InlineData(Squares.a1, Squares.c7, 6)]
    [InlineData(Squares.a1, Squares.c8, 7)]
    public void Chebyshev(Squares s1, Squares s2, int expected)
    {
        Square sq1 = s1;
        Square sq2 = s2;

        var actual = sq1.Distance(sq2);

        Assert.Equal(expected, actual);
    }
}