namespace Rudzoft.ChessLib.Test.DataTests;

public sealed class RecordHashTests
{
    private sealed record C(string One, string Two, DateOnly Date);
    
    [Fact]
    public void SwappedRecordValuesProduceVarianceInHashCodes()
    {
        var date = new DateOnly(2021, 1, 1);
        var c1 = new C("one", "two", date);
        var c2 = new C("two", "one", date);

        var h1 = c1.GetHashCode();
        var h2 = c2.GetHashCode();
        
        Assert.NotEqual(h1, h2);
    }
}