/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2019 Rudy Alex Kohn

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


using Rudz.Chess;
using Rudz.Chess.Enums;
using Rudz.Chess.Types;

namespace Chess.Tests
{
    using Xunit;

    public class PositionTests
    {
        [Fact]
        public void AddPieceTest()
        {
            Position position = new Position(null);

            position.AddPiece(EPieces.WhiteKing, ESquare.a7);
            Square square = position.Occupied.Lsb();
            Assert.Equal(ESquare.a7, square.Value);

            Piece piece = position.GetPiece(square);
            Assert.Equal(EPieces.WhiteKing, piece.Value);

            // Test piecetype overload

            position = new Position(null);

            position.AddPiece(EPieceType.Knight, ESquare.d5, PlayerExtensions.Black);
            square = position.Occupied.Lsb();
            Assert.Equal(ESquare.d5, square.Value);

            piece = position.GetPiece(square);
            Assert.Equal(EPieces.BlackKnight, piece.Value);
        }

        [Fact]
        public void PinnedPiecesTest()
        {
            Position cb = new Position(null);

            cb.AddPiece(EPieces.WhiteKing, ESquare.a6);
            cb.AddPiece(EPieces.WhiteBishop, ESquare.d5);

            cb.AddPiece(EPieces.BlackKing, ESquare.b3);
            cb.AddPiece(EPieces.BlackPawn, ESquare.c4); // this is a pinned pieces

            BitBoard b = cb.GetPinnedPieces(ESquare.b3, PlayerExtensions.Black);

            // b must contain one square at this point
            int pinnedCount = b.Count;

            Assert.Equal(1, pinnedCount);

            // test it's the correct square
            Square pinnedSquare = b.Lsb();

            Assert.Equal(ESquare.c4, pinnedSquare.Value);
        }

        // TODO : Add test functions for the rest of the methods and properties in position class
    }
}