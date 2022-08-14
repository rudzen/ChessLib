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

namespace Chess.Test.Pieces;

using Rudz.Chess.Types;

/// <summary>
/// Position indication is as follows:
/// Alpha = Outer band (edge)
/// Beta  = 2nd band
/// Gamma = 3rd band
/// Delta = 4th band (inner 4 squares)
/// </summary>
public abstract class PieceAttacks : IPieceAttacks
{
    protected enum EBands
    {
        Alpha = 0,
        Beta = 1,
        Gamma = 2,
        Delta = 3
    }

    /*
     * Patterns
     *
     * Alpha:               Beta:
     * X X X X X X X X      0 0 0 0 0 0 0 0
     * X 0 0 0 0 0 0 X      0 X X X X X X 0
     * X 0 0 0 0 0 0 X      0 X 0 0 0 0 X 0
     * X 0 0 0 0 0 0 X      0 X 0 0 0 0 X 0
     * X 0 0 0 0 0 0 X      0 X 0 0 0 0 X 0
     * X 0 0 0 0 0 0 X      0 X 0 0 0 0 X 0
     * X 0 0 0 0 0 0 X      0 X X X X X X 0
     * X X X X X X X X      0 0 0 0 0 0 0 0
     *
     * Gamma:               Delta:
     * 0 0 0 0 0 0 0 0      0 0 0 0 0 0 0 0
     * 0 0 0 0 0 0 0 0      0 0 0 0 0 0 0 0
     * 0 0 X X X X 0 0      0 0 0 0 0 0 0 0
     * 0 0 X 0 0 X 0 0      0 0 0 X X 0 0 0
     * 0 0 X 0 0 X 0 0      0 0 0 X X 0 0 0
     * 0 0 X X X X 0 0      0 0 0 0 0 0 0 0
     * 0 0 0 0 0 0 0 0      0 0 0 0 0 0 0 0
     * 0 0 0 0 0 0 0 0      0 0 0 0 0 0 0 0
     *
     */

    protected static readonly BitBoard[] Bands = { 0xff818181818181ff, 0x7e424242427e00, 0x3c24243c0000, 0x1818000000 };

    public abstract void AlphaPattern();

    public abstract void BetaPattern();

    public abstract void GammaPattern();

    public abstract void DeltaPattern();
}
