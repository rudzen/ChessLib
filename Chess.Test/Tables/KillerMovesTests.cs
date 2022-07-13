using Rudz.Chess.Enums;
using Rudz.Chess.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Chess.Test.Tables
{
    public class KillerMovesTests
    {
        [Fact]
        public void BaseAddMove()
        {
            var km = KillerMoves.Create(128);
            var depth = 1;
            var move = Rudz.Chess.Types.Move.Create(Squares.a2, Squares.a3);
            var pc = PieceTypes.Pawn.MakePiece(Players.White);

            km.UpdateValue(depth, move, pc);

            var value = km.GetValue(depth, move, pc);

            Assert.Equal(2, value);
        }

        [Fact]
        public void GetValueWithWrongDepthYieldsZero()
        {
            var km = KillerMoves.Create(128);
            var depth = 1;
            var move = Rudz.Chess.Types.Move.Create(Squares.a2, Squares.a3);
            var pc = PieceTypes.Pawn.MakePiece(Players.White);

            km.UpdateValue(depth, move, pc);

            var value = km.GetValue(depth + 1, move, pc);

            Assert.Equal(0, value);
        }
    }
}
