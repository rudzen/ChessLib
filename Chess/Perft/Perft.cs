using System.Threading.Tasks;
using EnsureThat;

namespace Rudz.Chess.Perft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /*

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
        private readonly List<PerftPosition> _positions;

        /// <summary>
        /// How deep the test should proceed.
        /// </summary>
        private readonly int _perftLimit;

        private readonly Action<string, ulong> _callback;

        public Perft(int depth, Action<string, ulong> callback, ICollection<PerftPosition> positions = null)
        {
            EnsureArg.IsGt(depth, 0, nameof(depth));
            _positions = positions == null ? new List<PerftPosition>() : positions.ToList();
            _perftLimit = depth;
            _callback = callback;
        }

        public Perft(int depth, Action<string, ulong> callback = null) : this(depth, callback, null)
        { }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<ulong> DoPerft()
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
                var res = await game.Perft(_perftLimit);
                total += res;
                _callback?.Invoke(position.Fen, res);
            }

            return total;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearPositions() => _positions.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPosition(PerftPosition pp)
        {
            EnsureArg.IsNotNull(pp.Value, nameof(pp.Value));
            EnsureArg.IsNotNullOrWhiteSpace(pp.Fen, nameof(pp.Fen));
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