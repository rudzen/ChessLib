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

using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Hash.Tables.Transposition;

/// <summary>
/// Stores an array of <see cref="TranspositionTableEntry"/>. In essence it acts like a entry
/// bucket of 4 elements for each position stored in the <see cref="TranspositionTable"/>
/// </summary>
public sealed class TTCluster : ITTCluster
{
    public static readonly TranspositionTableEntry DefaultEntry = new(
        0,
        Move.EmptyMove,
        0,
        1,
        int.MaxValue,
        int.MaxValue,
        Bound.Void);

    public TTCluster()
    {
        Reset();
    }

    public TranspositionTableEntry[] Cluster { get; private set; }

    public TranspositionTableEntry this[int key]
    {
        get => Cluster[key];
        set => Cluster[key] = value;
    }

    public void Reset()
        => Cluster = new[] { DefaultEntry, DefaultEntry, DefaultEntry, DefaultEntry };
}