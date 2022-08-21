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
using Rudz.Chess;
using Rudz.Chess.Enums;
using Rudz.Chess.Extensions;
using Rudz.Chess.Factories;
using Rudz.Chess.MoveGeneration;
using Rudz.Chess.Types;
using Xunit;

namespace Chess.Test.MoveTests;

public sealed class MoveNotationTests
{
    [Fact]
    public void FanRankAmbiguationPositive()
    {
        // Tests both knights moving to same square for Rank ambiguity

        const string fen = "8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1";

        var movingPiece = new Piece(Pieces.WhiteKnight);
        var fromOneSquare = new Square(Squares.d2);
        var fromTwoSquare = new Square(Squares.f2);
        var toSquare = new Square(Squares.e4);

        var uniChar = movingPiece.GetUnicodeChar();
        var toSquareString = toSquare.ToString();

        var expectedPrimary = $"{uniChar}{fromOneSquare.FileChar}{toSquareString}";
        var expectedSecondary = $"{uniChar}{fromTwoSquare.FileChar}{toSquareString}";

        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Position(board, pieceValue);
        var g = GameFactory.Create(pos);
        g.NewGame(fen);

        var w1 = new Move(fromOneSquare, toSquare);
        var w2 = new Move(fromTwoSquare, toSquare);

        var ambiguity = MoveAmbiguity.Create(pos);

        var actualPrimary = ambiguity.ToNotation(w1);
        var actualSecondary = ambiguity.ToNotation(w2);

        Assert.Equal(expectedPrimary, actualPrimary);
        Assert.Equal(expectedSecondary, actualSecondary);
    }

    [Fact]
    public void FanRankFileAmbiguationPositive()
    {
        // Tests both knights moving to same square for Rank ambiguity

        const string fen = "8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1";
        const MoveNotations notation = MoveNotations.Fan;

        var movingPiece = new Piece(Pieces.WhiteKnight);
        var fromOneSquare = new Square(Squares.d2);
        var fromTwoSquare = new Square(Squares.f2);
        var toSquare = new Square(Squares.e4);

        var uniChar = movingPiece.GetUnicodeChar();
        var toSquareString = toSquare.ToString();

        var expectedPrimary = $"{uniChar}{fromOneSquare.FileChar}{toSquareString}";
        var expectedSecondary = $"{uniChar}{fromTwoSquare.FileChar}{toSquareString}";

        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Position(board, pieceValue);
        var g = GameFactory.Create(pos);
        g.NewGame(fen);

        var w1 = new Move(fromOneSquare, toSquare);
        var w2 = new Move(fromTwoSquare, toSquare);

        var ambiguity = MoveAmbiguity.Create(pos);

        var actualPrimary = ambiguity.ToNotation(w1, notation);
        var actualSecondary = ambiguity.ToNotation(w2, notation);

        Assert.Equal(expectedPrimary, actualPrimary);
        Assert.Equal(expectedSecondary, actualSecondary);
    }

    [Fact]
    public void FanFileAmbiguationPositive()
    {
        // Tests both knights moving to same square for File ambiguity

        const string fen = "8/6k1/8/8/3N4/8/1K1N4/8 w - - 0 1";
        const MoveNotations notation = MoveNotations.Fan;

        var movingPiece = new Piece(Pieces.WhiteKnight);
        var fromOneSquare = new Square(Squares.d2);
        var fromTwoSquare = new Square(Squares.d4);
        var toSquare = new Square(Squares.f3);

        var uniChar = movingPiece.GetUnicodeChar();
        var toSquareString = toSquare.ToString();

        var expectedPrimary = $"{uniChar}{fromOneSquare.RankChar}{toSquareString}";
        var expectedSecondary = $"{uniChar}{fromTwoSquare.RankChar}{toSquareString}";

        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Position(board, pieceValue);
        var g = GameFactory.Create(pos);
        g.NewGame(fen);

        var w1 = new Move(fromOneSquare, toSquare);
        var w2 = new Move(fromTwoSquare, toSquare);

        var ambiguity = MoveAmbiguity.Create(pos);

        var actualPrimary = ambiguity.ToNotation(w1, notation);
        var actualSecondary = ambiguity.ToNotation(w2, notation);

        Assert.Equal(expectedPrimary, actualPrimary);
        Assert.Equal(expectedSecondary, actualSecondary);
    }

