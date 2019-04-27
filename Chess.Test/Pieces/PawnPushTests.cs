/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2019 Rudy Alex Kohn

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

namespace Chess.Test.Pieces
{
    using Rudz.Chess.Enums;
    using Rudz.Chess.Types;
    using Xunit;

    public sealed class PawnPushTests
    {
        [Fact]
        public void PawnPushNorth()
        {
            BitBoard fullBoard = 0xffffffffffff00;
            Direction direction = EDirection.North;

            const int expected = 1;

            foreach (var sq in fullBoard)
            {
                var toSq = sq + direction;
                var actual = sq.Distance(toSq);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void PawnPushSouth()
        {
            BitBoard fullBoard = 0xffffffffffff00;
            Direction direction = EDirection.South;

            const int expected = 1;

            foreach (var sq in fullBoard)
            {
                var toSq = sq + direction;
                var actual = sq.Distance(toSq);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void PawnDoublePushNorth()
        {
            BitBoard fullBoard = BitBoards.RANK2;
            Direction direction = EDirection.North;

            const int expected = 2;

            foreach (var sq in fullBoard)
            {
                var toSq = sq + 2 * direction;
                var actual = sq.Distance(toSq);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void PawnDoublePushSouth()
        {
            BitBoard fullBoard = BitBoards.RANK7;
            Direction direction = EDirection.South;

            const int expected = 2;

            foreach (var sq in fullBoard)
            {
                var toSq = sq + 2 * direction;
                var actual = sq.Distance(toSq);
                Assert.Equal(expected, actual);
            }
        }
    }
}