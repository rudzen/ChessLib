/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2018 Rudy Alex Kohn

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

using Rudz.Chess.Fen;

namespace ChessLibTest
{
    using NUnit.Framework;
    using Rudz.Chess;

    [TestFixture]
    public class FenTests
    {
        [Test]
        public void SetFenTest()
        {
            FenError expected = new FenError(0, 0);

            Game game = new Game();

            FenError actual = game.NewGame();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetFenTest()
        {
            Game game = new Game();

            FenError expectedError = new FenError(0, 0);

            FenError actualError = game.NewGame();

            // verify no errors given (same as SetFen test)
            Assert.AreEqual(expectedError, actualError);

            FenData expectedFen = new FenData(Fen.StartPositionFen);

            FenData actualFen = game.GetFen();

            Assert.AreEqual(expectedFen, actualFen);
        }
    }
}