    [Fact]
    public void SanRankAmbiguationPositive()
    {
        // Tests both knights moving to same square for Rank ambiguity

        const string fen = "8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1";
        const MoveNotations notation = MoveNotations.San;

        var movingPiece = new Piece(Pieces.WhiteKnight);
        var fromOneSquare = new Square(Squares.d2);
        var fromTwoSquare = new Square(Squares.f2);
        var toSquare = new Square(Squares.e4);

        var uniChar = movingPiece.GetPieceChar();
        var toSquareString = toSquare.ToString();

        var expectedPrimary = $"{uniChar}{fromOneSquare.FileChar}{toSquareString}";
        var expectedSecondary = $"{uniChar}{fromTwoSquare.FileChar}{toSquareString}";

        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Position(board, pieceValue);
        var g = GameFactory.Create(pos);
        g.NewGame(fen);

        var w1 = new Move(fromOneSquare, toSquare);
        var w2 = new Move(fromTwoSquare, toSquare);

        var ambiguity = MoveAmbiguity.Create(pos);

        var actualPrimary = ambiguity.ToNotation(w1, notation);
        var actualSecondary = ambiguity.ToNotation(w2, notation);

        Assert.Equal(expectedPrimary, actualPrimary);
        Assert.Equal(expectedSecondary, actualSecondary);
    }

    [Fact]
    public void SanRankFileAmbiguationPositive()
    {
        // Tests both knights moving to same square for Rank ambiguity

        const string fen = "8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1";
        const MoveNotations notation = MoveNotations.San;

        var movingPiece = new Piece(Pieces.WhiteKnight);
        var fromOneSquare = new Square(Squares.d2);
        var fromTwoSquare = new Square(Squares.f2);
        var toSquare = new Square(Squares.e4);

        var uniChar = movingPiece.GetPieceChar();
        var toSquareString = toSquare.ToString();

        var expectedPrimary = $"{uniChar}{fromOneSquare.FileChar}{toSquareString}";
        var expectedSecondary = $"{uniChar}{fromTwoSquare.FileChar}{toSquareString}";

        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Position(board, pieceValue);
        var g = GameFactory.Create(pos);
        g.NewGame(fen);

        var w1 = new Move(fromOneSquare, toSquare);
        var w2 = new Move(fromTwoSquare, toSquare);

        var ambiguity = MoveAmbiguity.Create(pos);

        var actualPrimary = ambiguity.ToNotation(w1, notation);
        var actualSecondary = ambiguity.ToNotation(w2, notation);

        Assert.Equal(expectedPrimary, actualPrimary);
        Assert.Equal(expectedSecondary, actualSecondary);
    }

    [Fact]
    public void SanFileAmbiguationPositive()
    {
        // Tests both knights moving to same square for File ambiguity

        const string fen = "8/6k1/8/8/3N4/8/1K1N4/8 w - - 0 1";
        const MoveNotations notation = MoveNotations.San;

        var movingPiece = new Piece(Pieces.WhiteKnight);
        var fromOneSquare = new Square(Squares.d2);
        var fromTwoSquare = new Square(Squares.d4);
        var toSquare = new Square(Squares.f3);

        var uniChar = movingPiece.GetPieceChar();
        var toSquareString = toSquare.ToString();

        var expectedPrimary = $"{uniChar}{fromOneSquare.RankChar}{toSquareString}";
        var expectedSecondary = $"{uniChar}{fromTwoSquare.RankChar}{toSquareString}";

        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Position(board, pieceValue);
        var g = GameFactory.Create(pos);
        g.NewGame(fen);

        var w1 = new Move(fromOneSquare, toSquare);
        var w2 = new Move(fromTwoSquare, toSquare);

        var ambiguity = MoveAmbiguity.Create(pos);

        var actualPrimary = ambiguity.ToNotation(w1, notation);
        var actualSecondary = ambiguity.ToNotation(w2, notation);

        Assert.Equal(expectedPrimary, actualPrimary);
        Assert.Equal(expectedSecondary, actualSecondary);
    }

