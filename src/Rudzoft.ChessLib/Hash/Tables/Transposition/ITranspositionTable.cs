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

using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Hash.Tables.Transposition;

public interface ITranspositionTable
{
    /// <summary>
    /// Number of table hits
    /// </summary>
    ulong Hits { get; }

    int Size { get; }

    /// <summary>
    /// Increases the generation of the table by one
    /// </summary>
    void NewSearch();

    /// <summary>
    /// Sets the size of the table in Mb
    /// </summary>
    /// <param name="mbSize">The size to set it to</param>
    /// <returns>The number of clusters in the table</returns>
    ulong SetSize(int mbSize);

    /// <summary>
    /// Finds a cluster in the table based on a position key
    /// </summary>
    /// <param name="key">The position key</param>
    /// <returns>The cluster of the keys position in the table</returns>
    ITTCluster FindCluster(in HashKey key);

    void Refresh(TranspositionTableEntry tte);

    /// <summary>
    /// Probes the transposition table for a entry that matches the position key.
    /// </summary>
    /// <param name="key">The position key</param>
    /// <param name="e">The entry to apply, will be set to default if not found in table</param>
    /// <returns>(true, entry) if one was found, (false, empty) if not found</returns>
    bool Probe(in HashKey key, ref TranspositionTableEntry e);

    /// <summary>
    /// Probes the table for the first cluster index which matches the position key
    /// </summary>
    /// <param name="key">The position key</param>
    /// <returns>The cluster entry</returns>
    TranspositionTableEntry ProbeFirst(in HashKey key);

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
    void Store(in HashKey key, int value, Bound type, sbyte depth, Move move, int statValue);

    /// <summary>
    /// Get the approximation full % of the table // todo : fix
    /// </summary>
    /// <returns>The % as integer value</returns>
    int Fullness();

    /// <summary>
    /// Clears the current table
    /// </summary>
    void Clear();
}
