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

using System.Linq;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Test.ScoreTests;

public sealed class ScoreTests
{
    private static readonly Score[] TestValues;

    static ScoreTests()
    {
        TestValues = new Score[20];
        TestValues[0] = new Score(35, 23);
        TestValues[1] = new Score(-35, 23);
        TestValues[2] = new Score(35, -23);
        TestValues[3] = new Score(-35, -23);
        TestValues[4] = new Score(350, 230);
        TestValues[5] = new Score(3542, 234);
        TestValues[6] = new Score(35, -223);
        TestValues[7] = new Score(0, 0);
        TestValues[8] = new Score(56, 23);
        TestValues[9] = new Score(15423, -23);
        TestValues[10] = new Score(2, 1);
        TestValues[11] = new Score(425, 23);
        TestValues[12] = new Score(35, 4223);
        TestValues[13] = new Score(-35231, -24223);
        TestValues[14] = new Score(-1, 1);
        TestValues[15] = new Score(1, -1);
        TestValues[16] = new Score(2, 23);
        TestValues[17] = new Score(55, 23);
        TestValues[18] = new Score(0, 23);
        TestValues[19] = new Score(650, -2323);
    }

    [Fact]
    public void ScoreMg()
    {
        const int expectedMg = -14656;
        const int expectedEg = -21989;
        var result = TestValues
            .Aggregate(Score.Zero, static (current, expected) => current + expected);

        Assert.Equal( expectedMg, result.Mg());
        Assert.Equal( expectedEg, result.Eg());
    }

    [Theory]
    [InlineData(10, 10, true, 10, 10)]
    [InlineData(10, 10, false, 0, 0)]
    public void BoolOperator(int initialMg, int initialEg, bool modifier, int expectedMg, int expectedEg)
    {
        var initial = new Score(initialMg, initialEg);
        var expected = new Score(expectedMg, expectedEg);
        var actual = initial * modifier;

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(10, 10, 2, 5.0f, 5.0f)]
    [InlineData(10, 10, 4, 2.5f, 2.5f)]
    public void IntDivideOperator(int initialMg, int initialEg, int divideBy, float expectedMg, float expectedEg)
    {
        var initial = new Score(initialMg, initialEg);
        var expected = new Score(expectedMg, expectedEg);
        var actual = initial / divideBy;

        Assert.Equal(expected, actual);
    }
}