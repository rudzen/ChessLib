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

namespace Chess.Test.Move
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Rudz.Chess;
    using Rudz.Chess.Enums;
    using Rudz.Chess.Types;
    using Xunit;

    public sealed class MoveTests
    {
        [Fact]
        public void TestSquares()
        {
            // test all squares, including invalid moves (same from and to)
            for (var i = 0; i < 64; i++)
            {
                Square expectedFrom = i;
                var actualInt = expectedFrom.ToInt();
                Assert.Equal(i, actualInt);
                for (var j = 1 /* NOTE! from 1 !! */; j < 64; j++)
                {
                    Square expectedTo = j;

                    // on purpose.. creating the move in this loop
                    var move = new Rudz.Chess.Types.Move(expectedFrom, expectedTo);

                    Assert.False(move.IsNullMove());

                    var actualFrom = move.GetFromSquare();
                    var actualTo = move.GetToSquare();

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
            var move = new Rudz.Chess.Types.Move(expectedMovingPiece, expectedCapturedPiece, expectedFrom, expectedTo, expectedMoveType, expectedPromotionPiece);

            var actualFrom = move.GetFromSquare();
            var actualTo = move.GetToSquare();
            var actualMovingEPieceType = move.GetMovingPieceType();
            var actualMovingPiece = move.GetMovingPiece();
            var actualCapturedPiece = move.GetCapturedPiece();
            var actualPromotionPiece = move.GetPromotedPiece();
            var actualEMoveType = move.GetMoveType();

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
            var sb = new StringBuilder(256);

            var moves = new List<Rudz.Chess.Types.Move>(128);
            var movesString = new List<Movestrings>(128);

            var game = new Game();

            game.NewGame();

            var tmp = new StringBuilder(128);

            // build move list and expected result
            for (Square s1 = ESquare.a1; s1; s1++)
            {
                for (Square s2 = ESquare.a2; s2; s2++)
                {
                    if (s1 == s2)
                        continue;

                    moves.Add(new Rudz.Chess.Types.Move(s1, s2));
                    tmp.Clear();
                    tmp.Append(' ');
                    tmp.Append(s1.ToString());
                    tmp.Append(s2.ToString());
                    movesString.Add(new Movestrings(tmp.ToString()));
                }
            }

            var result = new StringBuilder(128);

            for (var i = 0; i < moves.Count; i++)
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
            var game = new Game();

            game.NewGame();

            var result = new StringBuilder(1024 * 16);
            var expected = new StringBuilder(1024 * 16);

            var moves = new List<Rudz.Chess.Types.Move>(256);

            var rngeezuz = new Random(DateTime.Now.Millisecond);

            // generate 256 random moves
            for (var i = 0; i < 256; i++)
            {
                Square rndSquareFrom = (ESquare)rngeezuz.Next((int)ESquare.a1, (int)ESquare.h8);
                Square rndSquareTo = (ESquare)rngeezuz.Next((int)ESquare.a1, (int)ESquare.h8);
                moves.Add(new Rudz.Chess.Types.Move(rndSquareFrom, rndSquareTo));

                expected.Append(' ');
                expected.Append(rndSquareFrom.ToString());
                expected.Append(rndSquareTo.ToString());
            }

            // generate a bitch string for them all.
            foreach (var move in moves)
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