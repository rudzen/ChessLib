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
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;
using Rudzoft.ChessLib.Validation;

namespace Rudzoft.ChessLib.Test.PositionTests;

public sealed class PositionTests
{
    private readonly IServiceProvider _serviceProvider;

    public PositionTests()
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
    public void AddPieceTest()
    {
        const string expectedFen = "rnbqkbnr/Pppppppp/8/8/8/8/PPP1PPPP/RNBQKBNR w KQkq - 0 1";
        var pos = _serviceProvider.GetRequiredService<IPosition>();

        var fenData = new FenData("rnbqkbnr/pppppppp/8/8/8/8/PPP1PPPP/RNBQKBNR w KQkq - 0 1");
        var state = new State();

        pos.Set(in fenData, ChessMode.Normal, state);

        var square = Square.A7;
        pos.AddPiece(Piece.WhitePawn, square);

        var actualFen = pos.FenNotation;

        Assert.Equal(expectedFen, actualFen);

        var piece = pos.GetPiece(square);
        Assert.Equal(Piece.WhitePawn, piece);
    }

    [Fact]
    public void KingBlocker()
    {
        // black has an absolute pinned piece at e6
        const string fen = "1rk5/8/4n3/5B2/1N6/8/8/1Q1K4 b - - 3 53";

        const int expected = 1;
        var expectedSquare = new Square(Ranks.Rank6, Files.FileE);

        var pos = _serviceProvider.GetRequiredService<IPosition>();

        var fenData = new FenData(fen);
        var state = new State();

        pos.Set(in fenData, ChessMode.Normal, state);

        var b = pos.KingBlockers(Player.Black);

        // b must contain one square at this point
        var pinnedCount = b.Count;

        Assert.Equal(expected, pinnedCount);

        // test for correct square
        var actual = b.Lsb();

        Assert.Equal(expectedSquare, actual);
    }

    [Theory]
    [InlineData("KPK", "k7/8/8/8/8/8/8/KP6 w - - 0 10")]
    [InlineData("KNNK", "k7/8/8/8/8/8/8/KNN5 w - - 0 10")]
    [InlineData("KBNK", "k7/8/8/8/8/8/8/KBN5 w - - 0 10")]
    public void SetByCodeCreatesSameMaterialKey(string code, string fen)
    {
        var pos = _serviceProvider.GetRequiredService<IPosition>();

        var fenData = new FenData(fen);
        var state = new State();

        pos.Set(in fenData, ChessMode.Normal, state);

        var materialKey = pos.State.MaterialKey;

        var posCode = pos.Set(code, Player.White, in state);
        var codeMaterialKey = posCode.State.MaterialKey;

        Assert.Equal(materialKey, codeMaterialKey);

        var codeFen = pos.GenerateFen().ToString();

        Assert.Equal(fen, codeFen);
    }
}