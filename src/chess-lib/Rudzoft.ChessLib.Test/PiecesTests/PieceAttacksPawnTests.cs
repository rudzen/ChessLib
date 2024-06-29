/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2023 Rudy Alex Kohn

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

public sealed class PieceAttacksPawnTests : PieceAttacks
{
    /*
     * Pawn attack test information.
     *
     * - The bands are different for pawns, as their attack pattern at File A and H is different
     * - Pawns never move to 1st or 8th rank
     * - Each band tests cover both the white and black side
     */

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

    [Theory]
    [InlineData(PawnAlpha, Colors.White, 1)]
    [InlineData(PawnAlpha, Colors.Black, 1)]
    [InlineData(PawnBeta, Colors.White, 2)]
    [InlineData(PawnBeta, Colors.Black, 2)]
    [InlineData(PawnGamma, Colors.White, 2)]
    [InlineData(PawnGamma, Colors.Black, 2)]
    [InlineData(PawnDelta, Colors.White, 2)]
    [InlineData(PawnDelta, Colors.Black, 2)]
    public void PawnAttacks(ulong squares, Colors c, int expected)
    {
        var bb = BitBoard.Create(squares);
        var us = Color.Create(c);

        while (bb)
        {
            var sq = BitBoards.PopLsb(ref bb);
            var actual = sq.PawnAttack(us).Count;
            Assert.Equal(expected, actual);
        }
    }
}