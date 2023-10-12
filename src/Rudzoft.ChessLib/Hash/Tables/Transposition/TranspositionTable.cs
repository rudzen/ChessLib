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

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        SetSize(config.DefaultSize);
    }

    public ulong Hits { get; private set; }

    /// <summary>
    /// Sets the size of the transposition table,
    /// measured in megabytes.
    /// </summary>
    /// <param name="mbSize"></param>
    public void SetSize(int mbSize)
    {
        var newSize = 1024u;

        // Transposition table consists of clusters and each cluster consists
        // of ClusterSize number of TTEntries. Each non-empty entry contains
        // information of exactly one position and newSize is the number of
        // clusters we are going to allocate.
        while (2UL * newSize * 64 <= (ulong)(mbSize << 20))
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

        var entriesSpan = _entries.AsSpan();
        ref var entries = ref MemoryMarshal.GetReference(entriesSpan);

        for (uint i = 0; i < ClusterSize; i++)
        {
            ref var tte = ref Unsafe.Add(ref entries, ttePos);

            if (tte.key == 0 || tte.key == posKey32) // Empty or overwrite old
            {
                // Preserve any existing ttMove
                if (m == Move.EmptyMove)
                    m = tte.move16;

                tte.save(posKey32, v, t, d, m, _generation, statV, kingD);
                return;
            }

            ref var replaceTte = ref Unsafe.Add(ref entries, replacePos);

            // Implement replace strategy
            //if ((entries[replacePos].generation8 == generation ? 2 : 0) + (tte.generation8 == generation || tte.bound == 3/*Bound.BOUND_EXACT*/ ? -2 : 0) + (tte.depth16 < entries[replacePos].depth16 ? 1 : 0) > 0)
            //{
            //    replacePos = ttePos;
            //}

            if (replaceTte.generation8 == _generation)
            {
                if (tte.generation8 == _generation || tte.bound == Bound.Exact)
                {
                    // +2 
                    if (tte.depth16 < replaceTte.depth16) // +1
                        replacePos = ttePos;
                }
                else // +2
                    replacePos = ttePos;
            }
            // 0
            else if (!(tte.generation8 == _generation || tte.bound == Bound.Exact) &&
                     tte.depth16 < replaceTte.depth16) // +1
                replacePos = ttePos;

            ttePos++;
        }

        Unsafe.Add(ref entries, replacePos).save(posKey32, v, t, d, m, _generation, statV, kingD);
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

        var entries = _entries.AsSpan();
        ref var entriesRef = ref MemoryMarshal.GetReference(entries);

        for (var i = offset; i < ClusterSize + offset; i++)
        {
            ref var tte = ref Unsafe.Add(ref entriesRef, i);

            if (tte.key != posKey32)
                continue;

            ttePos = i;
            entry = tte;
            Hits++;
            return true;
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