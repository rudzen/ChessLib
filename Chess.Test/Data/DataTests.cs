/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2020 Rudy Alex Kohn

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

namespace Chess.Test.Data
{
    using Rudz.Chess.Enums;
    using Rudz.Chess.Types;
    using System.Text;
    using Xunit;

    public sealed class DataTests
    {
        [Fact]
        public void TestSquareChars()
        {
            var chars = new StringBuilder(1024);
            var strings = new StringBuilder(1024);

            for (Square sq = Squares.a1; sq; ++sq)
            {
                chars.Clear();
                strings.Clear();
                chars.Append(sq.FileChar);
                chars.Append(sq.RankChar());
                strings.Append(sq.ToString());
                Assert.Equal(chars.ToString(), strings.ToString());
            }
        }
    }
}