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

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Hash;
using Rudzoft.ChessLib.ObjectPoolPolicies;
using Rudzoft.ChessLib.Types;
using Rudzoft.ChessLib.Validation;

namespace Rudzoft.ChessLib.Test.ZobristTests;

public sealed class ZobristHashTests
{
    private readonly IServiceProvider _serviceProvider;

    public ZobristHashTests()
    {
        _serviceProvider = new ServiceCollection()
            .AddTransient<IBoard, Board>()
            .AddSingleton<IValues, Values>()
            .AddSingleton<IZobrist, Zobrist>()
            .AddSingleton<ICuckoo, Cuckoo>()
            .AddSingleton<IPositionValidator, PositionValidator>()
            .AddTransient<IPosition, Position>()
            .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
            .AddSingleton(static serviceProvider =>
            {
                var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
                var policy = new MoveListPolicy();
                return provider.Create(policy);
            })
            .BuildServiceProvider();
    }

    [Theory]
    [InlineData(Fen.Fen.StartPositionFen, Squares.a2, Squares.a4)]
    [InlineData(Fen.Fen.StartPositionFen, Squares.a2, Squares.a3)]
    [InlineData(Fen.Fen.StartPositionFen, Squares.b1, Squares.c3)]
    public void BackAndForth(string fen, Squares from, Squares to)
    {
        var pos = _serviceProvider.GetRequiredService<IPosition>();

        var stateIndex = 0;
        var states = new List<State> { new() };

        pos.Set(fen, ChessMode.Normal, states[stateIndex]);

        var startKey = pos.State.PositionKey;

        var move = new Move(from, to);

        stateIndex++;
        states.Add(new State());

        pos.MakeMove(move, states[stateIndex]);

        var moveKey = pos.State.PositionKey;

        pos.TakeMove(move);

        var finalKey = pos.State.PositionKey;

        Assert.NotEqual(startKey, moveKey);
        Assert.Equal(startKey, states[0].PositionKey);
        Assert.NotEqual(startKey, states[1].PositionKey);
        Assert.Equal(startKey, finalKey);
    }

    [Fact]
    public void OnlyUniqueZobristHashKeys()
    {
        var zobrist = new Zobrist();

        var set = new HashSet<HashKey> { HashKey.Empty };

        var added = false;

        foreach (var sq in Square.All.AsSpan())
        {
            foreach (var pc in Piece.All.AsSpan())
            {
                added = set.Add(zobrist.GetZobristPst(sq, pc));
                Assert.True(added);
            }
        }

        foreach (var f in File.AllFiles.AsSpan())
        {
            added = set.Add(zobrist.GetZobristEnPassant(f));
            Assert.True(added);
        }

        var castleRights = new[]
        {
            CastleRight.None,
            CastleRight.WhiteKing,
            CastleRight.BlackKing,
            CastleRight.WhiteQueen,
            CastleRight.BlackQueen,
            CastleRight.King,
            CastleRight.Queen,
            CastleRight.White,
            CastleRight.Black,
            CastleRight.Any
        };

        foreach (var cr in castleRights)
        {
            added = set.Add(zobrist.GetZobristCastleling(cr));
            Assert.True(added);
        }

        added = set.Add(zobrist.GetZobristSide());
        Assert.True(added);
    }
}