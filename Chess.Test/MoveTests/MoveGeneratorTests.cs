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

namespace Chess.Test.MoveTests;

using Rudz.Chess;
using Rudz.Chess.Fen;
using Rudz.Chess.MoveGeneration;
using Rudz.Chess.Types;
using Xunit;

public class MoveGeneratorTests
{
    [Fact]
    public void InCheckMoveGeneration()
    {
        const string fen = "rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6";
        const int expectedMoves = 4;

        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Position(board, pieceValue);
        var fd = new FenData(fen);
        var state = new State();

        pos.Set(in fd, Rudz.Chess.Enums.ChessMode.NORMAL, state);

        // make sure black is in check
        Assert.True(pos.InCheck);

        // generate moves for black
        var mg = pos.GenerateMoves();
        var actualMoves = mg.Length;

        Assert.Equal(expectedMoves, actualMoves);
    }
}
