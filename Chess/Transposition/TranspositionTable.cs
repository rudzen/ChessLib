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

namespace Rudz.Chess.Transposition
{
    using EnsureThat;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Types;

    public sealed class TranspositionTable
    {
        private static readonly int ClusterSize;

        private List<TTCluster> _table;
        private ulong _elements;
        private int _fullnessElements;
        private sbyte _generation;

        static TranspositionTable()
        {
            unsafe
            {
                ClusterSize = sizeof(TTEntry) * 4;
            }
        }

        public TranspositionTable(int mbSize)
        {
            Size(mbSize);
        }

        public ulong Hits { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NewSearch() => ++_generation;

        public ulong Size(int mbSize)
        {
            var size = (int)(((ulong)mbSize << 20) / (ulong)ClusterSize);

            EnsureArg.IsGte(mbSize, 1, nameof(mbSize));

            if (_table == null)
            {
                _table = new List<TTCluster>(size);
                _elements = (ulong)size;
            }
            else if (_table.Count != size)
            {
                _elements = (ulong)size;
                _table.Clear();
                _table.Capacity = size;
                _table.TrimExcess();
            }

            _generation = 1;
            PopulateTable();
            _fullnessElements = Math.Min(size, 250);

            return (ulong)(size * ClusterSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TTCluster FindCluster(ulong key)
        {
            var idx = (int)((uint)(key) & (_elements - 1));
            return _table[idx];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Refresh(TTEntry tte) => tte.Generation = _generation;

        public (bool, TTEntry) Probe(ulong key)
        {
            var ttc = FindCluster(key);
            var keyH = (uint)(key >> 32);
            var g = _generation;

            // Probing the TT will automatically update the generation of the entry
            // in case the probing retrieves an element.

            TTEntry e = default;
            var set = false;
            for (var i = 0; i < ttc.Cluster.Length; ++i)
            {
                if (ttc.Cluster[i].Key32 != 0 && ttc.Cluster[i].Key32 != keyH)
                    continue;

                ttc.Cluster[i].Generation = g;
                e = ttc.Cluster[i];
                set = true;
                break;
            }

            if (set)
                Hits++;

            return (set, e);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TTEntry ProbeFirst(ulong key) => FindCluster(key)[0];

        public void Store(ulong key, int value, Bound type, sbyte depth, Move move, int statValue)
        {
            // Use the high 32 bits as key inside the cluster
            var e = new TTEntry((uint)(key >> 32), move, depth, _generation, value, statValue, type);

            var ttc = FindCluster(key);

            var clusterIndex = 0;
            var found = false;

            for (var i = 0; i < ttc.Cluster.Length; ++i)
            {
                if (ttc.Cluster[i].Key32 == 0 || ttc.Cluster[i].Key32 == e.Key32)
                {
                    clusterIndex = i;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var index = 0;
                var candidate = ttc.Cluster[index];
                var g = _generation;
                for (var i = 0; i < ttc.Cluster.Length; ++i)
                {
                    var ttEntry = ttc.Cluster[i];
                    var (cc1, cc2, cc3, cc4) =
                        (candidate.Generation == g,
                            ttEntry.Generation == g,
                            ttEntry.Type == Bound.Exact,
                            ttEntry.Depth < candidate.Depth);

                    if (cc1 && cc4 || !(cc2 || cc3) && (cc4 || cc1))
                    {
                        index = i;
                        candidate = ttEntry;
                    }
                }

                clusterIndex = index;
            }

            ttc.Cluster[clusterIndex].Save(e);
        }

        public int Fullness()
        {
            if (_generation == 1)
                return 0;
            var gen = _generation;
            var sum = 0;
            for (var i = 0; i < _fullnessElements; ++i)
                sum += _table[i].Cluster.Count(x => x.Generation == gen);

            return sum * 250 / _fullnessElements;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            for (var i = 0; i < _table.Count; ++i)
            {
                _table[i] = new TTCluster();
                for (var j = 0; j < _table[0].Cluster.Length; ++j)
                    _table[i].Cluster[j].Defaults();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PopulateTable()
        {
            for (var i = 0ul; i < _elements; ++i)
            {
                var ttc = new TTCluster();
                for (var j = 0; j < ttc.Cluster.Length; j++)
                {
                    ttc.Cluster[j] = new TTEntry();
                    ttc.Cluster[j].Defaults();
                }
                _table.Add(ttc);
            }
        }
    }
}