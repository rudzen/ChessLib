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

using Rudzoft.ChessLib.Evaluation;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Test.EvaluationTests;

public sealed class KpkBitBaseTests
{
    // theory data layout:
    // fen, player as raw int (0 = white, 1 = black), expected win by strong side
    [Theory]
    [InlineData("2k5/8/8/8/1PK5/8/8/8 w - - 0 1", 0, true)]
    [InlineData("5k2/8/6KP/8/8/8/8/8 b - - 0 1", 0, true)]
    [InlineData("8/8/8/K7/P2k4/8/8/8 w - - 0 1", 0, true)]
    [InlineData("8/8/8/K7/P2k4/8/8/8 b - - 0 1", 0, true)]
    public void KpkWin(string fen, int rawPlayer, bool expected)
    {
        var game = GameFactory.Create(fen);
        var pos = game.Pos;

        Player strongSide = rawPlayer;
        var weakSide = ~strongSide;

        var strongKing = KpkBitBase.Normalize(pos, strongSide,  pos.GetPieceSquare(PieceTypes.King, strongSide));
        var strongPawn = KpkBitBase.Normalize(pos, strongSide, pos.GetPieceSquare(PieceTypes.Pawn, strongSide));
        var weakKing = KpkBitBase.Normalize(pos, strongSide, pos.GetPieceSquare(PieceTypes.King, weakSide));

        var us = strongSide == pos.SideToMove ? Player.White : Player.Black;

        var won = !KpkBitBase.Probe(strongKing, strongPawn, weakKing, us);

        Assert.Equal(expected, won);
    }
}