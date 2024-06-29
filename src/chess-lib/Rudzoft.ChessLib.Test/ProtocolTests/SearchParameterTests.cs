using Rudzoft.ChessLib.Protocol.UCI;

namespace Rudzoft.ChessLib.Test.ProtocolTests;

public sealed class SearchParameterTests
{
    [Fact]
    public void SearchParamInfinite()
    {
        const string expected = "go infinite";
        var sp = new SearchParameters(true);

        var actual = sp.ToString();

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(12, 12, 100, 100, 0, "go wtime 12 btime 12 winc 100 binc 100")]
    [InlineData(120000, 12, 100, 100, 0, "go wtime 120000 btime 12 winc 100 binc 100")]
    [InlineData(120000, 12, 100, 100, 1231, "go wtime 120000 btime 12 movetime 1231 winc 100 binc 100")]
    public void SearchParams(
        ulong whiteTimeMilliseconds,
        ulong blackTimeMilliseconds,
        ulong whiteIncrementTimeMilliseconds,
        ulong blackIncrementTimeMilliseconds,
        ulong moveTime,
        string expected)
    {
        var sp = new SearchParameters(
            whiteTimeMilliseconds,
            blackTimeMilliseconds,
            whiteIncrementTimeMilliseconds,
            blackIncrementTimeMilliseconds,
            moveTime);

        var actual = sp.ToString();

        Assert.Equal(expected, actual);
    }
}