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

namespace Chess.Test.Move;

using Rudz.Chess.Enums;
using Rudz.Chess.Factories;
using Rudz.Chess.Types;
using System;
using System.Collections.Generic;
using System.Text;
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
            var actualInt = expectedFrom.AsInt();
            Assert.Equal(i, actualInt);
            for (var j = 1 /* NOTE! from 1 !! */; j < 64; j++)
            {
                Square expectedTo = j;

                // on purpose.. creating the move in this loop
                var move = new Move(expectedFrom, expectedTo);

                Assert.False(move.IsNullMove());

                var actualFrom = move.FromSquare();
                var actualTo = move.ToSquare();

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
        Square expectedFrom = Squares.a2;
        Square expectedTo = Squares.h8;
        var expectedPromotionPiece = PieceTypes.Queen;
        const MoveTypes expectedMoveType = MoveTypes.Promotion;

        // full move spectrum
        var move = Move.Create(expectedFrom, expectedTo, MoveTypes.Promotion, expectedPromotionPiece);

        var actualFrom = move.FromSquare();
        var actualTo = move.ToSquare();
        var actualPromotionPiece = move.PromotedPieceType();
        var actualEMoveType = move.MoveType();

        // test promotion status
        Assert.True(move.IsPromotionMove());
        Assert.True(move.IsQueenPromotion());

        // test squares
        Assert.Equal(expectedFrom, actualFrom);
        Assert.Equal(expectedTo, actualTo);

        // test promotion pieces
        Assert.Equal(expectedPromotionPiece, actualPromotionPiece);

        // move type
        Assert.True(move.IsQueenPromotion());
        Assert.True(move.IsPromotionMove());
        Assert.Equal(expectedMoveType, actualEMoveType);
        Assert.False(move.IsCastlelingMove());
        Assert.False(move.IsEnPassantMove());
    }

    [Fact]
    public void MoveToStringTest()
    {
        var moves = new List<Move>(128);
        var movesString = new List<Movestrings>(128);

        var game = GameFactory.Create();

        game.NewGame();

        var tmp = new StringBuilder(128);

        // build move list and expected result
        for (Square s1 = Squares.a1; s1; s1++)
        {
            for (Square s2 = Squares.a2; s2; s2++)
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

        var result = new StringBuilder(128);

        for (var i = 0; i < moves.Count; i++)
        {
            result.Clear();
            result.Append(' ');
            game.Pos.MoveToString(moves[i], in result);
            Assert.Equal(result.ToString(), movesString[i].ToString());
        }
    }

    [Fact]
    public void MoveListToStringTest()
    {
        var game = GameFactory.Create();

        game.NewGame();

        var result = new StringBuilder(1024 * 16);
        var expected = new StringBuilder(1024 * 16);

        var moves = new List<Move>(256);

        var rngeezuz = new Random(DateTime.Now.Millisecond);

        // generate 256-ish random moves
        for (var i = 0; i < 256; i++)
        {
            Square rndSquareFrom = (Squares)rngeezuz.Next((int)Squares.a1, (int)Squares.h8);
            Square rndSquareTo = (Squares)rngeezuz.Next((int)Squares.a1, (int)Squares.h8);

            // Skip same squares to and from
            if (rndSquareFrom == rndSquareTo)
                continue;

            moves.Add(new Move(rndSquareFrom, rndSquareTo));

            expected.Append(' ');
            expected.Append(rndSquareFrom.ToString());
            expected.Append(rndSquareTo.ToString());
        }

        // generate a bitch string for them all.
        foreach (var move in moves)
        {
            result.Append(' ');
            game.Pos.MoveToString(move, result);
        }

        Assert.Equal(expected.ToString(), result.ToString());
    }

    private readonly struct Movestrings
    {
        private readonly string _s;

        public Movestrings(string s) => _s = s;

        public override string ToString() => _s;
    }
}
