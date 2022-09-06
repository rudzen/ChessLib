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

namespace Rudzoft.ChessLib.Test.PiecesTests;

public sealed class PawnPushTests
{
    [Theory]
    [InlineData(Directions.North, 1)]
    [InlineData(Directions.South, 1)]
    public void PawnPush(Directions direction, int expected)
    {
        var fullBoard = BitBoards.PawnSquares;
        while (fullBoard)
        {
            var sq = BitBoards.PopLsb(ref fullBoard);
            var toSq = sq + direction;
            var actual = sq.Distance(toSq);
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void PawnDoublePushNorth()
    {
        var fullBoard = Rank.Rank2.BitBoardRank();
        var direction = Direction.NorthDouble;

        const int expected = 2;

        while (fullBoard)
        {
            var sq = BitBoards.PopLsb(ref fullBoard);
            var toSq = sq + direction;
            var actual = sq.Distance(toSq);
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void PawnDoublePushSouth()
    {
        var fullBoard = Rank.Rank7.BitBoardRank();
        var direction = Direction.SouthDouble;

        const int expected = 2;

        while (fullBoard)
        {
            var sq = BitBoards.PopLsb(ref fullBoard);
            var toSq = sq + direction;
            var actual = sq.Distance(toSq);
            Assert.Equal(expected, actual);
        }
    }
}