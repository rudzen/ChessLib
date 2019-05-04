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

namespace Rudz.Chess.TranspositionTable
{
    using EnsureThat;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Types;

    public sealed class TranspositionTable
    {
        private static readonly int ClusterSize;

        private List<TTCluster> _table;
        private ulong elements;
        private int fullnessElements;
        private byte generation;

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

        public void NewSearch()
        {
            ++generation;
        }

        public ulong Size(int mbSize)
        {
            var size = (int)(((ulong)mbSize << 20) / (ulong)ClusterSize);

            EnsureArg.IsGte(mbSize, 1, nameof(mbSize));

            if (_table == null)
            {
                _table = new List<TTCluster>(size);
                elements = (ulong)size;
            }
            else if (_table.Count != size)
            {
                elements = (ulong)size;
                _table.Clear();
                _table.Capacity = size;
                _table.TrimExcess();
            }
            PopulateTable();
            fullnessElements = Math.Min(size, 250);

            return (ulong)(size * ClusterSize);
        }

        public TTCluster find_cluster(ulong key)
        {
            var idx = (uint) ((uint) key * elements) >> 32;
            var idx2 = (ulong) ((uint) (key) & (elements - 1));
            return _table[(int) idx2];
            // return table[static_cast<size_t>(static_cast<uint32_t>(key) & (elements - 1))];
        }

        public void Refresh(TTEntry tte) => tte.generation = generation;

        public (bool, TTEntry) Probe(ulong key)
        {
            var ttc = find_cluster(key);
            var keyH = (uint)(key >> 32);
            var g = generation;

            // Probing the TT will automatically update the generation of the entry
            // in case the probing retrieves an element. (disabled for now)

            TTEntry e = default;
            var set = false;
            for (var i = 0; i < ttc.Cluster.Length; ++i)
            {
                if (ttc.Cluster[i].key32 != 0 && ttc.Cluster[i].key32 != keyH)
                    continue;

                ttc.Cluster[i].generation = g;
                e = ttc.Cluster[i];
                set = true;
                break;
            }

            return (set, e);
        }

        public TTEntry ProbeFirst(ulong key)
        {
            return find_cluster(key)[0];
        }

        public void Store(ulong key, int value, Bound type, sbyte depth, Move move, int statValue)
        {
            // Use the high 32 bits as key inside the cluster
            var e = new TTEntry((uint)(key >> 32), move, depth, generation, value, statValue, type);

            var ttc = find_cluster(key);

            var clusterIndex = 0;
            var found = false;

            for (var i = 0; i < ttc.Cluster.Length; ++i)
            {
                if (ttc.Cluster[i].key32 == 0 || ttc.Cluster[i].key32 == e.key32)
                {
                    clusterIndex = i;
                    found = true;
                    break;
                }
            }

            var candidate = ttc.Cluster[clusterIndex];

            if (!found)
            {
                var g = generation;
                var index = 0;
                for (var i = 0; i < ttc.Cluster.Length; i++)
                {
                    var ttEntry = ttc.Cluster[i];
                    var (cc1, cc2, cc3, cc4) =
                        (candidate.generation == g,
                            ttEntry.generation == g,
                            ttEntry.type == Bound.Exact,
                            ttEntry.depth < candidate.depth);

                    if (cc1 && cc4 || !(cc2 || cc3) && (cc4 || cc1))
                    {
                        index = i;
                        candidate = ttEntry;
                    }
                }

                clusterIndex = index;
            }

            //Console.WriteLine("Stored");
            ttc.Cluster[clusterIndex].Save(e);
        }

        public int Fullness()
        {
            var gen = generation;
            var sum = 0;
            for (var i = 0; i < fullnessElements; ++i)
                sum += _table[i].Cluster.Count(x => x.generation == gen);

            return sum * 250 / fullnessElements;
        }

        public void Clear()
        {
            for (var i = 0; i < _table.Count; ++i)
                _table[i] = new TTCluster();
        }

        private void PopulateTable()
        {
            for (var i = 0ul; i < elements; ++i)
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