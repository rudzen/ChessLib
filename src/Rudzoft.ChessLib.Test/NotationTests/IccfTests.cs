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

using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.Notation;
using Rudzoft.ChessLib.Notation.Notations;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Test.NotationTests;

public sealed class IccfTests
{
    [Fact]
    public void IccfRegularMove()
    {
        const string fen = "8/6k1/8/8/3N4/8/1K1N4/8 w - - 0 1";
        const MoveNotations notation = MoveNotations.ICCF;
        const string expectedPrimary = "4263";

        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Position(board, pieceValue);
        var g = GameFactory.Create(pos);
        g.NewGame(fen);

        var w1 = Move.Create(Squares.d2, Squares.f3);

        var ambiguity = MoveNotation.Create(pos);

        var actualPrimary = ambiguity.ToNotation(notation).Convert(w1);

        Assert.Equal(expectedPrimary, actualPrimary);
    }

    [Theory]
    [InlineData("8/1P4k1/8/8/8/8/1K6/8 w - - 0 1", PieceTypes.Queen, "27281")]
    [InlineData("8/1P4k1/8/8/8/8/1K6/8 w - - 0 1", PieceTypes.Rook, "27282")]
    [InlineData("8/1P4k1/8/8/8/8/1K6/8 w - - 0 1", PieceTypes.Bishop, "27283")]
    [InlineData("8/1P4k1/8/8/8/8/1K6/8 w - - 0 1", PieceTypes.Knight, "27284")]
    public void IccfPromotionMove(string fen, PieceTypes promoPt, string expected)
    {
        const MoveNotations notation = MoveNotations.ICCF;

        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Position(board, pieceValue);
        var g = GameFactory.Create(pos);
        g.NewGame(fen);

        var w1 = Move.Create(Squares.b7, Squares.b8, MoveTypes.Promotion, promoPt);

        var ambiguity = MoveNotation.Create(pos);

        var actual = ambiguity.ToNotation(notation).Convert(w1);

        Assert.Equal(expected, actual);
    }
}