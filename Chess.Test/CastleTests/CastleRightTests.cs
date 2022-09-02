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

namespace Chess.Test.CastleTests;

public sealed class CastleRightTests
{
    [Theory]
    [InlineData(CastlelingRights.BlackCastleling)]
    [InlineData(CastlelingRights.BlackOo)]
    [InlineData(CastlelingRights.BlackOoo)]
    [InlineData(CastlelingRights.KingSide)]
    [InlineData(CastlelingRights.QueenSide)]
    [InlineData(CastlelingRights.WhiteCastleling)]
    [InlineData(CastlelingRights.WhiteOo)]
    [InlineData(CastlelingRights.WhiteOoo)]
    public void Simple(CastlelingRights v)
    {
        CastleRight cr = v;
        Assert.True(cr.Has(v));
    }

    [Fact]
    public void TildeOperator()
    {
        var cr = CastleRight.KING_SIDE;

        var noKingSide = ~cr;

        Assert.False(noKingSide.Has(CastlelingRights.BlackOo));
        Assert.False(noKingSide.Has(CastlelingRights.WhiteOo));
    }

    [Fact]
    public void Not()
    {
        var cr = CastleRight.ANY;
        cr = cr.Not(CastlelingRights.BlackOo);

        Assert.False(cr.Has(CastlelingRights.BlackOo));
        Assert.True(cr.Has(CastlelingRights.WhiteOo));
        Assert.True(cr.Has(CastlelingRights.QueenSide));
    }
}
