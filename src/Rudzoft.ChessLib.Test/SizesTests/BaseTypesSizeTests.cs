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

using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Types;
using File = Rudzoft.ChessLib.Types.File;

namespace Rudzoft.ChessLib.Test.SizesTests;

public sealed class BaseTypesSizeTests
{
    [Fact]
    public void MoveSize()
    {
        const int expected = 2;
        var actual = Unsafe.SizeOf<Move>();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void PieceSize()
    {
        const int expected = 1;
        var actual = Unsafe.SizeOf<Piece>();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SquareSize()
    {
        const int expected = 1;
        var actual = Unsafe.SizeOf<Square>();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RankSize()
    {
        const int expected = 4;
        var actual = Unsafe.SizeOf<Rank>();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FileSize()
    {
        const int expected = 4;
        var actual = Unsafe.SizeOf<File>();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DepthSize()
    {
        const int expected = 4;
        var actual = Unsafe.SizeOf<Depth>();
        Assert.Equal(expected, actual);
    }
}