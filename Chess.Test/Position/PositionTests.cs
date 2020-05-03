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

namespace Chess.Test.Position
{
    using Rudz.Chess;
    using Rudz.Chess.Enums;
    using Rudz.Chess.Types;
    using Xunit;

    public sealed class PositionTests
    {
        [Fact]
        public void AddPieceTest()
        {
            var board = new Board();
            var position = new Position(board);

            position.AddPiece(Pieces.WhiteKing, Squares.a7);
            var pieces = position.Pieces();
            var square = pieces.Lsb();
            Assert.Equal(Squares.a7, square.Value);

            var piece = position.GetPiece(square);
            Assert.Equal(Pieces.WhiteKing, piece.Value);

            // test overload
            pieces = position.Pieces(piece);
            Assert.False((pieces & square).IsEmpty);

            // Test piece type overload

            board = new Board();
            position = new Position(board);

            position.AddPiece(PieceTypes.Knight.MakePiece(Player.Black), Squares.d5);
            pieces = position.Pieces();
            square = pieces.Lsb();
            Assert.Equal(Squares.d5, square.Value);

            piece = position.GetPiece(square);
            Assert.Equal(Pieces.BlackKnight, piece.Value);
        }

        [Fact]
        public void PinnedPiecesTest()
        {
            const int expected = 1;

            var board = new Board();
            var cb = new Position(board);

            cb.AddPiece(Pieces.WhiteKing, Squares.a6);
            cb.AddPiece(Pieces.WhiteBishop, Squares.d5);

            cb.AddPiece(Pieces.BlackKing, Squares.b3);
            cb.AddPiece(Pieces.BlackPawn, Squares.c4); // this is a pinned pieces

            var b = cb.GetPinnedPieces(Squares.b3, Player.Black);

            // b must contain one square at this point
            var pinnedCount = b.Count;

            Assert.Equal(expected, pinnedCount);

            // test for correct square
            var pinnedSquare = b.Lsb();

            Assert.Equal(Squares.c4, pinnedSquare.Value);
        }

        // TODO : Add test functions for the rest of the methods and properties in position class
    }
}