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

namespace Chess.Test.Pieces
{
    using FluentAssertions;
    using System.Linq;
    using Xunit;

    public sealed class PieceAttacksKnightTests : PieceAttacksRegular
    {
        [Fact]
        public override void AlphaPattern()
        {
            const int index = (int)EBands.Alpha;
            const int attackIndex = 1;
            const ulong narrowLocations = 0x4281000000008142;

            foreach (var pieceLocation in Bands[index])
            {
                var attacks = RegAttacks[attackIndex](pieceLocation);
                var expected = (BoardCorners & pieceLocation) != 0
                    ? KnightExpected[index] >> 1 /* for corners */
                    : (narrowLocations & pieceLocation) != 0
                        ? KnightExpected[index] - 1 /* narrowLocations */
                        : KnightExpected[index];
                var actual = attacks.Count;
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public override void BetaPattern()
        {
            const int index = (int)EBands.Beta;
            const int attackIndex = 1;
            const ulong narrowLocations = 0x42000000004200;

            foreach (var pieceLocation in Bands[index])
            {
                var attacks = RegAttacks[attackIndex](pieceLocation);
                var expected = (narrowLocations & pieceLocation) != 0
                    ? KnightExpected[index] - 2
                    : KnightExpected[index];
                var actual = attacks.Count;
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public override void GammaPattern()
        {
            const int index = (int)EBands.Gamma;
            const int attackIndex = 1;
            var expected = KnightExpected[index];
            var actuals = Bands[index].Select(x => RegAttacks[attackIndex](x).Count);

            actuals.Should().AllBeEquivalentTo(expected);
        }

        [Fact]
        public override void DeltaPattern()
        {
            const int index = (int)EBands.Delta;
            const int attackIndex = 1;
            var expected = KnightExpected[index];
            var actuals = Bands[index].Select(x => RegAttacks[attackIndex](x).Count);

            actuals.Should().AllBeEquivalentTo(expected);
        }
    }
}