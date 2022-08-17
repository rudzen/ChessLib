using Rudz.Chess.Enums;
using Rudz.Chess.Types;
using Xunit;

namespace Chess.Test.SquareTests;

public class SquareFromStringTests
{
    [Fact]
    public void SquareFromStringIsValid()
    {
        const string squareString = "e2";

        var expected = new Square(Squares.e2);
        Square actual = squareString;

        Assert.Equal(expected, actual);
    }
}