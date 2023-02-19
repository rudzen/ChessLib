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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Options;
using Rudzoft.ChessLib.Exceptions;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Hash.Tables.Transposition;

public sealed class TranspositionTable : ITranspositionTable
{
    private static readonly int ClusterSize;

    private Cluster[] _table;
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

    public TranspositionTable(IOptions<TranspositionTableConfiguration> options)
    {
        _table = Array.Empty<Cluster>();
        Size = options.Value.DefaultSize;
        SetSize(Size);
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
        if (mbSize < 0)
            throw new TranspositionTableFailure($"Unable to create table with negative size: {mbSize}");

        Size = mbSize;
        var size = (int)(((ulong)mbSize << 20) / (ulong)ClusterSize);
        var resize = false;
        var currentSize = _table.Length;
        if (_table.Length == 0)
        {
            _table = new Cluster[size];
            PopulateTable(0, size);
        }
        else if (currentSize != size)
            resize = true;

        if (resize)
        {
            Array.Resize(ref _table, size);
            if (currentSize < size)
                PopulateTable(currentSize, size);
        }

        _elements = (ulong)size;
        _generation = 1;
        _fullnessElements = Math.Min(size, 250);

        return (ulong)(size * ClusterSize);
    }

    /// <summary>
    /// Finds a cluster in the table based on a position key
    /// </summary>
    /// <param name="key">The position key</param>
    /// <returns>The cluster of the keys position in the table</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Cluster FindCluster(in HashKey key)
    {
        var idx = (int)(key.LowerKey & (_elements - 1));
        return ref _table[idx];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Refresh(ref TranspositionTableEntry tte) => tte.Generation = _generation;

    /// <summary>
    /// Probes the transposition table for a entry that matches the position key.
    /// </summary>
    /// <param name="key">The position key</param>
    /// <param name="e">Target entry</param>
    /// <returns>(true, entry) if one was found, (false, empty) if not found</returns>
    public bool Probe(in HashKey key, ref TranspositionTableEntry e)
    {
        ref var ttc = ref FindCluster(in key);
        var g = _generation;

        // Probing the Table will automatically update the generation of the entry in case the
        // probing retrieves an element.

        var set = false;

        for (var i = 0; i < ClusterSize; ++i)
        {
            ref var entry = ref ttc[i];
            if (entry.Key32 != uint.MinValue && entry.Key32 != key.UpperKey)
                continue;

            entry.Generation = g;
            e = entry;
            set = true;
            Hits++;
            break;
        }
        
        if (!set)
            e = default;

        return set;
    }

    public void Save(in HashKey key, in TranspositionTableEntry e)
    {
        ref var ttc = ref FindCluster(in key);
        var entryIndex = 0;

        for (var i = 0; i < ClusterSize; ++i)
        {
            ref var entry = ref ttc[i];
            if (entry.Key32 != uint.MinValue && entry.Key32 != key.UpperKey)
                continue;

            entryIndex = i;
            break;
        }
        
        ttc[entryIndex] = e;
    }

    /// <summary>
    /// Probes the table for the first cluster index which matches the position key
    /// </summary>
    /// <param name="key">The position key</param>
    /// <returns>The cluster entry</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref TranspositionTableEntry ProbeFirst(in HashKey key) => ref FindCluster(key)[0];

    /// <summary>
    /// Stores a move in the transposition table. It will automatically detect the best cluster
    /// location to store it in. If a similar move already is present, a simple check if done to
    /// make sure it actually is an improvement of the previous move.
    /// </summary>
    /// <param name="key">The position key</param>
    /// <param name="value">The value of the move</param>
    /// <param name="type">The bound type, e.i. did it exceed alpha or beta</param>
    /// <param name="depth">The depth of the move</param>
    /// <param name="move">The move it self</param>
    /// <param name="statValue">The static value of the move</param>
    public void Store(in HashKey key, int value, Bound type, sbyte depth, Move move, int statValue)
    {
        ref var ttc = ref FindCluster(in key);

        var clusterIndex = 0;
        var found = false;

        for (var i = 0; i < ClusterSize; ++i)
        {
            ref var entry = ref ttc[i];
            if (entry.Key32 != uint.MinValue && entry.Key32 != key.UpperKey)
                continue;

            clusterIndex = i;
            found = true;
            break;
        }

        if (!found)
        {
            var index = 0;
            ref var candidate = ref ttc[index];
            var g = _generation;

            for (var i = 0; i < ClusterSize; ++i)
            {
                ref var entry = ref ttc[i];
                var (cc1, cc2, cc3, cc4) = (candidate.Generation == g, entry.Generation == g, entry.Type == Bound.Exact, entry.Depth <= candidate.Depth);
                if ((cc1 && cc4) || (!(cc2 || cc3) && (cc4 || cc1)))
                {
                    index = i;
                    candidate = entry;
                }
            }

            clusterIndex = index;
        }

        var e = new TranspositionTableEntry(key.UpperKey, move, depth, _generation, value, statValue, type);

        ref var target = ref ttc[clusterIndex];
        target.Save(in e);
    }

    /// <summary>
    /// Get the approximation full % of the table // todo : fix
    /// </summary>
    /// <returns>The % as integer value</returns>
    public int Fullness()
    {
        if (_generation == 1)
            return 0;
        var gen = _generation;
        var sum = 0;

        for (var i = 0; i < _fullnessElements; ++i)
        {
            for (var j = 0; j < ClusterSize; ++j)
            {
                ref var entry = ref _table[i][j];
                if (entry.Generation == gen)
                    sum++;
            }
        }

        return sum * 250 / _fullnessElements;
    }

    /// <summary>
    /// Clears the current table
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        ref var tableSpace = ref MemoryMarshal.GetArrayDataReference(_table);
        for (var i = 0; i < _table.Length; ++i)
            Unsafe.Add(ref tableSpace, i).Reset();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PopulateTable(int from, int to)
    {
        for (var i = from; i < to; ++i)
            _table[i] = new Cluster();
    }
}
