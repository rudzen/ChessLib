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

using System.Collections.Generic;
using System.Linq;
using Rudzoft.ChessLib.Extensions;

namespace Rudzoft.ChessLib.Test.DataTests;

public sealed class StringExtensionsTests
{
    [Theory]
    [InlineData("this is a simple spaced string", new[] { 4, 7, 9, 16, 23 })]
    [InlineData("yet another", new[] { 3 })]
    [InlineData("another", new int[0])]
    public void GetLocations(string s, int[] positions)
    {
        var actual = s.GetLocations().ToArray();

        Assert.Equal(positions.Length, actual.Length);
        Assert.Equal(positions, actual);
    }

    [Theory]
    [InlineData("setoption name UCI_Chess960 value false", ' ', ' ')]
    public void ParseBaseSplit(string command, char separator, char trimToken)
    {
        var q = StringExtensions.Parse(command, separator, trimToken);

        var qq = new Queue<string>(command.Split(' '));

        Assert.Equal(q, qq);
        Assert.NotNull(q);
    }
}