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
using Microsoft.Extensions.Options;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Hash.Tables.Transposition;

public sealed class TranspositionTable : ITranspositionTable
{
    //     private static readonly int ClusterSize = Unsafe.SizeOf<TTEntry>() * 4;
    private const uint ClusterSize = 4;

    private static readonly TTEntry StaticEntry = new();

    private TTEntry[] _entries;
    private uint _size;
    private uint _sizeMask;
    private byte _generation; // Size must be not bigger then TTEntry::generation8

    public TranspositionTable(IOptions<TranspositionTableConfiguration> options)
    {
        var config = options.Value;
        SetSize((uint)config.DefaultSize);
    }

    public ulong Hits { get; private set; }

    /// <summary>
    /// Sets the size of the transposition table,
    /// measured in megabytes.
    /// </summary>
    /// <param name="mbSize"></param>
    public void SetSize(uint mbSize)
    {
        var newSize = 1024u;

        // Transposition table consists of clusters and each cluster consists
        // of ClusterSize number of TTEntries. Each non-empty entry contains
        // information of exactly one position and newSize is the number of
        // clusters we are going to allocate.
        while (2UL * newSize * 64 <= mbSize << 20)
            newSize *= 2;

        if (newSize == _size)
            return;

        _size = newSize;
        _sizeMask = _size - 1;

        _entries = new TTEntry[_size * 4];
    }

    /// <summary>
    /// Overwrites the entire transposition table
    /// with zeroes. It is called whenever the table is resized, or when the
    /// user asks the program to clear the table (from the UCI interface).
    /// </summary>
    public void Clear()
    {
        if (_entries != null)
            Array.Clear(_entries, 0, _entries.Length);
        Hits = ulong.MinValue;
    }

    /// <summary>
    /// writes a new entry containing position key and
    /// valuable information of current position. The lowest order bits of position
    /// key are used to decide on which cluster the position will be placed.
    /// When a new entry is written and there are no empty entries available in cluster,
    /// it replaces the least valuable of entries. A TTEntry t1 is considered to be
    /// more valuable than a TTEntry t2 if t1 is from the current search and t2 is from
    /// a previous search, or if the depth of t1 is bigger than the depth of t2.
    /// </summary>
    /// <param name="posKey"></param>
    /// <param name="v"></param>
    /// <param name="t"></param>
    /// <param name="d"></param>
    /// <param name="m"></param>
    /// <param name="statV"></param>
    /// <param name="kingD"></param>
    public void Store(in HashKey posKey, Value v, Bound t, Depth d, Move m, Value statV, Value kingD)
    {
        // Use the high 32 bits as key inside the cluster
        var posKey32 = posKey.UpperKey;

        var replacePos = (posKey.LowerKey & _sizeMask) << 2;
        var ttePos = replacePos;

        for (uint i = 0; i < ClusterSize; i++)
        {
            ref var tte = ref _entries[ttePos];

            if (tte.key == 0 || tte.key == posKey32) // Empty or overwrite old
            {
                // Preserve any existing ttMove
                if (m == Move.EmptyMove)
                    m = tte.move16;

                _entries[ttePos].save(posKey32, v, t, d, m, _generation, statV, kingD);
                return;
            }

            // Implement replace strategy
            //if ((entries[replacePos].generation8 == generation ? 2 : 0) + (tte.generation8 == generation || tte.bound == 3/*Bound.BOUND_EXACT*/ ? -2 : 0) + (tte.depth16 < entries[replacePos].depth16 ? 1 : 0) > 0)
            //{
            //    replacePos = ttePos;
            //}

            if (_entries[replacePos].generation8 == _generation)
            {
                if (tte.generation8 == _generation || tte.bound == Bound.Exact)
                {
                    // +2 
                    if (tte.depth16 < _entries[replacePos].depth16) // +1
                        replacePos = ttePos;
                }
                else // +2
                    replacePos = ttePos;
            }
            else // 0
            if (!(tte.generation8 == _generation || tte.bound == Bound.Exact) &&
                tte.depth16 < _entries[replacePos].depth16) // +1
                replacePos = ttePos;

            ttePos++;
        }

        _entries[replacePos].save(posKey32, v, t, d, m, _generation, statV, kingD);
    }

