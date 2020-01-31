/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2020 Rudy Alex Kohn

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

namespace Chess.Test.Square
{
    using Rudz.Chess.Enums;
    using Rudz.Chess.Types;
    using Xunit;

    public sealed class SquareTests
    {
        [Fact]
        public void SquareOppositeColorPositiveTest()
        {
            const bool expected = true;

            var sq1 = new Square(ESquare.a1);
            var sq2 = new Square(ESquare.a2);

            var actual = sq1.IsOppositeColor(sq2);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SquareOppositeColorNegativeTest()
        {
            const bool expected = false;

            var sq1 = new Square(ESquare.a1);
            var sq2 = new Square(ESquare.a3);

            var actual = sq1.IsOppositeColor(sq2);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RelativeSquareBlackTest()
        {
            const ESquare expected = ESquare.h8;

            var s = new Square(Ranks.Rank1, Files.FileH);

            var actual = s.Relative(PlayerExtensions.Black);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RelativeSquareWhiteTest()
        {
            const ESquare expected = ESquare.c3;

            var s = new Square(Ranks.Rank3, Files.FileC);

            var actual = s.Relative(PlayerExtensions.White);

            Assert.Equal(expected, actual);
        }
    }
}