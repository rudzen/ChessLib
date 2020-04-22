/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2020 Rudy Alex Kohn

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

using Rudz.Chess.Factories;

namespace Chess.Test.Gameplay
{
    using System.Collections.Generic;
    using Rudz.Chess;
    using Rudz.Chess.Enums;
    using Rudz.Chess.Types;
    using Xunit;

    public sealed class FoolsCheckMateTests
    {
        [Fact]
        public void FoolsCheckMateTest()
        {
            // generate moves
            var moves = new List<Move>(4) {
                                                      new Move(Pieces.WhitePawn, Squares.f2, Squares.f3),
                                                      new Move(Pieces.BlackPawn, Squares.e7, Squares.e5, MoveTypes.Doublepush, Pieces.NoPiece),
                                                      new Move(Pieces.WhitePawn, Squares.g2, Squares.g4, MoveTypes.Doublepush, Pieces.NoPiece),
                                                      new Move(Pieces.BlackQueen, Squares.d8, Squares.h4)
                                                  };

            // construct game and start a new game
            var position = new Position();
            var game = GameFactory.Create(position);
            game.NewGame();

            // make the moves necessary to create a mate
            foreach (var move in moves)
                Assert.True(game.MakeMove(move));

            // verify in check is actually true
            Assert.True(game.State.InCheck);

            var moveList = game.Pos.GenerateMoves();

            var resultingMoves = moveList.GetMoves();
            
            // verify that no legal moves actually exists.
            Assert.True(resultingMoves.IsEmpty);
        }
    }
}