    /// <summary>
    /// looks up the current position in the
    /// transposition table. Returns a pointer to the TTEntry or NULL if
    /// position is not found.
    /// </summary>
    /// <param name="posKey"></param>
    /// <param name="ttePos"></param>
    /// <param name="entry"></param>
    /// <returns>true if found, otherwise false</returns>
    public bool Probe(in HashKey posKey, ref uint ttePos, out TTEntry entry)
    {
        var posKey32 = posKey.UpperKey;
        var offset = (posKey.LowerKey & _sizeMask) << 2;

        for (var i = offset; i < ClusterSize + offset; i++)
        {
            if (_entries[i].key == posKey32)
            {
                ttePos = i;
                entry = _entries[i];
                Hits++;
                return true;
            }
        }

        entry = StaticEntry;
        return false;
    }

    /// <summary>
    /// Is called at the beginning of every new
    /// search. It increments the "generation" variable, which is used to
    /// distinguish transposition table entries from previous searches from
    /// entries from the current search.
    /// </summary>
    public void NewSearch() => _generation++;
}

//
// public sealed class TranspositionTable : ITranspositionTable
// {
//     private static readonly int ClusterSize = Unsafe.SizeOf<TranspositionTableEntry>() * 4;
//
//     private Cluster[] _table;
//     private ulong _elements;
//     private int _fullnessElements;
//     private sbyte _generation;
//
//     public TranspositionTable(IOptions<TranspositionTableConfiguration> options)
//     {
//         _table = Array.Empty<Cluster>();
//         Size = options.Value.DefaultSize;
//         SetSize(Size);
//     }
//
//     /// <summary>
//     /// Number of table hits
//     /// </summary>
//     public ulong Hits { get; private set; }
//
//     public int Size { get; private set; }
//
//     /// <summary>
//     /// Increases the generation of the table by one
//     /// </summary>
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public void NewSearch() => ++_generation;
//
//     /// <summary>
//     /// Sets the size of the table in Mb
//     /// </summary>
//     /// <param name="mbSize">The size to set it to</param>
//     /// <returns>The number of clusters in the table</returns>
//     public ulong SetSize(int mbSize)
//     {
//         switch (mbSize)
//         {
//             case < 0:
//                 throw new TranspositionTableFailure($"Unable to create table with negative size: {mbSize}");
//             case 0:
//                 return 0;
//         }
//
//         Size = mbSize;
//         var size = (int)(((ulong)mbSize << 20) / (ulong)ClusterSize);
//         var resize = false;
//         var currentSize = _table.Length;
//         if (_table.Length == 0)
//         {
//             _table = new Cluster[size];
//             PopulateTable(0, size);
//         }
//         else if (currentSize != size)
//             resize = true;
//
//         if (resize)
//         {
//             Array.Resize(ref _table, size);
//             if (currentSize < size)
//                 PopulateTable(currentSize, size);
//         }
//
//         _elements = (ulong)size;
//         _generation = 1;
//         _fullnessElements = Math.Min(size, 250);
//
//         return (ulong)(size * ClusterSize);
//     }
//
//     /// <summary>
//     /// Finds a cluster in the table based on a position key
//     /// </summary>
//     /// <param name="key">The position key</param>
//     /// <returns>The cluster of the keys position in the table</returns>
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public ref Cluster FindCluster(in HashKey key)
//     {
//         var idx = (int)(key.LowerKey & (_elements - 1));
//         return ref _table[idx];
//     }
//
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public void Refresh(ref TranspositionTableEntry tte) => tte.Generation = _generation;
//
//     /// <summary>
//     /// Probes the transposition table for a entry that matches the position key.
//     /// </summary>
//     /// <param name="key">The position key</param>
//     /// <param name="e">Target entry</param>
//     /// <returns>(true, entry) if one was found, (false, empty) if not found</returns>
//     public bool Probe(in HashKey key, ref TranspositionTableEntry e)
//     {
//         ref var ttc = ref FindCluster(in key);
//         var g = _generation;
//
//         // Probing the Table will automatically update the generation of the entry in case the
//         // probing retrieves an element.
//
//         var set = false;
//
//         for (var i = 0; i < ClusterSize; ++i)
//         {
//             ref var entry = ref ttc[i];
//             if (entry.Key32 != uint.MinValue && entry.Key32 != key.UpperKey)
//                 continue;
//
//             entry.Generation = g;
//             e = entry;
//             set = true;
//             Hits++;
//             break;
//         }
//         
//         if (!set)
//             e = default;
//
//         return set;
//     }
//
//     public void Save(in HashKey key, in TranspositionTableEntry e)
//     {
//         ref var ttc = ref FindCluster(in key);
//         var entryIndex = 0;
//
//         for (var i = 0; i < ClusterSize; ++i)
//         {
//             ref var entry = ref ttc[i];
//             if (entry.Key32 != uint.MinValue && entry.Key32 != key.UpperKey)
//                 continue;
//
//             entryIndex = i;
//             break;
//         }
//         
//         ttc[entryIndex] = e;
//     }
//
//     /// <summary>
//     /// Probes the table for the first cluster index which matches the position key
//     /// </summary>
//     /// <param name="key">The position key</param>
//     /// <returns>The cluster entry</returns>
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public ref TranspositionTableEntry ProbeFirst(in HashKey key) => ref FindCluster(key)[0];
//
//     /// <summary>
//     /// Stores a move in the transposition table. It will automatically detect the best cluster
//     /// location to store it in. If a similar move already is present, a simple check if done to
//     /// make sure it actually is an improvement of the previous move.
//     /// </summary>
//     /// <param name="key">The position key</param>
//     /// <param name="value">The value of the move</param>
//     /// <param name="type">The bound type, e.i. did it exceed alpha or beta</param>
//     /// <param name="depth">The depth of the move</param>
//     /// <param name="move">The move it self</param>
//     /// <param name="statValue">The static value of the move</param>
//     public void Store(in HashKey key, int value, Bound type, sbyte depth, Move move, int statValue)
//     {
//         ref var ttc = ref FindCluster(in key);
//
//         var clusterIndex = 0;
//         var found = false;
//
//         for (var i = 0; i < ClusterSize; ++i)
//         {
//             ref var entry = ref ttc[i];
//             if (entry.Key32 != uint.MinValue && entry.Key32 != key.UpperKey)
//                 continue;
//
//             clusterIndex = i;
//             found = true;
//             break;
//         }
//
//         if (!found)
//         {
//             var index = 0;
//             ref var candidate = ref ttc[index];
//             var g = _generation;
//
//             for (var i = 0; i < ClusterSize; ++i)
//             {
//                 ref var entry = ref ttc[i];
//                 var (cc1, cc2, cc3, cc4) = (candidate.Generation == g, entry.Generation == g, entry.Type == Bound.Exact, entry.Depth <= candidate.Depth);
//                 if ((cc1 && cc4) || (!(cc2 || cc3) && (cc4 || cc1)))
//                 {
//                     index = i;
//                     candidate = entry;
//                 }
//             }
//
//             clusterIndex = index;
//         }
//
//         var e = new TranspositionTableEntry(key.UpperKey, move, depth, _generation, value, statValue, type);
//
//         ref var target = ref ttc[clusterIndex];
//         target.Save(in e);
//     }
//
//     /// <summary>
//     /// Get the approximation full % of the table // todo : fix
//     /// </summary>
//     /// <returns>The % as integer value</returns>
//     public int Fullness()
//     {
//         if (_generation == 1)
//             return 0;
//         var gen = _generation;
//         var sum = 0;
//
//         for (var i = 0; i < _fullnessElements; ++i)
//         {
//             for (var j = 0; j < ClusterSize; ++j)
//             {
//                 ref var entry = ref _table[i][j];
//                 if (entry.Generation == gen)
//                     sum++;
//             }
//         }
//
//         return sum * 250 / _fullnessElements;
//     }
//
//     /// <summary>
//     /// Clears the current table
//     /// </summary>
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public void Clear()
//     {
//         ref var tableSpace = ref MemoryMarshal.GetArrayDataReference(_table);
//         for (var i = 0; i < _table.Length; ++i)
//             Unsafe.Add(ref tableSpace, i).Reset();
//     }
//
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     private void PopulateTable(int from, int to)
//     {
//         for (var i = from; i < to; ++i)
//             _table[i] = new Cluster();
//     }
// }