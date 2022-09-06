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

public sealed class SquareTests
{
    [Fact]
    public void SquareOppositeColorPositive()
    {
        const bool expected = true;

        var sq1 = Square.A1;
        var sq2 = Square.A2;

        var actual = sq1.IsOppositeColor(sq2);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SquareOppositeColorNegative()
    {
        const bool expected = false;

        var sq1 = Square.A1;
        var sq2 = Square.A3;

        var actual = sq1.IsOppositeColor(sq2);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RelativeSquareBlack()
    {
        const Squares expected = Squares.h8;

        var s = new Square(Rank.Rank1, File.FileH);

        var actual = s.Relative(Player.Black);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RelativeSquareWhite()
    {
        const Squares expected = Squares.c3;

        var s = new Square(Rank.Rank3,  File.FileC);

        var actual = s.Relative(Player.White);

        Assert.Equal(expected, actual);
    }
}