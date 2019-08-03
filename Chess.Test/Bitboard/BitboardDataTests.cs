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

namespace Chess.Test.Bitboard
{
    using Rudz.Chess.Enums;
    using Rudz.Chess.Types;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class BitboardDataTests
    {
        [Fact]
        public async Task AlignedSimplePositiveTest()
        {
            const bool expected = true;

            const ESquare sq1 = ESquare.a1;
            const ESquare sq2 = ESquare.a2;
            const ESquare sq3 = ESquare.a3;

            var actual = await Task.Run(() => BitBoards.Aligned(sq1, sq2, sq3)).ConfigureAwait(false);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task AlignedSimpleTotalTest()
        {
            const int expected = 10640;

            var actual = await Task.Run(() =>
            {
                var sum = 0;
                foreach (var s1 in BitBoards.AllSquares)
                    sum += BitBoards.AllSquares
                        .Sum(s2 => BitBoards.AllSquares
                            .Count(s3 => s1.Aligned(s2, s3)));
                return sum;
            }).ConfigureAwait(false);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AlignedSimpleTotalNoIdenticalsTest()
        {
            const int expected = 9184;

            var actual = 0;

            foreach (var s1 in BitBoards.AllSquares)
                actual += BitBoards.AllSquares
                    .Where(x2 => x2 != s1)
                    .Sum(s2 => BitBoards.AllSquares
                        .Where(x3 => x3 != s2)
                        .Count(s3 => s1.Aligned(s2, s3)));

            Assert.Equal(expected, actual);
        }
    }
}