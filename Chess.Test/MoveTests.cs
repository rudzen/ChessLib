/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2019 Rudy Alex Kohn

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

using Rudz.Chess;
using Rudz.Chess.Enums;
using Rudz.Chess.Types;

namespace Chess.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Xunit;

    public class MoveTests
    {
        [Fact]
        public void TestSquares()
        {
            // test all squares, including invalid moves (same from and to)
            for (int i = 0; i < 64; i++)
            {
                Square expectedFrom = i;
                int actualInt = expectedFrom.ToInt();
                Assert.Equal(i, actualInt);
                for (int j = 1 /* NOTE! from 1 !! */; j < 64; j++)
                {
                    Square expectedTo = j;

                    // on purpose.. creating the move in this loop
                    Move move = new Move(expectedFrom, expectedTo);

                    Assert.False(move.IsNullMove());

                    Square actualFrom = move.GetFromSquare();
                    Square actualTo = move.GetToSquare();

                    Assert.Equal(expectedFrom, actualFrom);
                    Assert.Equal(expectedTo, actualTo);

                    // valid move check, if from and to are the same, the move is "invalid"
                    if (i == j)
                        Assert.False(move.IsValidMove());
                    else
                        Assert.True(move.IsValidMove());
                }
            }
        }

        [Fact]
        public void TestAllBasicMove()
        {
            Square expectedFrom = ESquare.a2;
            Square expectedTo = ESquare.h8;
            const EPieceType expectedMovingPieceType = EPieceType.Pawn;
            Piece expectedMovingPiece = EPieces.WhitePawn;
            Piece expectedCapturedPiece = EPieces.BlackKnight;
            Piece expectedPromotionPiece = EPieces.WhiteQueen;
            const EMoveType expectedMoveType = EMoveType.Promotion;

            // full move spectrum
            Move move = new Move(expectedMovingPiece, expectedCapturedPiece, expectedFrom, expectedTo, expectedMoveType, expectedPromotionPiece);

            Square actualFrom = move.GetFromSquare();
            Square actualTo = move.GetToSquare();
            EPieceType actualMovingEPieceType = move.GetMovingPieceType();
            Piece actualMovingPiece = move.GetMovingPiece();
            Piece actualCapturedPiece = move.GetCapturedPiece();
            Piece actualPromotionPiece = move.GetPromotedPiece();
            EMoveType actualEMoveType = move.GetMoveType();

            // test promotion status
            Assert.True(move.IsPromotionMove());
            Assert.True(move.IsQueenPromotion());

            // test squares
            Assert.Equal(expectedFrom, actualFrom);
            Assert.Equal(expectedTo, actualTo);

            // test pieces
            Assert.Equal(expectedMovingPieceType, actualMovingEPieceType);
            Assert.Equal(expectedMovingPiece, actualMovingPiece);
            Assert.Equal(expectedCapturedPiece, actualCapturedPiece);
            Assert.Equal(expectedPromotionPiece, actualPromotionPiece);

            // move type
            Assert.Equal(expectedMoveType, actualEMoveType);
            Assert.False(move.IsCastlelingMove());
            Assert.False(move.IsDoublePush());
            Assert.False(move.IsEnPassantMove());
        }

        [Fact]
        public void MoveToStringTest()
        {
            StringBuilder sb = new StringBuilder(256);

            IList<Move> moves = new List<Move>(128);
            IList<Movestrings> movesString = new List<Movestrings>(128);

            Game game = new Game();

            game.NewGame();

            StringBuilder tmp = new StringBuilder(128);

            // build move list and expected result
            for (Square s1 = ESquare.a1; s1; s1++)
            {
                for (Square s2 = ESquare.a2; s2; s2++)
                {
                    if (s1 == s2)
                        continue;

                    moves.Add(new Move(s1, s2));
                    tmp.Clear();
                    tmp.Append(' ');
                    tmp.Append(s1.ToString());
                    tmp.Append(s2.ToString());
                    movesString.Add(new Movestrings(tmp.ToString()));
                }
            }

            StringBuilder result = new StringBuilder(128);

            for (int i = 0; i < moves.Count; i++)
            {
                result.Clear();
                result.Append(' ');
                game.MoveToString(moves[i], result);
                Assert.Equal(result.ToString(), movesString[i].ToString());
            }
        }

        [Fact]
        public void MoveListToStringTest()
        {
            Game game = new Game();

            game.NewGame();

            StringBuilder result = new StringBuilder(1024 * 16);
            StringBuilder expected = new StringBuilder(1024 * 16);

            IList<Move> moves = new List<Move>(256);

            Random rngeezuz = new Random(DateTime.Now.Millisecond);

            // generate 256 random moves
            for (int i = 0; i < 256; i++)
            {
                Square rndSquareFrom = (ESquare)rngeezuz.Next((int)ESquare.a1, (int)ESquare.h8);
                Square rndSquareTo = (ESquare)rngeezuz.Next((int)ESquare.a1, (int)ESquare.h8);
                moves.Add(new Move(rndSquareFrom, rndSquareTo));

                expected.Append(' ');
                expected.Append(rndSquareFrom.ToString());
                expected.Append(rndSquareTo.ToString());
            }

            // generate a bitch string for them all.
            foreach (Move move in moves)
            {
                result.Append(' ');
                game.MoveToString(move, result);
            }

            Assert.Equal(expected.ToString(), result.ToString());
        }

        private struct Movestrings
        {
            private readonly string _s;

            public Movestrings(string s) => _s = s;

            public override string ToString() => _s;
        }
    }
}