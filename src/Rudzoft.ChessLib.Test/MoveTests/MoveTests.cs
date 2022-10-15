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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Test.MoveTests;

public sealed class MoveTests
{
    [Fact]
    public void MoveSquares()
    {
        var bb = BitBoards.AllSquares;
        while (bb)
        {
            var expectedFrom =  BitBoards.PopLsb(ref bb);
            var bb2 = bb;
            while (bb2)
            {
                var expectedTo = BitBoards.PopLsb(ref bb2);
                var move = Move.Create(expectedFrom, expectedTo);

                Assert.False(move.IsNullMove());

                var (actualFrom, actualTo) = move;

                Assert.Equal(expectedFrom, actualFrom);
                Assert.Equal(expectedTo, actualTo);
                Assert.True(move.IsValidMove());
            }
        }
    }

    [Fact]
    public void AllBasicMove()
    {
        var expectedFrom = Square.A2;
        var expectedTo = Square.H8;
        const PieceTypes expectedPromotionPiece = PieceTypes.Queen;
        const MoveTypes expectedMoveType = MoveTypes.Promotion;

        // full move spectrum
        var move = Move.Create(expectedFrom, expectedTo, MoveTypes.Promotion, expectedPromotionPiece);

        var (actualFrom, actualTo) = move;
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
        Assert.False(move.IsCastleMove());
        Assert.False(move.IsEnPassantMove());
    }

    [Fact]
    public void MoveToString()
    {
        var moves = new List<Move>(3528);
        var movesString = new List<MoveStrings>(3528);

        var game = GameFactory.Create();

        game.NewGame();

        var tmp = new StringBuilder(8);

        // build move list and expected result
        for (Square s1 = Squares.a1; s1 <= Squares.h8; s1++)
        {
            for (Square s2 = Squares.a2; s2 <= Squares.h8; s2++)
            {
                if (s1 == s2)
                    continue;

                moves.Add(Move.Create(s1, s2));
                tmp.Clear();
                tmp.Append(' ');
                tmp.Append(s1.ToString());
                tmp.Append(s2.ToString());
                movesString.Add(new MoveStrings(tmp.ToString()));
            }
        }

        var result = new StringBuilder(128);

        var i = 0;
        foreach (var move in CollectionsMarshal.AsSpan(moves))
        {
            result.Clear();
            result.Append(' ');
            game.Pos.MoveToString(move, in result);
            Assert.Equal(result.ToString(), movesString[i++].ToString());

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

            moves.Add(Move.Create(rndSquareFrom, rndSquareTo));

            expected.Append(' ');
            expected.Append(rndSquareFrom.ToString());
            expected.Append(rndSquareTo.ToString());
        }

        // generate a bitch string for them all.
        foreach (var move in CollectionsMarshal.AsSpan(moves))
        {
            result.Append(' ');
            game.Pos.MoveToString(move, result);
        }

        Assert.Equal(expected.ToString(), result.ToString());
    }

    private readonly struct MoveStrings
    {
        private readonly string _s;

        public MoveStrings(string s) => _s = s;

        public override string ToString() => _s;
    }
}