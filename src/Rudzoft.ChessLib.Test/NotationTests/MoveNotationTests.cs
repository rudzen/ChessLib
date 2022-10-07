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
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Notation;
using Rudzoft.ChessLib.Notation.Notations;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Test.NotationTests;

public sealed class MoveNotationTests
{
    [Theory]
    [InlineData("8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1", MoveNotations.Fan, PieceTypes.Knight, Squares.d2, Squares.f2, Squares.e4)]
    public void FanRankAmbiguities(string fen, MoveNotations moveNotation, PieceTypes movingPt, Squares fromSqOne, Squares fromSqTwo, Squares toSq)
    {
        var g = GameFactory.Create(fen);
        var pc = movingPt.MakePiece(g.Pos.SideToMove);

        var fromOne = new Square(fromSqOne);
        var fromTwo = new Square(fromSqTwo);
        var to = new Square(toSq);

        Assert.True(fromOne.IsOk);
        Assert.True(fromTwo.IsOk);
        Assert.True(to.IsOk);

        var pieceChar = pc.GetUnicodeChar();
        var toString = to.ToString();

        var moveOne = Move.Create(fromOne, to);
        var moveTwo = Move.Create(fromTwo, to);

        var ambiguity = MoveNotation.Create(g.Pos);

        var expectedOne = $"{pieceChar}{fromOne.FileChar}{toString}";
        var expectedTwo = $"{pieceChar}{fromTwo.FileChar}{toString}";

        var actualOne = ambiguity.ToNotation(moveNotation).Convert(moveOne);
        var actualTwo = ambiguity.ToNotation(moveNotation).Convert(moveTwo);

        Assert.Equal(expectedOne, actualOne);
        Assert.Equal(expectedTwo, actualTwo);
    }

    [Theory]
    [InlineData("8/6k1/8/8/3N4/8/1K1N4/8 w - - 0 1", MoveNotations.Fan, PieceTypes.Knight, Squares.d2, Squares.d4, Squares.f3)]
    public void FanFileAmbiguities(string fen, MoveNotations moveNotation, PieceTypes movingPt, Squares fromSqOne, Squares fromSqTwo, Squares toSq)
    {
        var g = GameFactory.Create(fen);
        var pc = movingPt.MakePiece(g.Pos.SideToMove);

        var fromOne = new Square(fromSqOne);
        var fromTwo = new Square(fromSqTwo);
        var to = new Square(toSq);

        Assert.True(fromOne.IsOk);
        Assert.True(fromTwo.IsOk);
        Assert.True(to.IsOk);

        var pieceChar = pc.GetUnicodeChar();
        var toString = to.ToString();

        var moveOne = Move.Create(fromOne, to);
        var moveTwo = Move.Create(fromTwo, to);

        var ambiguity = MoveNotation.Create(g.Pos);

        var expectedOne = $"{pieceChar}{fromOne.RankChar}{toString}";
        var expectedTwo = $"{pieceChar}{fromTwo.RankChar}{toString}";

        var actualOne = ambiguity.ToNotation(moveNotation).Convert(moveOne);
        var actualTwo = ambiguity.ToNotation(moveNotation).Convert(moveTwo);

        Assert.Equal(expectedOne, actualOne);
        Assert.Equal(expectedTwo, actualTwo);
    }

    [Theory]
    [InlineData("8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1", MoveNotations.San, PieceTypes.Knight, Squares.d2, Squares.f2, Squares.e4)]
    public void SanRankAmbiguities(string fen, MoveNotations moveNotation, PieceTypes movingPt, Squares fromSqOne, Squares fromSqTwo, Squares toSq)
    {
        var g = GameFactory.Create(fen);
        var pc = movingPt.MakePiece(g.Pos.SideToMove);

        var fromOne = new Square(fromSqOne);
        var fromTwo = new Square(fromSqTwo);
        var to = new Square(toSq);

        Assert.True(fromOne.IsOk);
        Assert.True(fromTwo.IsOk);
        Assert.True(to.IsOk);

        var pieceChar = pc.GetPieceChar();
        var toString = to.ToString();

        var moveOne = Move.Create(fromOne, to);
        var moveTwo = Move.Create(fromTwo, to);

        var ambiguity = MoveNotation.Create(g.Pos);

        var expectedOne = $"{pieceChar}{fromOne.FileChar}{toString}";
        var expectedTwo = $"{pieceChar}{fromTwo.FileChar}{toString}";

        var actualOne = ambiguity.ToNotation(moveNotation).Convert(moveOne);
        var actualTwo = ambiguity.ToNotation(moveNotation).Convert(moveTwo);

        Assert.Equal(expectedOne, actualOne);
        Assert.Equal(expectedTwo, actualTwo);
    }

