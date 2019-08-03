/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2019 Rudy Alex Kohn

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

namespace Rudz.Chess.Perft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /*
    // (first version)
    // * Summary *

    BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17763.379 (1809/October2018Update/Redstone5)
    Intel Core i7-8086K CPU 4.00GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
    .NET Core SDK=2.2.101
      [Host]     : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT  [AttachedDebugger]
      DefaultJob : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT

    Method	N	Mean	Error	StdDev
    Result	1	296.4 us	1.496 us	1.326 us
    Result	2	300.2 us	1.678 us	1.570 us
    Result	3	379.0 us	1.897 us	1.584 us
    Result	4	2,408.1 us	12.816 us	11.988 us
    Result	5	64,921.2 us	1,310.703 us	1,402.437 us
    Result	6	1,912,300.6 us	3,551.167 us	3,148.017 us
         */

    public sealed class Perft : IPerft
    {
        /// <summary>
        /// The positional data for the run
        /// </summary>
        private readonly List<IPerftPosition> _positions;

        /// <summary>
        /// How deep the test should proceed.
        /// </summary>
        private readonly int _perftLimit;

        private readonly Action<string, ulong> _callback;

        public Perft(int depth, Action<string, ulong> callback, IEnumerable<IPerftPosition> positions = null)
        {
            _positions = positions == null ? new List<IPerftPosition>() : positions.ToList();
            _perftLimit = depth;
            _callback = callback;
        }

        public Perft(int depth, Action<string, ulong> callback = null) : this(depth, callback, null)
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong DoPerft()
        {
            var total = 0ul;

            if (_positions.Count == 0)
            {
                _callback?.Invoke("Unable to run without any perft positions (did you forget to call AddStartPosition()?", total);
                return total;
            }

            var game = new Game();
            foreach (var position in _positions)
            {
                game.SetFen(position.Fen);
                var res = game.Perft(_perftLimit);
                total += res;
                _callback?.Invoke(position.Fen, res);
            }

            return total;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearPositions() => _positions.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPosition(IPerftPosition pp)
        {
            _positions.Add(pp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddStartPosition()
        {
            var vals = new List<ulong>(6)
            {
                20,
                400,
                8902,
                197281,
                4865609,
                119060324
            };
            _positions.Add(new PerftPosition(Fen.Fen.StartPositionFen, vals));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetPositionCount(int index, int depth) => _positions[index].Value[depth - 1];
    }
}