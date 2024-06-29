﻿/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2023 Rudy Alex Kohn

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

using Microsoft.Extensions.DependencyInjection;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Notation;
using Rudzoft.ChessLib.Notation.Notations;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Test.NotationTests;

public sealed class RanTests
{
    private readonly IServiceProvider _serviceProvider = new ServiceCollection()
                                                         .AddChessLib()
                                                         .BuildServiceProvider();

    [Theory]
    [InlineData("8/6k1/8/8/3N4/8/1K1N4/8 w - - 0 1", MoveNotations.Ran, PieceTypes.Knight, Squares.d2, Squares.d4,
        Squares.f3)]
    [InlineData("8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1", MoveNotations.Ran, PieceTypes.Knight, Squares.d2, Squares.f2,
        Squares.e4)]
    [InlineData("8/6k1/8/8/3N4/8/1K1N4/8 w - - 0 1", MoveNotations.Lan, PieceTypes.Knight, Squares.d2, Squares.d4,
        Squares.f3)]
    [InlineData("8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1", MoveNotations.Lan, PieceTypes.Knight, Squares.d2, Squares.f2,
        Squares.e4)]
    public void RanLanAmbiguities(
        string fen,
        MoveNotations moveNotations,
        PieceTypes movingPt,
        Squares fromSqOne,
        Squares fromSqTwo,
        Squares toSq)
    {
        var pos = _serviceProvider.GetRequiredService<IPosition>();

        var fenData = new FenData(fen);
        var state   = new State();

        pos.Set(in fenData, ChessMode.Normal, state);

        var pt = new PieceType(movingPt);
        var pc = pt.MakePiece(pos.SideToMove);

        var fromOne = new Square(fromSqOne);
        var fromTwo = new Square(fromSqTwo);
        var to      = new Square(toSq);

        Assert.True(fromOne.IsOk);
        Assert.True(fromTwo.IsOk);
        Assert.True(to.IsOk);

        var pieceChar = pc.GetPieceChar();
        var toString  = to.ToString();

        var moveOne = Move.Create(fromOne, to);
        var moveTwo = Move.Create(fromTwo, to);

        var moveNotation = _serviceProvider.GetRequiredService<IMoveNotation>();
        var notation     = moveNotation.ToNotation(moveNotations);

        var expectedOne = $"{pieceChar}{fromOne}-{toString}";
        var expectedTwo = $"{pieceChar}{fromTwo}-{toString}";

        var actualOne = notation.Convert(pos, moveOne);
        var actualTwo = notation.Convert(pos, moveTwo);

        Assert.Equal(expectedOne, actualOne);
        Assert.Equal(expectedTwo, actualTwo);
    }
}