    [Theory]
    [InlineData("8/6k1/8/8/3N4/8/1K1N4/8 w - - 0 1", MoveNotations.San, PieceTypes.Knight, Squares.d2, Squares.d4, Squares.f3)]
    public void SanFileAmbiguities(string fen, MoveNotations moveNotation, PieceTypes movingPt, Squares fromSqOne, Squares fromSqTwo, Squares toSq)
    {
        var g = GameFactory.Create(fen);
        var pc = movingPt.MakePiece(g.Pos.SideToMove);

        var fromOne = new Square(fromSqOne);
        var fromTwo = new Square(fromSqTwo);
        var to = new Square(toSq);

        Assert.True(fromOne.IsOk);
        Assert.True(fromTwo.IsOk);
        Assert.True(to.IsOk);

        var pieceChar = pc.GetPieceChar();
        var toString = to.ToString();

        var moveOne = Move.Create(fromOne, to);
        var moveTwo = Move.Create(fromTwo, to);

        var ambiguity = MoveNotation.Create(g.Pos);

        var expectedOne = $"{pieceChar}{fromOne.RankChar}{toString}";
        var expectedTwo = $"{pieceChar}{fromTwo.RankChar}{toString}";

        var actualOne = ambiguity.ToNotation(moveNotation).Convert(moveOne);
        var actualTwo = ambiguity.ToNotation(moveNotation).Convert(moveTwo);

        Assert.Equal(expectedOne, actualOne);
        Assert.Equal(expectedTwo, actualTwo);
    }

    [Theory]
    [InlineData("8/6k1/8/8/3N4/8/1K1N4/8 w - - 0 1", MoveNotations.Ran, PieceTypes.Knight, Squares.d2, Squares.d4, Squares.f3)]
    [InlineData("8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1", MoveNotations.Ran, PieceTypes.Knight, Squares.d2, Squares.f2, Squares.e4)]
    [InlineData("8/6k1/8/8/3N4/8/1K1N4/8 w - - 0 1", MoveNotations.Lan, PieceTypes.Knight, Squares.d2, Squares.d4, Squares.f3)]
    [InlineData("8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1", MoveNotations.Lan, PieceTypes.Knight, Squares.d2, Squares.f2, Squares.e4)]
    public void RanLanAmbiguities(string fen, MoveNotations moveNotation, PieceTypes movingPt, Squares fromSqOne, Squares fromSqTwo, Squares toSq)
    {
        var g = GameFactory.Create(fen);
        var pc = movingPt.MakePiece(g.Pos.SideToMove);

        var fromOne = new Square(fromSqOne);
        var fromTwo = new Square(fromSqTwo);
        var to = new Square(toSq);

        Assert.True(fromOne.IsOk);
        Assert.True(fromTwo.IsOk);
        Assert.True(to.IsOk);

        var pieceChar = pc.GetPieceChar();
        var toString = to.ToString();

        var moveOne = Move.Create(fromOne, to);
        var moveTwo = Move.Create(fromTwo, to);

        var ambiguity = MoveNotation.Create(g.Pos);

        var expectedOne = $"{pieceChar}{fromOne}-{toString}";
        var expectedTwo = $"{pieceChar}{fromTwo}-{toString}";

        var actualOne = ambiguity.ToNotation(moveNotation).Convert(moveOne);
        var actualTwo = ambiguity.ToNotation(moveNotation).Convert(moveTwo);

        Assert.Equal(expectedOne, actualOne);
        Assert.Equal(expectedTwo, actualTwo);
    }

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

    [Fact]
    public void RookSanAmbiguity()
    {
        // Tests rook ambiguity notation for white rooks @ e1 and g2. Original author : johnathandavis

        const string fen = "5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53";
        const MoveNotations notation = MoveNotations.San;
        var expectedNotations = new[] { "Ree2", "Rge2" };

        var game = GameFactory.Create(fen);

        var sideToMove = game.Pos.SideToMove;
        var targetPiece = PieceTypes.Rook.MakePiece(sideToMove);

        var moveNotation = MoveNotation.Create(game.Pos);

        var sanMoves = game.Pos
            .GenerateMoves()
            .Select(static em => em.Move)
            .Where(m => game.Pos.GetPiece(m.FromSquare()) == targetPiece)
            .Select(m => moveNotation.ToNotation(notation).Convert(m))
            .ToArray();

        foreach (var notationResult in expectedNotations)
            Assert.Contains(sanMoves, s => s == notationResult);
    }
}