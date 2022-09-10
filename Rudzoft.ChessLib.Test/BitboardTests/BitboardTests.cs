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

namespace Rudzoft.ChessLib.Test.BitboardTests;

public sealed class BitboardTests
{
    [Fact]
    public void MakeBitBoard()
    {
        // a few squares
        var b1 = BitBoards.MakeBitboard(Square.A1, Square.B1, Square.A2, Square.B2);
        var b2 = Square.A1 | Square.B1 | Square.A2 | Square.B2;
        Assert.Equal(b1, b2);

        // a single square (not needed, but still has to work in case of list of squares etc)
        var b3 = BitBoards.MakeBitboard(Square.H3);
        var b4 = Square.H3.AsBb();
        Assert.Equal(b3, b4);
    }

    [Fact]
    public void BitBoardOrAll()
    {
        Span<Square> baseSquares = stackalloc Square[8]
            { Square.A1, Square.A2, Square.A3, Square.A4, Square.A5, Square.A6, Square.A7, Square.A8 };
        var bb = BitBoard.Empty.OrAll(baseSquares);
        while (bb)
        {
            var s = BitBoards.PopLsb(ref bb);
            var valid = baseSquares.BinarySearch(s) > -1;
            Assert.True(valid);
        }
    }

    [Fact]
    public void WhiteAreaCount()
    {
        const int expected = 32;
        var actual = BitBoards.WhiteArea.Count;
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void BlackAreaCount()
    {
        const int expected = 32;
        var actual = BitBoards.BlackArea.Count;
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void LightSquaresCount()
    {
        const int expected = 32;
        var actual = Player.White.ColorBB().Count;
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DarkSquaresCount()
    {
        const int expected = 32;
        var actual = Player.Black.ColorBB().Count;
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(Files.FileA, 8)]
    [InlineData(Files.FileB, 8)]
    [InlineData(Files.FileC, 8)]
    [InlineData(Files.FileD, 8)]
    [InlineData(Files.FileE, 8)]
    [InlineData(Files.FileF, 8)]
    [InlineData(Files.FileG, 8)]
    [InlineData(Files.FileH, 8)]
    public void FileSquaresCount(Files files, int expected)
    {
        var f = new File(files);
        var actual = f.BitBoardFile().Count;
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(Ranks.Rank1, 8)]
    [InlineData(Ranks.Rank2, 8)]
    [InlineData(Ranks.Rank3, 8)]
    [InlineData(Ranks.Rank4, 8)]
    [InlineData(Ranks.Rank5, 8)]
    [InlineData(Ranks.Rank6, 8)]
    [InlineData(Ranks.Rank7, 8)]
    [InlineData(Ranks.Rank8, 8)]
    public void RankSquaresCount(Ranks ranks, int expected)
    {
        var r = new Rank(ranks);
        var actual = r.BitBoardRank().Count;
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void EmptyCount()
    {
        const int expected = 0;
        var actual = BitBoards.EmptyBitBoard.Count;
        Assert.Equal(expected, actual);
        Assert.True(BitBoards.EmptyBitBoard.IsEmpty);
    }

    [Fact]
    public void FullCount()
    {
        const int expected = Square.Count;
        var actual = BitBoards.AllSquares.Count;
        Assert.Equal(expected, actual);
    }
}