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

namespace Chess.Test.SizesTests;

using FluentAssertions;
using Xunit;

public class BaseTypesSizeTests
{
    [Fact]
    public unsafe void MoveSize()
    {
        const int expected = 2;
        var actual = sizeof(Rudz.Chess.Types.Move);
        actual.Should().Be(expected);
    }

    [Fact]
    public unsafe void PieceSize()
    {
        const int expected = 1;
        var actual = sizeof(Rudz.Chess.Types.Piece);
        actual.Should().Be(expected);
    }

    [Fact]
    public unsafe void SquareSize()
    {
        const int expected = 4;
        var actual = sizeof(Rudz.Chess.Types.Square);
        actual.Should().Be(expected);
    }

    [Fact]
    public unsafe void RankSize()
    {
        const int expected = 4;
        var actual = sizeof(Rudz.Chess.Types.Rank);
        actual.Should().Be(expected);
    }
}
