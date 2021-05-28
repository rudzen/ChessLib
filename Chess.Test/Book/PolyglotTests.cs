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

using Rudz.Chess.Factories;
using Xunit;

namespace Chess.Test.Book
{
    public class PolyglotTests
    {
        [Fact]
        public void PolyZobristSideTest()
        {
            const string fn = "D:\\coded_c#\\Portfish\\src\\book.bin";

            var game = GameFactory.Create();
            game.NewGame();
            using var b = new Rudz.Chess.Polyglot.Book(game.Pos)
            {
                FileName = fn
            };

            var m = b.Probe(true);

            var notExpected = Rudz.Chess.Types.Move.EmptyMove;

            Assert.True(m.IsNullMove());
        }
    }
}