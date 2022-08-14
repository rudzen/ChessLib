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

namespace Chess.Test.ScoreTests;

using Rudz.Chess.Types;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public sealed class ScoreTests
{
    private static readonly KeyValuePair<int, int>[] TestValues;

    static ScoreTests()
    {
        TestValues = new KeyValuePair<int, int>[20];
        TestValues[0] = new KeyValuePair<int, int>(35, 23);
        TestValues[1] = new KeyValuePair<int, int>(-35, 23);
        TestValues[2] = new KeyValuePair<int, int>(35, -23);
        TestValues[3] = new KeyValuePair<int, int>(-35, -23);
        TestValues[4] = new KeyValuePair<int, int>(350, 230);
        TestValues[5] = new KeyValuePair<int, int>(3542, 234);
        TestValues[6] = new KeyValuePair<int, int>(35, -223);
        TestValues[7] = new KeyValuePair<int, int>(0, 0);
        TestValues[8] = new KeyValuePair<int, int>(56, 23);
        TestValues[9] = new KeyValuePair<int, int>(15423, -23);
        TestValues[10] = new KeyValuePair<int, int>(2, 1);
        TestValues[11] = new KeyValuePair<int, int>(425, 23);
        TestValues[12] = new KeyValuePair<int, int>(35, 4223);
        TestValues[13] = new KeyValuePair<int, int>(-35231, -24223);
        TestValues[14] = new KeyValuePair<int, int>(-1, 1);
        TestValues[15] = new KeyValuePair<int, int>(1, -1);
        TestValues[16] = new KeyValuePair<int, int>(2, 23);
        TestValues[17] = new KeyValuePair<int, int>(55, 23);
        TestValues[18] = new KeyValuePair<int, int>(0, 23);
        TestValues[19] = new KeyValuePair<int, int>(650, -2323);
    }

    [Fact]
    public void ScoreMg()
    {
        var scores = TestValues.Select(x => new Score(x.Key, x.Value));
        var i = 0;
        foreach (var score in scores)
        {
            var expected = TestValues[i++];
            var actual = new KeyValuePair<int, int>(score.Mg(), score.Eg());
            Assert.Equal(expected, actual);
        }
    }
}
