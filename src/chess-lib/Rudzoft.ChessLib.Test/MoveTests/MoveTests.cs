/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2023 Rudy Alex Kohn

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

using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Hash;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;
using Rudzoft.ChessLib.Validation;

namespace Rudzoft.ChessLib.Test.MoveTests;

public sealed class MoveTests
{
    private readonly IServiceProvider _serviceProvider;

    public MoveTests()
    {
        _serviceProvider = new ServiceCollection()
                           .AddTransient<IBoard, Board>()
                           .AddSingleton<IValues, Values>()
                           .AddSingleton<IRKiss, RKiss>()
                           .AddSingleton<IZobrist, Zobrist>()
                           .AddSingleton<ICuckoo, Cuckoo>()
                           .AddSingleton<IPositionValidator, PositionValidator>()
                           .AddTransient<IPosition, Position>()
                           .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                           .AddSingleton(static serviceProvider =>
                           {
                               var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
                               var policy = new DefaultPooledObjectPolicy<MoveList>();
                               return provider.Create(policy);
                           })
                           .BuildServiceProvider();
    }

    [Fact]
    public void MoveSquares()
    {
        var bb = BitBoards.AllSquares;
        while (bb)
        {
            var expectedFrom = BitBoards.PopLsb(ref bb);
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
        var result = new StringBuilder(128);

        var allSquares = Square.All.AsSpan();
        var allSquares2 = allSquares[1..];

        var moves = new List<Move>(allSquares.Length * allSquares2.Length);
        var movesString = new List<string>(allSquares.Length * allSquares2.Length);

        //build move list and expected result
        foreach (var s1 in allSquares)
        {
            foreach (var s2 in allSquares2)
            {
                if (s1 == s2)
                    continue;

                moves.Add(Move.Create(s1, s2));
                result.Clear();
                result.Append(' ');
                result.Append(s1.ToString());
                result.Append(s2.ToString());
                movesString.Add(result.ToString());
            }
        }

        result.Clear();

        var pos = _serviceProvider.GetRequiredService<IPosition>();
        var fenData = new FenData(Fen.Fen.StartPositionFen);
        var state = new State();
        pos.Set(in fenData, ChessMode.Normal, state);

        var i = 0;

        var movesSpan = CollectionsMarshal.AsSpan(moves);
        foreach (var move in movesSpan)
        {
            result.Clear();
            result.Append(' ');
            pos.MoveToString(move, in result);
            Assert.Equal(result.ToString(), movesString[i++]);
        }
    }

    [Fact]
    public void MoveListToStringTest()
    {
        var result = new StringBuilder(1024 * 16);
        var expected = new StringBuilder(1024 * 16);

        var moves = new List<Move>(256);

        var rngeezuz = new Random(DateTime.Now.Millisecond);

        // generate 256-ish random moves
        for (var i = 0; i < 256; i++)
        {
            Square rndSquareFrom = rngeezuz.Next(Square.A1, Square.H8);
            Square rndSquareTo = rngeezuz.Next(Square.A1, Square.H8);

            // Skip same squares to and from
            if (rndSquareFrom == rndSquareTo)
                continue;

            moves.Add(Move.Create(rndSquareFrom, rndSquareTo));

            expected.Append(' ');
            expected.Append(rndSquareFrom.ToString());
            expected.Append(rndSquareTo.ToString());
        }

        var pos = _serviceProvider.GetRequiredService<IPosition>();
        var fenData = new FenData(Fen.Fen.StartPositionFen);
        var state = new State();
        pos.Set(in fenData, ChessMode.Normal, state);

        var movesSpan = CollectionsMarshal.AsSpan(moves);
        foreach (var move in movesSpan)
        {
            result.Append(' ');
            pos.MoveToString(move, result);
        }

        Assert.Equal(expected.ToString(), result.ToString());
    }
}