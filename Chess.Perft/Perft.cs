/*
Perft, a chess perft test library

MIT License

Copyright (c) 2017-2020 Rudy Alex Kohn

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

namespace Chess.Perft
{
    using Interfaces;
    using Rudz.Chess;
    using Rudz.Chess.Fen;
    using Rudz.Chess.Types;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

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
        private readonly IDictionary<HashKey, ulong> _results;
        
        public Perft(IGame game, IEnumerable<IPerftPosition> positions)
        {
            Positions = positions.ToList();
            CurrentGame = game;
            _results = new Dictionary<HashKey, ulong>(Positions.Count);
        }

        public Action<string> BoardPrintCallback { get; set; }

        /// <summary>
        /// The positional data for the run
        /// </summary>
        public List<IPerftPosition> Positions { get; set; }

        public IGame CurrentGame { get; set; }
        public int Depth { get; set; }
        public ulong Expected { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<ulong> DoPerft(int depth)
        {
            if (Positions.Count == 0)
                yield break;

            foreach (var fd in Positions.Select(p => new FenData(p.Fen)))
            {
                Game.Table.NewSearch();
                CurrentGame.Pos.SetFen(fd);

                var result = CurrentGame.Perft(depth, true);
                
                // BoardPrintCallback?.Invoke(fd.ToString());
                yield return result;
            }
        }

        public Task<ulong> DoPerftAsync(int depth)
            => Task.Run(()
                => CurrentGame.Perft(depth, true));

        public string GetBoard()
            => CurrentGame.ToString();

        public void SetGamePosition(IPerftPosition pp)
        {
            var fp = new FenData(pp.Fen);
            CurrentGame.Pos.SetFen(fp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearPositions() => Positions.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPosition(IPerftPosition pp)
            => Positions.Add(pp);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasPositionCount(int index, int depth)
        {
            var pos = Positions[index];

            if (pos.Value == null)
                return false;

            if (!pos.Value.Any())
                return false;

            var depthValue = pos.Value.FirstOrDefault(v => v.Item1 == depth && v.Item2 > 0);

            return !depthValue.Equals(default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetPositionCount(int index, int depth)
            => Positions[index].Value[depth - 1].Item2;
    }
}