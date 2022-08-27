using Rudz.Chess.Enums;
using Rudz.Chess.Types;
using Xunit;

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
