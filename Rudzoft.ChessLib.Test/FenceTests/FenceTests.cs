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

using Rudzoft.ChessLib.Factories;

namespace Rudzoft.ChessLib.Test.FenceTests;

public sealed class FenceTests
{
    [Theory]
    [InlineData("8/8/k7/p1p1p1p1/P1P1P1P1/8/8/4K3 w - - 0 1", true)]
    [InlineData("4k3/5p2/1p1p1P1p/1P1P1P1P/3P4/8/4K3/8 w - - 0 1", true)]
    [InlineData("5k2/8/8/p1p1pPp1/P1P1P1P1/8/8/4K3 w - - 0 1", true)]
    [InlineData("8/8/k7/p1p1pPp1/P1P1P1P1/8/8/4K3 w - - 0 1", false)] // white pawn cannot be blocked by the black king
    [InlineData("5k2/8/8/pPp1pPp1/P1P1P1P1/8/8/4K3 w - - 0 1", false)]
    [InlineData("8/2p5/kp2p1p1/p1p1P1P1/P1P2P2/1P4K1/8/8 w - - 0 1", false)]
    public void Blocked(string fen, bool expected)
    {
        var g = GameFactory.Create(fen);
        var pos = g.Pos;

        var actual = BlockageFactory.IsBlocked(in pos);

        Assert.Equal(expected, actual);
    }
}