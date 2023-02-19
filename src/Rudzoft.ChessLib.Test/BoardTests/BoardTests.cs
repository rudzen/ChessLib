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
using Rudzoft.ChessLib.ObjectPoolPolicies;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Test.BoardTests;

public sealed class BoardTests
{
    private readonly IServiceProvider _serviceProvider;

    public BoardTests()
    {
        _serviceProvider = new ServiceCollection()
            .AddTransient<IBoard, Board>()
            .AddSingleton<IValues, Values>()
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
    [InlineData("rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6", PieceTypes.Pawn, Players.White, 8)]
    [InlineData("rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6", PieceTypes.Pawn, Players.Black, 8)]
    [InlineData("rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6", PieceTypes.Knight, Players.White, 2)]
    [InlineData("rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6", PieceTypes.Knight, Players.Black, 2)]
    [InlineData("rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6", PieceTypes.Bishop, Players.White, 2)]
    [InlineData("rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6", PieceTypes.Bishop, Players.Black, 2)]
    [InlineData("rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6", PieceTypes.Rook, Players.White, 2)]
    [InlineData("rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6", PieceTypes.Rook, Players.Black, 2)]
    [InlineData("rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6", PieceTypes.Queen, Players.White, 1)]
    [InlineData("rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6", PieceTypes.Queen, Players.Black, 1)]
    [InlineData("rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6", PieceTypes.King, Players.White, 1)]
    [InlineData("rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6", PieceTypes.King, Players.Black, 1)]
    [InlineData("5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53", PieceTypes.Pawn, Players.White, 4)]
    [InlineData("5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53", PieceTypes.Pawn, Players.Black, 3)]
    [InlineData("5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53", PieceTypes.Knight, Players.White, 1)]
    [InlineData("5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53", PieceTypes.Knight, Players.Black, 1)]
    [InlineData("5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53", PieceTypes.Bishop, Players.White, 0)]
    [InlineData("5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53", PieceTypes.Bishop, Players.Black, 0)]
    [InlineData("5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53", PieceTypes.Rook, Players.White, 2)]
    [InlineData("5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53", PieceTypes.Rook, Players.Black, 2)]
    [InlineData("5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53", PieceTypes.Queen, Players.White, 0)]
    [InlineData("5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53", PieceTypes.Queen, Players.Black, 0)]
    [InlineData("5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53", PieceTypes.King, Players.White, 1)]
    [InlineData("5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53", PieceTypes.King, Players.Black, 1)]
    public void BoardPieceCount(string fen, PieceTypes pt, Player p, int expected)
    {
        var pos = _serviceProvider.GetRequiredService<IPosition>();

        var fenData = new FenData(fen);
        var state = new State();

        pos.Set(in fenData, ChessMode.Normal, state);

        var board = pos.Board;

        var posCount = pos.Pieces(pt, p).Count;
        var boardCount = board.PieceCount(pt, p);
        
        Assert.Equal(posCount, boardCount);
        Assert.Equal(expected, boardCount);        
    }
}