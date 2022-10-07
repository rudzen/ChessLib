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

/// <inheritdoc/>
public sealed class PieceAttacksBishopTests : PieceAttacks
{
    /// <summary>
    /// Testing results of blocked bishop attacks, they should always return 2 on the sides, and 1
    /// in the corner
    /// </summary>
    [Fact]
    public void BishopBorderBlocked()
    {
        BitBoard border = Alpha;
        BitBoard borderInner = Beta;
        var corners = BitBoards.MakeBitboard(Square.A1, Square.A8, Square.H1, Square.H8);

        const int expectedCorner = 1; // just a single attack square no matter what
        const int expectedSide = 2;

        /*
                     * borderInner (X = set bit) :
                     *
                     * 0 0 0 0 0 0 0 0
                     * 0 X X X X X X 0
                     * 0 X 0 0 0 0 X 0
                     * 0 X 0 0 0 0 X 0
                     * 0 X 0 0 0 0 X 0
                     * 0 X 0 0 0 0 X 0
                     * 0 X X X X X X 0
                     * 0 0 0 0 0 0 0 0
                     *
                     */

        while (border)
        {
            var sq = BitBoards.PopLsb(ref border);
            var attacks = sq.BishopAttacks(borderInner);
            Assert.False(attacks.IsEmpty);
            var expected = corners & sq ? expectedCorner : expectedSide;
            var actual = attacks.Count;
            Assert.Equal(expected, actual);
        }

    }
}