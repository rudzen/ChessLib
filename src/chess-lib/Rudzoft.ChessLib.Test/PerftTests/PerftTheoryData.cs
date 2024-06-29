/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2024 Rudy Alex Kohn

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

namespace Rudzoft.ChessLib.Test.PerftTests;

public sealed class PerftTheoryData : TheoryData<string, int, ulong>
{
    public PerftTheoryData(string[] fens, int[] depths, ulong[] results)
    {
        ArgumentNullException.ThrowIfNull(fens);
        ArgumentNullException.ThrowIfNull(depths);
        ArgumentNullException.ThrowIfNull(results);
        if (fens.Length != depths.Length || fens.Length != results.Length)
            throw new ArgumentException("The number of FENs, depths, and results must be the same.");

        for (var i = 0; i < fens.Length; i++)
            Add(fens[i], depths[i], results[i]);
    }
}