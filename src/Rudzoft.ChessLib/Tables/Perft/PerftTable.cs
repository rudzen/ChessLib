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
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Tables.Perft;

public struct PerftTableEntry : IPerftTableEntry
{
    public HashKey Key { get; set; }
    
    public ulong Count { get; set; }
    
    public int Depth { get; set; }
    
    public Score Evaluate(IPosition pos)
    {
        throw new NotImplementedException();
    }
}

public sealed class PerftTable : HashTable<IPerftTableEntry>
{
    private const int HashMemory = 4;
    private static readonly int ElementSize = Unsafe.SizeOf<PerftTableEntry>();

    private readonly int _mask;

    public PerftTable()
    {
        Initialize(ElementSize, HashMemory, static key => new PerftTableEntry { Key = key, Count = ulong.MinValue, Depth = -1 });
        _mask = Count - 1;
    }
    
    public ref IPerftTableEntry TryGet(in IPosition pos, int depth, out bool found)
    {
        var posKey = pos.State.PositionKey;
        var entryKey = posKey & _mask ^ depth;
        ref var entry = ref this[entryKey];
        found = entry.Key == entryKey && entry.Depth == depth;
        // if (!found)
        //     entry.Key = entryKey;
        return ref entry;
    }

    public void Store(in HashKey key, int depth, in ulong count)
    {
        var entryKey = key & _mask ^ depth;
        ref var entry = ref this[entryKey];
        entry.Key = entryKey;
        entry.Depth = depth;
        entry.Count = count;
    }
}