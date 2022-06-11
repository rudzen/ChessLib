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

namespace Chess.Test.Protocol;

using FluentAssertions;
using Rudz.Chess.Enums;
using Rudz.Chess.Factories;
using Rudz.Chess.Protocol.UCI;
using System;
using Xunit;

public sealed class UciTests
{
    [Fact]
    public void NpsSimpleTest()
    {
        const ulong expected = 0UL;

        const ulong nodes = 1000UL;

        var ts = TimeSpan.FromSeconds(1);

        var uci = new Uci();

        var actual = uci.Nps(nodes, ts);

        actual.Should().Be(expected);
    }

    [Fact]
    public void MoveFromUciBasicTest()
    {
        const string uciMove = "a2a3";
        var expected = new Rudz.Chess.Types.Move(Squares.a2, Squares.a3);
        var uci = new Uci();

        var game = GameFactory.Create();
        game.NewGame();

        var actual = uci.MoveFromUci(game.Pos, uciMove);

        actual.Should().Be(expected);
    }
}
