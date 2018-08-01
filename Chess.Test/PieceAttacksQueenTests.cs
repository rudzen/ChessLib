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

namespace ChessLibTest
{
    using NUnit.Framework;
    using Rudz.Chess.Types;

    [TestFixture]
    public class PieceAttacksQueenTests : PieceAttacksSliders
    {
        [Test]
        public override void AlphaPattern()
        {
            const int index = (int)EBands.Alpha;
            const int sliderIndex = 2;

            foreach (Square pieceLocation in Bands[index])
            {
                BitBoard attacks = SlideAttacks[sliderIndex](pieceLocation, EmptyBoard);
                Assert.AreEqual(BishopExpected[index] + RookExpected[index], attacks.Count);
            }
        }

        [Test]
        public override void BetaPattern()
        {
            const int index = (int)EBands.Beta;
            const int sliderIndex = 2;

            foreach (Square pieceLocation in Bands[index])
            {
                BitBoard attacks = SlideAttacks[sliderIndex](pieceLocation, EmptyBoard);
                Assert.AreEqual(BishopExpected[index] + RookExpected[index], attacks.Count);
            }
        }

        [Test]
        public override void GammaPattern()
        {
            const int index = (int)EBands.Gamma;
            const int sliderIndex = 2;

            foreach (Square pieceLocation in Bands[index])
            {
                BitBoard attacks = SlideAttacks[sliderIndex](pieceLocation, EmptyBoard);
                Assert.AreEqual(BishopExpected[index] + RookExpected[index], attacks.Count);
            }
        }

        [Test]
        public override void DeltaPattern()
        {
            const int index = (int)EBands.Delta;
            const int sliderIndex = 2;
            foreach (Square pieceLocation in Bands[index])
            {
                BitBoard attacks = SlideAttacks[sliderIndex](pieceLocation, EmptyBoard);
                Assert.AreEqual(BishopExpected[index] + RookExpected[index], attacks.Count);
            }
        }
    }
}