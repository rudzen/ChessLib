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

namespace Rudzoft.ChessLib.Test.PositionTests;

public sealed class EnPassantFenTests
{
    private readonly IServiceProvider _serviceProvider;

    public EnPassantFenTests()
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
    [InlineData("rnbkqbnr/pp1pp1pp/5p2/2pP4/8/8/PPP1PPPP/RNBKQBNR w KQkq c6 0 1", Squares.c6)] // valid
    [InlineData("rnbkqbnr/pp1pp1pp/5p2/2pP4/8/8/PPP1PPPP/RNBKQBNR w KQkq c7 0 1", Squares.none)] // invalid rank
    [InlineData("rnbkqbnr/pp1pp1pp/5p2/2pP4/8/8/PPP1PPPP/RNBKQBNR w KQkq - 0 1", Squares.none)] // no square set
    [InlineData("rnbkqbnr/pp1pp1pp/5p2/2pP4/8/8/PPP1PPPP/RNBKQBNR w KQkq c 0 1", Squares.none)] // only file set
    [InlineData("rnbqkbnr/pppppp2/7p/8/3PP1pP/5P2/PPP3P1/RNBQKBNR b KQkq h3 0 1", Squares.h3)] // valid
    [InlineData("rnbqkbnr/pppppp2/7p/8/3PP1pP/5P2/PPP3P1/RNBQKBNR b KQkq h4 0 1", Squares.none)] // invalid rank
    [InlineData("rnbqkbnr/pppppp2/7p/8/3PP1pP/5P2/PPP3P1/RNBQKBNR b KQkq - 0 1", Squares.none)] // no square set
    [InlineData("rnbqkbnr/pppppp2/7p/8/3PP1pP/5P2/PPP3P1/RNBQKBNR b KQkq -- 0 1", Squares.none)] // invalid format
    [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq c6 0 1", Squares.none)] // start pos with ep square
    [InlineData("rnbqkbnr/2pppp2/p6p/1p2PB2/3P2pP/5P2/PPP3P1/RNBQK1NR b KQkq h3 0 1", Squares.h3)] // valid
    public void EnPassantSquare(string fen, Squares expected)
    {
        var pos = _serviceProvider.GetRequiredService<IPosition>();

        var fenData = new FenData(fen);
        var state = new State();

        pos.Set(in fenData, ChessMode.Normal, state);

        var actual = pos.EnPassantSquare;
        Assert.Equal(expected, actual.Value);
    }
}