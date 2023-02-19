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
using Microsoft.Extensions.Options;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Hash.Tables.Transposition;
using Rudzoft.ChessLib.Protocol.UCI;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Test.ProtocolTests;

public sealed class UciTests
{
    private readonly IServiceProvider _serviceProvider;

    public UciTests()
    {
        var transpositionTableConfiguration = new TranspositionTableConfiguration { DefaultSize = 1 };
        var options = Options.Create(transpositionTableConfiguration);

        _serviceProvider = new ServiceCollection()
            .AddSingleton(options)
            .AddSingleton<IValues, Values>()
            .AddTransient<IBoard, Board>()
            .AddTransient<IPosition, Position>()
            .AddSingleton(static _ =>
            {
                IUci uci = new Uci();
                uci.Initialize();
                return uci;
            })
            .BuildServiceProvider();
    }

    [Fact]
    public void NpsSimple()
    {
        const ulong expected = ulong.MinValue;

        const ulong nodes = 1000UL;

        var ts = TimeSpan.FromSeconds(1);

        var uci = _serviceProvider.GetRequiredService<IUci>();

        var actual = uci.Nps(nodes, in ts);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MoveFromUciBasic()
    {
        const string uciMove = "a2a3";
        var expected = Move.Create(Square.A2, Square.A3);

        var uci = _serviceProvider.GetRequiredService<IUci>();
        var pos = _serviceProvider.GetRequiredService<IPosition>();

        var fenData = new FenData(Fen.Fen.StartPositionFen);
        var state = new State();

        pos.Set(in fenData, ChessMode.Normal, state);

        var actual = uci.MoveFromUci(pos, uciMove);

        Assert.Equal(expected, actual);
    }
}