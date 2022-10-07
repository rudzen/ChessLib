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

namespace Rudzoft.ChessLib.Test.FileTests;

public sealed class FileEdgeDistanceTests
{
    [Theory]
    [InlineData(Files.FileA, 0)]
    [InlineData(Files.FileB, 1)]
    [InlineData(Files.FileC, 2)]
    [InlineData(Files.FileD, 3)]
    [InlineData(Files.FileE, 3)]
    [InlineData(Files.FileF, 2)]
    [InlineData(Files.FileG, 1)]
    [InlineData(Files.FileH, 0)]
    public void FileEdgeDistanceFolding(Files fs, int expected)
    {
        var f = new File(fs);
        var actual = f.EdgeDistance();
        Assert.Equal(expected, actual);
    }
}