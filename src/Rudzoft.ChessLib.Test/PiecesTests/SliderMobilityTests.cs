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

using System.Linq;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Test.PiecesTests;

public sealed class SliderMobilityTests : PieceAttacks, IClassFixture<SliderMobilityFixture>
{
    private readonly SliderMobilityFixture _fixture;

    public SliderMobilityTests(SliderMobilityFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(Alpha, PieceTypes.Bishop, 7)]
    [InlineData(Beta, PieceTypes.Bishop, 9)]
    [InlineData(Gamma, PieceTypes.Bishop, 11)]
    [InlineData(Delta, PieceTypes.Bishop, 13)]
    [InlineData(Alpha, PieceTypes.Rook, 14)]
    [InlineData(Beta, PieceTypes.Rook, 14)]
    [InlineData(Gamma, PieceTypes.Rook, 14)]
    [InlineData(Delta, PieceTypes.Rook, 14)]
    [InlineData(Alpha, PieceTypes.Queen, 21)]
    [InlineData(Beta, PieceTypes.Queen, 23)]
    [InlineData(Gamma, PieceTypes.Queen, 25)]
    [InlineData(Delta, PieceTypes.Queen, 27)]
    public void BishopMobility(ulong pattern, PieceTypes pt, int expectedMobility)
    {
        var sliderIndex = _fixture.SliderIndex(pt);
        var bb = new BitBoard(pattern);

        var expected = bb.Count * expectedMobility;
        var actual = bb.Select(x => _fixture.SliderAttacks[sliderIndex](x, BitBoard.Empty).Count).Sum();

        Assert.Equal(expected, actual);
    }
}