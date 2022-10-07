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

namespace Rudzoft.ChessLib.Test.CastleTests;

public sealed class CastleRightTests
{
    [Theory]
    [InlineData(CastleRights.Black)]
    [InlineData(CastleRights.BlackKing)]
    [InlineData(CastleRights.BlackQueen)]
    [InlineData(CastleRights.King)]
    [InlineData(CastleRights.Queen)]
    [InlineData(CastleRights.White)]
    [InlineData(CastleRights.WhiteKing)]
    [InlineData(CastleRights.WhiteQueen)]
    public void Simple(CastleRights v)
    {
        CastleRight cr = v;
        Assert.True(cr.Has(v));
    }

    [Fact]
    public void TildeOperator()
    {
        var cr = CastleRight.King;

        var noKingSide = ~cr;

        Assert.False(noKingSide.Has(CastleRights.BlackKing));
        Assert.False(noKingSide.Has(CastleRights.WhiteKing));
    }

    [Fact]
    public void Not()
    {
        var cr = CastleRight.Any;
        cr = cr.Not(CastleRights.BlackKing);

        Assert.False(cr.Has(CastleRights.BlackKing));
        Assert.True(cr.Has(CastleRights.WhiteKing));
        Assert.True(cr.Has(CastleRights.Queen));
    }
}
