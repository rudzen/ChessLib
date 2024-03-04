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

using System.Runtime.CompilerServices;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Notation.Notations;

public sealed class IccfNotation : Notation
{
    public IccfNotation(ObjectPool<MoveList> moveLists) : base(moveLists) { }

    /// <summary>
    /// <para>Converts a move to ICCF notation.</para>
    /// </summary>
    /// <param name="pos">The current position</param>
    /// <param name="move">The move to convert</param>
    /// <returns>ICCF move string</returns>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string Convert(IPosition pos, Move move)
    {
        var (from, to) = move;

        Span<char> re = stackalloc char[5];
        var i = 0;

        re[i++] = (char)('1' + from.File);
        re[i++] = (char)('1' + from.Rank);
        re[i++] = (char)('1' + to.File);
        re[i++] = (char)('1' + to.Rank);

        // ReSharper disable once InvertIf
        if (move.IsPromotionMove())
        {
            var c = PieceTypeToValue(move.PromotedPieceType());
            re[i++] = (char)('0' + c);
        }

        return new(re[..i]);
    }

    private static int PieceTypeToValue(PieceType pt)
    {
        if (pt == PieceType.Queen)
            return 1;
        if (pt == PieceType.Rook)
            return 2;
        if (pt == PieceType.Bishop)
            return 3;
        if (pt == PieceType.Knight)
            return 4;
        throw new NotImplementedException();
    }
}