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

using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Test.MoveTests;

public sealed class MoveGen_49
{
    [Fact]
    public void MoveListContainsMismatchedElement()
    {
        const string fen = "r3kb1r/p3pppp/p1n2n2/2pp1Q2/3P1B2/2P1PN2/Pq3PPP/RN2K2R w KQkq - 0 9";

        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Position(board, pieceValue);
        var fd = new FenData(fen);
        var state = new State();

        pos.Set(in fd, ChessMode.Normal, state);

        var ml = pos.GenerateMoves();

        const bool expected = false;
        var actual = ml.Contains(new Move(new Square(Ranks.Rank1, Files.FileE), new Square(Ranks.Rank1, Files.FileG),
            MoveTypes.Castling));

        Assert.Equal(expected, actual);
    }
}