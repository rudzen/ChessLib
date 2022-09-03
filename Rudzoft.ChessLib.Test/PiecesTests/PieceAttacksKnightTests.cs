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

public sealed class PieceAttacksKnightTests : PieceAttacks, IClassFixture<RegularMobilityFixture>
{
    private readonly RegularMobilityFixture _fixture;

    public PieceAttacksKnightTests(RegularMobilityFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void AlphaPattern()
    {
        const int index = (int)EBands.Alpha;
        const int attackIndex = 1;
        const ulong narrowLocations = 0x4281000000008142;

        var bb = Bands[index];

        while (bb)
        {
            var sq = BitBoards.PopLsb(ref bb);
            var attacks = _fixture.RegAttacks[attackIndex](sq);
            var expected = !(_fixture.BoardCorners & sq).IsEmpty
                ? _fixture.KnightExpected[index] >> 1 /* for corners */
                : !(narrowLocations & sq).IsEmpty
                    ? _fixture.KnightExpected[index] - 1 /* narrowLocations */
                    : _fixture.KnightExpected[index];
            var actual = attacks.Count;
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void BetaPattern()
    {
        const int index = (int)EBands.Beta;
        const int attackIndex = 1;
        const ulong narrowLocations = 0x42000000004200;

        var bb = Bands[index];

        while (bb)
        {
            var sq = BitBoards.PopLsb(ref bb);
            var attacks = _fixture.RegAttacks[attackIndex](sq);
            var expected = !(narrowLocations & sq).IsEmpty
                ? _fixture.KnightExpected[index] - 2
                : _fixture.KnightExpected[index];
            var actual = attacks.Count;
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void GammaPattern()
    {
        const int index = (int)EBands.Gamma;
        const int attackIndex = 1;
        var expected = _fixture.KnightExpected[index];

        var bb = Bands[index];

        while (bb)
        {
            var pieceLocation = BitBoards.PopLsb(ref bb);
            var actual = _fixture.RegAttacks[attackIndex](pieceLocation).Count;
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void DeltaPattern()
    {
        const int index = (int)EBands.Delta;
        const int attackIndex = 1;
        var expected = _fixture.KnightExpected[index];

        var bb = Bands[index];

        while (bb)
        {
            var pieceLocation = BitBoards.PopLsb(ref bb);
            var actual = _fixture.RegAttacks[attackIndex](pieceLocation).Count;
            Assert.Equal(expected, actual);
        }
    }
}