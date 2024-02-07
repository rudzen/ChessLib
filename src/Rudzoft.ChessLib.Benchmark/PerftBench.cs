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

using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Rudzoft.ChessLib.Hash;
using Rudzoft.ChessLib.Hash.Tables.Transposition;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.ObjectPoolPolicies;
using Rudzoft.ChessLib.Perft;
using Rudzoft.ChessLib.Perft.Interfaces;
using Rudzoft.ChessLib.Protocol.UCI;
using Rudzoft.ChessLib.Types;
using Rudzoft.ChessLib.Validation;

namespace Rudzoft.ChessLib.Benchmark;

[MemoryDiagnoser]
// ReSharper disable once ClassCanBeSealed.Global
public class PerftBench
{
    private IPerft _perft;

    [Params(5, 6)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        var pp = PerftPositionFactory.Create(
            Guid.NewGuid().ToString(),
            Fen.Fen.StartPositionFen,
            [
                new(1, 20),
                new(2, 400),
                new(3, 8902),
                new(4, 197281),
                new(5, 4865609),
                new(6, 119060324)
            ]);

        var ttConfig = new TranspositionTableConfiguration { DefaultSize = 1 };
        var options = Options.Create(ttConfig);
        var tt = new TranspositionTable(options);

        var moveListObjectPool = new DefaultObjectPool<IMoveList>(new MoveListPolicy());

        var uci = new Uci(moveListObjectPool);
        uci.Initialize();
        var cpu = new Cpu();

        var sp = new SearchParameters();

        var board = new Board();
        var values = new Values();
        var rKiss = new RKiss();
        var zobrist = new Zobrist(rKiss);
        var cuckoo = new Cuckoo(zobrist);
        var validator = new PositionValidator(zobrist);

        var pos = new Position(board, values, zobrist, cuckoo, validator, moveListObjectPool);

        var game = new Game(tt, uci, cpu, sp, pos, moveListObjectPool);
        _perft = new Perft.Perft(game, new[] { pp });
    }

    [Benchmark]
    public async Task<UInt128> PerftIAsync()
    {
        var total = UInt128.MinValue;
        for (var i = 0; i < N; i++)
            await foreach (var res in _perft.DoPerft(N).ConfigureAwait(false))
                total += res;
        return total;
    }

    [Benchmark]
    public async Task<UInt128> Perft()
    {
        var total = UInt128.MinValue;
        for (var i = 0; i < N; ++i)
            total += await _perft.DoPerftAsync(N);

        return total;
    }
}