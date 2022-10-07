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

using System;

namespace Rudzoft.ChessLib.Hash.Tables;

public abstract class HashTable<T> : IHashTable<T> where T : new()
{
    private T[] _table;

    public int Count => _table.Length;

    public ref T this[ulong key] => ref _table[(uint)key & (_table.Length - 1)];

    /// <summary>
    /// Initialized the table array. In case the table array is initialized with a different
    /// size the existing elements are being kept. The index operator[] will return a reference
    /// struct to avoid any copying, which actually edits the entry "in-place" which avoids
    /// having to store the entry again.
    ///
    /// The process is a bit tricky:
    /// - Because of the nature of C# it requires a custom object initializer function
    /// - The initializer should initialize a single entry object
    /// </summary>
    /// <param name="elementSize">The size of <see cref="T"/></param>
    /// <param name="tableSizeMb">The number of elements to store in the array</param>
    /// <param name="initializer">Initializer function to initialize a single entry object</param>
    public void Initialize(int elementSize, int tableSizeMb, Func<T> initializer)
    {
        var arraySize = tableSizeMb * 1024 * 1024 / elementSize;
        if (_table != null)
        {
            if (_table.Length == arraySize)
                return;

            var currentLength = _table.Length;

            Array.Resize(ref _table, arraySize);

            if (currentLength > arraySize)
                return;

            for (var i = currentLength; i < arraySize; ++i)
                _table[i] = initializer();
        }
        else
        {
            _table = new T[arraySize];
            for (var i = 0; i < _table.Length; ++i)
                _table[i] = initializer();
        }
    }
}