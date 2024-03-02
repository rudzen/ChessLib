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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Evaluation;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Hash;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;
using Rudzoft.ChessLib.Validation;

namespace Rudzoft.ChessLib.Test.EvaluationTests;

public sealed class KpkBitBaseTests
{
    private readonly IServiceProvider _serviceProvider;

    public KpkBitBaseTests()
    {
        _serviceProvider = new ServiceCollection()
            .AddTransient<IBoard, Board>()
            .AddSingleton<IValues, Values>()
            .AddSingleton<IRKiss, RKiss>()
            .AddSingleton<IZobrist, Zobrist>()
            .AddSingleton<ICuckoo, Cuckoo>()
            .AddSingleton<IPositionValidator, PositionValidator>()
            .AddTransient<IPosition, Position>()
            .AddSingleton<IKpkBitBase, KpkBitBase>()
            .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
            .AddSingleton(static serviceProvider =>
            {
                var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
                var policy = new DefaultPooledObjectPolicy<MoveList>();
                return provider.Create(policy);
            })
            .BuildServiceProvider();
    }

    // theory data layout:
    // fen, player, expected win by strong side
    [Theory]
    [InlineData("2k5/8/8/8/1PK5/8/8/8 w - - 0 1", Players.White, true)]
    [InlineData("5k2/8/6KP/8/8/8/8/8 b - - 0 1", Players.White, true)]
    [InlineData("8/8/8/K7/P2k4/8/8/8 w - - 0 1", Players.White, true)]
    [InlineData("8/8/8/K7/P2k4/8/8/8 b - - 0 1", Players.White, true)]
    public void KpkWin(string fen, Players rawPlayer, bool expected)
    {
        var pos = _serviceProvider.GetRequiredService<IPosition>();
        var fenData = new FenData(fen);
        var state = new State();
        pos.Set(in fenData, ChessMode.Normal, state);

        Player strongSide = rawPlayer;
        var weakSide = ~strongSide;

        var kpkBitBase = _serviceProvider.GetRequiredService<IKpkBitBase>();

        var strongKing = kpkBitBase.Normalize(pos, strongSide,  pos.GetPieceSquare(PieceTypes.King, strongSide));
        var strongPawn = kpkBitBase.Normalize(pos, strongSide, pos.GetPieceSquare(PieceTypes.Pawn, strongSide));
        var weakKing = kpkBitBase.Normalize(pos, strongSide, pos.GetPieceSquare(PieceTypes.King, weakSide));

        var us = strongSide == pos.SideToMove ? Player.White : Player.Black;

        var won = !kpkBitBase.Probe(strongKing, strongPawn, weakKing, us);

        Assert.Equal(expected, won);
    }
}