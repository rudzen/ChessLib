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

using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Tables.History;

public sealed class HistoryHeuristic : IHistoryHeuristic
{
    private readonly int[][][] _table;

    public HistoryHeuristic()
    {
        _table = new int[Color.Count][][];
        Initialize(Color.White);
        Initialize(Color.Black);
    }

    public void Clear()
    {
        ClearTable(Color.White);
        ClearTable(Color.Black);
    }

    public void Set(Color c, Square from, Square to, int value)
        => _table[c][from][to] = value;

    public int Retrieve(Color c, Square from, Square to)
        => _table[c][from][to];

    private void Initialize(Color c)
    {
        _table[c] = new int[Square.Count][];
        for (var i = 0; i < _table[c].Length; ++i)
            _table[c][i] = new int[Square.Count];
    }

    private void ClearTable(Color c)
    {
        for (var i = 0; i < _table[c].Length; i++)
            _table[c][i].Clear();
    }
}