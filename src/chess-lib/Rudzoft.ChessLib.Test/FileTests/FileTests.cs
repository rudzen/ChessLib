﻿/*
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

using Rudzoft.ChessLib.Types;
using File = Rudzoft.ChessLib.Types.File;

namespace Rudzoft.ChessLib.Test.FileTests;

public sealed class FileTests
{
    [Fact]
    public void FileStructureInt()
    {
        const int val = 3;
        const Files expected = (Files)val;
        var f = new File(val);
        var actual = f.Value;
        Assert.Equal(expected, actual);

        f = val;
        actual = f.Value;
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FileStructureEFile()
    {
        const Files expected = Files.FileG;
        var f = new File(expected);
        var actual = f.Value;
        Assert.Equal(expected, actual);

        f = expected;
        actual = f.Value;
        Assert.Equal(expected, actual);
    }
}