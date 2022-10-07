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

public sealed class PieceAttacksRookTests : PieceAttacks
{
    /// <summary>
    /// Testing results of blocked rook attacks, they should always return 7 on the sides, and 14 in
    /// the corner
    /// </summary>
    [Fact]
    public void RookBorderBlocked()
    {
        /*
         * Test purpose : Testing blocked bishop attacks
         */
        BitBoard border = 0xff818181818181ff;
        BitBoard borderInner = 0x7e424242427e00;
        BitBoard corners = 0x8100000000000081;

        const int expectedCorner = 14;
        const int expectedSide = 8; // 7 to each side and 1 blocked

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

        // just to get the attacks
        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Position(board, pieceValue);

        while (border)
        {
            var sq = BitBoards.PopLsb(ref border);
            var attacks = pos.GetAttacks(sq, PieceTypes.Rook, in borderInner);
            Assert.False(attacks.IsEmpty);
            var expected = corners & sq ? expectedCorner : expectedSide;
            var actual = attacks.Count;
            Assert.Equal(expected, actual);
        }
    }
}