    [Fact]
    public void LanRankAmbiguationPositive()
    {
        // Tests both knights moving to same square for Rank ambiguity

        const string fen = "8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1";
        const MoveNotations notation = MoveNotations.Lan;

        var movingPiece = new Piece(Pieces.WhiteKnight);
        var fromOneSquare = new Square(Squares.d2);
        var fromTwoSquare = new Square(Squares.f2);
        var toSquare = new Square(Squares.e4);

        var uniChar = movingPiece.GetPieceChar();
        var toSquareString = toSquare.ToString();

        var expectedPrimary = $"{uniChar}{fromOneSquare}-{toSquareString}";
        var expectedSecondary = $"{uniChar}{fromTwoSquare}-{toSquareString}";

        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Position(board, pieceValue);
        var g = GameFactory.Create(pos);
        g.NewGame(fen);

        var w1 = new Move(fromOneSquare, toSquare);
        var w2 = new Move(fromTwoSquare, toSquare);

        var ambiguity = MoveAmbiguity.Create(pos);

        var actualPrimary = ambiguity.ToNotation(w1, notation);
        var actualSecondary = ambiguity.ToNotation(w2, notation);

        Assert.Equal(expectedPrimary, actualPrimary);
        Assert.Equal(expectedSecondary, actualSecondary);
    }

    [Fact]
    public void LanRankFileAmbiguationPositive()
    {
        // Tests both knights moving to same square for Rank ambiguity

        const string fen = "8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1";
        const MoveNotations notation = MoveNotations.Lan;

        var movingPiece = new Piece(Pieces.WhiteKnight);
        var fromOneSquare = new Square(Squares.d2);
        var fromTwoSquare = new Square(Squares.f2);
        var toSquare = new Square(Squares.e4);

        var uniChar = movingPiece.GetPieceChar();
        var toSquareString = toSquare.ToString();

        var expectedPrimary = $"{uniChar}{fromOneSquare}-{toSquareString}";
        var expectedSecondary = $"{uniChar}{fromTwoSquare}-{toSquareString}";

        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Position(board, pieceValue);
        var g = GameFactory.Create(pos);
        g.NewGame(fen);

        var w1 = new Move(fromOneSquare, toSquare);
        var w2 = new Move(fromTwoSquare, toSquare);

        var ambiguity = MoveAmbiguity.Create(pos);

        var actualPrimary = ambiguity.ToNotation(w1, notation);
        var actualSecondary = ambiguity.ToNotation(w2, notation);

        Assert.Equal(expectedPrimary, actualPrimary);
        Assert.Equal(expectedSecondary, actualSecondary);
    }

    [Fact]
    public void LanFileAmbiguationPositive()
    {
        // Tests both knights moving to same square for File ambiguity

        const string fen = "8/6k1/8/8/3N4/8/1K1N4/8 w - - 0 1";
        const MoveNotations notation = MoveNotations.Lan;

        var movingPiece = new Piece(Pieces.WhiteKnight);
        var fromOneSquare = new Square(Squares.d2);
        var fromTwoSquare = new Square(Squares.d4);
        var toSquare = new Square(Squares.f3);

        var uniChar = movingPiece.GetPieceChar();
        var toSquareString = toSquare.ToString();

        var expectedPrimary = $"{uniChar}{fromOneSquare}-{toSquareString}";
        var expectedSecondary = $"{uniChar}{fromTwoSquare}-{toSquareString}";

        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Position(board, pieceValue);
        var g = GameFactory.Create(pos);
        g.NewGame(fen);

        var w1 = new Move(fromOneSquare, toSquare);
        var w2 = new Move(fromTwoSquare, toSquare);

        var ambiguity = MoveAmbiguity.Create(pos);

        var actualPrimary = ambiguity.ToNotation(w1, notation);
        var actualSecondary = ambiguity.ToNotation(w2, notation);

        Assert.Equal(expectedPrimary, actualPrimary);
        Assert.Equal(expectedSecondary, actualSecondary);
    }

