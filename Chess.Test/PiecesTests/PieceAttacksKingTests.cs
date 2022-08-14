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

namespace Chess.Test.Pieces;

using FluentAssertions;
using System.Linq;
using Xunit;

public sealed class PieceAttacksKingTests : PieceAttacksRegular
{
    [Fact]
    public override void AlphaPattern()
    {
        const int index = (int)EBands.Alpha;
        const int attackIndex = 2;

        // special case, as the expected values vary depending on the kings location on the
        // outer rim
        foreach (var pieceLocation in Bands[index])
        {
            var attacks = RegAttacks[attackIndex](pieceLocation);
            var expected = (BoardCorners & pieceLocation) != 0 ? KingExpected[index] - 2 /* for corners */ : KingExpected[index];
            Assert.Equal(expected, attacks.Count);
        }
    }

    [Fact]
    public override void BetaPattern()
    {
        const int index = (int)EBands.Beta;
        const int attackIndex = 2;
        var expected = KingExpected[index];
        var actuals = Bands[index].Select(x => RegAttacks[attackIndex](x).Count);

        actuals.Should().AllBeEquivalentTo(expected);
    }

    [Fact]
    public override void GammaPattern()
    {
        const int index = (int)EBands.Gamma;
        const int attackIndex = 2;
        var expected = KingExpected[index];
        var actuals = Bands[index].Select(x => RegAttacks[attackIndex](x).Count);

        actuals.Should().AllBeEquivalentTo(expected);
    }

    [Fact]
    public override void DeltaPattern()
    {
        const int index = (int)EBands.Delta;
        const int attackIndex = 2;
        var expected = KingExpected[index];
        var actuals = Bands[index].Select(x => RegAttacks[attackIndex](x).Count);

        actuals.Should().AllBeEquivalentTo(expected);
    }
}
