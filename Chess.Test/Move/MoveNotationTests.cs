/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2020 Rudy Alex Kohn

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

namespace Chess.Test.Move
{
    using Rudz.Chess;
    using Rudz.Chess.Enums;
    using Rudz.Chess.Factories;
    using Rudz.Chess.Types;
    using Xunit;

    public sealed class MoveNotationTests
    {
        [Fact]
        public void FanRankAmbiguationPositiveTest()
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

            var ambiguity = new MoveAmbiguity(pos);

            var actualPrimary = ambiguity.ToNotation(w1);
            var actualSecondary = ambiguity.ToNotation(w2);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void FanRankFileAmbiguationPositiveTest()
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

            var ambiguity = new MoveAmbiguity(pos);

            var actualPrimary = ambiguity.ToNotation(w1, notation);
            var actualSecondary = ambiguity.ToNotation(w2, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void FanFileAmbiguationPositiveTest()
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

            var ambiguity = new MoveAmbiguity(pos);

            var actualPrimary = ambiguity.ToNotation(w1, notation);
            var actualSecondary = ambiguity.ToNotation(w2, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void SanRankAmbiguationPositiveTest()
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

            var ambiguity = new MoveAmbiguity(pos);

            var actualPrimary = ambiguity.ToNotation(w1, notation);
            var actualSecondary = ambiguity.ToNotation(w2, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void SanRankFileAmbiguationPositiveTest()
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

            var ambiguity = new MoveAmbiguity(pos);

            var actualPrimary = ambiguity.ToNotation(w1, notation);
            var actualSecondary = ambiguity.ToNotation(w2, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void SanFileAmbiguationPositiveTest()
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

            var ambiguity = new MoveAmbiguity(pos);

            var actualPrimary = ambiguity.ToNotation(w1, notation);
            var actualSecondary = ambiguity.ToNotation(w2, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void LanRankAmbiguationPositiveTest()
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

            var expectedPrimary = $"{uniChar}{fromOneSquare.ToString()}-{toSquareString}";
            var expectedSecondary = $"{uniChar}{fromTwoSquare.ToString()}-{toSquareString}";

            var board = new Board();
            var pieceValue = new PieceValue();
            var pos = new Position(board, pieceValue);
            var g = GameFactory.Create(pos);
            g.NewGame(fen);

            var w1 = new Move(fromOneSquare, toSquare);
            var w2 = new Move(fromTwoSquare, toSquare);

            var ambiguity = new MoveAmbiguity(pos);

            var actualPrimary = ambiguity.ToNotation(w1, notation);
            var actualSecondary = ambiguity.ToNotation(w2, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void LanRankFileAmbiguationPositiveTest()
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

            var expectedPrimary = $"{uniChar}{fromOneSquare.ToString()}-{toSquareString}";
            var expectedSecondary = $"{uniChar}{fromTwoSquare.ToString()}-{toSquareString}";

            var board = new Board();
            var pieceValue = new PieceValue();
            var pos = new Position(board, pieceValue);
            var g = GameFactory.Create(pos);
            g.NewGame(fen);

            var w1 = new Move(fromOneSquare, toSquare);
            var w2 = new Move(fromTwoSquare, toSquare);

            var ambiguity = new MoveAmbiguity(pos);

            var actualPrimary = ambiguity.ToNotation(w1, notation);
            var actualSecondary = ambiguity.ToNotation(w2, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void LanFileAmbiguationPositiveTest()
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

            var expectedPrimary = $"{uniChar}{fromOneSquare.ToString()}-{toSquareString}";
            var expectedSecondary = $"{uniChar}{fromTwoSquare.ToString()}-{toSquareString}";

            var board = new Board();
            var pieceValue = new PieceValue();
            var pos = new Position(board, pieceValue);
            var g = GameFactory.Create(pos);
            g.NewGame(fen);

            var w1 = new Move(fromOneSquare, toSquare);
            var w2 = new Move(fromTwoSquare, toSquare);

            var ambiguity = new MoveAmbiguity(pos);

            var actualPrimary = ambiguity.ToNotation(w1, notation);
            var actualSecondary = ambiguity.ToNotation(w2, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void RanRankAmbiguationPositiveTest()
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

            var expectedPrimary = $"{uniChar}{fromOneSquare.ToString()}-{toSquareString}";
            var expectedSecondary = $"{uniChar}{fromTwoSquare.ToString()}-{toSquareString}";

            var board = new Board();
            var pieceValue = new PieceValue();
            var pos = new Position(board, pieceValue);
            var g = GameFactory.Create(pos);
            g.NewGame(fen);

            var w1 = new Move(fromOneSquare, toSquare);
            var w2 = new Move(fromTwoSquare, toSquare);

            var ambiguity = new MoveAmbiguity(pos);

            var actualPrimary = ambiguity.ToNotation(w1, notation);
            var actualSecondary = ambiguity.ToNotation(w2, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void RanRankFileAmbiguationPositiveTest()
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

            var expectedPrimary = $"{uniChar}{fromOneSquare.ToString()}-{toSquareString}";
            var expectedSecondary = $"{uniChar}{fromTwoSquare.ToString()}-{toSquareString}";

            var board = new Board();
            var pieceValue = new PieceValue();
            var pos = new Position(board, pieceValue);
            var g = GameFactory.Create(pos);
            g.NewGame(fen);

            var w1 = new Move(fromOneSquare, toSquare);
            var w2 = new Move(fromTwoSquare, toSquare);

            var ambiguity = new MoveAmbiguity(pos);

            var actualPrimary = ambiguity.ToNotation(w1, notation);
            var actualSecondary = ambiguity.ToNotation(w2, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void RanFileAmbiguationPositiveTest()
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

            var expectedPrimary = $"{uniChar}{fromOneSquare.ToString()}-{toSquareString}";
            var expectedSecondary = $"{uniChar}{fromTwoSquare.ToString()}-{toSquareString}";

            var board = new Board();
            var pieceValue = new PieceValue();
            var pos = new Position(board, pieceValue);
            var g = GameFactory.Create(pos);
            g.NewGame(fen);

            var w1 = new Move(fromOneSquare, toSquare);
            var w2 = new Move(fromTwoSquare, toSquare);

            var ambiguity = new MoveAmbiguity(pos);

            var actualPrimary = ambiguity.ToNotation(w1, notation);
            var actualSecondary = ambiguity.ToNotation(w2, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }
    }
}