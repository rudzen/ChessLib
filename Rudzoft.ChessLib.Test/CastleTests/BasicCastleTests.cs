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

using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Test.CastleTests;

public sealed class BasicCastleTests
{
    [Theory]
    [InlineData(Fen.Fen.StartPositionFen, CastlelingRights.None, false)]
    [InlineData("rnbqk2r/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1", CastlelingRights.KingSide, true)]
    [InlineData("rnbqk2r/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1", CastlelingRights.QueenSide, true)]
    [InlineData("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1", CastlelingRights.KingSide, true)]
    [InlineData("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1", CastlelingRights.QueenSide, true)]
    public void CanPotentiallyCastle(string fen, CastlelingRights cr, bool expected)
    {
        var pos = GameFactory.Create(fen).Pos;
        var fd = new FenData(fen);
        var state = new State();
        pos.Set(in fd, ChessMode.Normal, state);
        var actual = pos.CanCastle(cr);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(Fen.Fen.StartPositionFen, CastlelingRights.WhiteOo, true)]
    [InlineData(Fen.Fen.StartPositionFen, CastlelingRights.WhiteOoo, true)]
    [InlineData(Fen.Fen.StartPositionFen, CastlelingRights.BlackOo, true)]
    [InlineData(Fen.Fen.StartPositionFen, CastlelingRights.BlackOoo, true)]
    [InlineData("rnbqk2r/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1", CastlelingRights.WhiteOo, false)]
    [InlineData("rnbqk2r/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1", CastlelingRights.WhiteOoo, true)]
    [InlineData("rnbqk2r/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1", CastlelingRights.BlackOo, false)]
    [InlineData("rnbqk2r/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1", CastlelingRights.BlackOoo, true)]
    [InlineData("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1", CastlelingRights.WhiteOo, false)]
    [InlineData("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1", CastlelingRights.WhiteOoo, false)]
    [InlineData("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1", CastlelingRights.BlackOo, false)]
    [InlineData("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1", CastlelingRights.BlackOoo, false)]
    public void IsCastleImpeeded(string fen, CastlelingRights cr, bool expected)
    {
        var pos = GameFactory.Create(fen).Pos;
        var fd = new FenData(fen);
        var state = new State();
        pos.Set(in fd, ChessMode.Normal, state);
        var actual = pos.CastlingImpeded(cr);
        Assert.Equal(expected, actual);
    }
}