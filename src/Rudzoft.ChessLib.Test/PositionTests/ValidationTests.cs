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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Hash;
using Rudzoft.ChessLib.ObjectPoolPolicies;
using Rudzoft.ChessLib.Types;
using Rudzoft.ChessLib.Validation;

namespace Rudzoft.ChessLib.Test.PositionTests;

public sealed class ValidationTests
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationTests()
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

    [Fact]
    public void ValidationKingsNegative()
    {
        const PositionValidationTypes type = PositionValidationTypes.Kings;
        var expectedErrorMsg = $"king count for player {Player.White} was 2";

        var pos = _serviceProvider.GetRequiredService<IPosition>();

        var fenData = new FenData(Fen.Fen.StartPositionFen);
        var state = new State();

        pos.Set(in fenData, ChessMode.Normal, state);

        var pc = PieceTypes.King.MakePiece(Player.White);

        pos.AddPiece(pc, Square.E4);

        var (ok, actualErrorMessage) = pos.Validate(type);

        Assert.NotNull(actualErrorMessage);
        Assert.NotEmpty(actualErrorMessage);
        Assert.Equal(expectedErrorMsg, actualErrorMessage);
        Assert.False(ok);
    }

    [Fact]
    public void ValidateCastleling()
    {
        // position only has pawns, rooks and kings
        const string fen = "r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1";
        const PositionValidationTypes validationType = PositionValidationTypes.Castle;
        
        var pos = _serviceProvider.GetRequiredService<IPosition>();

        var fenData = new FenData(fen);
        var state = new State();

        pos.Set(in fenData, ChessMode.Normal, state);

        var validator = pos.Validate(validationType);

        Assert.True(validator.Ok);
        Assert.NotNull(validator.Errors);
        Assert.Empty(validator.Errors);
    }

    // TODO : Add tests for the rest of the validations
}