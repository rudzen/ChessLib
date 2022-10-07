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

using System;
using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.Protocol.UCI;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Test.ProtocolTests;

public sealed class UciTests
{
    [Fact]
    public void NpsSimple()
    {
        const ulong expected = ulong.MinValue;

        const ulong nodes = 1000UL;

        var ts = TimeSpan.FromSeconds(1);

        var uci = new Uci();

        var actual = uci.Nps(nodes, in ts);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MoveFromUciBasic()
    {
        const string uciMove = "a2a3";
        var expected = Move.Create(Square.A2, Square.A3);
        var uci = new Uci();

        var game = GameFactory.Create();
        game.NewGame();

        var actual = uci.MoveFromUci(game.Pos, uciMove);

        Assert.Equal(expected, actual);
    }
}