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

using System.IO;
using System.Reflection;
using FluentAssertions;
using Rudz.Chess;
using Rudz.Chess.Factories;
using Rudz.Chess.Fen;
using Rudz.Chess.Polyglot;
using Rudz.Chess.Protocol.UCI;
using Rudz.Chess.Types;
using Xunit;

namespace Chess.Test.BookTests;

public class PolyglotTests
{
    private const string BookFile = @"BookTests\gm2600.bin";

    [Fact]
    public void UnsetBookFileYieldsEmptyMove()
    {
        const string fen = Fen.StartPositionFen;

        var game = GameFactory.Create(fen);
        var book = new Book(game.Pos);

        var bookMove = book.Probe();

        var m = Move.EmptyMove;

        Assert.Equal(m, bookMove);
    }

    [Fact]
    public void BookFileLoadsCorrectly()
    {
        const string fen = Fen.StartPositionFen;

        var game = GameFactory.Create(fen);
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), BookFile);
        var book = new Book(game.Pos)
        {
            FileName = path
        };

        book.FileName.Should().NotBeNull();

        var expected = Move.Create(Squares.d2, Squares.d4);
        var actual = book.Probe();

        Assert.False(actual.IsNullMove());
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void BookStartPositionCorrectlyCalculatesPolyglotHash()
    {
        const ulong expected = 5060803636482931868UL;
        const string fen = Fen.StartPositionFen;

        var game = GameFactory.Create(fen);
        var book = new Book(game.Pos);

        var actual = book.ComputePolyglotKey().Key;
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(new string[] { }, 5060803636482931868UL)]
    [InlineData(new[] { "e2e4" }, 9384546495678726550UL)]
    [InlineData(new[] { "e2e4", "d7d5" }, 528813709611831216UL)]
    [InlineData(new[] { "e2e4", "d7d5", "e4e5", }, 7363297126586722772UL)]
    [InlineData(new[] { "e2e4", "d7d5", "e4e5", "f7f5" }, 2496273314520498040UL)]
    [InlineData(new[] { "e2e4", "d7d5", "e4e5", "f7f5", "e1e2" }, 7289745035295343297UL)]
    [InlineData(new[] { "e2e4", "d7d5", "e4e5", "f7f5", "e1e2", "e8f7" }, 71445182323015129UL)]
    [InlineData(new[] { "a2a4", "b7b5", "h2h4", "b5b4", "c2c4" }, 4359805404264691255UL)]
    [InlineData(new[] { "a2a4", "b7b5", "h2h4", "b5b4", "c2c4", "b4c3", "a1a3" }, 6647202560273257824UL)]
    public void HashKeyFromInitialPositionIsComputedCorrectly(string[] uciMoves, ulong expectedHashKey)
    {
        const string fen = Fen.StartPositionFen;

        var uci = new Uci();
        var game = GameFactory.Create(fen);
        var book = new Book(game.Pos);
        var state = new State();

        foreach (var uciMove in uciMoves)
        {
            var m = uci.MoveFromUci(game.Pos, uciMove);
            Assert.False(m.IsNullMove());
            game.Pos.MakeMove(m, state);
        }

        var actualKey = book.ComputePolyglotKey().Key;

        actualKey.Should().Be(expectedHashKey);
    }
}