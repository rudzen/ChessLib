using Rudz.Chess;
using Rudz.Chess.Enums;
using Rudz.Chess.Factories;
using Rudz.Chess.Fen;
using Rudz.Chess.MoveGeneration;
using Rudz.Chess.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Chess.Test.MoveTests;

public class MoveGen_49
{
    [Fact]
    public void MoveListContainsMismatchedElement()
    {
        const string fen = "r3kb1r/p3pppp/p1n2n2/2pp1Q2/3P1B2/2P1PN2/Pq3PPP/RN2K2R w KQkq - 0 9";

        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Rudz.Chess.Position(board, pieceValue);
        var fd = new FenData(fen);
        var state = new State();

        pos.Set(in fd, ChessMode.NORMAL, state);

        var ml = pos.GenerateMoves();

        const bool expected = false;
        var actual = ml.Contains(new Rudz.Chess.Types.Move(new Rudz.Chess.Types.Square(Ranks.Rank1, Files.FileE), new Rudz.Chess.Types.Square(Ranks.Rank1, Files.FileG), MoveTypes.Castling));

        Assert.Equal(expected, actual);

    }
}
