using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Enums;
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
}