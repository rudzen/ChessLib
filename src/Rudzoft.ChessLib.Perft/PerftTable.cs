/*
Perft, a chess perft test library

MIT License

Copyright (c) 2017-2022 Rudy Alex Kohn

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

using Rudzoft.ChessLib.Types;
using System;

namespace Rudzoft.ChessLib.Perft;

internal static class PerftTable
{
    private const int EntrySize = 24;
    private const int HashMemory = 256;
    private const int TtSize = HashMemory * 1024 * 1024 / EntrySize;
    private static readonly PerftHashEntry[] Table = new PerftHashEntry[TtSize];

    private struct PerftHashEntry
    {
        public HashKey Hash;
        public ulong Count;
        public int Depth;
    }

    public static void Store(in HashKey key, int depth, in ulong childCount)
    {
        var slot = (int)(key.Key % TtSize);
        Table[slot].Hash = key;
        Table[slot].Count = childCount;
        Table[slot].Depth = depth;
    }

    public static bool Retrieve(in HashKey key, int depth, out ulong childCount)
    {
        var slot = (int)(key.Key % TtSize);
        var match = Table[slot].Depth == depth && Table[slot].Hash == key;

        childCount = match ? Table[slot].Count : 0;
        return match;
    }

    public static void Clear()
    {
        Array.Clear(Table, 0, TtSize);
    }
}