/*
ChessLib, a chess data structure library

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

using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Tables;

public sealed class HistoryHeuristic : IHistoryHeuristic
{
    private readonly int[][][] _table;

    public HistoryHeuristic()
    {
        _table = new int[Player.Count][][];
        Initialize(Player.White);
        Initialize(Player.Black);
    }

    public void Clear()
    {
        ClearTable(Player.White);
        ClearTable(Player.Black);
    }

    public void Set(Player p, Square from, Square to, int value)
        => _table[p.Side][from.AsInt()][to.AsInt()] = value;

    public int Retrieve(Player p, Square from, Square to)
        => _table[p.Side][from.AsInt()][to.AsInt()];

    private void Initialize(Player p)
    {
        _table[p.Side] = new int[Square.Count][];
        for (var i = 0; i < _table[p.Side].Length; ++i)
            _table[p.Side][i] = new int[Square.Count];
    }

    private void ClearTable(Player p)
    {
        for (var i = 0; i < _table[p.Side].Length; i++)
            _table[p.Side][i].Clear();
    }
}