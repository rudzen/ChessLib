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

using System;

namespace Chess.Perft;

internal static class PerftTable
{
    private const int ENTRY_SIZE = 24;
    private const int HASH_MEMORY = 256;
    private const int TT_SIZE = HASH_MEMORY * 1024 * 1024 / ENTRY_SIZE;
    private static readonly PerftHashEntry[] _table = new PerftHashEntry[TT_SIZE];

    private struct PerftHashEntry
    {
        public ulong Hash;
        public ulong Count;
        public int Depth;
    }

    public static void Store(ulong zobristHash, int depth, ulong childCount)
    {
        var slot = (int)(zobristHash % TT_SIZE);
        _table[slot].Hash = zobristHash;
        _table[slot].Count = childCount;
        _table[slot].Depth = depth;
    }

    public static bool Retrieve(ulong zobristHash, int depth, out ulong childCount)
    {
        var slot = (int)(zobristHash % TT_SIZE);
        var match = _table[slot].Depth == depth && _table[slot].Hash == zobristHash;

        childCount = match ? _table[slot].Count : 0;
        return match;
    }

    public static void Clear()
    {
        Array.Clear(_table, 0, TT_SIZE);
    }
}