    [Fact]
    public void RanRankAmbiguationPositive()
    {
        // Tests both knights moving to same square for Rank ambiguity

        const string fen = "8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1";
        const MoveNotations notation = MoveNotations.Ran;

        var movingPiece = new Piece(Pieces.WhiteKnight);
        var fromOneSquare = new Square(Squares.d2);
        var fromTwoSquare = new Square(Squares.f2);
        var toSquare = new Square(Squares.e4);

        var uniChar = movingPiece.GetPieceChar();
        var toSquareString = toSquare.ToString();

        var expectedPrimary = $"{uniChar}{fromOneSquare}-{toSquareString}";
        var expectedSecondary = $"{uniChar}{fromTwoSquare}-{toSquareString}";

        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Position(board, pieceValue);
        var g = GameFactory.Create(pos);
        g.NewGame(fen);

        var w1 = new Move(fromOneSquare, toSquare);
        var w2 = new Move(fromTwoSquare, toSquare);

        var ambiguity = MoveAmbiguity.Create(pos);

        var actualPrimary = ambiguity.ToNotation(w1, notation);
        var actualSecondary = ambiguity.ToNotation(w2, notation);

        Assert.Equal(expectedPrimary, actualPrimary);
        Assert.Equal(expectedSecondary, actualSecondary);
    }

    [Fact]
    public void RanRankFileAmbiguationPositive()
    {
        // Tests both knights moving to same square for Rank ambiguity

        const string fen = "8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1";
        const MoveNotations notation = MoveNotations.Ran;

        var movingPiece = new Piece(Pieces.WhiteKnight);
        var fromOneSquare = new Square(Squares.d2);
        var fromTwoSquare = new Square(Squares.f2);
        var toSquare = new Square(Squares.e4);

        var uniChar = movingPiece.GetPieceChar();
        var toSquareString = toSquare.ToString();

        var expectedPrimary = $"{uniChar}{fromOneSquare}-{toSquareString}";
        var expectedSecondary = $"{uniChar}{fromTwoSquare}-{toSquareString}";

        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Position(board, pieceValue);
        var g = GameFactory.Create(pos);
        g.NewGame(fen);

        var w1 = new Move(fromOneSquare, toSquare);
        var w2 = new Move(fromTwoSquare, toSquare);

        var ambiguity = MoveAmbiguity.Create(pos);

        var actualPrimary = ambiguity.ToNotation(w1, notation);
        var actualSecondary = ambiguity.ToNotation(w2, notation);

        Assert.Equal(expectedPrimary, actualPrimary);
        Assert.Equal(expectedSecondary, actualSecondary);
    }

    [Fact]
    public void RanFileAmbiguationPositive()
    {
        // Tests both knights moving to same square for File ambiguity

        const string fen = "8/6k1/8/8/3N4/8/1K1N4/8 w - - 0 1";
        const MoveNotations notation = MoveNotations.Ran;

        var movingPiece = new Piece(Pieces.WhiteKnight);
        var fromOneSquare = new Square(Squares.d2);
        var fromTwoSquare = new Square(Squares.d4);
        var toSquare = new Square(Squares.f3);

        var uniChar = movingPiece.GetPieceChar();
        var toSquareString = toSquare.ToString();

        var expectedPrimary = $"{uniChar}{fromOneSquare}-{toSquareString}";
        var expectedSecondary = $"{uniChar}{fromTwoSquare}-{toSquareString}";

        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Position(board, pieceValue);
        var g = GameFactory.Create(pos);
        g.NewGame(fen);

        var w1 = new Move(fromOneSquare, toSquare);
        var w2 = new Move(fromTwoSquare, toSquare);

        var ambiguity = MoveAmbiguity.Create(pos);

        var actualPrimary = ambiguity.ToNotation(w1, notation);
        var actualSecondary = ambiguity.ToNotation(w2, notation);

        Assert.Equal(expectedPrimary, actualPrimary);
        Assert.Equal(expectedSecondary, actualSecondary);
    }

    [Fact]
    public void RookSanAmbiguity()
    {
        // Tests rook ambiguity notation for white rooks @ e1 and g2. Author : johnathandavis

        const string fen = "5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53";
        const MoveNotations notation = MoveNotations.San;
        var expectedNotations = new[] { "Ree2", "Rge2" };

        var game = GameFactory.Create(fen);

        var sanMoves = game.Pos
            .GenerateMoves()
            .Select(m => MoveAmbiguity.Create(game.Pos).ToNotation(m, notation))
            .ToArray();

        foreach (var notationResult in expectedNotations)
            Assert.Contains(sanMoves, s => s == notationResult);
    }
}