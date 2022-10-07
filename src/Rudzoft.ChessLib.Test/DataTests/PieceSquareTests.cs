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

namespace Rudzoft.ChessLib.Test.DataTests;

public sealed class PieceSquareTests
{
    [Fact]
    public void GetSquare()
    {
        const Squares expected = Squares.a5;
        var ps = new PieceSquareEventArgs(Piece.EmptyPiece, expected);
        var actual = ps.Square;
        var expectedSquare = new Square(expected);
        Assert.Equal(expectedSquare, actual);
    }

    [Fact]
    public void GetPiece()
    {
        const Pieces expected = Pieces.BlackKnight;
        var ps = new PieceSquareEventArgs(expected, Square.None);
        var actual = ps.Piece;
        var expectedPiece = new Piece(expected);
        Assert.Equal(expectedPiece, actual);
    }

    [Fact]
    public void GetPieceAndSquare()
    {
        for (var pc = Pieces.NoPiece; pc < Pieces.PieceNb; ++pc)
        {
            var expectedPiece = new Piece(pc);
            foreach (var sq in BitBoards.AllSquares)
            {
                var expectedSquare = new Square(sq);
                var ps = new PieceSquareEventArgs(expectedPiece, expectedSquare);
                Assert.Equal(expectedPiece, ps.Piece);
                Assert.Equal(expectedSquare, sq);
            }
        }
    }
}