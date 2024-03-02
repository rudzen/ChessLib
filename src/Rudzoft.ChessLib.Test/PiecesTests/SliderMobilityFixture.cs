/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2023 Rudy Alex Kohn

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

using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Test.PiecesTests;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class SliderMobilityFixture
{
    public int[] BishopExpected { get; } = [7, 9, 11, 13];

    public int[] RookExpected { get; } = [14, 14, 14, 14];

    public BitBoard SliderAttacks(PieceTypes pt, Square sq, in BitBoard occ)
    {
        var index = SliderIndex(pt);
        return index switch

        {
            0 => sq.BishopAttacks(in occ),
            1 => sq.RookAttacks(in occ),
            var _ => sq.QueenAttacks(in occ)
        };
    }

    private static int SliderIndex(PieceTypes pt)
        => pt.AsInt() - 3;
}