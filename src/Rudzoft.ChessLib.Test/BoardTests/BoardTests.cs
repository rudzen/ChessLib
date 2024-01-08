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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Hash;
using Rudzoft.ChessLib.ObjectPoolPolicies;
using Rudzoft.ChessLib.Types;
using Rudzoft.ChessLib.Validation;

namespace Rudzoft.ChessLib.Test.BoardTests;

public sealed class BoardTests
{
    private readonly IServiceProvider _serviceProvider;

    public BoardTests()
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
                var policy = new MoveListPolicy();
                return provider.Create(policy);
            })
            .BuildServiceProvider();
    }

    public sealed class BoardTestsTheoryData : TheoryData<string, PieceTypes, Player, int>
    {
        public BoardTestsTheoryData(string[] fens, PieceTypes[] pts, Player[] players, int[] expectedCounts)
        {
            Debug.Assert(fens != null);
            Debug.Assert(pts != null);
            Debug.Assert(players != null);
            Debug.Assert(expectedCounts != null);
            Debug.Assert(fens.Length == pts.Length);
            Debug.Assert(fens.Length == players.Length);
            Debug.Assert(fens.Length == expectedCounts.Length);

            var fensSpan = fens.AsSpan();
            var ptsSpan = pts.AsSpan();
            var playersSpan = players.AsSpan();
            var expectedCountSpan = expectedCounts.AsSpan();
            
            ref var fenSpace = ref MemoryMarshal.GetReference(fensSpan);
            ref var ptsSpace = ref MemoryMarshal.GetReference(ptsSpan);
            ref var playersSpace = ref MemoryMarshal.GetReference(playersSpan);
            ref var expectedCountSpace = ref MemoryMarshal.GetReference(expectedCountSpan);
            
            for (var i = 0; i < fens.Length; ++i)
            {
                var fen = Unsafe.Add(ref fenSpace, i);
                var pt = Unsafe.Add(ref ptsSpace, i);
                var player = Unsafe.Add(ref playersSpace, i);
                var expectedCount = Unsafe.Add(ref expectedCountSpace, i);
                Add(fen, pt, player, expectedCount);
            }
        }
    }

    private static readonly string[] Fens =
    [
        "rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6",
        "rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6",
        "rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6",
        "rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6",
        "rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6",
        "rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6",
        "rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6",
        "rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6",
        "rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6",
        "rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6",
        "rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6",
        "rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6",
        "5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53",
        "5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53",
        "5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53",
        "5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53",
        "5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53",
        "5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53",
        "5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53",
        "5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53",
        "5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53",
        "5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53",
        "5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53",
        "5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53"
    ];

    private static readonly PieceTypes[] PieceType =
    [
        PieceTypes.Pawn, PieceTypes.Pawn, PieceTypes.Knight, PieceTypes.Knight, PieceTypes.Bishop, PieceTypes.Bishop,
        PieceTypes.Rook, PieceTypes.Rook, PieceTypes.Queen, PieceTypes.Queen, PieceTypes.King, PieceTypes.King,
        PieceTypes.Pawn, PieceTypes.Pawn, PieceTypes.Knight, PieceTypes.Knight, PieceTypes.Bishop, PieceTypes.Bishop,
        PieceTypes.Rook, PieceTypes.Rook, PieceTypes.Queen, PieceTypes.Queen, PieceTypes.King, PieceTypes.King
    ];

    private static readonly Player[] Player =
    [
        Types.Player.White, Types.Player.Black, Types.Player.White, Types.Player.Black, Types.Player.White, Types.Player.Black,
        Types.Player.White, Types.Player.Black, Types.Player.White, Types.Player.Black, Types.Player.White, Types.Player.Black,
        Types.Player.White, Types.Player.Black, Types.Player.White, Types.Player.Black, Types.Player.White, Types.Player.Black,
        Types.Player.White, Types.Player.Black, Types.Player.White, Types.Player.Black, Types.Player.White, Types.Player.Black
    ];

    private static readonly int[] ExpectedCount =
    [
        8, 8, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1,
        4, 3, 1, 1, 0, 0, 2, 2, 0, 0, 1, 1
    ];

    public static readonly BoardTestsTheoryData TheoryData = new(Fens, PieceType, Player, ExpectedCount);

    [Theory]
    [MemberData(nameof(TheoryData))]
    public void BoardPieceCount(string fen, PieceTypes pt, Player p, int expected)
    {
        var pos = _serviceProvider.GetRequiredService<IPosition>();

        var fenData = new FenData(fen);
        var state = new State();

        pos.Set(in fenData, ChessMode.Normal, in state);

        var board = pos.Board;

        var posCount = pos.Pieces(pt, p).Count;
        var boardCount = board.PieceCount(pt, p);

        Assert.Equal(posCount, boardCount);
        Assert.Equal(expected, boardCount);
    }
}