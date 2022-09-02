﻿/*
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

using Rudzoft.ChessLib.Tables;
using Rudzoft.ChessLib.Types;

namespace Chess.Test.TablesTests;

public sealed class KillerMovesTests
{
    [Fact]
    public void BaseAddMove()
    {
        var km = KillerMoves.Create(128);
        const int depth = 1;
        var move = Move.Create(Square.A2, Square.A3);
        var pc = PieceTypes.Pawn.MakePiece(Player.White);

        km.UpdateValue(depth, move, pc);

        var value = km.GetValue(depth, move, pc);

        Assert.Equal(2, value);
    }

    [Fact]
    public void GetValueWithWrongDepthYieldsZero()
    {
        var km = KillerMoves.Create(128);
        const int depth = 1;
        var move = Move.Create(Square.A2, Square.A3);
        var pc = PieceTypes.Pawn.MakePiece(Player.White);

        km.UpdateValue(depth, move, pc);

        var value = km.GetValue(depth + 1, move, pc);

        Assert.Equal(0, value);
    }
}