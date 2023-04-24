using System;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Test.SquareTests;

public sealed class SquareRangeTests
{
    [Theory]
    [InlineData(Squares.a1, true, false)]
    [InlineData(Squares.a2, true, false)]
    [InlineData(Squares.a3, true, false)]
    [InlineData(Squares.a4, true, false)]
    [InlineData(Squares.a5, true, false)]
    [InlineData(Squares.a6, true, false)]
    [InlineData(Squares.a7, true, false)]
    [InlineData(Squares.a8, true, false)]
    [InlineData(Squares.b1, true, false)]
    [InlineData(Squares.b2, true, false)]
    [InlineData(Squares.b3, true, false)]
    [InlineData(Squares.b4, true, false)]
    [InlineData(Squares.b5, true, false)]
    [InlineData(Squares.b6, true, false)]
    [InlineData(Squares.b7, true, false)]
    [InlineData(Squares.b8, true, false)]
    [InlineData(Squares.c1, true, false)]
    [InlineData(Squares.c2, true, false)]
    [InlineData(Squares.c3, true, false)]
    [InlineData(Squares.c4, true, false)]
    [InlineData(Squares.c5, true, false)]
    [InlineData(Squares.c6, true, false)]
    [InlineData(Squares.c7, true, false)]
    [InlineData(Squares.c8, true, false)]
    [InlineData(Squares.d1, true, false)]
    [InlineData(Squares.d2, true, false)]
    [InlineData(Squares.d3, true, false)]
    [InlineData(Squares.d4, true, false)]
    [InlineData(Squares.d5, true, false)]
    [InlineData(Squares.d6, true, false)]
    [InlineData(Squares.d7, true, false)]
    [InlineData(Squares.d8, true, false)]
    [InlineData(Squares.e1, false, true)]
    [InlineData(Squares.e2, false, true)]
    [InlineData(Squares.e3, false, true)]
    [InlineData(Squares.e4, false, true)]
    [InlineData(Squares.e5, false, true)]
    [InlineData(Squares.e6, false, true)]
    [InlineData(Squares.e7, false, true)]
    [InlineData(Squares.e8, false, true)]
    [InlineData(Squares.f1, false, true)]
    [InlineData(Squares.f2, false, true)]
    [InlineData(Squares.f3, false, true)]
    [InlineData(Squares.f4, false, true)]
    [InlineData(Squares.f5, false, true)]
    [InlineData(Squares.f6, false, true)]
    [InlineData(Squares.f7, false, true)]
    [InlineData(Squares.f8, false, true)]
    [InlineData(Squares.g1, false, true)]
    [InlineData(Squares.g2, false, true)]
    [InlineData(Squares.g3, false, true)]
    [InlineData(Squares.g4, false, true)]
    [InlineData(Squares.g5, false, true)]
    [InlineData(Squares.g6, false, true)]
    [InlineData(Squares.g7, false, true)]
    [InlineData(Squares.g8, false, true)]
    [InlineData(Squares.h1, false, true)]
    [InlineData(Squares.h2, false, true)]
    [InlineData(Squares.h3, false, true)]
    [InlineData(Squares.h4, false, true)]
    [InlineData(Squares.h5, false, true)]
    [InlineData(Squares.h6, false, true)]
    [InlineData(Squares.h7, false, true)]
    [InlineData(Squares.h8, false, true)]
    [InlineData(Squares.none, false, false)]
    public void InWhiteSquaresRange(Squares sq, bool expectedInWhiteRange, bool expectedInBlackRange)
    {
        Square s = sq;
        var whiteSquares = Square.All.AsSpan(Square.WhiteSide);
        var blackSquares = Square.All.AsSpan(Square.BlackSide);

        var actualInWhite = whiteSquares.Contains(s);
        var actualInBlack = blackSquares.Contains(s);
        
        Assert.Equal(expectedInWhiteRange, actualInWhite);
        Assert.Equal(expectedInBlackRange, actualInBlack);
    }
}