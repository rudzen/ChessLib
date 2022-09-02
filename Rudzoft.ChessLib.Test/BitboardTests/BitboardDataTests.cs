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

using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Test.BitboardTests;

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
        var bb = BitBoards.AllSquares;
        while (bb)
        {
            var s1 = BitBoards.PopLsb(ref bb);
            var bb2 = BitBoards.AllSquares;
            while (bb2)
            {
                var s2 = BitBoards.PopLsb(ref bb2);
                var bb3 = BitBoards.AllSquares;
                while (bb3)
                {
                    var s3 = BitBoards.PopLsb(ref bb3);
                    actual += s1.Aligned(s2, s3).AsByte();
                }
            }
        }

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AlignedSimpleTotalNoIdentical()
    {
        const int expected = 9184;

        var actual = 0;
        var bb = BitBoards.AllSquares;
        while (bb)
        {
            var s1 = BitBoards.PopLsb(ref bb);
            var bb2 = BitBoards.AllSquares & ~s1;
            while (bb2)
            {
                var s2 = BitBoards.PopLsb(ref bb2);
                var bb3 = BitBoards.AllSquares & ~s2;
                while (bb3)
                {
                    var s3 = BitBoards.PopLsb(ref bb3);
                    actual += s1.Aligned(s2, s3).AsByte();
                }
            }
        }

        Assert.Equal(expected, actual);
    }
}