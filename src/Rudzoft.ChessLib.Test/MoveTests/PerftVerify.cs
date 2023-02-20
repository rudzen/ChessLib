using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Hash.Tables.Transposition;
using Rudzoft.ChessLib.ObjectPoolPolicies;
using Rudzoft.ChessLib.Protocol.UCI;
using Rudzoft.ChessLib.Types;
using Rudzoft.ChessLib.Validation;

namespace Rudzoft.ChessLib.Test.MoveTests;

public abstract class PerftVerify
{
    private readonly IServiceProvider _serviceProvider;

    protected PerftVerify()
    {
        var ttConfig = new TranspositionTableConfiguration { DefaultSize = 1 };
        var ttOptions = Options.Create(ttConfig);

        _serviceProvider = new ServiceCollection()
            .AddSingleton(ttOptions)
            .AddTransient<IBoard, Board>()
            .AddSingleton<IValues, Values>()
            .AddSingleton<IPositionValidator, PositionValidator>()
            .AddTransient<IPosition, Position>()
            .AddSingleton<ISearchParameters, SearchParameters>()
            .AddSingleton<IUci, Uci>()
            .AddSingleton<ICpu, Cpu>()
            .AddTransient<IGame, Game>()
            .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
            .AddSingleton(static serviceProvider =>
            {
                var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
                var policy = new MoveListPolicy();
                return provider.Create(policy);
            })
            .AddSingleton<ITranspositionTable, TranspositionTable>()
            .BuildServiceProvider();
    }

    protected void AssertPerft(string fen, int depth, in ulong expected)
    {
        var g = _serviceProvider.GetRequiredService<IGame>();
        g.NewGame(fen);
        
        var actual = g.Perft(depth);
        Assert.Equal(expected, actual);
    }
}