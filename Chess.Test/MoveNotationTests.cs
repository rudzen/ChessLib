using Rudz.Chess;
using Rudz.Chess.Enums;
using Rudz.Chess.Types;
using Xunit;

namespace Chess.Tests
{
    public class MoveNotationTests
    {
        [Fact]
        public void FanRankAmbiguationPositiveTest()
        {
            // Tests both knights moving to same square for Rank ambiguity

            const string fen = "8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1";
            const EMoveNotation notation = EMoveNotation.Fan;

            var movingPiece = new Piece(EPieces.WhiteKnight);
            var fromOneSquare = new Square(ESquare.d2);
            var fromTwoSquare = new Square(ESquare.f2);
            var toSquare = new Square(ESquare.e4);

            var uniChar = movingPiece.GetUnicodeChar();
            var toSquareString = toSquare.GetSquareString();

            var expectedPrimary = $"{uniChar}{fromOneSquare.FileChar()}{toSquareString}";
            var expectedSecondary = $"{uniChar}{fromTwoSquare.FileChar()}{toSquareString}";

            var g = new Game();
            g.NewGame(fen);

            var w1 = new Move(movingPiece, fromOneSquare, toSquare);
            var w2 = new Move(movingPiece, fromTwoSquare, toSquare);

            var actualPrimary = w1.ToNotation(g.State, notation);
            var actualSecondary = w2.ToNotation(g.State, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void FanRankFileAmbiguationPositiveTest()
        {
            // Tests both knights moving to same square for Rank ambiguity

            const string fen = "8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1";
            const EMoveNotation notation = EMoveNotation.Fan;

            var movingPiece = new Piece(EPieces.WhiteKnight);
            var fromOneSquare = new Square(ESquare.d2);
            var fromTwoSquare = new Square(ESquare.g5);
            var toSquare = new Square(ESquare.e4);

            var uniChar = movingPiece.GetUnicodeChar();
            var toSquareString = toSquare.GetSquareString();

            var expectedPrimary = $"{uniChar}{fromOneSquare.FileChar()}{toSquareString}";
            var expectedSecondary = $"{uniChar}{fromTwoSquare.FileChar()}{toSquareString}";

            var g = new Game();
            g.NewGame(fen);

            var w1 = new Move(movingPiece, fromOneSquare, toSquare);
            var w2 = new Move(movingPiece, fromTwoSquare, toSquare);

            var actualPrimary = w1.ToNotation(g.State, notation);
            var actualSecondary = w2.ToNotation(g.State, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void FanFileAmbiguationPositiveTest()
        {
            // Tests both knights moving to same square for File ambiguity

            const string fen = "8/6k1/8/8/3N4/8/1K1N4/8 w - - 0 1";
            const EMoveNotation notation = EMoveNotation.Fan;

            var movingPiece = new Piece(EPieces.WhiteKnight);
            var fromOneSquare = new Square(ESquare.d2);
            var fromTwoSquare = new Square(ESquare.d4);
            var toSquare = new Square(ESquare.f3);

            var uniChar = movingPiece.GetUnicodeChar();
            var toSquareString = toSquare.GetSquareString();

            var expectedPrimary = $"{uniChar}{fromOneSquare.RankOfChar()}{toSquareString}";
            var expectedSecondary = $"{uniChar}{fromTwoSquare.RankOfChar()}{toSquareString}";

            var g = new Game();
            g.NewGame(fen);

            var w1 = new Move(movingPiece, fromOneSquare, toSquare);
            var w2 = new Move(movingPiece, fromTwoSquare, toSquare);

            var actualPrimary = w1.ToNotation(g.State, notation);
            var actualSecondary = w2.ToNotation(g.State, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void SanRankAmbiguationPositiveTest()
        {
            // Tests both knights moving to same square for Rank ambiguity

            const string fen = "8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1";
            const EMoveNotation notation = EMoveNotation.San;

            var movingPiece = new Piece(EPieces.WhiteKnight);
            var fromOneSquare = new Square(ESquare.d2);
            var fromTwoSquare = new Square(ESquare.f2);
            var toSquare = new Square(ESquare.e4);

            var uniChar = movingPiece.GetPieceChar();
            var toSquareString = toSquare.GetSquareString();

            var expectedPrimary = $"{uniChar}{fromOneSquare.FileChar()}{toSquareString}";
            var expectedSecondary = $"{uniChar}{fromTwoSquare.FileChar()}{toSquareString}";

            var g = new Game();
            g.NewGame(fen);

            var w1 = new Move(movingPiece, fromOneSquare, toSquare);
            var w2 = new Move(movingPiece, fromTwoSquare, toSquare);

            var actualPrimary = w1.ToNotation(g.State, notation);
            var actualSecondary = w2.ToNotation(g.State, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void SanRankFileAmbiguationPositiveTest()
        {
            // Tests both knights moving to same square for Rank ambiguity

            const string fen = "8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1";
            const EMoveNotation notation = EMoveNotation.San;

            var movingPiece = new Piece(EPieces.WhiteKnight);
            var fromOneSquare = new Square(ESquare.d2);
            var fromTwoSquare = new Square(ESquare.g5);
            var toSquare = new Square(ESquare.e4);

            var uniChar = movingPiece.GetPieceChar();
            var toSquareString = toSquare.GetSquareString();

            var expectedPrimary = $"{uniChar}{fromOneSquare.FileChar()}{toSquareString}";
            var expectedSecondary = $"{uniChar}{fromTwoSquare.FileChar()}{toSquareString}";

            var g = new Game();
            g.NewGame(fen);

            var w1 = new Move(movingPiece, fromOneSquare, toSquare);
            var w2 = new Move(movingPiece, fromTwoSquare, toSquare);

            var actualPrimary = w1.ToNotation(g.State, notation);
            var actualSecondary = w2.ToNotation(g.State, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void SanFileAmbiguationPositiveTest()
        {
            // Tests both knights moving to same square for File ambiguity

            const string fen = "8/6k1/8/8/3N4/8/1K1N4/8 w - - 0 1";
            const EMoveNotation notation = EMoveNotation.San;

            var movingPiece = new Piece(EPieces.WhiteKnight);
            var fromOneSquare = new Square(ESquare.d2);
            var fromTwoSquare = new Square(ESquare.d4);
            var toSquare = new Square(ESquare.f3);

            var uniChar = movingPiece.GetPieceChar();
            var toSquareString = toSquare.GetSquareString();

            var expectedPrimary = $"{uniChar}{fromOneSquare.RankOfChar()}{toSquareString}";
            var expectedSecondary = $"{uniChar}{fromTwoSquare.RankOfChar()}{toSquareString}";

            var g = new Game();
            g.NewGame(fen);

            var w1 = new Move(movingPiece, fromOneSquare, toSquare);
            var w2 = new Move(movingPiece, fromTwoSquare, toSquare);

            var actualPrimary = w1.ToNotation(g.State, notation);
            var actualSecondary = w2.ToNotation(g.State, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void LanRankAmbiguationPositiveTest()
        {
            // Tests both knights moving to same square for Rank ambiguity

            const string fen = "8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1";
            const EMoveNotation notation = EMoveNotation.Lan;

            var movingPiece = new Piece(EPieces.WhiteKnight);
            var fromOneSquare = new Square(ESquare.d2);
            var fromTwoSquare = new Square(ESquare.f2);
            var toSquare = new Square(ESquare.e4);

            var uniChar = movingPiece.GetPieceChar();
            var toSquareString = toSquare.GetSquareString();

            var expectedPrimary = $"{uniChar}{fromOneSquare.GetSquareString()}-{toSquareString}";
            var expectedSecondary = $"{uniChar}{fromTwoSquare.GetSquareString()}-{toSquareString}";

            var g = new Game();
            g.NewGame(fen);

            var w1 = new Move(movingPiece, fromOneSquare, toSquare);
            var w2 = new Move(movingPiece, fromTwoSquare, toSquare);

            var actualPrimary = w1.ToNotation(g.State, notation);
            var actualSecondary = w2.ToNotation(g.State, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void LanRankFileAmbiguationPositiveTest()
        {
            // Tests both knights moving to same square for Rank ambiguity

            const string fen = "8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1";
            const EMoveNotation notation = EMoveNotation.Lan;

            var movingPiece = new Piece(EPieces.WhiteKnight);
            var fromOneSquare = new Square(ESquare.d2);
            var fromTwoSquare = new Square(ESquare.g5);
            var toSquare = new Square(ESquare.e4);

            var uniChar = movingPiece.GetPieceChar();
            var toSquareString = toSquare.GetSquareString();

            var expectedPrimary = $"{uniChar}{fromOneSquare.GetSquareString()}-{toSquareString}";
            var expectedSecondary = $"{uniChar}{fromTwoSquare.GetSquareString()}-{toSquareString}";

            var g = new Game();
            g.NewGame(fen);

            var w1 = new Move(movingPiece, fromOneSquare, toSquare);
            var w2 = new Move(movingPiece, fromTwoSquare, toSquare);

            var actualPrimary = w1.ToNotation(g.State, notation);
            var actualSecondary = w2.ToNotation(g.State, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void LanFileAmbiguationPositiveTest()
        {
            // Tests both knights moving to same square for File ambiguity

            const string fen = "8/6k1/8/8/3N4/8/1K1N4/8 w - - 0 1";
            const EMoveNotation notation = EMoveNotation.Lan;

            var movingPiece = new Piece(EPieces.WhiteKnight);
            var fromOneSquare = new Square(ESquare.d2);
            var fromTwoSquare = new Square(ESquare.d4);
            var toSquare = new Square(ESquare.f3);

            var uniChar = movingPiece.GetPieceChar();
            var toSquareString = toSquare.GetSquareString();

            var expectedPrimary = $"{uniChar}{fromOneSquare.GetSquareString()}-{toSquareString}";
            var expectedSecondary = $"{uniChar}{fromTwoSquare.GetSquareString()}-{toSquareString}";

            var g = new Game();
            g.NewGame(fen);

            var w1 = new Move(movingPiece, fromOneSquare, toSquare);
            var w2 = new Move(movingPiece, fromTwoSquare, toSquare);

            var actualPrimary = w1.ToNotation(g.State, notation);
            var actualSecondary = w2.ToNotation(g.State, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }


        [Fact]
        public void RanRankAmbiguationPositiveTest()
        {
            // Tests both knights moving to same square for Rank ambiguity

            const string fen = "8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1";
            const EMoveNotation notation = EMoveNotation.Ran;

            var movingPiece = new Piece(EPieces.WhiteKnight);
            var fromOneSquare = new Square(ESquare.d2);
            var fromTwoSquare = new Square(ESquare.f2);
            var toSquare = new Square(ESquare.e4);

            var uniChar = movingPiece.GetPieceChar();
            var toSquareString = toSquare.GetSquareString();

            var expectedPrimary = $"{uniChar}{fromOneSquare.GetSquareString()}-{toSquareString}";
            var expectedSecondary = $"{uniChar}{fromTwoSquare.GetSquareString()}-{toSquareString}";

            var g = new Game();
            g.NewGame(fen);

            var w1 = new Move(movingPiece, fromOneSquare, toSquare);
            var w2 = new Move(movingPiece, fromTwoSquare, toSquare);

            var actualPrimary = w1.ToNotation(g.State, notation);
            var actualSecondary = w2.ToNotation(g.State, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void RanRankFileAmbiguationPositiveTest()
        {
            // Tests both knights moving to same square for Rank ambiguity

            const string fen = "8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1";
            const EMoveNotation notation = EMoveNotation.Ran;

            var movingPiece = new Piece(EPieces.WhiteKnight);
            var fromOneSquare = new Square(ESquare.d2);
            var fromTwoSquare = new Square(ESquare.g5);
            var toSquare = new Square(ESquare.e4);

            var uniChar = movingPiece.GetPieceChar();
            var toSquareString = toSquare.GetSquareString();

            var expectedPrimary = $"{uniChar}{fromOneSquare.GetSquareString()}-{toSquareString}";
            var expectedSecondary = $"{uniChar}{fromTwoSquare.GetSquareString()}-{toSquareString}";

            var g = new Game();
            g.NewGame(fen);

            var w1 = new Move(movingPiece, fromOneSquare, toSquare);
            var w2 = new Move(movingPiece, fromTwoSquare, toSquare);

            var actualPrimary = w1.ToNotation(g.State, notation);
            var actualSecondary = w2.ToNotation(g.State, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }

        [Fact]
        public void RanFileAmbiguationPositiveTest()
        {
            // Tests both knights moving to same square for File ambiguity

            const string fen = "8/6k1/8/8/3N4/8/1K1N4/8 w - - 0 1";
            const EMoveNotation notation = EMoveNotation.Ran;

            var movingPiece = new Piece(EPieces.WhiteKnight);
            var fromOneSquare = new Square(ESquare.d2);
            var fromTwoSquare = new Square(ESquare.d4);
            var toSquare = new Square(ESquare.f3);

            var uniChar = movingPiece.GetPieceChar();
            var toSquareString = toSquare.GetSquareString();

            var expectedPrimary = $"{uniChar}{fromOneSquare.GetSquareString()}-{toSquareString}";
            var expectedSecondary = $"{uniChar}{fromTwoSquare.GetSquareString()}-{toSquareString}";

            var g = new Game();
            g.NewGame(fen);

            var w1 = new Move(movingPiece, fromOneSquare, toSquare);
            var w2 = new Move(movingPiece, fromTwoSquare, toSquare);

            var actualPrimary = w1.ToNotation(g.State, notation);
            var actualSecondary = w2.ToNotation(g.State, notation);

            Assert.Equal(expectedPrimary, actualPrimary);
            Assert.Equal(expectedSecondary, actualSecondary);
        }
    }
}