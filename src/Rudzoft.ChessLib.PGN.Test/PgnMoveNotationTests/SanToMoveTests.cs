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
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Hash;
using Rudzoft.ChessLib.Notation;
using Rudzoft.ChessLib.Notation.Notations;
using Rudzoft.ChessLib.ObjectPoolPolicies;
using Rudzoft.ChessLib.Types;
using Rudzoft.ChessLib.Validation;

namespace Rudzoft.ChessLib.PGN.Test.PgnMoveNotationTests;

public sealed class SanToMoveTests
{
    private const string SampleFilePath = @"samples/sample.pgn";
    private const int ExpectedGameCount = 2;

    private readonly IServiceProvider _serviceProvider;

    public SanToMoveTests()
    {
        _serviceProvider = new ServiceCollection()
            .AddTransient<IBoard, Board>()
            .AddSingleton<IValues, Values>()
            .AddSingleton<IZobrist, Zobrist>()
            .AddSingleton<ICuckoo, Cuckoo>()
            .AddSingleton<IPositionValidator, PositionValidator>()
            .AddTransient<IPosition, Position>()
            .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
            .AddSingleton<INotationToMove, NotationToMove>()
            .AddSingleton(static serviceProvider =>
            {
                var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
                var policy = new MoveListPolicy();
                return provider.Create(policy);
            })
            .AddPgnParser()
            .BuildServiceProvider();
    }

    [Fact]
    public async Task BasicSanConvert()
    {
        var pos = _serviceProvider.GetRequiredService<IPosition>();
        var fenData = new FenData(Fen.Fen.StartPositionFen);
        var state = new State();
        pos.Set(in fenData, ChessMode.Normal, in state);
        
        var games = new List<PgnGame>();
        var parser = _serviceProvider.GetRequiredService<IPgnParser>();

        await foreach (var game in parser.ParseFile(SampleFilePath))
            games.Add(game);

        var sanMoves = new List<string>();

        foreach (var pgnMove in games.First().Moves)
        {
            sanMoves.Add(pgnMove.WhiteMove);
            if (!string.IsNullOrWhiteSpace(pgnMove.BlackMove))
                sanMoves.Add(pgnMove.BlackMove);
        }

        var moveNotation = MoveNotation.Create(pos);
        var notation = moveNotation.ToNotation(MoveNotations.San);
        var converter = _serviceProvider.GetRequiredService<INotationToMove>();

        var chessMoves = sanMoves
            .Select(sanMove => converter.FromNotation(pos, sanMove, notation))
            .TakeWhile(static move => move != Move.EmptyMove);

        foreach (var move in chessMoves)
            pos.MakeMove(move, in state);
        
        Assert.Equal(ExpectedGameCount, games.Count);
        Assert.NotEmpty(games);
        Assert.Equal(sanMoves.Count - 1, pos.Ply);
    }

    [Fact]
    public async Task AllAtOnceConvert()
    {
        var pos = _serviceProvider.GetRequiredService<IPosition>();
        var fenData = new FenData(Fen.Fen.StartPositionFen);
        var state = new State();
        pos.Set(in fenData, ChessMode.Normal, in state);
        
        var games = new List<PgnGame>();
        var parser = _serviceProvider.GetRequiredService<IPgnParser>();

        await foreach (var game in parser.ParseFile(SampleFilePath))
            games.Add(game);

        var sanMoves = new List<string>();

        foreach (var pgnMove in games.First().Moves)
        {
            sanMoves.Add(pgnMove.WhiteMove);
            if (!string.IsNullOrWhiteSpace(pgnMove.BlackMove))
                sanMoves.Add(pgnMove.BlackMove);
        }

        var moveNotation = MoveNotation.Create(pos);
        var notation = moveNotation.ToNotation(MoveNotations.San);
        var converter = _serviceProvider.GetRequiredService<INotationToMove>();

        var actualMoves = converter.FromNotation(pos, sanMoves, notation);

        Assert.Equal(ExpectedGameCount, games.Count);
        Assert.NotEmpty(games);
        Assert.Equal(sanMoves.Count - 1, actualMoves.Count);
        Assert.Equal(sanMoves.Count - 1, pos.Ply);
    }
}