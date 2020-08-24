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
    using Rudz.Chess.Types;
    using System.Linq;
    using Xunit;

    public sealed class PieceAttacksPawnTests : PieceAttacks
    {
        /*
         * Pawn attack test information.
         *
         * - The bands are different for pawns, as their attack pattern at File A and H is different
         * - Pawns never move to 1st or 8th rank
         * - Each band tests cover both the white and black side
         */

        private static readonly BitBoard[] PawnBands = { 0x81818181818100, 0x42424242424200, 0x24242424242400, 0x18181818181800 };

        /*
         * Pawn bands :
         *
         * Alpha:               Beta:
         *
         * 0 0 0 0 0 0 0 0      0 0 0 0 0 0 0 0
         * X 0 0 0 0 0 0 X      0 X 0 0 0 0 X 0
         * X 0 0 0 0 0 0 X      0 X 0 0 0 0 X 0
         * X 0 0 0 0 0 0 X      0 X 0 0 0 0 X 0
         * X 0 0 0 0 0 0 X      0 X 0 0 0 0 X 0
         * X 0 0 0 0 0 0 X      0 X 0 0 0 0 X 0
         * X 0 0 0 0 0 0 X      0 X 0 0 0 0 X 0
         * 0 0 0 0 0 0 0 0      0 0 0 0 0 0 0 0
         *
         * Gamma:               Delta:
         *
         * 0 0 0 0 0 0 0 0      0 0 0 0 0 0 0 0
         * 0 0 X 0 0 X 0 0      0 0 0 X X 0 0 0
         * 0 0 X 0 0 X 0 0      0 0 0 X X 0 0 0
         * 0 0 X 0 0 X 0 0      0 0 0 X X 0 0 0
         * 0 0 X 0 0 X 0 0      0 0 0 X X 0 0 0
         * 0 0 X 0 0 X 0 0      0 0 0 X X 0 0 0
         * 0 0 X 0 0 X 0 0      0 0 0 X X 0 0 0
         * 0 0 0 0 0 0 0 0      0 0 0 0 0 0 0 0
         */

        private static readonly int[] PawnExpected = { 1, 2, 2, 2 };

        [Fact]
        public override void AlphaPattern()
        {
            const int index = (int)EBands.Alpha;
            var us = Player.White;
            var expected = PawnExpected[index];

            var actuals = PawnBands[index].Select(w => w.PawnAttack(us).Count);
            actuals.Should().AllBeEquivalentTo(expected);

            // black
            us = ~us;

            actuals = PawnBands[index].Select(x => x.PawnAttack(us).Count);
            actuals.Should().AllBeEquivalentTo(expected);
        }

        [Fact]
        public override void BetaPattern()
        {
            const int index = (int)EBands.Beta;
            var us = Player.White;
            var expected = PawnExpected[index];
            var actuals = PawnBands[index].Select(w => w.PawnAttack(us).Count);

            actuals.Should().AllBeEquivalentTo(expected);

            // black
            us = ~us;

            actuals = PawnBands[index].Select(x => x.PawnAttack(us).Count);
            actuals.Should().AllBeEquivalentTo(expected);
        }

        [Fact]
        public override void GammaPattern()
        {
            const int index = (int)EBands.Gamma;
            var us = Player.White;
            var expected = PawnExpected[index];
            var actuals = PawnBands[index].Select(w => w.PawnAttack(us).Count);

            actuals.Should().AllBeEquivalentTo(expected);

            // black
            us = ~us;

            actuals = PawnBands[index].Select(x => x.PawnAttack(us).Count);
            actuals.Should().AllBeEquivalentTo(expected);
        }

        [Fact]
        public override void DeltaPattern()
        {
            const int index = (int)EBands.Delta;
            var us = Player.White;
            var expected = PawnExpected[index];
            var actuals = PawnBands[index].Select(w => w.PawnAttack(us).Count);

            actuals.Should().AllBeEquivalentTo(expected);

            // black
            us = ~us;

            actuals = PawnBands[index].Select(x => x.PawnAttack(us).Count);
            actuals.Should().AllBeEquivalentTo(expected);
        }
    }
}