/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2022 Rudy Alex Kohn

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
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Rudzoft.ChessLib.Perft;
using Rudzoft.ChessLib.Perft.Interfaces;

namespace Rudzoft.ChessLib.Benchmark;

[MemoryDiagnoser]
public class PerftBench
{
    private IPerft _perft;

    private readonly PerftPosition _pp;

    public PerftBench()
    {
        _pp = PerftPositionFactory.Create(
            Guid.NewGuid().ToString(),
            Fen.Fen.StartPositionFen,
            new List<PerftPositionValue>(6)
            {
                    new(1, 20),
                    new(2, 400),
                    new(3, 8902),
                    new(4, 197281),
                    new(5, 4865609),
                    new(6, 119060324)
            });
    }

    [Params(5, 6)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        _perft = PerftFactory.Create();
        _perft.AddPosition(_pp);
    }

    [Benchmark]
    public async Task<ulong> PerftIAsync()
    {
        var total = ulong.MinValue;
        for (var i = 0; i < N; i++)
        {
            await foreach (var res in _perft.DoPerft(N).ConfigureAwait(false))
                total += res;
        }
        return total;
    }

    [Benchmark]
    public async Task<ulong> Perft()
    {
        var total = ulong.MinValue;
        for (var i = 0; i < N; ++i)
        {
            total += await _perft.DoPerftAsync(N);
        }

        return total;
    }

}
