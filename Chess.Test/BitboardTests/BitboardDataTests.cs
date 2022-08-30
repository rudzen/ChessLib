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

using System.Linq;
using Rudz.Chess.Types;

namespace Chess.Test.BitboardTests;

public sealed class BitboardDataTests
{
    [Fact]
    public void AlignedSimplePositive()
    {
        const bool expected = true;

        var actual = Square.A1.Aligned(Square.A2, Square.A3);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AlignedSimpleTotal()
    {
        const int expected = 10640;

        var actual = 0;
        foreach (var s1 in BitBoards.AllSquares)
            actual += BitBoards.AllSquares
                .Sum(s2 => BitBoards.AllSquares
                    .Count(s3 => s1.Aligned(s2, s3)));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AlignedSimpleTotalNoIdentical()
    {
        const int expected = 9184;

        var actual = 0;
        foreach (var s1 in BitBoards.AllSquares)
            actual += BitBoards.AllSquares.Where(x2 => x2 != s1)
                .Sum(s2 => BitBoards.AllSquares.Where(x3 => x3 != s2)
                    .Count(s3 => s1.Aligned(s2, s3)));

        Assert.Equal(expected, actual);
    }
}