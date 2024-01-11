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

public sealed class IccfNotation(ObjectPool<IMoveList> moveLists) : Notation(moveLists)
{
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
        var        i  = 0;

        re[i++] = (char)('1' + from.File.AsInt());
        re[i++] = (char)('1' + from.Rank.AsInt());
        re[i++] = (char)('1' + to.File.AsInt());
        re[i++] = (char)('1' + to.Rank.AsInt());

        // ReSharper disable once InvertIf
        if (move.IsPromotionMove())
        {
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            var c = move.PromotedPieceType() switch
            {
                PieceTypes.Queen  => 1,
                PieceTypes.Rook   => 2,
                PieceTypes.Bishop => 3,
                PieceTypes.Knight => 4,
                var _             => throw new NotImplementedException()
            };

            re[i++] = (char)('0' + c);
        }

        return new(re[..i]);
    }
}