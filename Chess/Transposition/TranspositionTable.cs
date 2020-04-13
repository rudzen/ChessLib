/*
ChessLib, a chess data structure library

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

namespace Rudz.Chess.Transposition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Types;

    public sealed class TranspositionTable : ITranspositionTable
    {
        private static readonly int ClusterSize;

        private List<ITTCluster> _table;
        private ulong _elements;
        private int _fullnessElements;
        private sbyte _generation;

        static TranspositionTable()
        {
            unsafe
            {
                ClusterSize = sizeof(TranspositionTableEntry) * 4;
            }
        }

        public TranspositionTable(int mbSize)
        {
            SetSize(mbSize);
        }

        /// <summary>
        /// Number of table hits
        /// </summary>
        public ulong Hits { get; private set; }

        public int Size { get; private set; }

        /// <summary>
        /// Increases the generation of the table by one
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NewSearch() => ++_generation;

        /// <summary>
        /// Sets the size of the table in Mb
        /// </summary>
        /// <param name="mbSize">The size to set it to</param>
        /// <returns>The number of clusters in the table</returns>
        public ulong SetSize(int mbSize)
        {
            Size = mbSize;
            var size = (int)(((ulong)mbSize << 20) / (ulong)ClusterSize);
            if (_table == null)
            {
                _table = new List<ITTCluster>(size);
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

        /// <summary>
        /// Finds a cluster in the table based on a position key
        /// </summary>
        /// <param name="key">The position key</param>
        /// <returns>The cluster of the keys position in the table</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITTCluster FindCluster(ulong key)
        {
            var idx = (int)((uint)(key) & (_elements - 1));
            return _table[idx];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Refresh(TranspositionTableEntry tte) => tte.Generation = _generation;

        /// <summary>
        /// Probes the transposition table for a entry that matches the position key.
        /// </summary>
        /// <param name="key">The position key</param>
        /// <returns>(true, entry) if one was found, (false, empty) if not found</returns>
        public (bool, TranspositionTableEntry) Probe(ulong key)
        {
            var ttc = FindCluster(key);
            var keyH = (uint)(key >> 32);
            var g = _generation;

            // Probing the Table will automatically update the generation of the entry
            // in case the probing retrieves an element.

            TranspositionTableEntry e = default;
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

        /// <summary>
        /// Probes the table for the first cluster index which matches the position key
        /// </summary>
        /// <param name="key">The position key</param>
        /// <returns>The cluster entry</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TranspositionTableEntry ProbeFirst(ulong key) => FindCluster(key)[0];

        /// <summary>
        /// Stores a move in the transposition table.
        /// It will automatically detect the best cluster location to store it in.
        /// If a similar move already is present, a simple check if done to make sure
        /// it actually is an improvement of the previous move.
        /// </summary>
        /// <param name="key">The position key</param>
        /// <param name="value">The value of the move</param>
        /// <param name="type">The bound type, e.i. did it exceed alpha or beta</param>
        /// <param name="depth">The depth of the move</param>
        /// <param name="move">The move it self</param>
        /// <param name="statValue">The static value of the move</param>
        public void Store(ulong key, int value, Bound type, sbyte depth, Move move, int statValue)
        {
            // Use the high 32 bits as key inside the cluster
            var e = new TranspositionTableEntry((uint)(key >> 32), move, depth, _generation, value, statValue, type);

            var ttc = FindCluster(key);

            var clusterIndex = 0;
            var found = false;

            for (var i = 0; i < ttc.Cluster.Length; ++i)
            {
                if (ttc.Cluster[i].Key32 != 0 && ttc.Cluster[i].Key32 != e.Key32)
                    continue;
                
                clusterIndex = i;
                found = true;
                break;
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

        /// <summary>
        /// Get the approximation full % of the table
        /// // todo : fix
        /// </summary>
        /// <returns>The % as integer value</returns>
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

        /// <summary>
        /// Clears the current table
        /// </summary>
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
                    ttc.Cluster[j] = new TranspositionTableEntry();
                    ttc.Cluster[j].Defaults();
                }
                _table.Add(ttc);
            }
        }
    }
}