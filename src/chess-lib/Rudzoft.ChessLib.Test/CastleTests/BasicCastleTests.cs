﻿/*
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Hash;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;
using Rudzoft.ChessLib.Validation;

namespace Rudzoft.ChessLib.Test.CastleTests;

public sealed class BasicCastleTests
{
    private readonly IServiceProvider _serviceProvider;

    public BasicCastleTests()
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

    [Theory]
    [InlineData(Fen.Fen.StartPositionFen, CastleRights.None, false)]
    [InlineData("rnbqk2r/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1", CastleRights.King, true)]
    [InlineData("rnbqk2r/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1", CastleRights.Queen, true)]
    [InlineData("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1", CastleRights.King, true)]
    [InlineData("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1", CastleRights.Queen, true)]
    public void CanPotentiallyCastle(string fen, CastleRights cr, bool expected)
    {
        var pos = _serviceProvider.GetRequiredService<IPosition>();
        var fenData = new FenData(fen);
        var state = new State();
        pos.Set(in fenData, ChessMode.Normal, state);

        var actual = pos.CanCastle(cr);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(Fen.Fen.StartPositionFen, CastleRights.WhiteKing, true)]
    [InlineData(Fen.Fen.StartPositionFen, CastleRights.WhiteQueen, true)]
    [InlineData(Fen.Fen.StartPositionFen, CastleRights.BlackKing, true)]
    [InlineData(Fen.Fen.StartPositionFen, CastleRights.BlackQueen, true)]
    [InlineData("rnbqk2r/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1", CastleRights.WhiteKing, false)]
    [InlineData("rnbqk2r/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1", CastleRights.WhiteQueen, true)]
    [InlineData("rnbqk2r/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1", CastleRights.BlackKing, false)]
    [InlineData("rnbqk2r/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1", CastleRights.BlackQueen, true)]
    [InlineData("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1", CastleRights.WhiteKing, false)]
    [InlineData("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1", CastleRights.WhiteQueen, false)]
    [InlineData("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1", CastleRights.BlackKing, false)]
    [InlineData("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1", CastleRights.BlackQueen, false)]
    public void IsCastleImpeeded(string fen, CastleRights cr, bool expected)
    {
        var pos = _serviceProvider.GetRequiredService<IPosition>();
        var fenData = new FenData(fen);
        var state = new State();
        pos.Set(in fenData, ChessMode.Normal, state);

        var actual = pos.CastlingImpeded(cr);
        Assert.Equal(expected, actual);
    }
}